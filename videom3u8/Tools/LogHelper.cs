using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace videom3u8.Tools
{
    public static class LogHelper
    {
        public static string logPath = Environment.CurrentDirectory + "\\log.log";
        public static void AddLog(string str)
        {
            FileStream fs = new FileStream(logPath, FileMode.OpenOrCreate);
            //byte[] data = System.Text.Encoding.Default.GetBytes(DateTime.Now+"----------\r"+str+"\r");
            byte[] data = System.Text.Encoding.Default.GetBytes(str + "\r");
            try
            {
                //设定书写的開始位置为文件的末尾  
                fs.Position = fs.Length;
                //将待写入内容追加到文件末尾  
                fs.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("文件打开失败" + ex.ToString(), "系统提示");
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
