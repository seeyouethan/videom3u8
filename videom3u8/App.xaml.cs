using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using videom3u8.Tools;
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
            log4net.Config.XmlConfigurator.Configure();
            this.Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += App_OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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


        /// <summary>
        /// UI线程抛出全局异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                LogHelper.AddErrorLog(e.Exception.ToString());
                MessageBox.Show("很抱歉，当前应用程序遇到一些问题，该操作已经终止，请进行重试，如果问题继续存在，请联系开发人员.", "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogHelper.AddErrorLog(ex.ToString());
                MessageBox.Show("应用程序发生不可恢复的异常，将要退出！");
            }
        }

        /// <summary>
        /// 非UI线程抛出全局异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    LogHelper.AddErrorLog(exception.ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.AddErrorLog(ex.ToString());
                MessageBox.Show("应用程序发生不可恢复的异常，将要退出！");
            }
        }
    }
}
