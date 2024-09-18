using System;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog.Config;
using System.IO;

namespace UserCurrencyConverter
{
    internal static class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\NLog.config"), true);

            logger.Debug("Начало работы программы.");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
