using System;
using ValidationUtilities;
using System.IO;

namespace MigrateValidateDseData
{
    public class Program
    {
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(currentDomain_UnhandledException);

            if ((int)DateTime.Now.DayOfWeek != 0 && (int)DateTime.Now.DayOfWeek != 6)
            {
                string validationResults = Validator.ValidateAllNewFiles();
                string subject = "Validation results for " + DateTime.Now.ToShortDateString();
                Mailer m = new Mailer();
                m.SendMessageToStaff(subject, validationResults);
            }
        }

        private static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            File.AppendAllText(Properties.Settings.Default.logfile, "\r\n" + DateTime.Now + ":" + e.Message + "\r\n" + e.StackTrace);
            Mailer m = new Mailer();
            m.SendErrorMessage("Unhandled error in MigrateValidateDseData", e);
            if (e.InnerException != null)
                m.SendErrorMessage("Inner exception for unhandled error in MigrateValidateDseData", e.InnerException);
        }
    }
}
