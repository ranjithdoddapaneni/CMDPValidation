using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ValidationDataAccess.Models;

namespace ValidationUtilities
{
    public class FileHelper
    {
        public static Sample[] ParseAllFiles(out string parsedFileList)
        {
            return ParseAllFiles(out parsedFileList, Properties.Settings.Default.DseOutputPath, Properties.Settings.Default.DseOutputBackup);
        }

        public static Sample[] ParseAllFiles(out string parsedFileList, string directory, string backupDirectory)
        {
            if (Directory.Exists(directory) && Directory.Exists(backupDirectory))
            {
                List<Sample> sampleList = new List<Sample>();
                StringBuilder fileList = new StringBuilder();
                string[] files = new string[0];
                try
                {
                     files = Directory.GetFiles(directory);
                }
                catch(Exception e)
                {
                    Mailer m = new Mailer();
                    m.SendErrorMessage("An error occurred while retreiving XML files", e);
                    LogProjectError(e);
                }
                foreach (var filename in files)
                {
                    string filetype = filename.Substring(filename.LastIndexOf('.'));
                    if (filetype.ToLower().Equals(".xml"))
                    {
                        fileList.Append(filename + "\r\n");
                        sampleList.AddRange(XmlHelper.ParseDataFromXml(filename));

                        string justFilename = Path.GetFileName(filename);
                        string newFilePath = Path.Combine(backupDirectory, justFilename);
                        try
                        {
                            File.Copy(filename, newFilePath, true);
                            File.Delete(filename);
                        }
                        catch(Exception e)
                        {
                            Mailer m = new Mailer();
                            m.SendErrorMessage("An error occurred while backing up XML files", e);
                            LogProjectError(e);
                        }

                    }
                }
                parsedFileList = fileList.ToString();
                return sampleList.ToArray();
            }
            else
            {
                parsedFileList = "Invalid directory path!";
                return null;
            }
        }

        public static string WriteToFile(ICollection<Sample> samples)
        {
            if (samples.Count > 0)
            {
                string filepath = string.Format(@"{0}\xml-Input-{1}-{2}-{3}-{4}-{5}.xml", Properties.Settings.Default.XMLSamplingInputPath, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                string backupFilepath = Path.Combine(Properties.Settings.Default.XMLSamplingInputBackup, Path.GetFileName(filepath));
                try
                {
                    File.WriteAllText(filepath, XmlHelper.CreateXmlString(samples.ToArray()));
                    File.Copy(filepath, backupFilepath, true);
                }
                catch(Exception e)
                {
                    Mailer m = new Mailer();
                    m.SendErrorMessage("An error occurred while writing XML to file", e);
                    LogProjectError(e);
                }
                StringBuilder sb = new StringBuilder("Samples in prepared XML File:\r\n");
                foreach (Sample s in samples)
                {
                    sb.Append(s.LabSampleIdentifier + "\r\n");
                }
                return sb.ToString();
            }
            else
                return "No new samples, no file created";
        }

        public static void LogProjectError(Exception e)
        {
            File.AppendAllText(Properties.Settings.Default.logfile, "\r\n" + DateTime.Now + ":" + e.Message + "\r\n" + e.StackTrace);
        }
    }
}
