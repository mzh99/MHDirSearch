using System.IO;

namespace OCSS.Util.DirSearch {
   
   public class SearchDef {
      public string StartFolder { get; set; }
      public string SearchMask { get; set; }
      public bool ProcessSubdirs { get; set; }
      public AttrSearchType AttributeSearchType { get; set; }
      public FileAttributes Attributes { get; set; }
      public FileAttributes AlwaysExcluded { get; set; }

      public SearchDef() : this(string.Empty, DirSearch.SearchMaskAllFilesAndFolders, true, AttrSearchType.IgnoreAttributeMatch, FileAttributes.Normal) { }

      public SearchDef(string startFolder, string searchMask, bool processSubs, AttrSearchType searchType, FileAttributes attributes) {
         StartFolder = startFolder;
         SearchMask = searchMask;
         ProcessSubdirs = processSubs;
         AttributeSearchType = searchType;
         Attributes = attributes;
         AlwaysExcluded = DirSearch.AttributesExcluded;
      }

   }

}
