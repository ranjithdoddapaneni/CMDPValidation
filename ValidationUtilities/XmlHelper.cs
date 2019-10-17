using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValidationDataAccess.Models;
using System.Xml.Linq;

namespace ValidationUtilities
{
    public class XmlHelper
    {
        protected static XNamespace EN = "urn:us:net:exchangenetwork";
        protected static string xmlDocFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
            "<EN:eDWR xmlns:EN=\"urn:us:net:exchangenetwork\" xmlns:SDWIS=\"http://www.epa.gov/sdwis\" xmlns:ns3=\"http://www.epa.gov/xml\">\r\n" +
            "<EN:Submission>\r\n" +
            "{0}\r\n" +
            "</EN:Submission>\r\n" +
            "</EN:eDWR>";

        public static string CreateXmlString(Sample[] sampleList)
        {
            StringBuilder submission = new StringBuilder();
            foreach (var s in sampleList)
            {
                string currentSample = s.BuildLabReportString();
                if (!string.IsNullOrEmpty(currentSample))
                {
                    submission.Append(currentSample);
                }
            }
            return string.Format(xmlDocFormat, submission);
        }

        public static Sample[] ParseDataFromXml(string filePath)
        {
            List<Sample> samplesFromFile = new List<Sample>();
            XDocument doc;
            doc = XDocument.Load(filePath);

            var LabReports =
                (from node in doc.Descendants(EN + "LabReport")
                 select node);

            foreach (var v in LabReports)
            {
                var Samples =
                    (from sam in v.Descendants(EN + "Sample")
                     select sam);
                foreach (var s in Samples)
                {
                    Sample currentSample = new Sample(v, EN, filePath);
                    samplesFromFile.Add(currentSample);
                    var SampleAnalysisResults =
                        (from res in v.Descendants(EN + "SampleAnalysisResults")
                         select res);
                    foreach (var r in SampleAnalysisResults)
                    {
                        Result currentResult = new Result(r, EN);
                        currentSample.AddAnalysisResult(currentResult);
                    }
                }
            }

            return samplesFromFile.ToArray();
        }
    }
}
