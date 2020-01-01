using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCSS.Util.DirSearch;

namespace MHDirSearch.Tests {

   [TestClass]
   public class UnitTest1 {
      [TestMethod]
      public void Test1() {
         //"E:\Data\Source\CS\Downloads\IoTSamples\.gitattributes"
         var ds = new DirSearch(".gitattributes", @"E:\Data\Source\CS\Downloads\IoTSamples\", AttrSearchType.AnyMatch, DirSearch.AllAttributes,  false);
         ds.Execute();
      }
   }

}
