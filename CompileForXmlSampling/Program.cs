using System;
using System.Collections.Generic;
using ValidationDataAccess;
using ValidationDataAccess.Models;
using ValidationUtilities;
using System.IO;

namespace CompileForXmlSampling
{
    public class Program
    {
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);

            if ((int)DateTime.Now.DayOfWeek != 0 && (int)DateTime.Now.DayOfWeek != 6)
            {
                string samplesInFile = FileHelper.WriteToFile((List<Sample>)DbHelper.GetSamplesForXmlSampling(true));
                if (!samplesInFile.Contains("No new samples"))
                {
                    string message = samplesInFile;
                    string subject = string.Format("XML File Prepared for XML Sampling for {0}",
                                                    DateTime.Now.ToShortDateString());
                    Mailer m = new Mailer();
                    m.SendMessageToAdmins(subject, message);
                }
            }
        }

        private static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            File.AppendAllText(Properties.Settings.Default.logfile, "\r\n" + DateTime.Now + ":" + e.Message + "\r\n" + e.StackTrace);
            Mailer m = new Mailer();
            m.SendErrorMessage("Unhandled error in CompileForXmlSampling", e);
            if (e.InnerException != null)
                m.SendErrorMessage("Inner exception for unhandled error in CompileForXmlSampling", e.InnerException);
        }
    }
}
