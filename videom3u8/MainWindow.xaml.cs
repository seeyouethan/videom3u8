using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;
using System.Xml;
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
            FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "请选择要源视频的目录" };
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
                else if (TabControl0.SelectedIndex == 2)
                {
                    FileDir2.Text = foldPath;
                }
            }
        }

        private string cmdPath = CommonStatic.FFmpegPath;

        private int cpuCount = 1;
        private int threadCount = 1;
        private List<string> list0 = new List<string>();
        private int jumpCunt = 0;
        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            list0 = CustomerHelper.fun1();

            //获取设置
            cpuCount = AppConfigHelper.GetAppConfigInt("CpuCount");
            threadCount = AppConfigHelper.GetAppConfigInt("ThreadCount");
            //清空当前列表
            ListQueue.Clear();
            //清空进度条
            this.pb.Maximum = 0;
            this.tb.Text = "0%";
            this.pb.Value = 0;

            var foldPath = "";
            var newfoldPath = "";
            var waterMarkImg = "";
            if (TabControl0.SelectedIndex == 0 || TabControl0.SelectedIndex == 2)
            {
                foldPath = FileDir.Text;
                newfoldPath = NewFileDir.Text;
                cmdPath = CommonStatic.FFmpegPath;
                if (TabControl0.SelectedIndex == 2)
                {
                    foldPath = FileDir2.Text;
                    newfoldPath = NewFileDir2.Text;
                    waterMarkImg = WaterMarkTextBox.Text.Trim();
                }
            }
            else if (TabControl0.SelectedIndex == 1)
            {
                foldPath = FileDir1.Text;
                newfoldPath = NewFileDir1.Text;
                cmdPath = CommonStatic.Qtpath;
            }

            if (TabControl0.SelectedIndex == 2 && string.IsNullOrEmpty(waterMarkImg))
            {
                System.Windows.Forms.MessageBox.Show("请选择要添加水印的图片", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
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
            if (TabControl0.SelectedIndex == 1 || TabControl0.SelectedIndex == 2)
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
            LogTextBox.Text = "该目录下共找到 " + ListQueue.Count + " 个视频，开始进行转换......\n";
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


        private void StartOnXml_OnClick(object sender, RoutedEventArgs e)
        {
            if (TabControl0.SelectedIndex == 2 && string.IsNullOrEmpty(waterMarkImg))
            {
                System.Windows.Forms.MessageBox.Show("请选择要添加水印的图片", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //获取设置
            cpuCount = AppConfigHelper.GetAppConfigInt("CpuCount");
            threadCount = AppConfigHelper.GetAppConfigInt("ThreadCount");
            //清空当前列表
            ListQueue.Clear();
            //清空进度条
            this.pb.Maximum = 0;
            this.tb.Text = "0%";
            this.pb.Value = 0;

            //先从根目录下查找xml
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                string xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "watermark.xml";
                xmldoc.Load(xmlPath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("xml文件加载出错", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LogHelper.AddErrorLog(ex.ToString());
                return;
            }
            //获取节点列表 
            XmlNodeList topNode = xmldoc.DocumentElement.ChildNodes;
            foreach (XmlElement element in topNode)
            {
                var str = element.InnerText;
                var item=new VideoMp4();
                var oldFilePath = str; // newFilePath.Substring(0,newFilePath.LastIndexOf("."))
                var filename = oldFilePath.Substring(oldFilePath.LastIndexOf("\\")).Replace("\\", "");
                var fileformat = filename.Substring(filename.LastIndexOf("."));
                var newfilename = filename.Substring(0, filename.LastIndexOf(".")) + "_znykt_cnki" + fileformat;
                var newFilePath = oldFilePath.Substring(0, oldFilePath.LastIndexOf("\\")) +"\\"+ newfilename;
                item.CmdString = string.Format(" -y -i \"{0}\" -vf \"movie={1},scale=170:60[watermark];[in][watermark]overlay=main_w-overlay_w-10:10[out]\" \"{2}\"", oldFilePath, waterMarkImg, newFilePath);
                item.NewFileName = newfilename;
                item.NewFilePath = newFilePath;
                item.OldFilePath = oldFilePath;
                item.OldfileName = filename;
                ListQueue.Enqueue(item);
            }

            if (ListQueue.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("该路径下未找到MP4格式的视频", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            LogTextBox.Clear();
            LogTextBox.Text = "该目录下共找到 " + ListQueue.Count + " 个视频，开始进行转换......\n";
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
                if (!(list0.Contains(fInfo.Name)) && fInfo.Extension == ".mp4")
                {
                    var oldFilePath = fInfo.FullName;
                    var newdir = "";

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
                    else if (TabControl0.SelectedIndex == 2)
                    {
                        //添加水印后仍然为mp4视频
                        newdir = fInfo.DirectoryName.Replace(FileDir2.Text, NewFileDir2.Text);
                        newPath = newdir + "\\";
                        newFilePath = newPath + fInfo.Name.Replace(fInfo.Extension, "") + ".mp4";
                    }

                    var item = new VideoMp4();
                    if (TabControl0.SelectedIndex == 0)
                    {
                        //转换为m3u8
                        item.CmdString = string.Format("-i \"{0}\" -hls_time 5 -hls_list_size 0 -c:v libx264 -c:a aac -strict -2 -threads {1} -f hls \"{2}\"", oldFilePath, cpuCount, newFilePath);
                    }
                    else if (TabControl0.SelectedIndex == 1)
                    {
                        //添加元信息为第一帧
                        item.CmdString = string.Format("\"{0}\" \"{1}\"", oldFilePath, newFilePath);
                    }
                    else if (TabControl0.SelectedIndex == 2)
                    {
                        //添加水印  这个图片写成绝对路径 不会被识别，只能放到和exe执行文件同一目录下
                        //示例：ffmpeg -y –i "C:\Users\Administrator\Desktop\watermark\am.mp4" -vf "movie=cnkiwatermark.png,scale=135:50[watermark];[in][watermark]overlay=main_w-overlay_w-10:10[out]" C:\Users\Administrator\Desktop\watermark2\am.mp4
                        item.CmdString = string.Format(" -y -i \"{0}\" -vf \"movie={1},scale=170:60[watermark];[in][watermark]overlay=main_w-overlay_w-10:10[out]\" \"{2}\"", oldFilePath, waterMarkImg, newFilePath);
                    }

                    item.OldfileName = fInfo.Name;
                    item.OldFilePath = fInfo.FullName;
                    if (TabControl0.SelectedIndex == 0)
                    {
                        item.NewFileName = fInfo.Name.Replace(fInfo.Extension, "") + ".m3u8";
                    }
                    else
                    {
                        item.NewFileName = fInfo.Name.Replace(fInfo.Extension, "") + ".mp4";
                    }
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
                    if (iCount < threadCount)
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


        public int iCount = 0;
        public int iAmount = 0;
        public int iNow = 0;
        public int tempNow = 1;
        private void ConvertProgress(object parameters)
        {

            TextLog("--------------------------------------");
            TextLog("正在转换第【 " + (tempNow++) + " 】个视频......");
            var item = (VideoMp4)parameters;
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
            TextLog("第【 " + iNow + " 】个视频转换完成。");
            LogHelper.AddEventLog("第【 " + iNow + "】个视频转换完成。");
            TextLog(item);
            if (iNow == iAmount)
            {
                TextLog("视频转换完成，共完成 " + iAmount + " 个视频的转换。");
                LogHelper.AddEventLog("视频转换完成，共完成 " + iAmount + " 个视频的转换。");
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
                    LogTextBox.Text += "转换完成，生成文件" + item.NewFileName + "\n";
                    LogTextBox.Text += "文件路径为" + item.NewFilePath + "\n";

                    LogHelper.AddEventLog("转换完成，生成文件" + item.NewFileName + "\n");
                    LogHelper.AddEventLog("文件路径为" + item.NewFilePath + "\n");
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
                    LogTextBox.Text += text + "\n";
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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            CpuCountLabel.Content = CpuHelper.GteCpuCount();
        }

        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
        Thread copyThread;
        private string waterMarkImg = "";
        /// <summary>
        /// 上传要添加水印的图片到执行exe的目录下（复制，若有同名的则直接覆盖）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadWaterMarkImgBtn_OnClick(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "*.png|*.png";

            //注意，此处一定要手动引入System.Window.Forms空间，否则你如果使用默认的DialogResult会发现没有OK属性
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WaterMarkTextBox.Text = openFileDialog.FileName;

                string fileName = openFileDialog.FileName;
                waterMarkImg= openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf("\\")).Replace("\\", "");
                string destPath = System.AppDomain.CurrentDomain.BaseDirectory + waterMarkImg;
                if (!File.Exists(fileName))
                {
                    MessageBox.Show("源文件不存在");
                    return;
                }

                //保存复制文件信息，以进行传递
                CopyFileInfo c = new CopyFileInfo() { SourcePath = fileName, DestPath = destPath };
                //线程异步调用复制文件
                copyThread = new Thread(new ParameterizedThreadStart(CopyFile));
                copyThread.Start(c);
            }
        }


        /// <summary>
        /// 复制文件的委托方法  参考 https://www.cnblogs.com/yangyancheng/archive/2011/04/03/2004875.html
        /// </summary>
        /// <param name="obj">复制文件的信息</param>
        private void CopyFile(object obj)
        {
            CopyFileInfo c = obj as CopyFileInfo;
            CopyFile(c.SourcePath, c.DestPath);
        }
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        private void CopyFile(string sourcePath, string destPath)
        {
            FileInfo f = new FileInfo(sourcePath);
            FileStream fsR = f.OpenRead();
            FileStream fsW = File.Create(destPath);
            long fileLength = f.Length;
            byte[] buffer = new byte[1024];
            int n = 0;

            while (true)
            {
                //this.displayCopyInfo.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle,
                //    new Action<long, long>(UpdateCopyProgress), fileLength, fsR.Position);

                //读写文件
                n = fsR.Read(buffer, 0, 1024);
                if (n == 0)
                {
                    break;
                }
                fsW.Write(buffer, 0, n);
                fsW.Flush();
                Thread.Sleep(1);
            }

            System.Windows.Forms.MessageBox.Show("水印图片上传成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            fsR.Close();
            fsW.Close();
        }

        public class CopyFileInfo
        {
            public string SourcePath { get; set; }
            public string DestPath { get; set; }
        }
    }
}
