using System;
using System.IO;

namespace OCSS.Util.DirSearch {

   public enum AttrSearchType { ExactMatch = 0, AllMatchPlusAnyOthers = 1, AnyMatch = 2, IgnoreAttributeMatch = 3 };

   /// <summary>Search files and folders using a wrapper around FileInfo, DirectoryInfo, and EnumerateFiles</summary>
   /// <remarks>Skips reparse points on folders</remarks>
   public class DirSearch {
      public static readonly string SearchMaskAllFilesAndFolders = "*.*";
      public static readonly string CurrentFolderInternalName = ".";

      public SearchDef SearchDefinition { get; set; }

      #region File and Folder caller delegates
      public Func<FileInfo, bool> OnFileMatch { get; set; }
      public Func<FileInfo, bool> OnFileFilter { get; set; }
      public Func<DirectoryInfo, bool> OnFolderMatch { get; set; }
      #endregion

      #region Exception caller delegates 
      public Func<string, bool> OnFileException;
      public Func<string, bool> OnFolderException;
      #endregion

      // Note: Keep this an old style delegate as it returns two bools
      public delegate void CustomFolderFilter(DirectoryInfo oneFolder, ref bool skip, ref bool skipChildFolders);
      public event CustomFolderFilter OnFolderFilter;

      private bool cancelFlag;
      private bool skipFileFlag;
      private bool skipFolderFlag;
      private bool skipChildFolders;

      public DirSearch() : this(new SearchDef()) { }

      public DirSearch(SearchDef searchDef) {
         SearchDefinition = searchDef;
         cancelFlag = false;
      }

      public void Execute() {
         if (string.IsNullOrEmpty(SearchDefinition.StartFolder))
            SearchDefinition.StartFolder = Directory.GetCurrentDirectory();
         cancelFlag = false;
         // make sure global excludes are always added
         SearchDefinition.AlwaysExcluded |= AttributeHelper.AttributesExcluded;
         FindFiles(SearchDefinition.StartFolder);
         // recommended to remove open handles on enumerated directories or files (MSDN)
         GC.Collect();
         GC.WaitForPendingFinalizers();
      }

      /// <summary>internal function to find files and folders</summary>
      /// <param name="startDir"></param>
      /// <remarks>function is recursive if pProcessSubs flag is true</remarks>
      private void FindFiles(string startDir) {
         if (cancelFlag)
            return;
         // make sure startDir ends with a slash
         if (startDir[startDir.Length - 1] != Path.DirectorySeparatorChar)
            startDir += Path.DirectorySeparatorChar;
         DirectoryInfo dirInfo = new DirectoryInfo(startDir);
         // check if any of the always exlude attributes are present
         if ((dirInfo.Attributes & SearchDefinition.AlwaysExcluded) != 0)
            return;
         // default to no skip so if no Event, it doesn't skip file(s) and folder(s)
         skipFolderFlag = false;
         skipChildFolders = false;
         OnFolderFilter?.Invoke(dirInfo, ref skipFolderFlag, ref skipChildFolders);
         if (skipFolderFlag == false) {
            if (OnFolderMatch != null) {
               cancelFlag = OnFolderMatch(dirInfo);
               if (cancelFlag)
                  return;
            }
            try {
               // Process all file entries
               foreach (var oneFile in dirInfo.EnumerateFiles(SearchDefinition.SearchMask)) {
                  // check if any of the always exlude attributes are present
                  if ((oneFile.Attributes & SearchDefinition.AlwaysExcluded) != 0)
                     continue;
                  // Debug.WriteLine($"OneFile Attribs: {OneFile.Attributes}");
                  if ((SearchDefinition.AttributeSearchType == AttrSearchType.IgnoreAttributeMatch) ||
                     (((oneFile.Attributes & SearchDefinition.Attributes) != 0) && (SearchDefinition.AttributeSearchType == AttrSearchType.AnyMatch)) ||
                     (((oneFile.Attributes & SearchDefinition.Attributes) == SearchDefinition.Attributes) && (SearchDefinition.AttributeSearchType == AttrSearchType.AllMatchPlusAnyOthers)) ||
                     ((oneFile.Attributes == SearchDefinition.Attributes) && (SearchDefinition.AttributeSearchType == AttrSearchType.ExactMatch))) {
                     skipFileFlag = false;   // default to no skip
                     if (OnFileFilter != null) {
                        skipFileFlag = OnFileFilter(oneFile);
                     }
                     if (skipFileFlag == false) {
                        if (OnFileMatch != null) {
                           cancelFlag = OnFileMatch(oneFile);
                           if (cancelFlag)
                              return;
                        }
                     }
                  }
               }
            }
            catch (UnauthorizedAccessException e) {
               if (OnFileException != null) {
                  cancelFlag = OnFileException(e.Message);
                  if (cancelFlag)
                     return;
               }
            }
            catch (PathTooLongException e) {
               cancelFlag = OnFileException(e.Message);
               if (cancelFlag)
                  return;
            }
         }
         // Recursively process all subfolder entries if ProcessSubs is true
         // Get all of the subfolders if custom folder filtering allowed it (or no custom filtering performed)
         if (skipChildFolders == false && SearchDefinition.ProcessSubdirs) {
            try {
               foreach (var oneFolder in dirInfo.EnumerateDirectories(SearchMaskAllFilesAndFolders, SearchOption.TopDirectoryOnly)) {
                  // skip current directory
                  if ((oneFolder.Name != CurrentFolderInternalName)) {
                     FindFiles(oneFolder.FullName);      // recursive call
                  }
               }
            }
            catch (UnauthorizedAccessException e) {
               if (OnFolderException != null) {
                  cancelFlag = OnFolderException(e.Message);
                  if (cancelFlag)
                     return;
               }
            }
            catch (PathTooLongException e) {
               if (OnFolderException != null) {
                  cancelFlag = OnFolderException(e.Message);
                  if (cancelFlag)
                     return;
               }
            }
         }
      }
   }

}
