using System;
using System.Net.Mail;
using System.Text;
using ValidationDataAccess;

namespace ValidationUtilities
{
    /// <summary>
    /// Summary description for Mailer.
    /// </summary>
    public class Mailer
    {
        private string sEmailAddress, sEmailServer, sAdminEmailAddress, sNoReplyEmail;
        //private string sEmailMessage = "";
        public Mailer()
        {
            loadEmailOptions();
        }

        private void loadEmailOptions()
        {
            StringBuilder emails = new StringBuilder();
            foreach(var emailAddress in Properties.Settings.Default.errorReportEmailList)
            {
                emails.Append(emailAddress + ",");
            }
            sEmailAddress = emails.ToString().Trim(',');
            StringBuilder adminEmails = new StringBuilder();
            foreach(var emailAddress in Properties.Settings.Default.adminEmails)
            {
                adminEmails.Append(emailAddress + ",");
            }
            sAdminEmailAddress = adminEmails.ToString().Trim(',');
            sEmailServer = Properties.Settings.Default.emailServer;
            sNoReplyEmail = Properties.Settings.Default.noReplyEmail;
        }

        public bool SendMessageToAdmins(string subject, string body)
        {
            return SendMessage(subject, body, sNoReplyEmail, sAdminEmailAddress, sAdminEmailAddress);
        }

        public bool SendMessageToStaff(string subject, string body)
        {
            return SendMessage(subject, body, sNoReplyEmail, sEmailAddress, sAdminEmailAddress);
        }

        public bool SendMessage(string subject, string body, string from, string to, string cc)
        {
            try
            {
                MailMessage mess = new MailMessage();
                mess.From = new MailAddress(from);
                to = to.Replace(';',',');
                to = to.Trim(',');
                mess.To.Add(to);
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    cc = cc.Replace(';',',');
                    cc = cc.Trim(',');
                    mess.CC.Add(cc);
                }
                mess.Subject = subject;
                mess.Body = body;
                ServerCheck(ref mess);
                SmtpClient emailSender = new SmtpClient(sEmailServer);
                emailSender.Send(mess);
            }
            catch(Exception e)
            {
                SendErrorMessage("Error in Mailer.SendMessage", e);
                return false;
            }
            return true;
        }

        public void SendErrorMessage(string message, Exception e)
        {
            SendErrorMessage(message, e, message);
        }

        public void SendErrorMessage(string message, Exception e, string subject)
        {
            MailMessage mess = new MailMessage();
            mess.To.Add("DODDAPRK@DHEC.SC.GOV");
            mess.Subject = subject;
            mess.From = new MailAddress("DODDAPRK@DHEC.SC.GOV");
            mess.Body = DateTime.Now.ToString() + ":" + message + ": " + e.Message;
            ServerCheck(ref mess);
            SmtpClient emailSender = new SmtpClient("publicsmtp.dhec.sc.gov");
            emailSender.Send(mess);
        }

        private void ServerCheck(ref MailMessage m)
        {
            if (!DbHelper.IsProductionInstance())
            {
                m.Subject += "***TEST***";
            }
        }
    }
}
