using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCSS.Util.DirSearch;
using System.Diagnostics;
using System.IO;

namespace MHDirSearch.Tests {

   [TestClass]
   public class FixedTreeTests {

      // todo: extract root.7z to a test location for testing this and update this variable to point to that location before running this integration test suite
      private static readonly string RootLoc = @"E:\Data\DL\Root";
      private static readonly string AllFiles = "*.*";
      private static readonly string NoMatchFilePatttern = "*.xyz";
      // increment when adding a folder in testing archive
      private static readonly int MinFolderTotal = 5;

      [TestMethod]
      public void AllFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 13, "Number of files not >= 13");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void TopLevelReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, false);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 7, "Number of files not >= 7");
         Assert.IsTrue(numFolders >= 1, "Number of folders not >= 1");
      }

      [TestMethod]
      public void TopLevelWithNoHitsReturnsNoFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(NoMatchFilePatttern, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, false);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.AreEqual(0, numFiles, "Number of files not 0");
      }

      [TestMethod]
      public void TopLevelAndSubsWithNoHitsReturnsNoFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(NoMatchFilePatttern, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.AreEqual(0, numFiles, "Number of files not 0");
      }

      [TestMethod]
      public void TxtFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch("*.txt", RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 10, "Number of files not >= 10");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void DatFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch("*.dat", RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 2, "Number of files not >= 2");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void FilesStartingWithRootReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch("Root*.*", RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 3, "Number of files not >= 3");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void ROFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.AnyMatch, FileAttributes.ReadOnly, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 2, "Number of RO files not >= 2");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void SysFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.AnyMatch, FileAttributes.System, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 2, "Number of Sys files not >= 2");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void SysAndHiddenFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.AllMatchPlusAnyOthers, FileAttributes.Hidden | FileAttributes.System, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 1, "Number of Sys+Hidden files not >= 1");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void SysOrHiddenFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.AnyMatch, FileAttributes.Hidden | FileAttributes.System, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 4, "Number of Sys|Hidden files not >= 4");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void HiddenFilesAndSubsReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.AllMatchPlusAnyOthers, FileAttributes.Hidden, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 3, "Number of Hidden files not >= 3");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void EmptyPathUsesCurrentDir() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, string.Empty, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.AreEqual(Directory.GetCurrentDirectory(), searcher.BaseDir, "base dir != GetCurrentDirectory()");
      }

      [TestMethod]
      public void NullPathUsesCurrentDir() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, null, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.Execute();
         Assert.AreEqual(Directory.GetCurrentDirectory(), searcher.BaseDir, "base dir != GetCurrentDirectory()");
      }

      [TestMethod]
      public void CustomFileFilterOnAllFilesReturnsOneHit() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.OnFileFilter += (FileInfo oneFile, ref bool skip) => { skip = oneFile.Name != "Root2.dat";};
         searcher.Execute();
         Assert.AreEqual(1, numFiles, "Number of files not 1");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void CustomFileFilterOnAllFilesWithNoMatchesReturnsZero() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(NoMatchFilePatttern, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.OnFileFilter += (FileInfo oneFile, ref bool skip) => { skip = oneFile.Name != "Root2.dat";};
         searcher.Execute();
         Assert.AreEqual(0, numFiles, "Number of files not 0");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void CustomFileFilterOnSublevelReturnsOneHit() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.OnFileFilter += (FileInfo oneFile, ref bool skip) => { skip = oneFile.Name != "L2F1-1.txt";};
         searcher.Execute();
         Assert.AreEqual(1, numFiles, "Number of files not 1");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void CustomFileFilterZeroByteFileslReturnsFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.OnFileFilter += (FileInfo oneFile, ref bool skip) => { skip = oneFile.Length != 0; };
         searcher.Execute();
         Assert.IsTrue(numFiles >= 4, "Number of files not >= 4");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

      [TestMethod]
      public void CustomFolderFilterOnHiddenFolderReturnsOneHit() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(AllFiles, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.OnFolderFilter += (DirectoryInfo folder, ref bool skip, ref bool skipChildren) => { skip = ((folder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden);};
         searcher.Execute();
         Assert.IsTrue(numFolders >= 1, "Number of folders not >= 1");
      }

      [TestMethod]
      public void CustomFolderFilterWithChildFoldersOnWorks() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch("L3-L2F1*.*", RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.OnFolderFilter += (DirectoryInfo folder, ref bool skip, ref bool skipChildren) => { skip = (folder.Name == "L2F1"); skipChildren = false; };
         searcher.Execute();
         Assert.IsTrue(numFolders >= MinFolderTotal - 1, $"Number of folders not >= {MinFolderTotal - 1}");
         Assert.IsTrue(numFiles >= 2, "Number of files not >= 1");
      }

      [TestMethod]
      public void CustomFileFilterOnSublevelWithNoMatchesReturnsZero() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch(NoMatchFilePatttern, RootLoc, AttrSearchType.IgnoreAttributeMatch, 0, true);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { numFiles++; };
         searcher.OnFileFilter += (FileInfo oneFile, ref bool skip) => { skip = oneFile.Name != "L2F1-1.txt";};
         searcher.Execute();
         Assert.AreEqual(0, numFiles, "Number of files not 0");
         Assert.IsTrue(numFolders >= MinFolderTotal, $"Number of folders not >= {MinFolderTotal}");
      }

   }

}
