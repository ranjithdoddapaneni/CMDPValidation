namespace CmdpErrorReviewer
{
    partial class RejectionReportViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource2 = new Microsoft.Reporting.WinForms.ReportDataSource();
            this.reportViewer1 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SampleBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ResultBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.SampleBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResultBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // reportViewer1
            // 
            this.reportViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            reportDataSource1.Name = "dsSample";
            reportDataSource1.Value = this.SampleBindingSource;
            reportDataSource2.Name = "dsResult";
            reportDataSource2.Value = this.ResultBindingSource;
            this.reportViewer1.LocalReport.DataSources.Add(reportDataSource1);
            this.reportViewer1.LocalReport.DataSources.Add(reportDataSource2);
            this.reportViewer1.LocalReport.ReportEmbeddedResource = "CmdpErrorReviewer.RejectionReport.rdlc";
            this.reportViewer1.Location = new System.Drawing.Point(12, 12);
            this.reportViewer1.Name = "reportViewer1";
            this.reportViewer1.Size = new System.Drawing.Size(860, 537);
            this.reportViewer1.TabIndex = 0;
            // 
            // SampleBindingSource
            // 
            this.SampleBindingSource.DataSource = typeof(ValidationDataAccess.Models.Sample);
            // 
            // ResultBindingSource
            // 
            this.ResultBindingSource.DataSource = typeof(ValidationDataAccess.Models.Result);
            // 
            // RejectionReportViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.reportViewer1);
            this.Name = "RejectionReportViewer";
            this.Text = "RejectionReportViewer";
            this.Load += new System.EventHandler(this.RejectionReportViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SampleBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResultBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer reportViewer1;
        private System.Windows.Forms.BindingSource SampleBindingSource;
        private System.Windows.Forms.BindingSource ResultBindingSource;
    }
}