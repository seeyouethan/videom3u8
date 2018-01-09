using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace videom3u8.Entity
{
    public class VideoMp4
    {
        public string OldfileName { get; set; }
        public string OldFilePath { get; set; }
        public string NewFilePath { get; set; }
        public string NewFileName { get; set; }

        public string CmdString { get; set; }

    }
}
