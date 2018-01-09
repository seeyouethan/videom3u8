using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using videom3u8.Entity;
using videom3u8.Tools;
using MessageBox = System.Windows.MessageBox;

namespace videom3u8
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Select_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请要转换的视频的路径";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                //System.Windows.Forms.MessageBox.Show("已选择文件夹:" + foldPath, "选择文件夹提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                FileDir.Text = foldPath;
            }
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            var foldPath = FileDir.Text;
            var newfoldPath = NewFileDir.Text;
            if (string.IsNullOrEmpty(foldPath))
            {
                System.Windows.Forms.MessageBox.Show("请选择要转换的视频的路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(newfoldPath))
            {
                System.Windows.Forms.MessageBox.Show("请输入要转换的视频的新路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DirectoryInfo thisOne = new DirectoryInfo(foldPath);
            DeepFileDir(thisOne);

            if (ListQueue.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("该路径下未找到MP4格式的视频", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            LogTextBox.Clear();
            LogTextBox.Text="该目录下共找到 " + ListQueue.Count + " 个视频，正在进行转换......\n";
            iAmount = ListQueue.Count;
            iCount = 0;
            iNow = 0;
            Thread thread = new Thread(ThreadStart);
            thread.IsBackground = true;
            thread.Start();
            Select.IsEnabled = false;
            Start.IsEnabled = false;
        }
        
        private Queue<VideoMp4> ListQueue = new Queue<VideoMp4>();


        private void DeepFileDir(DirectoryInfo theDir)//递归目录 文件 
        {
            FileInfo[] fileInfo = theDir.GetFiles(); //目录下的文件 
            foreach (FileInfo fInfo in fileInfo)
            {
                if (fInfo.Extension == ".mp4")
                {
                    var oldFilePath = fInfo.FullName;
                    var newdir = fInfo.DirectoryName.Replace(FileDir.Text, NewFileDir.Text);
                    //var newPath = fInfo.DirectoryName + "\\" + fInfo.Name.Replace(fInfo.Extension, "") + "\\";
                    var newPath = newdir + "\\" + fInfo.Name.Replace(fInfo.Extension, "") + "\\";
                    var newFilePath = newPath  + fInfo.Name.Replace(fInfo.Extension, "") + ".m3u8";

                    var item=new VideoMp4();
                    item.CmdString = string.Format("-i \"{0}\" -hls_time 20 -hls_list_size 0 -c:v libx264 -c:a aac -strict -2 -f hls \"{1}\"", oldFilePath, newFilePath);
                    item.OldfileName = fInfo.Name;
                    item.OldFilePath = fInfo.FullName;
                    item.NewFileName = fInfo.Name.Replace(fInfo.Extension, "") + ".m3u8";
                    item.NewFilePath = newFilePath;
                    ListQueue.Enqueue(item);
                    //创建目录
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(newPath)))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                }
            }
            DirectoryInfo[] subDirectories = theDir.GetDirectories();//子目录 
            foreach (DirectoryInfo dirinfo in subDirectories)
            {
                DeepFileDir(dirinfo);
            }
        }

        private void ThreadStart()
        {
            while (true)
            {
                if (ListQueue.Count > 0)
                {
                    try
                    {
                        ScanQueue();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.AddLog(ex.ToString());
                    }
                }
                else
                {
                    //没有任务，休息3秒钟  
                    //Thread.Sleep(3000);
                    System.Threading.Thread.CurrentThread.Abort();
                }
            }
        }


        //要执行的方法  
        private void ScanQueue()
        {
            while (ListQueue.Count > 0)
            {
                try
                {
                    if (iCount <= 9)
                    {
                        //从队列中取出  
                        var item = ListQueue.Dequeue();
                        //取出的item就可以用了，里面有你要的东西 
                        ParameterizedThreadStart threadStart = new ParameterizedThreadStart(ConvertProgress);
                        Thread thread = new Thread(threadStart);
                        thread.Start(item);
                        iCount++;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        public int iCount=0;
        public int iAmount = 0;
        public int iNow = 0;

        private void ConvertProgress(object parameters)
        {

            var item = (VideoMp4) parameters;
            Process p = new Process();
            p.StartInfo.FileName = CommonStatic.FFmpegPath;
            p.StartInfo.Arguments = item.CmdString;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            //p.ErrorDataReceived += new DataReceivedEventHandler(Log);
            p.Start();
            p.BeginErrorReadLine();
            p.WaitForExit();
            p.Close();
            iCount--;
            if (iCount < 0) iCount = 0;
            iNow++;
            TextLog("正在转换第 " + iNow + " 个视频......");
            TextLog(item);
            if (iNow == iAmount)
            {
                TextLog("视频转换完成，共完成 " + iAmount + " 个视频的转换。");
                EnableButton();
            }
            p.Dispose();

        }

        private void Log(object sendProcess, DataReceivedEventArgs output)
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new ESBasic.CbGeneric<object, DataReceivedEventArgs>(Log), sendProcess, output);
            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    if (!String.IsNullOrEmpty(output.Data))
                    {
                        //Log
                        LogHelper.AddLog(output.Data);
                    }
                }));
            }
        }

        private void TextLog(VideoMp4 item)
        {
            if (this.Dispatcher.CheckAccess())
            {
                //ignore
            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    LogTextBox.Text += "转换完成，生成文件" + item.NewFileName+"\n";
                    LogTextBox.Text += "文件路径为" + item.NewFilePath + "\n";
                }));
            }
        }

        private void TextLog(string text)
        {
            if (this.Dispatcher.CheckAccess())
            {
                //ignore
            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    LogTextBox.Text += text+ "\n";
                }));
            }
        }

        private void EnableButton()
        {
            if (this.Dispatcher.CheckAccess())
            {
                //ignore
            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    Start.IsEnabled = true;
                    Select.IsEnabled = true;
                }));
            }
        }

    }
}
