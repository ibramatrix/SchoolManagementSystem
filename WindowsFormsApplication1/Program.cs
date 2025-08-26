using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Add global exception handlers here
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "Unhandled UI Exception");
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                MessageBox.Show(ex.ToString(), "Unhandled Non-UI Exception");
            };

            Application.Run(new StartLoadingScreen());
        }
    }
}
