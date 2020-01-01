using System;
using System.Diagnostics;
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

      public delegate void FileMatch(FileInfo OneFile, ref bool CancelFlag);
      public event FileMatch OnFileMatch;

      public delegate void FolderMatch(DirectoryInfo OneFolder, ref bool CancelFlag);
      public event FolderMatch OnFolderMatch;

      public delegate void FileExcept(string ErrorMsg);
      public event FileExcept OnFileExcept;

      public delegate void FolderExcept(string ErrorMsg);
      public event FolderExcept OnFolderExcept;

      private bool pCancelFlag;

      public DirSearch(): this(SearchMaskAllFilesAndFolders, string.Empty, AttrSearchType.AnyMatch, AllAttributes, false) { }

      public DirSearch(string searchMask, string baseDir): this(searchMask, baseDir, AttrSearchType.AnyMatch, AllAttributes, false) { }

      public DirSearch(string searchMask, string baseDir, AttrSearchType searchType, FileAttributes fileAttrBits, bool processSubs) {
         this.SearchMask = searchMask;
         this.SearchType = searchType;
         FileAttribs = fileAttrBits;
         this.ProcessSubs = processSubs;
         this.BaseDir = baseDir;
         pCancelFlag = false;
      }

      public void Execute() {
         if ((BaseDir == string.Empty) || (BaseDir == CurrentFolderInternalName)) {
            // if base directory is empty, use the current directory
            BaseDir = Directory.GetCurrentDirectory();
         }
         pCancelFlag = false;
         FindFiles(BaseDir);
         // recommended to remove open handles on enumerated directories or files (MSDN)
         GC.Collect();
         GC.WaitForPendingFinalizers();
      }

      /// <summary>internal function to find files and folders</summary>
      /// <param name="startDir"></param>
      /// <remarks>function is recursive if pProcessSubs flag is true</remarks>
      private void FindFiles(string startDir) {
         if (pCancelFlag)
            return;
         if (startDir.EndsWith(Path.DirectorySeparatorChar.ToString()) == false)
            startDir += Path.DirectorySeparatorChar.ToString();

         DirectoryInfo pDir = new DirectoryInfo(startDir);

         try {
            foreach (var OneFile in pDir.EnumerateFiles(SearchMask)) {
               // Debug.WriteLine($"OneFile Attribs: {OneFile.Attributes}");
               if ((SearchType == AttrSearchType.IgnoreAttributeMatch) ||
                  (((OneFile.Attributes & FileAttribs) != 0) && (SearchType == AttrSearchType.AnyMatch)) ||
                  (((OneFile.Attributes & FileAttribs) == FileAttribs) && (SearchType == AttrSearchType.AllMatchPlusAnyOthers)) ||
                  ((OneFile.Attributes == FileAttribs) && (SearchType == AttrSearchType.ExactMatch))) {
                  if (OnFileMatch != null) {
                     OnFileMatch(OneFile, ref pCancelFlag);
                     if (pCancelFlag)
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
         // Get all of the SubDirs
         try {
            foreach (var oneFolder in pDir.EnumerateDirectories(SearchMaskAllFilesAndFolders, SearchOption.TopDirectoryOnly)) {
               // skip temp and reparse points
               if ((oneFolder.Attributes & FileAttributes.ReparsePoint) == 0) {
                  if (oneFolder.Name != CurrentFolderInternalName) {
                     if (OnFolderMatch != null) {
                        OnFolderMatch(oneFolder, ref pCancelFlag);
                        if (pCancelFlag)
                           return;
                        if (ProcessSubs)
                           FindFiles(oneFolder.FullName);      // recursive call
                     }
                  }
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
