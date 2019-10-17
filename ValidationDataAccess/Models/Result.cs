using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Text;

namespace ValidationDataAccess.Models
{
    [Table("CmdpResults")]
    public class Result : ISamplingObject
    {
        private const string analysisEndDateNull = "<EN:AnalysisEndDate xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:nil=\"true\" />";
        private const string analysisEndTimeNull = "<EN:AnalysisEndTime xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:nil=\"true\" />";
        private const string analysisEndDateFormat = "<EN:AnalysisEndDate>{0}</EN:AnalysisEndDate>";
        private const string analysisEndTimeFormat = "<EN:AnalysisEndTime>{0}</EN:AnalysisEndTime>";

        public Result()
        {
            initResult();
        }

        /// <summary>
        /// Creates a new instance of Result
        /// </summary>
        /// <param name="resultXml">Must be a EN:SampleAnalysisResults XML element from the XMLSampling schema</param>
        /// <param name="resultNamespace">The value of the EN namespace as defined in the XMLSampling schema</param>
        public Result(XElement resultXml, XNamespace resultNamespace)
        {
            initResult();
            SampleAnalysisResultsXml = resultXml;
            SampleAnalysisResultXmlString = resultXml.ToString();
            var element = (from res in resultXml.Descendants(resultNamespace + "LabSampleIdentifier") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.LabSampleIdentifier = element.Value;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "PWSIdentifier") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.PWSIdentifier = element.Value;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "LabAccreditationIdentifier") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.LabAccredidationIdentifier = element.Value;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "DataQualityCode") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.DataQualityCode = element.Value;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "AnalyteCode") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.AnalyteCode = element.Value;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "MethodIdentifier") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.MethodIdentifier = element.Value;
            }
            //We get a AnalysisResult element here first because some elements within it have the same name as elements elswhere,
            //so we get the AnalysisResult elements first to prevent ambiguity
            var resultElement = (from res in resultXml.Descendants(resultNamespace + "AnalysisResult") select res).SingleOrDefault();
            var justResult = (from res in resultXml.Descendants(resultNamespace + "Result") select res).SingleOrDefault();
            if (justResult != null)
            {
                element = (from res in justResult.Descendants(resultNamespace + "MeasurementQualifier") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.MeasurementQualifier = element.Value;
                }
                element = (from res in justResult.Descendants(resultNamespace + "MeasurementValue") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.MeasurementValue = Convert.ToDouble(element.Value);
                }
                element = (from res in justResult.Descendants(resultNamespace + "MeasurementUnit") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.MeasurementUnit = element.Value;
                }
                element = (from res in justResult.Descendants(resultNamespace + "MeasurementSignificantDigit") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.MeasurementSignificantDigit = Convert.ToInt32(element.Value);
                }
                element = (from res in justResult.Descendants(resultNamespace + "MicrobialResultCountTypeCode") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.MicrobialResultCountTypeCode = element.Value;
                }
            }
            element = (from res in resultXml.Descendants(resultNamespace + "DetectionLimitTypeCode") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.DetectionLimitTypeCode = element.Value;
            }
            var justDetectionLimit = (from res in resultXml.Descendants(resultNamespace + "DetectionLimit") select res).SingleOrDefault();
            if(justDetectionLimit != null)
            {
                element = (from res in justDetectionLimit.Descendants(resultNamespace + "MeasurementQualifier") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.DetectionLimitMeasurementQualifier = element.Value;
                }
                element = (from res in justDetectionLimit.Descendants(resultNamespace + "MeasurementValue") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.DetectionLimitMeasurementValue = Convert.ToDouble(element.Value);
                }
                element = (from res in justDetectionLimit.Descendants(resultNamespace + "MeasurementUnit") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.DetectionLimitMeasurementUnit = element.Value;
                }
                element = (from res in justDetectionLimit.Descendants(resultNamespace + "MeasurementSignificantDigit") select res).SingleOrDefault();
                if (element != null && !string.IsNullOrWhiteSpace(element.Value))
                {
                    this.DetectionLimitMeasurementSignificantDigit = Convert.ToInt32(element.Value);
                }
            }
            element = (from res in resultXml.Descendants(resultNamespace + "AnalysisStartDate") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.AnalysisStartDate = Convert.ToDateTime(element.Value);
                _hasAnalysisStartDate = true;
                _analysisStartDateString = element.Value;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "AnalysisStartTime") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value) && this.AnalysisStartDate != new DateTime(0))
            {
                this.AnalysisStartDate = ((DateTime)this.AnalysisStartDate).Add(TimeSpan.Parse(element.Value));
                DateTime testForZeroTime = DateTime.MinValue;
                testForZeroTime = testForZeroTime.Add(TimeSpan.Parse(element.Value));
                if (testForZeroTime.Equals(DateTime.MinValue))
                    _hasAnalysisStartTime = false;
                else
                    _hasAnalysisStartTime = true;
                _analysisStartTimeString = element.Value;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "AnalysisEndDate") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                this.AnalysisEndDate = Convert.ToDateTime(element.Value);
                _hasAnalysisEndDate = true;
            }
            element = (from res in resultXml.Descendants(resultNamespace + "AnalysisEndTime") select res).SingleOrDefault();
            if (element != null && !string.IsNullOrWhiteSpace(element.Value) && this.AnalysisEndDate != new DateTime(0))
            {
                this.AnalysisEndDate = ((DateTime)this.AnalysisEndDate).Add(TimeSpan.Parse(element.Value));
                DateTime testForZeroTime = DateTime.MinValue;
                testForZeroTime = testForZeroTime.Add(TimeSpan.Parse(element.Value));
                if (testForZeroTime.Equals(DateTime.MinValue))
                    _hasAnalysisEndTime = false;
                else
                    _hasAnalysisEndTime = true;
            }
            CheckAndCopyAnalysisDates();
        }
        private bool _hasAnalysisStartDate;
        private bool _hasAnalysisStartTime;
        private bool _hasAnalysisEndDate;
        private bool _hasAnalysisEndTime;
        private string _analysisStartDateString;
        private string _analysisStartTimeString;
        private string _analyteName; 

        #region EntityFrameworkProperties

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ParentSample")]
        public int SampleId { get; set; }

        [Required]
        public string LabSampleIdentifier { get; set; }

        [Required]
        public string PWSIdentifier { get; set; }

        [Required]
        public string LabAccredidationIdentifier { get; set; }

        public string DataQualityCode { get; set; }

        public string AnalyteCode { get; set; }

        public string MethodIdentifier { get; set; }

        public string MeasurementQualifier { get; set; }

        public double? MeasurementValue { get; set; }

        public string MeasurementUnit { get; set; }

        public int? MeasurementSignificantDigit { get; set; }

        public string MicrobialResultCountTypeCode { get; set; }

        public string DetectionLimitTypeCode { get; set; }
        
        public string DetectionLimitMeasurementQualifier { get; set; }

        public double? DetectionLimitMeasurementValue { get; set; }

        public string DetectionLimitMeasurementUnit { get; set; }

        public int? DetectionLimitMeasurementSignificantDigit { get; set; }

        public DateTime? AnalysisStartDate { get; set; }

        public DateTime? AnalysisEndDate { get; set; }

        public string SampleAnalysisResultXmlString { get; set; }

        public string Errors { get; set; }

        public bool? DataValidation { get; set; }

        public DateTime? DataValidationDate { get; set; }

        public bool? StaffValidation { get; set; }

        public DateTime? StaffValidationDate { get; set; }

        public string StaffValidator { get; set; }

        public bool? XmlSamplingValidation { get; set; }

        public DateTime? XmlSamplingValidationDate { get; set; }

        public DateTime? XmlCompilationDate { get; set; }

        public virtual Sample ParentSample { get; set; }

        #endregion

        [NotMapped]
        public XElement SampleAnalysisResultsXml { get; set; }
        public bool hasAnalysisStartDate { get { return Id != 0 ? AnalysisStartDate != null : _hasAnalysisStartDate; } }
        public bool hasAnalysisStartTime { get { return Id != 0 ? AnalysisStartDate != null : _hasAnalysisStartTime; } }
        public bool hasAnalysisEndDate { get { return Id != 0 ? AnalysisEndDate != null : _hasAnalysisEndDate; } }
        public bool hasAnalysisEndTime { get { return Id != 0 ? AnalysisEndDate != null : _hasAnalysisEndTime; } }
        public string XmlFilename { get { return ParentSample.XmlFilename; } }
        public string AnalyteName
        {
            get
            {
                if(string.IsNullOrEmpty(_analyteName) && !string.IsNullOrEmpty(this.AnalyteCode))
                {
                    _analyteName = DbHelper.GetAnalyteName(this.AnalyteCode);
                }
                return _analyteName;
            }
        }

        public bool HasErrors { get { return DataValidation == false || XmlSamplingValidation == false; } }
        public DateTime AnalysisDate
        {
            get
            {
                return (DateTime)(AnalysisStartDate == null ? AnalysisEndDate : AnalysisStartDate);
            }
        }

        private void initResult()
        {
            ErrorListList = new List<string>();
            _hasAnalysisStartDate = false;
            _hasAnalysisStartTime = false;
            _hasAnalysisEndDate = false;
            _hasAnalysisEndTime = false;
            if (!string.IsNullOrWhiteSpace(Errors))
            {
                ErrorListList.Add(Errors);
            }
        }

        //FYI this only works in the instance where the date or time has the nil=true tag, not if it's left blank entirely
        private void CheckAndCopyAnalysisDates()
        {
            if(hasAnalysisStartDate && hasAnalysisStartTime && !hasAnalysisEndDate && !hasAnalysisEndTime)
            {
                if (SampleAnalysisResultXmlString.Contains(analysisEndDateNull))
                {
                    SampleAnalysisResultXmlString = SampleAnalysisResultXmlString.Replace(analysisEndDateNull, string.Format(analysisEndDateFormat, _analysisStartDateString));
                }
                if (SampleAnalysisResultXmlString.Contains(analysisEndTimeNull))
                {
                    SampleAnalysisResultXmlString = SampleAnalysisResultXmlString.Replace(analysisEndTimeNull, string.Format(analysisEndTimeFormat, _analysisStartTimeString));
                }
            }
        }

        private List<string> ErrorListList;
        
        private string GetAllErrorsString()
        {
            StringBuilder allErrors = new StringBuilder();
            foreach (var err in ErrorListList)
            {
                allErrors.Append(string.Format("{0}\r\n", err));
            }
            return allErrors.ToString();
        }

        public void AddError(string s, bool dataValidationError)
        {
            AddError(s, dataValidationError, false);
        }
        
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

        public bool IsPfosPfoa()
        {
            return this.AnalyteCode == "2805" || this.AnalyteCode == "2806";
        }

        public override string ToString()
        {
            return string.Format("{0}:\r\n\tLabSampleIdentifier: {1}\r\n\t" +
                "PWSIdentifier: {2}\r\n\t" +
                "LabAccredidationIdentifier: {3}\r\n\t" +
                "DataQualityCode: {4}\r\n\t" +
                "AnalyteCode: {5}\r\n\t" +
                "MethodIdentifier: {6}\r\n\t" +
                "MeasurementQualifier: {7}\r\n\t" +
                "MeasurementValue: {8}\r\n\t" +
                "MeasurementUnit: {9}\r\n\t" +
                "DetectionLimitTypeCode: {10}\r\n\t" +
                "DetectionLimit: {11}\r\n\t" +
                "AnalysisStartDate: {12}\r\n\t" +
                "AnalysisEndDate: {13}\r\n",
                base.ToString(),
                LabSampleIdentifier,
                PWSIdentifier,
                LabAccredidationIdentifier,
                DataQualityCode,
                AnalyteCode,
                MethodIdentifier,
                MeasurementQualifier,
                MeasurementValue,
                MeasurementUnit,
                DetectionLimitTypeCode,
                MeasurementSignificantDigit,
                AnalysisStartDate,
                AnalysisEndDate);
        }
    }
}
