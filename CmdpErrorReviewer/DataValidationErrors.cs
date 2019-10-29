using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ValidationDataAccess;
using ValidationDataAccess.Models;
using ValidationUtilities;

namespace CmdpErrorReviewer
{
    public partial class DataValidationErrors : Form
    {
        private const string sampleTabText = "Sample Errors";
        private const string resultTabText = "Sample Result Errors";

        public DataValidationErrors()
        {
            InitializeComponent(); 
            if(!DbHelper.IsProductionInstance())
            {
                this.Text += "***TEST***";
            }
            UpdateErrorList();
        }

        private void UpdateErrorList()
        {
            //TODO Update GetValidationErrorWaterSystems to also account for XmlSampling rejections
            List<WaterSystem> systems = (List<WaterSystem>)DbHelper.GetValidationErrorWaterSystems();
            if (systems.Count == 0)
            {
                MessageBox.Show("There are currently no errors to display!");
                this.Close();
            }
            lbxWaterSystems.DataSource = systems;
            lbxWaterSystems.DisplayMember = "PwsName";
            lbxWaterSystems.ValueMember = "Pwsid";
        }

        private void UpdateErrorListForWaterSystem(string pwsid)
        { 
            List<ISamplingObject> samples = (List<ISamplingObject>)DbHelper.GetDataValidationErrors(pwsid);
            //TODO update GetXmlSamplingErrors to filter by PWSID
            samples.AddRange(DbHelper.GetXmlSamplingErrors(pwsid));
            if (samples.Count == 0)
            {
                MessageBox.Show("There are currently no errors to display!");
                this.Close();
            }
            lbxErrors.DataSource = samples;
            lbxErrors.DisplayMember = "LabSampleIdentifier";
            lbxErrors.ValueMember = "Id";
        }

        private void ClearAllFields()
        {
            string z = string.Empty;
            tbxErrorText.Text = z;
            lblLabSampleId.Text = z;
            lblPWSID.Text = z;
            lblFedType.Text = z;
            lblFacilityId.Text = z;
            lblSamplingPoint.Text = z;
            lblSampleType.Text = z;
            lblMonitoringType.Text = z;
            lblCollectionDate.Text = z;
            lblCollectionLocation.Text = z;
            lblLabReceiptDate.Text = z;
            lblSampleCollector.Text = z;
            lblAnalyzingLab.Text = z;
            lblSubmittingLab.Text = z;
            lblJobId.Text = z;

            lblAnalyte.Text = z;
            lblAnalyteCode.Text = z;
            lblMethodCode.Text = z;
            lblMeasurementQualifier.Text = z;
            lblMeasurementValue.Text = z;
            lblMeasurementUnit.Text = z;
            lblDetectionLimitType.Text = z;
            lblAnalysisStart.Text = z;
            lbxResultList.DataSource = null;
        }

        private void SetSampleFields(Sample selectedSample, int resultId)
        {
            tbxErrorText.Text = selectedSample.Errors;
            lblLabSampleId.Text = selectedSample.LabSampleIdentifier;
            lblPWSID.Text = selectedSample.PWSIdentifier;
            lblFacilityId.Text = selectedSample.PWSFacilityIdentifier;
            lblSamplingPoint.Text = selectedSample.SampleLocationIdentifier;
            lblSampleType.Text = selectedSample.SampleRuleCode;
            lblMonitoringType.Text = selectedSample.SampleMonitoringTypeCode;
            if (selectedSample.SampleCollectionEndDate != null)
                lblCollectionDate.Text = selectedSample.SampleCollectionEndDate.ToString();
            else
                lblCollectionDate.Text = string.Empty;
            lblCollectionLocation.Text = selectedSample.SampleLocationCollectionAddress;
            if (selectedSample.SampleLaboratoryReceiptDate != null)
                lblLabReceiptDate.Text = ((DateTime)selectedSample.SampleLaboratoryReceiptDate).ToShortDateString();
            else
                lblLabReceiptDate.Text = string.Empty;
            lblSampleCollector.Text = selectedSample.SampleCollector;
            lblAnalyzingLab.Text = selectedSample.LabAccredidationIdentifier;
            lblSubmittingLab.Text = selectedSample.SubmittingLabIdentifier;
            lblJobId.Text = selectedSample.cmdpJobId;
            lblFedType.Text = selectedSample.FedType;
            if(selectedSample.AnalysisResults.Length == 0)
            {
                DbHelper.PopulateResultsForSample(ref selectedSample, false);
            }
            Result[] results = selectedSample.AnalysisResults;
            lbxResultList.DataSource = results;
            lbxResultList.DisplayMember = "AnalyteCode";
            lbxResultList.ValueMember = "Id";
            if (resultId > 0)
            {
                for(int i = 0; i < results.Length; i++)
                {
                    if(results[i].Id == resultId)
                    {
                        lbxResultList.SelectedIndex = i;
                    }
                }
            }
        }

        private void lbxErrors_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllFields();
            if (lbxErrors.SelectedItem is Sample)
            {                
                Sample selectedSample = (Sample)lbxErrors.SelectedItem;
                SetSampleFields(selectedSample, -1);
            }
            else if(lbxErrors.SelectedItem is Result)
            {
                Result selectedResult = (Result)lbxErrors.SelectedItem;
                Sample selectedSample = selectedResult.ParentSample;
                SetSampleFields(selectedSample, selectedResult.Id);
            }
        }

        private void LbxWaterSystems_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ClearAllFields();
            UpdateErrorListForWaterSystem(((WaterSystem)lbxWaterSystems.SelectedItem).Pwsid);
        }

        private void lbxResultList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Result selectedResult = (Result)lbxResultList.SelectedItem;
            if (selectedResult != null)
            {
                lblAnalyte.Text = selectedResult.AnalyteName;
                lblAnalyteCode.Text = selectedResult.AnalyteCode;
                lblMethodCode.Text = selectedResult.MethodIdentifier;
                lblMeasurementQualifier.Text = selectedResult.MeasurementQualifier;
                if (selectedResult.MeasurementValue != null)
                    lblMeasurementValue.Text = selectedResult.MeasurementValue.ToString();
                else
                    lblMeasurementValue.Text = string.Empty;
                lblMeasurementUnit.Text = selectedResult.MeasurementUnit;
                lblDetectionLimitType.Text = selectedResult.DetectionLimitTypeCode;
                if (selectedResult.AnalysisStartDate != null)
                    lblAnalysisStart.Text = selectedResult.AnalysisStartDate.ToString();
                else
                    lblAnalysisStart.Text = string.Empty;
                if (!string.IsNullOrWhiteSpace(selectedResult.Errors) && string.IsNullOrWhiteSpace(tbxErrorText.Text))
                {
                    tbxErrorText.Text = selectedResult.Errors;
                }
            }
        }

        private void btnValidRejection_Click(object sender, EventArgs e)
        {
            LabEmailForm email = null;
            try
            {
                ISamplingObject sample = (ISamplingObject)lbxErrors.SelectedItem;
                email = new LabEmailForm(sample);
                email.ShowDialog(this);
            }
            catch(Exception ex)
            {
                if (email != null)
                    email.Close();
                MessageBox.Show("An error occurred while preparing lab message");
            }
            if (email.emailSent)
            {
                ISamplingObject selectedItem;
                bool updateSuccess = false;
                try
                {
                    selectedItem = (ISamplingObject)lbxErrors.SelectedItem;
                    updateSuccess = DbHelper.SetStaffValidation(selectedItem, false);
                }
                catch (Exception)
                {
                }
                if (!updateSuccess)
                {
                    MessageBox.Show("An error occurred while updating staff validation");
                }
                else
                {
                    UpdateErrorList();
                }
            }
        }

        private void btnFalseRejection_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("This option should be selected after relevant data has been updated in SDWIS, " +
                "i.e. after lab certification or monitoring period association have been updated. " +
                "If not, the sample will continue to be rejected until this data is updated. " +
                "Do you wish to continue?", "Action Required", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            ISamplingObject selectedItem;
            bool updateSuccess = false;
            try
            {
                selectedItem = (ISamplingObject)lbxErrors.SelectedItem;
                updateSuccess = DbHelper.SetStaffValidation(selectedItem, true);
            }
            catch(Exception)
            {
            }
            if (!updateSuccess)
            {
                MessageBox.Show("An error occurred while updating staff validation");
            }
            else
            {
                MessageBox.Show("This sample will be revalidated with the next batch.");
                UpdateErrorList();
            }
        }

        private void btnRemoveRejection_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("This will remove the rejection from the list without revalidating or notifying the lab",
                "Remove without further action?", MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                return;
            }

            ISamplingObject selectedItem;
            bool updateSuccess = false;
            try
            {
                selectedItem = (ISamplingObject)lbxErrors.SelectedItem;
                updateSuccess = DbHelper.SetStaffValidation(selectedItem, false);
            }
            catch (Exception)
            {
            }
            if (!updateSuccess)
            {
                MessageBox.Show("An error occurred while updating staff validation");
            }
            else
            {
                UpdateErrorList();
            }
        }

        private void lblPrint_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RejectionReportViewer rrv = new CmdpErrorReviewer.RejectionReportViewer((Sample)lbxErrors.SelectedItem);
            rrv.Show();
            rrv.Focus();
        }

        /*private void lbxErrors_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush myBrush = Brushes.Black;
            e.DrawBackground();
            var currentItem = ((ListBox)sender).Items[e.Index];
            if (currentItem is Sample)
            {
                if (((Sample)currentItem).DataValidation == false)
                {
                    myBrush = Brushes.Maroon;
                }
                else if (((Sample)currentItem).XmlSamplingValidation == false)
                {
                    myBrush = Brushes.DarkMagenta;
                }
            }
            else if (currentItem is Result)
            {
                if (((Result)currentItem).DataValidation == false)
                {
                    myBrush = Brushes.Navy;
                }
                else if (((Result)currentItem).XmlSamplingValidation == false)
                {
                    myBrush = Brushes.DarkGreen;
                }
            }
            e.Graphics.DrawString(((ISamplingObject)((ListBox)sender).Items[e.Index]).LabSampleIdentifier, e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }*/
    }
}
