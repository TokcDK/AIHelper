using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Install.Types.Files.Archive.Extractors
{
    class SevenZipExtractor : ArchiveExtractorBase
    {
        public override string Mask => "*.7z";
    }
}
