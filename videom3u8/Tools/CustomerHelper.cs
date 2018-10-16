using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace videom3u8.Tools
{
    public static class CustomerHelper
    {
        

        public static List<string> fun1()
        {
            string line = string.Empty;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(@"Logs_2018-08-27.log"))
            {
                line = reader.ReadLine();
                
                while (line != null)
                {
                    if (line.Contains("doreplace"))
                    {
                        var temp=line.Replace("doreplace", "").Replace(".m3u8", ".mp4");
                        lines.Add(temp);
                    }
                    line = reader.ReadLine();
                }
            }
            return lines;
        }
    }
}
