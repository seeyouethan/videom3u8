using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace videom3u8
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "ElectronicNeedleTherapySystem", out ret);

            if (!ret)
            {
                System.Windows.Forms.MessageBox.Show("当前已有一个程序实例运行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }

        }

        protected override void OnExit(ExitEventArgs e)　　　　　//该重写函数实现在程序退出时关闭某个进程
        {
            Process[] myProgress;
            myProgress = Process.GetProcesses();　　　　　　　　　　//获取当前启动的所有进程
            foreach (Process p in myProgress)　　　　　　　　　　　　//关闭当前启动的Excel进程
            {
                if (p.ProcessName == "ffmpeg")　　　　　　　　　　//通过进程名来寻找
                {
                    p.Kill();
                    return;
                }
            }
            base.OnExit(e);
        }
    }
}
