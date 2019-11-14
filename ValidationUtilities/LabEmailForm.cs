using System;
using System.Windows.Forms;
using ValidationDataAccess;
using ValidationDataAccess.Models;

namespace ValidationUtilities
{
    public partial class LabEmailForm : Form
    {
        public bool emailSent { get; set; }

        public LabEmailForm(ISamplingObject s)
        {
            InitializeComponent();
            emailSent = false;
            string subject = string.Format("SC Drinking Water Sample Validation Error for {0}{1}", s.LabSampleIdentifier, s is Result ? ", " + ((Result)s).AnalyteCode : "");
            FillForm(s.LabAccredidationIdentifier, subject, WriteMessage(s));
        }

        private string WriteMessage(ISamplingObject s)
        {
            string result = string.Empty;
            string analyteCode = string.Empty;
            if(s is Result)
            {
                result = "Result ";
                analyteCode = string.Format("Analyte: {0}\r\n", ((Result)s).AnalyteCode);
            }
            string message = string.Format("SC Drinking Water Sample {0}Validation Error\r\n" +
                                            "PWSID: {1}\r\n" +
                                            "PWS Name: {2}\r\n" +
                                            "Sample Job Id: {3}\r\n" +
                                            "Sample Id: {4}\r\n{5}{6}",
                                            result,
                                            s.PWSIdentifier,
                                            s is Sample ? ((Sample)s).PWSName : string.Empty,
                                            s is Sample ? ((Sample)s).cmdpJobId : string.Empty,
                                            s.LabSampleIdentifier,
                                            analyteCode,
                                            s.Errors);
            return message;
        }

        private void FillForm(string labId, string emailSubject, string emailBody)
        {
            //TODO figure out how to get email addresses for lab and staff 
            EmailFrom.Text = DbHelper.GetCurrentUserEmail();
            EmailTo.Text = DbHelper.GetEmailForLab(labId);
            EmailCc.Text = Properties.Settings.Default.cmdpEmail;
            EmailSubject.Text = emailSubject;
            EmailBody.Text = emailBody;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Mailer m = new Mailer();
            emailSent = m.SendMessage(EmailSubject.Text, EmailBody.Text, EmailFrom.Text, EmailTo.Text, EmailCc.Text);
            if (!emailSent)
            {
                MessageBox.Show("An error occurred while attempting to send the email. Please try again.");
                return;
            }
            this.Close();
        }

        private void EmailBody_TextChanged(object sender, EventArgs e)
        {

        }

        private void LabEmailForm_Load(object sender, EventArgs e)
        {

        }
    }
}
