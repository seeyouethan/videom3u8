using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            FolderBrowserDialog dialog = new FolderBrowserDialog {Description = "请选择要源视频的目录"};
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                if (TabControl0.SelectedIndex == 0)
                {
                    FileDir.Text = foldPath;
                }
                else if (TabControl0.SelectedIndex == 1)
                {
                    FileDir1.Text = foldPath;
                }
            }
        }

        public string cmdPath = CommonStatic.FFmpegPath;

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            //清空当前列表
            ListQueue.Clear();
            //清空进度条
            this.pb.Maximum = 0;
            this.tb.Text = "0%";
            this.pb.Value = 0;

            var foldPath = "";
            var newfoldPath = "";
            if (TabControl0.SelectedIndex == 0)
            {
                foldPath = FileDir.Text;
                newfoldPath = NewFileDir.Text;
                cmdPath = CommonStatic.FFmpegPath;

            }
            else if (TabControl0.SelectedIndex == 1)
            {
                foldPath = FileDir1.Text;
                newfoldPath = NewFileDir1.Text;
                cmdPath = CommonStatic.Qtpath;
            }
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
            if (TabControl0.SelectedIndex == 1)
            {
                //在 【将元信息设置到第一帧】 模式下，生成文件的路径不能够和源文件路径一样，否则会出错
                if (foldPath == newfoldPath)
                {
                    System.Windows.Forms.MessageBox.Show("【生成文件路径】不能够和【源文件路径】一样。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            DirectoryInfo thisOne = new DirectoryInfo(foldPath);
            DeepFileDir(thisOne);

            if (ListQueue.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("该路径下未找到MP4格式的视频", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            LogTextBox.Clear();
            LogTextBox.Text="该目录下共找到 " + ListQueue.Count + " 个视频，开始进行转换......\n";
            iAmount = ListQueue.Count;
            iCount = 0;
            iNow = 0;
            Thread thread = new Thread(ThreadStart);
            thread.IsBackground = true;
            thread.Start();

            Select.IsEnabled = false;
            Start.IsEnabled = false;

            Select1.IsEnabled = false;
            Start1.IsEnabled = false;

            pb.Maximum = ListQueue.Count;
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
                    var newdir ="";

                    var newPath = "";
                    var newFilePath = "";

                    if (TabControl0.SelectedIndex == 0)
                    {
                        //转换为m3u8
                        newdir = fInfo.DirectoryName.Replace(FileDir.Text, NewFileDir.Text);
                        newPath = newdir + "\\" + fInfo.Name.Replace(fInfo.Extension, "") + "\\";
                        newFilePath = newPath + fInfo.Name.Replace(fInfo.Extension, "") + ".m3u8";
                    }
                    else if (TabControl0.SelectedIndex == 1)
                    {
                        newdir = fInfo.DirectoryName.Replace(FileDir1.Text, NewFileDir1.Text);
                        newPath = newdir + "\\";
                        newFilePath = newPath + fInfo.Name.Replace(fInfo.Extension, "") + ".mp4";
                    }

                    var item=new VideoMp4();
                    if (TabControl0.SelectedIndex == 0)
                    {
                        //转换为m3u8
                        item.CmdString = string.Format("-i \"{0}\" -hls_time 20 -hls_list_size 0 -c:v libx264 -c:a aac -strict -2 -f hls \"{1}\"", oldFilePath, newFilePath);
                    }
                    else if (TabControl0.SelectedIndex == 1)
                    {
                        //添加元信息为第一帧
                        item.CmdString = string.Format("\"{0}\"  \"{1}\"", oldFilePath, newFilePath);
                    }
                    
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
                        LogHelper.AddErrorLog(ex.ToString());
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
                    Thread.Sleep(100);

                    var threadCount = AppConfigHelper.GetAppConfig("ThreadCount");
                    if (string.IsNullOrEmpty(threadCount))
                    {
                        threadCount = "2";
                    }
                    var iThreadCount = 2;
                    try
                    {
                        iThreadCount = Convert.ToInt32(threadCount);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.AddErrorLog(ex.ToString());
                        iThreadCount = 2;
                    }
                    if (iCount <= iThreadCount)
                    {
                        //从队列中取出  
                        var item = ListQueue.Dequeue();
                        //取出的item就可以用了，里面有你要的东西 
                        ParameterizedThreadStart threadStart = new ParameterizedThreadStart(ConvertProgress);
                        Thread thread = new Thread(threadStart);
                        iCount++;
                        thread.Start(item);
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
            p.StartInfo.FileName = cmdPath;

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

            //进度条
            this.Dispatcher.Invoke(new Action(delegate
            {
                var jindu = Math.Round((iNow / pb.Maximum * 1.0), 2) * 100;

                this.pb.Value = iNow;
                this.tb.Text = jindu + "%";
                Thread.Sleep(10);
            }));

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
                        LogHelper.AddErrorLog(output.Data);
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
                    Start1.IsEnabled = true;
                    Select1.IsEnabled = true;
                }));
            }
        }

        private void TabControl0_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (Process p in Process.GetProcessesByName("ffmpeg"))
            {
                if (!p.CloseMainWindow())
                {
                    p.Kill();
                }
            }
            Environment.Exit(0);
        }
    }
}
