using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace videom3u8.Tools
{
    public static class CommonStatic
    {
        public static readonly string FFmpegPath = Environment.Is64BitOperatingSystem ?
            Environment.CurrentDirectory + "\\resource\\ffmpeg\\ffmpeg64\\ffmpeg.exe"
            : Environment.CurrentDirectory + "\\resource\\ffmpeg\\\\ffmpeg32\\ffmpeg.exe";
    }
}
