using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace videom3u8.Tools
{
    public static class CpuHelper
    {

        //引入32库，kernel32.dll内封装了WIN32 API          
        [DllImport("kernel32")]
        public static extern void GetSystemInfo(ref CPU_INFO cpuinfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct CPU_INFO
        {
            public uint dwOemId;
            public uint dwPageSize;
            public uint lpMinimumApplicationAddress;
            public uint lpMaximumApplicationAddress;
            public uint dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public uint dwProcessorLevel;
            public uint dwProcessorRevision;
        }

        public static int GteCpuCount()
        {
            CPU_INFO CpuInfo;
            CpuInfo = new CPU_INFO();
            //设置为引用类型，可以让CpuInfo的值可以被修改  
            GetSystemInfo(ref CpuInfo);
            try
            {
                return Convert.ToInt32(CpuInfo.dwNumberOfProcessors);
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
    }
}
