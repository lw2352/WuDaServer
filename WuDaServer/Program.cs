using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace WuDaServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //全局异常捕捉
            Application.ThreadException += Application_ThreadException; //UI线程异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; //多线程异常

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        //UI线程异常
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            //执行异常处理
            MessageBox.Show(e.Exception.ToString());
            UtilClass.writeLog(e.Exception.ToString());
        }

        //多线程异常
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //执行异常处理
            MessageBox.Show(((Exception)e.ExceptionObject).ToString());
            UtilClass.writeLog(((Exception)e.ExceptionObject).ToString());
        }

    }
}
