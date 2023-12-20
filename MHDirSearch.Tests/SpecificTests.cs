using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCSS.Util.DirSearch;
using System.Diagnostics;
using System.IO;

namespace MHDirSearch.Tests {

   [TestClass]
   public class SpecificTests {

      /// <summary>This test is only valid when thumbs truly exists in folder</summary>
      [TestMethod]
      public void NoHiddenOrSystemReturnsZeroFiles() {
         int numFolders = 0;
         int numFiles = 0;
         DirSearch searcher = new DirSearch("Thumbs.*", @"E:\Data\Graphics\", AttrSearchType.AnyMatch, DirSearch.AllAttributes, DirSearch.SystemHidden, false);
         searcher.OnFolderMatch += (DirectoryInfo info, ref bool flag) => { numFolders++; };
         searcher.OnFileMatch += (FileInfo info, ref bool flag) => { 
            numFiles++; 
         };
         searcher.Execute();
         Assert.IsTrue(numFiles == 0, "Number of files not zero");
      }
   }

}
