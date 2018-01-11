using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace videom3u8.Tools
{
    public static class LogHelper
    {
        private static ILog logWriter = LogManager.GetLogger("LogWriter");


        public static void AddErrorLog(string str)
        {
            logWriter.Error(str);
        }

        public static void AddEventLog(string str)
        {
            logWriter.Info(str);
        }


        public static void AddFFmpegLog(string str)
        {
            try
            {
                FileStream fs = new FileStream(Environment.CurrentDirectory + "\\Log\\FFmpegLog.log", FileMode.OpenOrCreate);
                byte[] data = System.Text.Encoding.Default.GetBytes(DateTime.Now + "-----------\r\n" + str + "\r\n");

                fs.Position = fs.Length;
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("非常抱歉，FFmpeg日志打开失败，请联系开发人员。" + ex.ToString(), "系统提示");
            }
            finally
            {
            }
        }
    }
}
