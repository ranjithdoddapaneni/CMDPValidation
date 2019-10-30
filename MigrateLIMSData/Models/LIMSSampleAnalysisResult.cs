using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MigrateLIMSData.LIMSDB;

namespace MigrateLIMSData.Models
{
    public class LIMSSampleAnalysisResult
    {
        public string XMlSampleElement { get; set; }
        public LIMSSampleAnalysisResult()
        {
            InitSampleNode();
        }
        public LIMSSampleAnalysisResult(RESULTRow row,SAMPLERow sampleRow, int recordID)
        {
            InitSampleNode();
            this.RecordID = recordID.ToString();
            this.LabSampleIdentifier = row.SAMPNO;
            this.PWSIdentifier = string.Format("SC{0}", sampleRow.LOCCODE.Split('-')[0]);
            this.SampleCollectionEndDate = sampleRow.COLDATE.ToString("yyyy-MM-dd");
            this.LabAccreditationIdentifier = "40201";
            this.LabAccreditationAuthorityName = "STATE";
            this.AnalyteCode = row.ANALYTENUM;
            this.DetectionLimitTypeCode = "MRL";
            this.MeasurementQualifier = "Y";
            this.MeasurementValue = row.MDL;
            this.MeasurementUnit = row.RLTUNIT.Trim().ToUpper();
            this.MeasurementSignificantDigit = "6";
            this.DataQualityCode = "A";
        }
        protected void InitSampleNode()
        {
            XMlSampleElement =
               "<EN:SampleAnalysisResults>\r\n" + "<SDWIS:RecordID>" + "{0}" + "</SDWIS:RecordID>\r\n" +
               "<EN:LabSampleIdentifier>" + "{1}" + "</EN:LabSampleIdentifier>\r\n" +
               "<EN:PWSIdentifier>" + "{2}" + "</EN:PWSIdentifier>\r\n" +
               "<EN:SampleCollectionEndDate>" + "{3}" + "</EN:SampleCollectionEndDate>\r\n" +
               "<EN:LabAnalysisIdentification>" +
               "<EN:LabAccreditation>\r\n" +
               "<EN:LabAccreditationIdentifier>" + "{4}" + "</EN:LabAccreditationIdentifier>\r\n" +
               "<EN:LabAccreditationAuthorityName>" + "{5}" + "</EN:LabAccreditationAuthorityName>\r\n" +
               "</EN:LabAccreditation>\r\n" +
               "</EN:LabAnalysisIdentification>\r\n" +
               "<EN:AnalyteIdentification>\r\n" +
               "<EN:AnalyteCode>" + "{6}" + "</EN:AnalyteCode>\r\n" +
               "</EN:AnalyteIdentification>\r\n" +
               "<EN:AnalysisResult>\r\n" +
               "<EN:DetectionLimitTypeCode>" + "{7}" + "</EN:DetectionLimitTypeCode>\r\n" +
               "<EN:DetectionLimit>\r\n" +
               "<EN:MeasurementQualifier>" + "{8}" + "</EN:MeasurementQualifier>\r\n" +
               "<EN:MeasurementValue>" + "{9}" + "</EN:MeasurementValue>\r\n" +
               "<EN:MeasurementUnit>" + "{10}" + " </EN:MeasurementUnit>\r\n" +
               "<EN:MeasurementSignificantDigit>" + "{11}" + "</EN:MeasurementSignificantDigit>\r\n" +
               "</EN:DetectionLimit>\r\n" +
               "</EN:AnalysisResult>\r\n" +
               "<EN:QAQCSummary>\r\n" +
               "<EN:DataQualityCode>" + "{12}" + "</EN:DataQualityCode>\r\n" +
               "</EN:QAQCSummary>\r\n" +
               "</EN:SampleAnalysisResults>\r\n";
        }
        public string RecordID { get; set; }
        public string LabSampleIdentifier { get; set; }
        public string PWSIdentifier { get; set; }
        public string SampleCollectionEndDate { get; set; }
        public string LabAccreditationIdentifier { get; set; }
        public string LabAccreditationAuthorityName { get; set; }
        public string AnalyteCode { get; set; }
        public string DetectionLimitTypeCode { get; set; }
        public string MeasurementQualifier { get; set; }
        public string MeasurementValue { get; set; }
        public string MeasurementUnit { get; set; }
        public string MeasurementSignificantDigit { get; set; }
        public string DataQualityCode { get; set; }
        public string FetchSampleXML()
        {
            return string.Format(XMlSampleElement, RecordID, LabSampleIdentifier, PWSIdentifier, SampleCollectionEndDate, 
                 LabAccreditationIdentifier, LabAccreditationAuthorityName,
                AnalyteCode, DetectionLimitTypeCode, MeasurementQualifier, MeasurementValue, MeasurementUnit, MeasurementSignificantDigit, DataQualityCode);
        }
    }
}
