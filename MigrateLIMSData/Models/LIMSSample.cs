using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MigrateLIMSData.LIMSDB;

namespace MigrateLIMSData.Models
{
    public class LIMSSample
    {
        public LIMSSample()
        {
            InitSampleNode();
        }
        public LIMSSample(SAMPLERow row,int rowNum)
        {
            InitSampleNode();
            if (row.LOCCODE.Contains("-"))
            {
                this.RecordID = rowNum.ToString();
                this.LabSampleIdentifier = row.SAMPNO;
                this.PWSIdentifier =string.Format("SC{0}",row.LOCCODE.Split('-')[0]);
                this.PWSFacilityIdentifier = row.LOCCODE.Split('-')[1];
                this.SampleRuleCode = "GE";
                this.SampleMonitoringTypeCode = "RT";
                this.ComplianceSampleIndicator = "Y";
                this.AdditionalSampleIndicator = "N";
                this.SampleCollectionEndDate = row.COLDATE.ToString("yyyy-MM-dd");
                this.SampleCollectionEndTime = row.COLDATE.ToString("hh:mm:ss");
                this.SampleLocationIdentifier= row.LOCCODE.Split('-')[1];
            }
        }
        protected void InitSampleNode()
        {
            XMlSampleElement =
            "<EN:Sample>\r\n" + "<SDWIS:RecordID>" + "{0}" + "</SDWIS:RecordID>\r\n" + "<EN:SampleIdentification>\r\n" +
            "<EN:LabSampleIdentifier>" + "{1}" + "</EN:LabSampleIdentifier>\r\n" +
            "<EN:PWSIdentifier>" + "{2}" + "</EN:PWSIdentifier>\r\n" +
            "<EN:PWSFacilityIdentifier>" + "{3}" + "</EN:PWSFacilityIdentifier>\r\n" +
            "<EN:SampleRuleCode>" + "{4}" + "</EN:SampleRuleCode>\r\n" +
            "<EN:SampleMonitoringTypeCode>" + "{5}" + "</EN:SampleMonitoringTypeCode>\r\n" +
            "<EN:ComplianceSampleIndicator>" + "{6}" + "</EN:ComplianceSampleIndicator>\r\n" +
            "<EN:AdditionalSampleIndicator>" + "{7}" + "</EN:AdditionalSampleIndicator>\r\n" +
            "<EN:SampleCollectionEndDate>" + "{8}" + "</EN:SampleCollectionEndDate>\r\n" +
            "<EN:SampleCollectionEndTime>" + "{9}" + "</EN:SampleCollectionEndTime>\r\n" +
            "</EN:SampleIdentification>\r\n" + "<EN:SampleLocationIdentification>\r\n" +
            "<EN:SampleLocationIdentifier>" + "{10}" + "</EN:SampleLocationIdentifier>\r\n" +
            "</EN:SampleLocationIdentification>\r\n" + "</EN:Sample>\r\n";
        }
        public string RecordID { get; set; }
        public string LabSampleIdentifier { get; set; }
        public string PWSIdentifier { get; set; }
        public string PWSFacilityIdentifier { get; set; }
        public string SampleRuleCode { get; set; }
        public string SampleMonitoringTypeCode { get; set; }
        public string ComplianceSampleIndicator { get; set; }
        public string AdditionalSampleIndicator { get; set; }
        public string SampleCollectionEndDate { get; set; }
        public string SampleCollectionEndTime { get; set; }
        public string SampleLocationIdentifier { get; set; }
        public string XMlSampleElement { get; set; }
        public string FetchSampleXML()
        {
            return string.Format(XMlSampleElement, RecordID, LabSampleIdentifier, PWSIdentifier, PWSFacilityIdentifier, SampleRuleCode, SampleMonitoringTypeCode, ComplianceSampleIndicator, AdditionalSampleIndicator, SampleCollectionEndDate, SampleCollectionEndTime, SampleLocationIdentifier);
        }
    }
}
