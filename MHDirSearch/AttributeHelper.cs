using System.IO;

namespace OCSS.Util {

   public static class AttributeHelper {
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

   }

}
