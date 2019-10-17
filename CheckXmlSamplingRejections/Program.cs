using System;
using ValidationDataAccess;
using ValidationUtilities;
using System.IO;

namespace CheckXmlSamplingRejections
{
    public class Program
    {
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);

            if ((int)DateTime.Now.DayOfWeek != 0 && (int)DateTime.Now.DayOfWeek != 6)
            {
                int newRejections = DbHelper.CheckForXmlSamplingRejections(DateTime.Today);
                string message = string.Format("{0} new XML Sampling rejection{1} found for {2}. {3}",
                                                newRejections,
                                                newRejections == 1 ? string.Empty : "s",
                                                DateTime.Now.ToShortDateString(),
                                                newRejections > 0 ? "Please review using CMDP Rejection Reviewer" : string.Empty);
                string subject = string.Format("{0} New XML Sampling Rejections Found for {1}",
                                                newRejections > 0 ? string.Empty : "No",
                                                DateTime.Now.ToShortDateString());
                Mailer m = new Mailer();
                m.SendMessageToStaff(subject, message);
            }
        }

        private static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            File.AppendAllText(Properties.Settings.Default.logfile, "\r\n" + DateTime.Now + ":" + e.Message + "\r\n" + e.StackTrace);
            Mailer m = new Mailer();
            m.SendErrorMessage("Unhandled error in CheckXmlSamplingRejections", e);
            if (e.InnerException != null)
                m.SendErrorMessage("Inner exception for unhandled error in CheckXmlSamplingRejections", e.InnerException);
        }
    }
}
