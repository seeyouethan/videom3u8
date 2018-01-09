using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace videom3u8.Tools
{
    public class FileHelper
    {
        public void DeepFileDir(DirectoryInfo theDir)//递归目录 文件 
        {
            DirectoryInfo[] subDirectories = theDir.GetDirectories();//获得目录 
            foreach (DirectoryInfo dirinfo in subDirectories)
            {
                FileInfo[] fileInfo = dirinfo.GetFiles(); //目录下的文件 
                foreach (FileInfo fInfo in fileInfo)
                {
                    //添加该目录下的文件

                }
                DeepFileDir(dirinfo);
            }
        }
    }
}
