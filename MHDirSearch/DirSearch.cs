using System;
using System.IO;

namespace OCSS.Util.DirSearch {

   /// <summary>Allows searching files and folders as a wrapper around FileInfo, DirectoryInfo, and EnumerateFiles</summary>
   /// <remarks>
   // File and Directory search
   // Translated from my original Delphi version.
   /// Added functionality to filter on specific file attributes and add delegates for file-matching and folder-matching.
   ///
   /// Notes:
   /// ******************************
   /// As of 2010:
   /// Exceptions will cause the current file and/or subdirectories to be skipped.
   /// This is not preventable according to MS unless you use Win32 or other methods.
   /// People were hoping this would be an option in .NET 4, but MS did not address this limitation.
   /// MS stated that it may be possible in 5+.
   /// </remarks>
   ///
   public enum AttrSearchType { stExact, stAll, stAny };

   public class DirSearch {
      public static readonly string MASK_ALL_FILES_AND_FOLDERS = "*.*";
      public static readonly string CURRENT_FOLDER = ".";

      // Common attribute combinations
      public static readonly FileAttributes ALLFILEATTRIB = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Normal;
      public static readonly FileAttributes ALLFILEATTRIB_MINUS_SYS_AND_HIDDEN = FileAttributes.ReadOnly | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Normal;
      public static readonly FileAttributes ALLFILEATTRIB_MINUS_SYS = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Normal;
      public static readonly FileAttributes ALLFILEATTRIB_MINUS_HIDDEN = FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Normal;

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

      public DirSearch(): this(MASK_ALL_FILES_AND_FOLDERS, string.Empty, AttrSearchType.stAny, ALLFILEATTRIB, false) { }

      public DirSearch(string searchMask, string baseDir): this(searchMask, baseDir, AttrSearchType.stAny, ALLFILEATTRIB, false) { }

      public DirSearch(string searchMask, string baseDir, AttrSearchType searchType, FileAttributes fileAttrBits, bool processSubs) {
         this.SearchMask = searchMask;
         this.SearchType = searchType;
         FileAttribs = fileAttrBits;
         this.ProcessSubs = processSubs;
         this.BaseDir = baseDir;
         pCancelFlag = false;
      }

      public void Execute() {
         if ((BaseDir == string.Empty) || (BaseDir == CURRENT_FOLDER)) {
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
         if (!startDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            startDir = startDir + Path.DirectorySeparatorChar.ToString();

         DirectoryInfo pDir = new DirectoryInfo(startDir);

         try {
            foreach (var OneFile in pDir.EnumerateFiles(SearchMask)) {
               if ((((OneFile.Attributes & FileAttribs) != 0) && (SearchType == AttrSearchType.stAny)) ||
                  (((OneFile.Attributes & FileAttribs) == FileAttribs) && (SearchType == AttrSearchType.stAll)) ||
                  ((OneFile.Attributes == FileAttribs) && (SearchType == AttrSearchType.stExact))) {
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
            foreach (var oneFolder in pDir.EnumerateDirectories(MASK_ALL_FILES_AND_FOLDERS, SearchOption.TopDirectoryOnly)) {
               // skip temp and reparse points
               if (((oneFolder.Attributes & FileAttributes.ReparsePoint) == 0) && ((oneFolder.Attributes & FileAttributes.Temporary) == 0)) {
                  if (!oneFolder.Name.StartsWith(".")) {
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
