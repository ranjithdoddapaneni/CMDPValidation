using System;
using System.Collections.Generic;
using System.Text;
using ValidationDataAccess;
using ValidationDataAccess.Models;
using ValidationUtilities;

namespace MigrateValidateDseData
{
    public class Validator
    {
        protected const string DATE_ALLOWED_MIN = "1970-01-01";

        /*
         * CMDP – AK  - before the file is loaded to XML Sampling
         * 1)	Lab should be certified for the analyte and method used to analyze the analyte.
         * 2)	For TCR sample – consider 48 hour waiver
         * 3)	These should be required in submission,:
         *      a.	Collection Date (already required but include in validation)
         *      b.	Collection Time
         *      c.	Analysis Start Date
         *      d.	Analysis Start Time
         *      e.	Sample Collectors Name
         * 
         * SDWIS/XML Sampling
         * 1)	Make sure TSALAMA (lab is certified for the Analysis method code used to analyze the analyte)  is in SDWIS
         * 2)	TCR Monitoring Period Association exists
         * 3)	RT/TR  TCR Schedule exist for the pws
         */

        //TODO This method was for early debug purposes and can be deleted when ready
        /*public static string CheckFile(string filePath)
        {
            Sample[] sampleList = XmlHelper.ParseDataFromXml(filePath);

            SampleValidation(ref sampleList);

            //XDocument xDoc = CreateXml(sampleList);
            string xDocString = XmlHelper.CreateXmlString(sampleList);

            StringBuilder output = new StringBuilder();
            foreach (var s in sampleList)
            {
                output.Append(s);
                output.Append(s.GetAllErrorsString());
            }
            //output.Append(xDoc.ToString());
            output.Append(xDocString);
            return output.ToString();
        }*/

        public static bool ValidateFile(string filename)
        {
            try
            {
                var sampleList = XmlHelper.ParseDataFromXml(filename);
                string acceptedSamples;
                string rejectSamples;
                SampleValidation(ref sampleList, out acceptedSamples, out rejectSamples);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string ValidateAllNewFiles()
        {
            if (!DbHelper.CheckConnections())
            {
                return "Database connection failed: migration aborted.";
            }

            string filesParsed;
            Sample[] sampleList = FileHelper.ParseAllFiles(out filesParsed);
            List<Sample> correctedSamples = (List<Sample>)DbHelper.GetCorrectedDataValidationErrors();
            correctedSamples.AddRange(sampleList);
            sampleList = correctedSamples.ToArray();

            string acceptedSamples;
            string rejectSamples;
            SampleValidation(ref sampleList, out acceptedSamples, out rejectSamples);
            return string.Format("Files parsed:\r\n{0}\r\n\r\nSamples accepted:\r\n{1}\r\n\r\nSamples with errors:\r\n{2}",
                                filesParsed, acceptedSamples, rejectSamples);
        }

        private static void SampleValidation(ref Sample[] samples, out string acceptedSamples, out string rejectSamples)
        {
            SortedSet<string> rejectSamplesSS = new SortedSet<string>();
            SortedSet<string> acceptedSamplesSS = new SortedSet<string>();
            foreach (Sample s in samples)
            {
                bool IsTcSample = s.SampleRuleCode == "TC";
                bool Needs3014 = false;
                bool testHoldingTime = false;
                bool needOneMoreDatabaseUpdate = false;
                string pwsid = s.PWSIdentifier;

                DateTime sampleLabReceiptDate = new DateTime(0);
                DateTime sampleCollectionDate = new DateTime(0);

                try
                {
                    if (!s.HasCollectionDate)
                    {
                        s.AddError("Required field: Sample collection date is missing", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }

                    if (!s.HasCollectionTime)
                    {
                        s.AddError("Required field: Sample collection time is missing", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }
                    /*if (!s.HasCollectorName)
                    {
                        s.AddError("Required field: Sample collector name is missing");
                    }*/

                    //if (!s.IsCompositeSample && !s.HasLabReceiptDate)
                    //{
                    //    s.AddError("Required field: Lab receipt date is missing", true);
                    //    rejectSamplesSS.Add(s.LabSampleIdentifier);
                    //}
                    if (string.IsNullOrWhiteSpace(s.SampleLocationIdentifier))
                    {
                        s.AddError("Required field: Sampling location identifier is missing", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }
                    if (!s.IsCompositeSample && string.IsNullOrWhiteSpace(s.SampleLocationCollectionAddress))
                    {
                        s.AddError("Required field: Sample collection location is missing", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }
                    if (s.LabSampleIdentifier.Length > 20)
                    {
                        s.AddError("Invalid data: Lab sample ID is too long. Maximum allowed length: 20 characters", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }

                    if (s.SampleCollectionEndDate != null)
                    {
                        sampleCollectionDate = (DateTime)s.SampleCollectionEndDate;
                    }
                    if (s.SampleLaboratoryReceiptDate != null)
                    {
                        sampleLabReceiptDate = (DateTime)s.SampleLaboratoryReceiptDate;
                    }
                    testHoldingTime = TestHoldingTime(s);

                    if (s.HasCollectionDate && DateIsNotInAllowedRange(sampleCollectionDate))
                    {
                        s.AddError("Collection date (" + sampleCollectionDate + ") is not within the allowed date range", true);
                        s.SampleCollectionEndDate = null;
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }
                    if (s.HasLabReceiptDate && DateIsNotInAllowedRange(sampleLabReceiptDate))
                    {
                        s.AddError("Lab receipt date (" + sampleLabReceiptDate + ") is not within the allowed date range", true);
                        s.SampleLaboratoryReceiptDate = null;
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }
                    if (!s.IsCompositeSample && s.HasCollectionDate && s.HasLabReceiptDate && DateSecondIsPriorToDateFirst(sampleCollectionDate, sampleLabReceiptDate, true))
                    {
                        s.AddError("Lab receipt was performed prior to the collection date", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }
                    s.SetDataValidation();
                }
                catch (Exception e)
                {
                    Mailer m = new Mailer();
                    m.SendErrorMessage("An error ocurred while validating the following sample:\r\n" + s.ToString(), e, "Exception in MigrateValidateDseData");
                    s.AddError("Sample rejected due to a program error that occurred during validation. Contact system administrator for more information.", true);
                    rejectSamplesSS.Add(s.LabSampleIdentifier);
                    s.SetDataValidation();
                }

                int sampleId = -1;
                try
                {
                    sampleId = DbHelper.AddSample(s);
                }
                catch (Exception e)
                {
                    Mailer m = new Mailer();
                    m.SendErrorMessage("An error occurred while saving the following sample to the database:\r\n" + s.ToString(), e, "Exception in MigrateValidateDseData");
                    rejectSamplesSS.Add(s.LabSampleIdentifier);
                    continue;
                }

                foreach (Result r in s.AnalysisResults)
                {
                    try
                    {
                        r.SampleId = sampleId;
                        if (IsTcSample)
                        {
                            if (r.AnalyteCode.Trim() == "3100")
                            {
                                s.Has3100 = true;
                                if (r.MeasurementQualifier == null)
                                {
                                    s.AddError("No measurement qualifier (present/absent) provided for " + r.AnalyteCode, true);
                                    rejectSamplesSS.Add(s.LabSampleIdentifier);
                                }
                                else if (r.MeasurementQualifier.Trim().ToUpper() == "P" && s.SampleMonitoringTypeCode.Trim().ToUpper() == "RT")
                                {
                                    Needs3014 = true;
                                }
                            }
                            else if (r.AnalyteCode.Trim() == "3014")
                            {
                                s.Has3014 = true;
                            }
                        }
                        else
                        {
                            if (r.AnalyteCode.Trim() != "3100" && r.AnalyteCode.Trim() != "3013" && r.AnalyteCode.Trim() != "3014"
                                && (r.MeasurementValue == null || r.MeasurementValue == 0)
                                && (r.DetectionLimitMeasurementValue == null || r.DetectionLimitMeasurementValue == 0))
                            {
                                s.AddError("No reporting limit given for Sample Result " + r.AnalyteCode, true);
                                rejectSamplesSS.Add(s.LabSampleIdentifier);
                            }
                        }
                        //if(!r.IsPfosPfoa() && string.IsNullOrWhiteSpace(r.MethodIdentifier))
                        //{
                        //    s.AddError("No method code given for Sample Result " + r.AnalyteCode, true);
                        //    rejectSamplesSS.Add(s.LabSampleIdentifier);
                        //    r.MethodIdentifier = string.Empty;
                        //}
                        if (r.AnalyteCode == "3014" && r.MethodIdentifier.Trim().ToUpper() == "9223B-QT" && r.MeasurementQualifier.Trim().ToUpper() == "P" && s.StaffValidation == null)
                        {
                            s.AddError("FYI: This E. coli sample analyzed with 9223B-QT will need to have its result manually entered in SDWIS", true);
                            rejectSamplesSS.Add(s.LabSampleIdentifier);
                        }
                        //if (!r.hasAnalysisStartDate)
                        //{
                        //    s.AddError("Required field for Sample Result " + r.AnalyteCode + " Analysis start date is missing", true);
                        //    rejectSamplesSS.Add(s.LabSampleIdentifier);
                        //}
                        //if (!r.hasAnalysisStartTime)
                        //{
                        //    s.AddError("Required field for Sample Result " + r.AnalyteCode + " Analysis start time is missing", true);
                        //    rejectSamplesSS.Add(s.LabSampleIdentifier);
                        //}
                        /*if (!r.hasAnalysisEndDate)
                        {
                            r.AddError("Required field: Analysis end date is missing", true);
                            rejectSamples.Append(s.LabSampleIdentifier + "\r\n");
                        }
                        if (!r.hasAnalysisEndTime)
                        {
                            r.AddError("Required field: Analysis end time is missing", true);
                            rejectSamples.Append(s.LabSampleIdentifier + "\r\n");
                        }*/

                        if (r.hasAnalysisStartDate || r.hasAnalysisEndDate)
                        {
                            DateTime resultLabAnalysis = r.AnalysisDate;

                            if (DateIsNotInAllowedRange(resultLabAnalysis))
                            {
                                s.AddError("Lab analysis date (" + resultLabAnalysis + ") for Sample Result " + r.AnalyteCode + " is not within the allowed date range", true);
                                r.AnalysisStartDate = null;
                                r.AnalysisEndDate = null;
                                rejectSamplesSS.Add(s.LabSampleIdentifier);
                            }
                            if (!s.IsCompositeSample && s.HasLabReceiptDate && DateSecondIsPriorToDateFirst(sampleLabReceiptDate, resultLabAnalysis, true))
                            {
                                s.AddError("Analysis date for Sample Result " + r.AnalyteCode + " was performed prior to the lab receipt date", true);
                                rejectSamplesSS.Add(s.LabSampleIdentifier);
                            }
                            if (!s.IsCompositeSample && DateSecondIsPriorToDateFirst(sampleCollectionDate, resultLabAnalysis, false))
                            {
                                s.AddError("Analysis date for Sample Result " + r.AnalyteCode + " was performed prior to the collection date", true);
                                rejectSamplesSS.Add(s.LabSampleIdentifier);
                            }
                            if (testHoldingTime)
                            {
                                if (IsSampleTooOld(pwsid, sampleCollectionDate, resultLabAnalysis))
                                {
                                    s.AddError("Sample age for Sample Result " + r.AnalyteCode + " is beyond the allowed limit", true);
                                    rejectSamplesSS.Add(s.LabSampleIdentifier);
                                }
                            }
                            //RD: Modified  the code to relax for 40201 lab and SCRAD.
                            if (!r.IsPfosPfoa() && !IsLabSCRADOrDHEC(r.LabAccredidationIdentifier) &&  !string.IsNullOrWhiteSpace(r.MethodIdentifier) && !IsLabCertifiedForMethod(r.LabAccredidationIdentifier, r.MethodIdentifier, r.AnalyteCode, resultLabAnalysis))
                            {
                                s.AddError("Lab is not certified for this method (" + r.MethodIdentifier + ") and analyte (" + r.AnalyteCode + ") for the given analysis date", true);
                                rejectSamplesSS.Add(s.LabSampleIdentifier);
                            }
                        }
                        //RD: Modified the code to relax for 40201 lab and SCRAD.
                        else if (!r.IsPfosPfoa() &&  !IsLabSCRADOrDHEC(r.LabAccredidationIdentifier) && !string.IsNullOrWhiteSpace(r.MethodIdentifier) && !IsLabCertifiedForMethod(r.LabAccredidationIdentifier, r.MethodIdentifier, r.AnalyteCode))
                        {
                            s.AddError("Lab is not certified for this method (" + r.MethodIdentifier + ") and analyte (" + r.AnalyteCode + ").", true);
                            rejectSamplesSS.Add(s.LabSampleIdentifier);
                        }
                        r.SetDataValidation();
                    }
                    catch (Exception e)
                    {
                        Mailer m = new Mailer();
                        m.SendErrorMessage("An error ocurred while validating the following result:\r\n" + r.ToString(), e, "Exception in MigrateValidateDseData");
                        s.AddError("Sample rejected due to a program error that occurred during validation of sample result " + r.AnalyteCode + ". Contact system administrator for more information.", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                    }

                    try
                    {
                        DbHelper.AddResult(r);
                    }
                    catch (Exception e)
                    {
                        Mailer m = new Mailer();
                        m.SendErrorMessage("An error occurred while saving the following result to the database:\r\n" + r.ToString(), e, "Exception in MigrateValidateDseData");
                        s.AddError("Sample rejected due to a program error that occurred during validation of sample result " + r.AnalyteCode + ". Contact system administrator for more information.", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                        needOneMoreDatabaseUpdate = true;
                    }
                }

                //since these last two rejections don't automatically get saved to the database since we can't 
                //check for them until after the sample and all results are saved, so we'll check to see if any
                //of these errors apply, and if they do we'll do one last database update.
                try
                {
                    if (IsTcSample && s.Has3100 && Needs3014 && !s.Has3014)
                    {
                        s.AddError("Missing E. coli sample result: A positive total coliform result must also be submitted with an E. coli result", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                        needOneMoreDatabaseUpdate = true;
                    }
                    if (s.AnalysisResults.Length == 0)
                    {
                        s.AddError("Sample submitted with no analysis result", true);
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                        needOneMoreDatabaseUpdate = true;
                    }

                    if (!rejectSamplesSS.Contains(s.LabSampleIdentifier))
                    {
                        acceptedSamplesSS.Add(s.LabSampleIdentifier);
                    }
                    s.SetDataValidation();
                }
                catch (Exception e)
                {
                    Mailer m = new Mailer();
                    m.SendErrorMessage("An error ocurred while validating the following sample:\r\n" + s.ToString(), e, "Exception in MigrateValidateDseData");
                    s.AddError("Sample rejected due to a program error that occurred during validation. Contact system administrator for more information.", true);
                    rejectSamplesSS.Add(s.LabSampleIdentifier);
                    s.SetDataValidation();
                    needOneMoreDatabaseUpdate = true;
                }

                if (needOneMoreDatabaseUpdate)
                {
                    try
                    {
                        DbHelper.UpdateDatabase();
                    }
                    catch (Exception e)
                    {
                        Mailer m = new Mailer();
                        m.SendErrorMessage("An error occurred while saving the following sample to the database:\r\n" + s.ToString(), e, "Exception in MigrateValidateDseData");
                        rejectSamplesSS.Add(s.LabSampleIdentifier);
                        continue;
                    }
                }
            }
            StringBuilder rejected = new StringBuilder();
            foreach (string s in rejectSamplesSS)
            {
                rejected.Append(s + "\r\n");
            }
            StringBuilder accepted = new StringBuilder();
            foreach (string s in acceptedSamplesSS)
            {
                accepted.Append(s + "\r\n");
            }
            rejectSamples = rejected.ToString();
            acceptedSamples = accepted.ToString();
        }
        private static bool IsLabSCRADOrDHEC(string LabStateId)
        {
            return LabStateId == "40201" || LabStateId == "SCRAD";
        }
        private static bool IsLabCertifiedForMethod(string LabStateId, string MethodCode, string AnalyteCode)
        {
            return IsLabCertifiedForMethod(LabStateId, MethodCode, AnalyteCode, DateTime.Now);
        }

        private static bool IsLabCertifiedForMethod(string LabStateId, string MethodCode, string AnalyteCode, DateTime LabAnalysisDate)
        {
            return DbHelper.IsLabCertifiedForMethodDb(LabStateId, MethodCode, AnalyteCode, LabAnalysisDate);
        }

        private static bool TestHoldingTime(Sample s)
        {
            return s.SampleRuleCode == "TC";
        }

        private static bool DateIsNotInAllowedRange(DateTime A_strDate)
        {
            bool boolReturn = false;
            boolReturn = A_strDate.Date < Convert.ToDateTime(DATE_ALLOWED_MIN).Date || A_strDate.Date > DateTime.Today.Date;
            return boolReturn;
        }

        private static bool DateSecondIsPriorToDateFirst(DateTime A_strFirstDate, DateTime A_strSecondDate, bool disregardTime)
        {
            if (disregardTime)
            {
                A_strFirstDate = A_strFirstDate.Date;
                A_strSecondDate = A_strSecondDate.Date;
            }
            return A_strFirstDate > A_strSecondDate;
        }

        private static bool IsSampleTooOld(string pwsid, DateTime CollectionDate, DateTime AnalysisDate)
        {
            return false;
        }
    }
}
