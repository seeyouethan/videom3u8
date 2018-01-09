using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace videom3u8.Tools
{
    public class CmdHelper
    {
        public static StringBuilder Msg = new StringBuilder();
        public static string ExecutCmd(string cmd, string args)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = cmd;
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;

                p.EnableRaisingEvents = true;
                p.Start();
                p.PriorityClass = ProcessPriorityClass.Normal;
                //result.Append(p.StandardError.ReadToEnd());
                //result.Append(p.StandardOutput.ReadToEnd());

                p.OutputDataReceived += p_OutputDataReceived;
                p.ErrorDataReceived += p_ErrorDataReceived;

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();
                //Msg.Clear();
                //Msg = "";
            }

            return Msg.ToString();
        }

        static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Msg.AppendLine(e.Data);
            //Msg = e.Data;
        }

        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Msg.AppendLine(e.Data);
            //Msg = e.Data;
        }
    }
}
