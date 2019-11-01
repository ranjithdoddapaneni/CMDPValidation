using MigrateLIMSData.LIMSDBTableAdapters;
using MigrateLIMSData.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MigrateLIMSData.LIMSDB;

namespace MigrateLIMSData
{
    public class ProcessRADSamples
    {
        public static LIMSDB lims;
        protected static string xmlDocFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
           "<EN:eDWR xmlns:EN=\"urn:us:net:exchangenetwork\" xmlns:SDWIS=\"http://www.epa.gov/sdwis\" xmlns:ns3=\"http://www.epa.gov/xml\">\r\n" +
           "<EN:Submission>\r\n" +
            "<EN:LabReport>\r\n" +
            "<EN:LabIdentification>\r\n" +
           "<EN:LabAccreditation>\r\n" +
            "<EN:LabAccreditationIdentifier>" + "{0}" + "</EN:LabAccreditationIdentifier>\r\n" +
            "<EN:LabAccreditationAuthorityName>" + "{1}" + "</EN:LabAccreditationAuthorityName>\r\n" +
            "</EN:LabAccreditation>\r\n" +
            "</EN:LabIdentification>\r\n" +
            "{2}" + "{3}" +
            "</EN:LabReport>\r\n" +
           "</EN:Submission>\r\n" +
           "</EN:eDWR>";
        public static void ProcessRADSampleJob()
        {
            RADSampleTableAdapter sample = new RADSampleTableAdapter();
            lims = new LIMSDB();
            sample.Fill(lims.RADSample);
            int i = 1;
            RADSample objSample;
            StringBuilder sampleCollection = new StringBuilder();
            StringBuilder resultCollection = new StringBuilder();
            string path = string.Format(@"{0}\RAD_XMLSampling-{1}-{2}-{3}-{4}-{5}.xml", Properties.Settings.Default.OutputPath, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);

            foreach (RADSampleRow sampleRow in lims.RADSample.Rows)
            {
                objSample = new RADSample(sampleRow, i);
                sampleCollection.Append(objSample.FetchSampleXML());
                ProcessSample(sampleRow, resultCollection);
            }
            var str = string.Format(xmlDocFormat, "SCRAD", "STATE", sampleCollection.ToString(), resultCollection.ToString());
            File.WriteAllText(path, str);
        }
        public static void ProcessSample(RADSampleRow row, StringBuilder resultCollection)
        {
            RADResultTableAdapter result = new RADResultTableAdapter();
            result.Fill(lims.RADResult, row.SAMPNO);
            RADSampleAnalysisResult res = null;
            int i = 1;
            foreach (RADResultRow r in lims.RADResult.Rows)
            {
                res = new RADSampleAnalysisResult(r, row, i);
                resultCollection.Append(res.FetchSampleXML());
                i++;
            }
        }
    }
}
