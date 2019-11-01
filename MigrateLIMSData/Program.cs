using MigrateLIMSData.LIMSDBTableAdapters;
using MigrateLIMSData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationDataAccess.Models;
using static MigrateLIMSData.LIMSDB;

namespace MigrateLIMSData
{
    public class Program
    {
        public static LIMSDB lims;
        protected static string xmlDocFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
           "<EN:eDWR xmlns:EN=\"urn:us:net:exchangenetwork\" xmlns:SDWIS=\"http://www.epa.gov/sdwis\" xmlns:ns3=\"http://www.epa.gov/xml\">\r\n" +
           "<EN:Submission>\r\n" +
            "<EN:LabReport>\r\n" +
            "<EN:LabIdentification>\r\n" +
           "<EN:LabAccreditation>\r\n" +
            "<EN:LabAccreditationIdentifier>" + "{0}" +"</EN:LabAccreditationIdentifier>\r\n" +
            "<EN:LabAccreditationAuthorityName>" +"{1}" +"</EN:LabAccreditationAuthorityName>\r\n" +
            "</EN:LabAccreditation>\r\n" +
            "</EN:LabIdentification>\r\n" +
            "{2}"+"{3}"+
            "</EN:LabReport>\r\n" +
           "</EN:Submission>\r\n" +
           "</EN:eDWR>";
        public static void Main(string[] args)
        {
            //ProcessRADSamples.ProcessRADSampleJob();
            SAMPLETableAdapter SAMPLE = new SAMPLETableAdapter();
            lims = new LIMSDB();
            SAMPLE.Fill(lims.SAMPLE);
            int i = 1;
            LIMSSample sample = null;
            string path = string.Format(@"{0}\XMLSampling-{1}-{2}-{3}-{4}-{5}.xml", Properties.Settings.Default.OutputPath, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);

            StringBuilder sampleCollection = new StringBuilder();
            StringBuilder resultCollection = new StringBuilder();

            foreach (SAMPLERow v in lims.SAMPLE.Rows)
            {
                sample = new LIMSSample(v, i);
                sampleCollection.Append(sample.FetchSampleXML());
                ProcessSample(v, resultCollection);
            }
            var str = string.Format(xmlDocFormat, "40201", "STATE", sampleCollection.ToString(), resultCollection.ToString());
            File.WriteAllText(path, str);
        }
        public static void ProcessSample(SAMPLERow row,StringBuilder resultCollection)
        {
            RESULTTableAdapter result = new RESULTTableAdapter();
            result.Fill(lims.RESULT, row.SAMPNO);            
            LIMSSampleAnalysisResult res = null;
            int i = 1;
            foreach (RESULTRow r in lims.RESULT.Rows)
            {
                res = new LIMSSampleAnalysisResult(r, row, i);
                resultCollection.Append(res.FetchSampleXML());
                i++;
            }            
        }
    }
}
