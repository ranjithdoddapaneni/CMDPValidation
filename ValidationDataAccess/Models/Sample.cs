using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ValidationDataAccess.Models
{
    [Table("CmdpSamples")]
    public class Sample : ISamplingObject
    {
        private const string RoutineTags = "<EN:SampleMonitoringTypeCode>RT</EN:SampleMonitoringTypeCode>";
        private const string SpecialTags = "<EN:SampleMonitoringTypeCode>SP</EN:SampleMonitoringTypeCode>";

        public Sample()
        {
            initSample();
        }
        /// <summary>
        /// Creates a new instance of Sample
        /// </summary>
        /// <param name="labReportXml">Must be a EN:LabReport XML element from the XMLSampling schema</param>
        /// <param name="sampleNamespace">The value of the EN namespace as defined in the XMLSampling schema</param>
        /// <param name="labReportXml">The EN:LabReport element of which this sample is a part. This will be used for reconstructing the XML file later.</param>
        public Sample(XElement labReportXml, XNamespace sampleNamespace, string filename)
        {
            initSample();
            XmlNamespace = sampleNamespace;
            this.XmlFilename = filename;
            this.XmlParseDate = DateTime.Now;
            this.LabIdentificationXml = (from sam in labReportXml.Descendants(sampleNamespace + "LabIdentification") select sam).SingleOrDefault();
            LabIdentificationXmlString = LabIdentificationXml.ToString();
            this.SampleXml = (from sam in labReportXml.Descendants(sampleNamespace + "Sample") select sam).SingleOrDefault();
            SampleXmlString = SampleXml.ToString();
            var element = (from sam in SampleXml.Descendants(sampleNamespace + "LabSampleIdentifier") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.LabSampleIdentifier = element.Value;
            }
            element = (from sam in LabIdentificationXml.Descendants(sampleNamespace + "LabAccreditationIdentifier") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.LabAccredidationIdentifier = element.Value;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "PWSIdentifier") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.PWSIdentifier = element.Value;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "PWSFacilityIdentifier") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.PWSFacilityIdentifier = element.Value;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleRuleCode") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleRuleCode = element.Value;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleMonitoringTypeCode") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleMonitoringTypeCode = element.Value;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleCollectionEndDate") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleCollectionEndDate = Convert.ToDateTime(element.Value);
                _hasCollectionDate = true;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleCollectionEndTime") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value) && this.SampleCollectionEndDate != new DateTime(0))
            {
                this.SampleCollectionEndDate = ((DateTime)this.SampleCollectionEndDate).Add(TimeSpan.Parse(element.Value));
                DateTime testForZeroTime = DateTime.MinValue;
                testForZeroTime = testForZeroTime.Add(TimeSpan.Parse(element.Value));
                if (testForZeroTime.Equals(DateTime.MinValue))
                    _hasCollectionTime = false;
                else
                    _hasCollectionTime = true;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleVolume") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleVolume = Convert.ToDouble(element.Value);
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleLocationIdentifier") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleLocationIdentifier = element.Value;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleLocationCollectionAddress") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleLocationCollectionAddress = element.Value;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleCollector") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleCollector = element.Value;
                _hasCollectorName = true;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "SampleLaboratoryReceiptDate") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.SampleLaboratoryReceiptDate = Convert.ToDateTime(element.Value);
                _hasLabReceiptDate = true;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "LabSampleCompositeDate") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.LabSampleCompositeDate = Convert.ToDateTime(element.Value);
                _isCompositeSample = true;
            }
            element = (from sam in SampleXml.Descendants(sampleNamespace + "Comments") select sam).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                string comments = element.Value;
                if(comments.Contains("Reporting Lab"))
                {
                    string labId = comments.Substring(comments.LastIndexOf(" ") + 1);
                    this.SubmittingLabIdentifier = labId;
                }
            }
            HandleFedTypeAndMonitoringType();
        }
        private void initSample()
        {
            AnalysisResultList = new List<Result>();
            ErrorListList = new List<string>();
            SampleCollectionEndDate = new DateTime(0);
            SampleVolume = -1;
            _hasCollectionDate = false;
            _hasCollectionTime = false;
            _hasCollectorName = false;
            _hasLabReceiptDate = false;
            _isCompositeSample = false;
            Has3100 = false;
            Has3014 = false;
            if(!string.IsNullOrWhiteSpace(Errors))
            {
                ErrorListList.Add(Errors);
            }
        }
        private bool _hasCollectionDate;
        private bool _hasCollectionTime;
        private bool _hasCollectorName;
        private bool _hasLabReceiptDate;
        private bool _isCompositeSample;
        private bool _hasResults;
        private string _PwsName;
        private string _FedType;
        protected string labReportXmlFormat = "<EN:LabReport>\r\n{0}</EN:LabReport>\r\n";

        #region EntityFrameworkProperties

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string LabSampleIdentifier { get; set; }

        [Required]
        public string PWSIdentifier { get; set; }

        public string PWSFacilityIdentifier { get; set; }

        [Required]
        public string SampleRuleCode { get; set; }

        [Required]
        public string SampleMonitoringTypeCode { get; set; }

        public DateTime? SampleCollectionEndDate { get; set; }

        public double? SampleVolume { get; set; }

        public string SampleLocationIdentifier { get; set; }

        public string SampleLocationCollectionAddress { get; set; }

        public string SampleCollector { get; set; }

        public DateTime? SampleLaboratoryReceiptDate { get; set; }

        public DateTime? LabSampleCompositeDate { get; set; }

        [Required]
        public string LabIdentificationXmlString { get; set; }

        [Required]
        public string LabAccredidationIdentifier { get; set; }

        public string SubmittingLabIdentifier { get; set; }

        [Required]
        public string SampleXmlString { get; set; }

        public string Errors { get; set; }

        public bool? DataValidation { get; set; }

        public DateTime? DataValidationDate { get; set; }

        public bool? StaffValidation { get; set; }

        public DateTime? StaffValidationDate { get; set; }

        public string StaffValidator { get; set; }

        public bool? XmlSamplingValidation { get; set; }

        public DateTime? XmlSamplingValidationDate { get; set; }

        public DateTime? XmlCompilationDate { get; set; }

        [Required]
        public string XmlFilename { get; set; }

        [Required]
        public DateTime XmlParseDate { get; set; }
        
        #endregion

        [NotMapped]
        public XElement LabIdentificationXml { get; set; }
        [NotMapped]
        public XElement SampleXml { get; set; }
        [NotMapped]
        public XNamespace XmlNamespace { get; set; }
        [NotMapped]
        public bool Has3100 { get; set; }
        [NotMapped]
        public bool Has3014 { get; set; }
        public bool HasResults { get { return AnalysisResultList.Count > 0; } }
        public bool HasErrors { get { return DataValidation == false || XmlSamplingValidation == false; } }
        public bool HasCollectionDate { get { return Id != 0 ? SampleCollectionEndDate != null : _hasCollectionDate; } }
        public bool HasCollectionTime { get { return Id != 0 ? SampleCollectionEndDate != null : _hasCollectionTime; } }
        public bool HasCollectorName { get { return Id != 0 ? SampleCollector != null : _hasCollectorName; } }
        public bool HasLabReceiptDate { get { return Id != 0 ? SampleLaboratoryReceiptDate != null : _hasLabReceiptDate; } }
        public bool IsCompositeSample { get { return _isCompositeSample; } }
        public string cmdpJobId { get { return XmlFilename.Substring(XmlFilename.LastIndexOf('_') + 1, XmlFilename.LastIndexOf('.') - XmlFilename.LastIndexOf('_') - 1); } }
        public string FedType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_FedType))
                {
                    _FedType = DbHelper.GetPwsFedType(this.PWSIdentifier).Trim();
                }
                return _FedType;
            }
        }
        public string PWSName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_PwsName))
                {
                    _PwsName = DbHelper.GetPwsName(this.PWSIdentifier);
                }
                return _PwsName;
            }
        }

        private List<Result> AnalysisResultList;
        public Result[] AnalysisResults
        {
            get
            {
                return AnalysisResultList.ToArray();
            }
        }

        public bool AddAnalysisResult(Result r)
        {
            if (!ResultIsFromSample(r))
            {
                return false;
            }
            else
            {
                AnalysisResultList.Add(r);
                return true;
            }
        }

        public void AddAnalysisResult(ICollection<Result> results)
        {
            AnalysisResultList.AddRange(results);
        }

        private List<string> ErrorListList;

        /// <summary>
        /// Adds an error to the sample
        /// </summary>
        /// <param name="s">Text description of the error</param>
        /// <param name="dataValidationError">Mark TRUE if the error is a data validation error, FALSE otherwise</param>
        public void AddError(string s, bool dataValidationError)
        {
            AddError(s, dataValidationError, false);
        }

        /// <summary>
        /// Adds an error to the sample
        /// </summary>
        /// <param name="s">Text description of the error</param>
        /// <param name="dataValidationError">Mark TRUE if the error is a data validation error, FALSE otherwise</param>
        /// <param name="xmlSamplingValidationError">Mark TRUE if the error is a xml sampling validation error, FALSE otherwise</param>
        public void AddError(string s, bool dataValidationError, bool xmlSamplingValidationError)
        {
            ErrorListList.Add(s);
            Errors = GetAllErrorsString();
            if (dataValidationError)
                DataValidation = false;
            if (xmlSamplingValidationError)
                XmlSamplingValidation = false;
        }

        public void SetDataValidation()
        {
            DataValidationDate = DateTime.Now;
            if (DataValidation == null)
                DataValidation = true;
        }

        private void HandleFedTypeAndMonitoringType()
        {
            if(this.FedType == "NP" && this.SampleRuleCode == "TC")
            {
                this.SampleMonitoringTypeCode = "SP";
                this.SampleXmlString = this.SampleXmlString.Replace(RoutineTags, SpecialTags);
            }
        }

        private string GetAllErrorsString()
        {
            StringBuilder allErrors = new StringBuilder();
            foreach (var err in ErrorListList)
            {
                allErrors.Append(string.Format("{0}\r\n", err));
            }
            foreach (var res in AnalysisResultList)
            {
                allErrors.Append(string.Format("{0}\r\n", res.Errors));
            }
            return allErrors.ToString();
        }

        public override string ToString()
        {
            StringBuilder results = new StringBuilder();
            if (AnalysisResultList != null && AnalysisResultList.Count > 0)
            {
                foreach (var v in AnalysisResultList)
                {
                    results.Append(v.ToString());
                }
            }
            return string.Format("{0}:\r\n\t" +
                "LabSampleIdentifier: {1}\r\n\t" +
                "PWSIdentifier: {2}\r\n\t" +
                "PWSFacilityIdentifier: {3}\r\n\t" +
                "SampleRuleCode: {4}\r\n\t" +
                "SampleMonitoringTypeCode: {5}\r\n\t" +
                "SampleCollectionEndDate: {6}\r\n\t" +
                "SampleVolume: {7}\r\n\t" +
                "SampleLocationIdentifier: {8}\r\n\t" +
                "SampleCollector: {9}\r\n\t" +
                "SampleLaboratoryReceiptDate: {10}\r\n\t" +
                "AnalysisResults: {11}\r\n",
                base.ToString(),
                LabSampleIdentifier,
                PWSIdentifier,
                PWSFacilityIdentifier,
                SampleRuleCode,
                SampleMonitoringTypeCode,
                SampleCollectionEndDate,
                SampleVolume,
                SampleLocationIdentifier,
                SampleCollector,
                SampleLaboratoryReceiptDate,
                results.ToString());
        }

        public bool ResultIsFromSample(Result r)
        {
            if (string.IsNullOrWhiteSpace(this.LabSampleIdentifier)
                || r == null
                || string.IsNullOrWhiteSpace(r.LabSampleIdentifier)
                || r.LabSampleIdentifier != this.LabSampleIdentifier)
                return false;
            else return true;
        }

        public XElement BuildLabReport()
        {
            if (this.HasErrors)
                return null;

            XElement labReport = new XElement(XmlNamespace + "LabReport");
            labReport.Add(this.LabIdentificationXml);
            labReport.Add(this.SampleXml);
            foreach (Result r in this.AnalysisResultList)
            {
                if (!r.HasErrors)
                {
                    labReport.Add(r.SampleAnalysisResultsXml);
                }
            }
            return labReport;
        }

        public string BuildLabReportString()
        {
            if (this.HasErrors)
                return null;

            StringBuilder labReport = new StringBuilder();
            labReport.Append(this.LabIdentificationXmlString + "\r\n");
            labReport.Append(this.SampleXmlString + "\r\n");
            foreach (Result r in this.AnalysisResultList)
            {
                if (!r.HasErrors)
                {
                    labReport.Append(r.SampleAnalysisResultXmlString);
                }
            }
            return string.Format(labReportXmlFormat, labReport.ToString());
        }
    }
}
