﻿using System;
using System.IO;

namespace OCSS.Util.DirSearch {

   public enum AttrSearchType { ExactMatch = 0, AllMatchPlusAnyOthers = 1, AnyMatch = 2, IgnoreAttributeMatch = 3};

   /// <summary>Search files and folders using a wrapper around FileInfo, DirectoryInfo, and EnumerateFiles</summary>
   /// <remarks>Skips reparse points on folders</remarks>
   public class DirSearch {
      public static readonly string SearchMaskAllFilesAndFolders = "*.*";
      public static readonly string CurrentFolderInternalName = ".";

      // Common attribute combinations
      public static readonly FileAttributes AllAttributes = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System |
                                                            FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Normal |
                                                            FileAttributes.SparseFile | FileAttributes.Compressed | FileAttributes.NotContentIndexed |
                                                            FileAttributes.Encrypted | FileAttributes.IntegrityStream | FileAttributes.NoScrubData;
      public static readonly FileAttributes SystemHidden = FileAttributes.Hidden | FileAttributes.System;
      public static readonly FileAttributes AllAttributesMinusSysAndHidden = AllAttributes & ~SystemHidden;
      public static readonly FileAttributes AllAttributesMinusSys = AllAttributes & ~FileAttributes.System;
      public static readonly FileAttributes AllAttributesMinusHidden = AllAttributes & ~FileAttributes.Hidden;

      public static readonly FileAttributes AttributesExcluded = FileAttributes.Device | FileAttributes.Offline | FileAttributes.ReparsePoint;

      public SearchDef SearchDefinition { get; set; }

      public delegate void FileMatch(FileInfo oneFile, ref bool cancelFlag);
      public event FileMatch OnFileMatch;

      public delegate void FolderMatch(DirectoryInfo oneFolder, ref bool cancelFlag);
      public event FolderMatch OnFolderMatch;

      public delegate void FileExcept(string ErrorMsg);
      public event FileExcept OnFileExcept;

      public delegate void FolderExcept(string ErrorMsg);
      public event FolderExcept OnFolderExcept;

      public delegate void CustomFolderFilter(DirectoryInfo oneFolder, ref bool skip, ref bool skipChildFolders);
      public event CustomFolderFilter OnFolderFilter;

      public delegate void CustomFileFilter(FileInfo oneFile, ref bool skip);
      public event CustomFileFilter OnFileFilter;

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
         SearchDefinition.AlwaysExcluded |= AttributesExcluded;
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
         // default to no skip so if no Event, if doesn't skip file(s) and folder(s)
         skipFolderFlag = false;
         skipChildFolders = false;
         OnFolderFilter?.Invoke(dirInfo, ref skipFolderFlag, ref skipChildFolders);
         if (skipFolderFlag == false) {
            OnFolderMatch?.Invoke(dirInfo, ref cancelFlag);
            if (cancelFlag)
               return;
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
                     OnFileFilter?.Invoke(oneFile, ref skipFileFlag);
                     if (skipFileFlag == false) {
                        OnFileMatch?.Invoke(oneFile, ref cancelFlag);
                        if (cancelFlag)
                           return;
                     }
                  }
               }
            }
            catch (UnauthorizedAccessException e) {
               OnFileExcept?.Invoke(e.Message);
            }
            catch (PathTooLongException e) {
               OnFileExcept?.Invoke(e.Message);
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
               OnFolderExcept?.Invoke(e.Message);
            }
            catch (PathTooLongException e) {
               OnFolderExcept?.Invoke(e.Message);
            }
         }
      }
   }

}
