using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MigrateLIMSData.LIMSDB;

namespace MigrateLIMSData.Models
{
    public static class AnalyteCodes
    {
        public const string ALPHA_ACTIVITY = "Alpha Activity";
        public const string BETA_ACTIVITY = "Beta Activity";
        public const string RA226_ACTIVITY = "Ra-226 Activity";
        public const string RA228_ACTIVITY = "Ra-228 Activity";
        public const string URANIUM_ACTIVITY = "Uranium Activity";
        public const string TRITIUM_ACTIVITY = "Tritium Activity";
    }
    public enum AnalyteConsts
    {
        ALPHA_ACTIVITY = 4002,
        BETA_ACTIVITY = 4100,
        RA226_ACTIVITY = 4020,
        RA228_ACTIVITY = 4030,
        URANIUM_ACTIVITY = 4006,
        TRITIUM_ACTIVITY = 4120
    }
    public enum AnalyteValues
    {
        ALPHA_ACTIVITY = 3,
        BETA_ACTIVITY = 4,
        RA226_ACTIVITY = 1,
        RA228_ACTIVITY = 1,
        URANIUM_ACTIVITY = 1,
        TRITIUM_ACTIVITY = 1000
    }
    public class RADSampleAnalysisResult
    {
        public string XMlSampleElement { get; set; }
        public RADSampleAnalysisResult()
        {
            InitSampleResultNode();
        }
        public RADSampleAnalysisResult(RADResultRow row, RADSampleRow sampleRow, int recordID)
        {
            InitSampleResultNode();
            this.RecordID = recordID.ToString();
            this.LabSampleIdentifier = row.SAMPNO;
            this.PWSIdentifier = string.Format("SC{0}", sampleRow.LOCCODE.Split('-')[0]);
            this.SampleCollectionEndDate = sampleRow.COLDATE.ToString("yyyy-MM-dd");
            this.LabAccreditationIdentifier = "SCRAD";
            this.LabAccreditationAuthorityName = "STATE";
            this.AnalysisResult = FetchAnalysisResult(row.ANALYTE, row.RESULT);
            this.AnalyteCode = FetchAnalyteCodeByAnalyte(row.ANALYTE);
            this.MeasurementQualifier = "Y";
            this.MeasurementValue = row.MDL;
            this.MeasurementUnit = row.RLTUNIT.Trim().ToUpper();
            this.MeasurementSignificantDigit = "6";
            this.DataQualityCode = "A";
        }
        protected string FetchAnalysisResult(string analyte, string result)
        {
            StringBuilder analysisResult = new StringBuilder();
            if (result.Contains("<LLD"))
            {
                analysisResult.Append("<EN:DetectionLimitTypeCode>MRL</EN:DetectionLimitTypeCode>");
                analysisResult.Append("<EN:DetectionLimit>");
                analysisResult.Append("<EN:MeasurementQualifier>Y</EN:MeasurementQualifier>");
                analysisResult.Append(string.Format("<EN:MeasurementValue>{0}</EN:MeasurementValue>", FetchAnalyteValueByAnalyte(analyte)));
                analysisResult.Append("<EN:MeasurementUnit>pCi/L</EN:MeasurementUnit>");
                analysisResult.Append("<EN:MeasurementSignificantDigit>1</EN:MeasurementSignificantDigit>");
                analysisResult.Append("</EN:DetectionLimit>");
            }
            else
            {
                analysisResult.Append("<EN:Result>");
                analysisResult.Append(string.Format("<EN:MeasurementValue>{0}</EN:MeasurementValue>", result));
                analysisResult.Append("<EN:MeasurementUnit>pCi/L</EN:MeasurementUnit>");
                analysisResult.Append(string.Format("<EN:MeasurementSignificantDigit>{0}</EN:MeasurementSignificantDigit>", result.Split('.').Length > 1 ? result.Split('.').ElementAt(1).Length : 1));
                analysisResult.Append("</EN:Result>");
                analysisResult.Append("<EN:DetectionLimit>");
                analysisResult.Append("<EN:MeasurementQualifier>N</EN:MeasurementQualifier>");
                analysisResult.Append(string.Format("<EN:MeasurementValue>{0}</EN:MeasurementValue>", result));
                analysisResult.Append("<EN:MeasurementUnit>pCi/L</EN:MeasurementUnit>");
                analysisResult.Append(string.Format("<EN:MeasurementSignificantDigit>{0}</EN:MeasurementSignificantDigit>", result.Split('.').Length > 1 ? result.Split('.').ElementAt(1).Length : 1));
                analysisResult.Append("</EN:DetectionLimit>");
            }
            return analysisResult.ToString();
        }
        protected string FetchAnalyteCodeByAnalyte(string analyte)
        {
            switch (analyte)
            {
                case AnalyteCodes.ALPHA_ACTIVITY:
                    return ((int)AnalyteConsts.ALPHA_ACTIVITY).ToString();
                case AnalyteCodes.BETA_ACTIVITY:
                    return ((int)AnalyteConsts.BETA_ACTIVITY).ToString();
                case AnalyteCodes.RA226_ACTIVITY:
                    return ((int)AnalyteConsts.RA226_ACTIVITY).ToString();
                case AnalyteCodes.RA228_ACTIVITY:
                    return ((int)AnalyteConsts.RA228_ACTIVITY).ToString();
                case AnalyteCodes.URANIUM_ACTIVITY:
                    return ((int)AnalyteConsts.URANIUM_ACTIVITY).ToString();
                case AnalyteCodes.TRITIUM_ACTIVITY:
                    return ((int)AnalyteConsts.TRITIUM_ACTIVITY).ToString();
                default: return string.Empty;
            }
        }
        protected string FetchAnalyteValueByAnalyte(string analyte)
        {
            switch (analyte)
            {
                case AnalyteCodes.ALPHA_ACTIVITY:
                    return ((int)AnalyteValues.ALPHA_ACTIVITY).ToString();
                case AnalyteCodes.BETA_ACTIVITY:
                    return ((int)AnalyteValues.BETA_ACTIVITY).ToString();
                case AnalyteCodes.RA226_ACTIVITY:
                    return ((int)AnalyteValues.RA226_ACTIVITY).ToString();
                case AnalyteCodes.RA228_ACTIVITY:
                    return ((int)AnalyteValues.RA228_ACTIVITY).ToString();
                case AnalyteCodes.URANIUM_ACTIVITY:
                    return ((int)AnalyteValues.URANIUM_ACTIVITY).ToString();
                case AnalyteCodes.TRITIUM_ACTIVITY:
                    return ((int)AnalyteValues.TRITIUM_ACTIVITY).ToString();
                default: return string.Empty;
            }
        }
        protected void InitSampleResultNode()
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
               "{7}\r\n" +
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
        public string AnalysisResult { get; set; }
        public string MeasurementQualifier { get; set; }
        public string MeasurementValue { get; set; }
        public string MeasurementUnit { get; set; }
        public string MeasurementSignificantDigit { get; set; }
        public string DataQualityCode { get; set; }
        public string FetchSampleXML()
        {
            return string.Format(XMlSampleElement, RecordID, LabSampleIdentifier, PWSIdentifier, SampleCollectionEndDate,
                 LabAccreditationIdentifier, LabAccreditationAuthorityName,
                AnalyteCode, AnalysisResult, MeasurementQualifier, MeasurementValue, MeasurementUnit, MeasurementSignificantDigit, DataQualityCode);
        }
    }
}
