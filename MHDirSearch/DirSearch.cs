using System;
using System.IO;

namespace OCSS.Util.DirSearch {

   public enum AttrSearchType { ExactMatch, AllMatchPlusAnyOthers, AnyMatch, IgnoreAttributeMatch };

   /// <summary>Search files and folders using a wrapper around FileInfo, DirectoryInfo, and EnumerateFiles</summary>
   public class DirSearch {
      public static readonly string SearchMaskAllFilesAndFolders = "*.*";
      public static readonly string CurrentFolderInternalName = ".";

      // Common attribute combinations
      public static readonly FileAttributes AllAttributes = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory |
                                                            FileAttributes.Archive | FileAttributes.Normal | FileAttributes.SparseFile | FileAttributes.ReparsePoint |
                                                            FileAttributes.Compressed | FileAttributes.NotContentIndexed | FileAttributes.Encrypted |
                                                            FileAttributes.IntegrityStream | FileAttributes.NoScrubData;
      public static readonly FileAttributes AllAttributesMinusSysAndHidden = AllAttributes & ~(FileAttributes.Hidden | FileAttributes.System);
      public static readonly FileAttributes AllAttributesMinusSys = AllAttributes & ~FileAttributes.System;
      public static readonly FileAttributes AllAttributesMinusHidden = AllAttributes & ~FileAttributes.Hidden;

      public string SearchMask { get; set; }
      public string BaseDir { get; set; }
      public bool ProcessSubs { get; set; }
      public AttrSearchType SearchType { get; set; }
      public FileAttributes FileAttribs { get; set; }

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

      public DirSearch(): this(SearchMaskAllFilesAndFolders, string.Empty, AttrSearchType.AnyMatch, AllAttributes, false) { }

      public DirSearch(string searchMask, string baseDir): this(searchMask, baseDir, AttrSearchType.AnyMatch, AllAttributes, false) { }

      public DirSearch(string searchMask, string baseDir, AttrSearchType searchType, FileAttributes fileAttrBits, bool processSubs) {
         this.SearchMask = searchMask;
         this.SearchType = searchType;
         this.FileAttribs = fileAttrBits;
         this.ProcessSubs = processSubs;
         this.BaseDir = string.IsNullOrEmpty(baseDir) ? Directory.GetCurrentDirectory() : baseDir;
         cancelFlag = false;
      }

      public void Execute() {
         if (string.IsNullOrEmpty(BaseDir))
            BaseDir = Directory.GetCurrentDirectory();
         cancelFlag = false;
         FindFiles(BaseDir);
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
         if (startDir.EndsWith(Path.DirectorySeparatorChar.ToString()) == false)
            startDir += Path.DirectorySeparatorChar.ToString();
         DirectoryInfo dirInfo = new DirectoryInfo(startDir);
         // default to no skip so if no Event, if doesn't skip file(s) and folder(s)
         skipFolderFlag = false;
         skipChildFolders = false;
         OnFolderFilter?.Invoke(dirInfo, ref skipFolderFlag, ref skipChildFolders);
         if (skipFolderFlag == false) {
            // send message to subscribers for folder processing (excluding reparse points)
            if ((dirInfo.Attributes & FileAttributes.ReparsePoint) == 0) {
               if (OnFolderMatch != null) {
                  OnFolderMatch(dirInfo, ref cancelFlag);
                  if (cancelFlag)
                     return;
               }
            }
            try {
               // Process all file entries
               foreach (var oneFile in dirInfo.EnumerateFiles(SearchMask)) {
                  // Debug.WriteLine($"OneFile Attribs: {OneFile.Attributes}");
                  if ((SearchType == AttrSearchType.IgnoreAttributeMatch) ||
                     (((oneFile.Attributes & FileAttribs) != 0) && (SearchType == AttrSearchType.AnyMatch)) ||
                     (((oneFile.Attributes & FileAttribs) == FileAttribs) && (SearchType == AttrSearchType.AllMatchPlusAnyOthers)) ||
                     ((oneFile.Attributes == FileAttribs) && (SearchType == AttrSearchType.ExactMatch))) {
                     // default to no skip so if no Event, it doesn't skip file
                     skipFileFlag = false;
                     OnFileFilter?.Invoke(oneFile, ref skipFileFlag);
                     if (skipFileFlag == false && OnFileMatch != null) {
                        OnFileMatch(oneFile, ref cancelFlag);
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
         if (skipChildFolders == false && ProcessSubs) {
            try {
               foreach (var oneFolder in dirInfo.EnumerateDirectories(SearchMaskAllFilesAndFolders, SearchOption.TopDirectoryOnly)) {
                  // skip reparse points and current directory
                  if (((oneFolder.Attributes & FileAttributes.ReparsePoint) == 0) && (oneFolder.Name != CurrentFolderInternalName)) {
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
