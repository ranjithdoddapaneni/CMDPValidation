using System;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using ValidationDataAccess.Models;
using ValidationDataAccess.Models.dsSdwisTableAdapters;
using System.Security.Principal;

namespace ValidationDataAccess
{
    public class DbHelper
    {

        private static ValidatorDb db = new ValidatorDb();
        private static dsSdwis SdwisDataset = new dsSdwis();
        
        public static bool IsProductionInstance()
        {
            return Properties.Settings.Default.UseProductionSettings;
        }

        public static bool CheckConnections()
        {
            using (var AddOnConnection = new SqlConnection(ValidatorDb.AddOnConString))
            {
                try
                {
                    AddOnConnection.Open();
                }
                catch (SqlException)
                {
                    return false;
                }
            }
            using (var SDWISConnection = new SqlConnection(ValidatorDb.SdwisConString))
            {
                try
                {
                    SDWISConnection.Open();
                }
                catch (SqlException)
                {
                    return false;
                }
            }
            return true;
        }

        private static string GetCurrentUser()
        {
            string currentUsername = WindowsIdentity.GetCurrent().Name.ToString();
            currentUsername = currentUsername.Replace(string.Format("{0}\\", Properties.Settings.Default.UserDomain.ToUpper()), string.Empty);
            currentUsername = currentUsername.Replace(string.Format("{0}\\", Properties.Settings.Default.UserDomain.ToLower()), string.Empty);
            return currentUsername;
        }

        public static string GetCurrentUserEmail()
        {
            USERINFOTableAdapter ta = new USERINFOTableAdapter();
            ta.Connection.ConnectionString = ValidatorDb.AddOnConString;
            ta.Fill(SdwisDataset.USERINFO, GetCurrentUser());
            string email = string.Empty;
            if(SdwisDataset.USERINFO.Rows.Count > 0)
            {
                email = SdwisDataset.USERINFO.Rows[0]["EMAIL"].ToString();
            }
            return email;
        }

        public static string GetEmailForLab(string labId)
        {
            TINLGCOMTableAdapter ta = new TINLGCOMTableAdapter();
            ta.Connection.ConnectionString = ValidatorDb.SdwisConString;
            ta.Fill(SdwisDataset.TINLGCOM, labId);
            string email = string.Empty;
            foreach(DataRow r in SdwisDataset.TINLGCOM.Rows)
            {
                email += !string.IsNullOrEmpty(email) ? r["ELECTRONIC_ADDRESS"] + ";" : r["ELECTRONIC_ADDRESS"];
            }
            return email;
        }

        public static bool IsLabCertifiedForMethodDb(string LabStateId, string MethodCode, string AnalyteCode, DateTime LabAnalysisDate)
        {
            CERTIFICATION_CHECKTableAdapter ta = new CERTIFICATION_CHECKTableAdapter();
            ta.Connection.ConnectionString = ValidatorDb.SdwisConString;
            ta.Fill(SdwisDataset.CERTIFICATION_CHECK, LabStateId, AnalyteCode, MethodCode, LabAnalysisDate);

            bool isCertified = false;
            foreach(DataRow r in SdwisDataset.CERTIFICATION_CHECK.Rows)
            {
                int certCount = Convert.ToInt32(r["CERT_COUNT"]);
                if (certCount > 0)
                    isCertified = true;
            }
            return isCertified;

            /*SqlConnection connection = new SqlConnection(ValidatorDb.SdwisConString);
            SqlCommand command = new SqlCommand("select COUNT(*) " +
                                                "from TSALAMA lc " +
                                                    "join TSALAB l on lc.TSALAB_IS_NUMBER = l.TSALAB_IS_NUMBER " +
                                                    "join TSAANLYT a on lc.SMAA_ANLYT_IS_NO = a.TSAANLYT_IS_NUMBER " +
                                                    "join TSASMN am on lc.SMAA_TSASMN_IS_NO = am.TSASMN_IS_NUMBER " +
                                                "where l.LAB_ID_NUMBER = @lab_state_id " +
                                                "AND a.CODE = @ANALYTE_CODE " +
                                                "AND am.CODE = @METHOD_CODE " +
                                                "AND(lc.BEGIN_DATE <= @ANALYSIS_DATE OR lc.BEGIN_DATE is null) " +
                                                "AND(lc.END_DATE >= @ANALYSIS_DATE OR lc.END_DATE is null) ");
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@LAB_STATE_ID", SqlDbType.VarChar, 20);
            command.Parameters.Add("@ANALYTE_CODE", SqlDbType.VarChar, 10);
            command.Parameters.Add("@METHOD_CODE", SqlDbType.VarChar, 20);
            command.Parameters.Add("@ANALYSIS_DATE", SqlDbType.DateTime);
            command.Parameters["@LAB_STATE_ID"].Value = LabStateId;
            command.Parameters["@ANALYTE_CODE"].Value = AnalyteCode;
            command.Parameters["@METHOD_CODE"].Value = MethodCode;
            command.Parameters["@ANALYSIS_DATE"].Value = LabAnalysisDate;
            command.Connection = connection;

            try
            {
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    int resultCount = Convert.ToInt32(result);
                    return resultCount > 0;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }*/
        }

        public static long getAllowedSampleAge(string pwsid)
        {
            SqlConnection connection = new SqlConnection(ValidatorDb.AddOnConString);
            SqlCommand command = new SqlCommand("SELECT HOURS FROM SAMPLE_AGE_HOURS WHERE PWSID = @PWSID");
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@PWSID", SqlDbType.VarChar, 50);
            command.Parameters["@PWSID"].Value = pwsid;
            command.Connection = connection;

            try
            {
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    return 30;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public static string GetPwsName(string pwsid)
        {
            SqlConnection connection = new SqlConnection(ValidatorDb.SdwisConString);
            SqlCommand command = new SqlCommand("select name from TINWSYS where number0 = @PWSID");
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@PWSID", SqlDbType.VarChar, 50);
            command.Parameters["@PWSID"].Value = pwsid;
            command.Connection = connection;

            try
            {
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return result.ToString();
                }
                else
                {
                    throw new ArgumentException("Invalid argument: GetPwsName for " + pwsid + " returned zero values");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public static string GetPwsFedType(string pwsid)
        {
            SqlConnection connection = new SqlConnection(ValidatorDb.SdwisConString);
            SqlCommand command = new SqlCommand("select D_PWS_FED_TYPE_CD from TINWSYS where number0 = @PWSID");
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@PWSID", SqlDbType.VarChar, 50);
            command.Parameters["@PWSID"].Value = pwsid;
            command.Connection = connection;

            try
            {
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return result.ToString();
                }
                else
                {
                    throw new ArgumentException("Invalid argument: GetPwsFedType for " + pwsid + " returned zero values");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public static string GetAnalyteName(string analyteCode)
        {
            SqlConnection connection = new SqlConnection(ValidatorDb.SdwisConString);
            SqlCommand command = new SqlCommand("select name from TSAANLYT where code = @analyteCode");
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@analyteCode", SqlDbType.VarChar, 4);
            command.Parameters["@analyteCode"].Value = analyteCode;
            command.Connection = connection;

            try
            {
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return result.ToString();
                }
                else
                {
                    throw new ArgumentException("Invalid argument: GetAnalyteName for " + analyteCode + " returned zero values");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public static void CheckForXmlSamplingRejections()
        {
            CheckForXmlSamplingRejections(DateTime.Today);
        }

        public static int CheckForXmlSamplingRejections(DateTime rejectionDate)
        {
            int newRejections = 0;
            try
            {
                MIGRATION_ERROR_REPORTTableAdapter ta = new MIGRATION_ERROR_REPORTTableAdapter();
                ta.Connection.ConnectionString = ValidatorDb.SdwisConString;
                ta.Fill(SdwisDataset.MIGRATION_ERROR_REPORT, rejectionDate);

                if (SdwisDataset.MIGRATION_ERROR_REPORT.Rows.Count > 0)
                {
                    newRejections = SdwisDataset.MIGRATION_ERROR_REPORT.Rows.Count;
                    foreach (DataRow errorReport in SdwisDataset.MIGRATION_ERROR_REPORT.Rows)
                    {
                        var labSampleNumber = errorReport["b_lab_sample_num"];
                        var pwsid = errorReport["b_pws_number"];
                        var labIdNumber = errorReport["b_lab_id_number"];
                        var analyteCode = errorReport["b_analyte_cd"];
                        var errorCode = errorReport["b_error_cd"];
                        var errorDescription = errorReport["b_error_desc"];

                        //TODO sometime XML Sampling has errors with no lab sample Id, and we should probably know about them,
                        //     but without that we have no way of connecting the error with the sample in our database
                        if (labSampleNumber == null || labSampleNumber == DBNull.Value || pwsid == null || pwsid == DBNull.Value
                            || errorDescription != null && errorDescription.ToString().Contains("FLAG"))
                        {
                            continue;
                        }
                        if (!string.IsNullOrWhiteSpace((string)labSampleNumber))
                        {
                            //if(analyteCode == null || string.IsNullOrWhiteSpace((string)analyteCode))
                            //{
                                //Sample
                                Sample sample = db.CmdpSamples
                                                .Where(s => s.PWSIdentifier == (string)pwsid && s.LabSampleIdentifier == (string)labSampleNumber)
                                                .OrderByDescending(s => s.XmlCompilationDate)
                                                .FirstOrDefault();
                                if(sample != null)
                                {
                                    sample.AddError(errorDescription.ToString(), false, true);
                                    sample.XmlSamplingValidationDate = DateTime.Now;
                                    db.SaveChanges();
                                }
                            /*}
                            else
                            {
                                //Sample Result
                                Result sampleResult = db.CmdpResults
                                                .Where(r => r.PWSIdentifier == (string)pwsid 
                                                    && r.LabSampleIdentifier == (string)labSampleNumber
                                                    && r.AnalyteCode == (string)analyteCode)
                                                .OrderByDescending(r => r.XmlCompilationDate)
                                                .FirstOrDefault();
                                if (sampleResult != null)
                                {
                                    sampleResult.AddError(errorDescription.ToString(), false, true);
                                    sampleResult.XmlSamplingValidationDate = DateTime.Now;
                                    db.SaveChanges();
                                }
                            }*/
                        }
                    }
                }
            }
            catch(Exception e)
            {
                //TODO something more meaningful here
                throw;
            }
            return newRejections;
        }

        public static void UpdateDatabase()
        {
            db.SaveChanges();
        }

        /// <returns>The database id of the newly added sample</returns>
        public static int AddSample(Sample s)
        {
            try
            {
                if (s.Id == 0)
                    db.CmdpSamples.Add(s);
                db.SaveChanges();
                return s.Id;
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("An error occurred while saving sample #{0} in job #{1}. All samples after this sample in the job file were not migrated, but all samples appearing before it in the job file likely were migrated.",
                        s.LabSampleIdentifier,
                        s.cmdpJobId),
                    ex);
            }
        }

        public static bool SetStaffValidation(ISamplingObject item, bool validationResult)
        {
            if(item is Sample)
            {
                return SetSampleStaffValidation((Sample)item, validationResult);
            }
            else if(item is Result)
            {
                return SetResultStaffValidation((Result)item, validationResult);
            }
            else
            {
                return false;
            }
        }

        public static bool SetSampleStaffValidation(int sampleId, bool validationResult)
        {
            try
            {
                var sample = db.CmdpSamples.SingleOrDefault(s => s.Id == sampleId);
                return SetSampleStaffValidation(sample, validationResult);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates staff validation flag on sample object.
        /// </summary>
        /// <param name="sample">sample to update</param>
        /// <param name="validationResult">Value to set staff validation flag to</param>
        /// <param name="dataValidation">Set to true if staff is validating a data validation, false is validating a XML sampling validation</param>
        /// <returns></returns>
        private static bool SetSampleStaffValidation(Sample sample, bool validationResult)
        {
            try
            {
                if (sample != null)
                {
                    sample.StaffValidation = validationResult;
                    sample.StaffValidationDate = DateTime.Now;
                    sample.StaffValidator = DbHelper.GetCurrentUser();
                    if(validationResult == true)
                    {
                        if (sample.DataValidation == false)
                            sample.DataValidation = null;
                        else
                            sample.XmlSamplingValidation = null;
                    }
                    //PopulateResultsForSample(ref sample, false);
                    //foreach(Result r in sample.AnalysisResults)
                    //{
                    //    SetResultStaffValidation(r, validationResult);
                    //}
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SetResultStaffValidation(int resultId, bool validationResult)
        {
            try
            {
                var result = db.CmdpResults.SingleOrDefault(r => r.Id == resultId);
                return SetResultStaffValidation(result, validationResult);
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates staff validation flag on result object.
        /// </summary>
        /// <param name="resultId">Database ID of result to update</param>
        /// <param name="validationResult">Value to set staff validation flag to</param>
        /// <returns></returns>
        private static bool SetResultStaffValidation(Result result, bool validationResult)
        {
            try
            {
                if (result != null)
                {
                    result.StaffValidation = validationResult;
                    result.StaffValidationDate = DateTime.Now;
                    result.StaffValidator = DbHelper.GetCurrentUser();
                    if (validationResult == true)
                    {
                        if (result.DataValidation == false)
                            result.DataValidation = null;
                        else
                            result.XmlSamplingValidation = null;
                    }
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <param name="r">The database id of the newly added result</param>
        public static int AddResult(Result r)
        {
            if (r.Id == 0)
                db.CmdpResults.Add(r);
            db.SaveChanges();
            return r.Id;
        }

        public static ICollection<Sample> GetDataValidationErrorSamples()
        {
            var samples = db.CmdpSamples.Where(s => s.DataValidation == false && (s.StaffValidation == null || s.StaffValidation == true)).ToList<Sample>();
            //PopulateResultsForSample(ref samples, false);
            return samples;
        }

        public static ICollection<Sample> GetDataValidationErrorSamples(string pwsid)
        {
            var samples = db.CmdpSamples.Where(s => s.PWSIdentifier.ToUpper() == pwsid.ToUpper() && s.DataValidation == false && (s.StaffValidation == null || s.StaffValidation == true)).ToList<Sample>();
            //PopulateResultsForSample(ref samples, false);
            return samples;
        }

        public static ICollection<Result> GetDataValidationErrorResults()
        {
            return db.CmdpResults.Where(r => r.DataValidation == false && (r.StaffValidation == null || r.StaffValidation == true)).ToList<Result>();
        }

        public static ICollection<ISamplingObject> GetDataValidationErrors()
        {
            return GetDataValidationErrors(null);
        }

        public static ICollection<ISamplingObject> GetDataValidationErrors(string pwsid)
        {
            List<ISamplingObject> validationErrors = new List<ISamplingObject>();
            ICollection<Sample> samples;
            if (pwsid == null)
                samples = GetDataValidationErrorSamples();
            else
                samples = GetDataValidationErrorSamples(pwsid);
            //var results = GetDataValidationErrorResults();
            validationErrors.AddRange(samples);
            //validationErrors.AddRange(results);
            return validationErrors;
        }

        public static ICollection<WaterSystem> GetValidationErrorWaterSystems()
        {
            List<ISamplingObject> validationErrors = (List<ISamplingObject>)GetDataValidationErrors();
            List<ISamplingObject> xmlErrors = (List<ISamplingObject>)GetXmlSamplingErrors();
            SortedSet<WaterSystem> waterSystems = new SortedSet<WaterSystem>();
            try
            {
                foreach(var sam in validationErrors)
                {
                    waterSystems.Add(new WaterSystem(sam.PWSIdentifier));
                }
                foreach(var samp in xmlErrors)
                {
                    waterSystems.Add(new WaterSystem(samp.PWSIdentifier));
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return waterSystems.ToList<WaterSystem>();
        }

        public static ICollection<Sample> GetXmlSamplingErrorSamples()
        {
            var samples = db.CmdpSamples.Where(s => s.XmlSamplingValidation == false && (s.StaffValidation == null || s.StaffValidation == true)).ToList<Sample>();
            PopulateResultsForSample(ref samples, false);
            return samples;
        }

        public static ICollection<Sample> GetXmlSamplingErrorSamples(string pwsid)
        {
            var samples = db.CmdpSamples.Where(s => s.PWSIdentifier.ToUpper() == pwsid.ToUpper() && s.XmlSamplingValidation == false && (s.StaffValidation == null || s.StaffValidation == true)).ToList<Sample>();
            PopulateResultsForSample(ref samples, false);
            return samples;
        }

        public static ICollection<Result> GetXmlSamplingErrorResults()
        {
            return db.CmdpResults.Where(r => r.XmlSamplingValidation == false && (r.StaffValidation == null || r.StaffValidation == true)).ToList<Result>();
        }
        public static ICollection<ISamplingObject> GetXmlSamplingErrors()
        {
            return GetXmlSamplingErrors(null);
        }

        public static ICollection<ISamplingObject> GetXmlSamplingErrors(string pwsid)
        {
            List<ISamplingObject> validationErrors = new List<ISamplingObject>();
            ICollection<Sample> samples;
            if (pwsid == null)
                samples = GetXmlSamplingErrorSamples();
            else
                samples = GetXmlSamplingErrorSamples(pwsid);
            //var results = GetXmlSamplingErrorResults();
            validationErrors.AddRange(samples);
            //validationErrors.AddRange(results);
            return validationErrors;
        }

        public static ICollection<Sample> GetCorrectedDataValidationErrors()
        {
            var samples = db.CmdpSamples.Where(s => s.DataValidation == null && s.StaffValidation == true).ToList<Sample>();
            PopulateResultsForSample(ref samples, true);
            /*var results = db.CmdpResults.Where(r => r.DataValidation == null && r.StaffValidation == true).ToList<Result>();
            samples.AddRange(GetSamplesForResults(results, true));*/
            return samples;
        }

        public static List<Sample> GetSamplesForResults(List<Result> results, bool hideRejected)
        {
            List<Sample> samples = new List<Sample>();
            Dictionary<int, Sample> resultParentSamples = new Dictionary<int, Sample>();
            foreach (Result r in results)
            {
                Sample resultParentSample = GetParentSampleOfResult(r);
                if ((resultParentSample.DataValidation == true 
                        && (resultParentSample.XmlSamplingValidation == true || resultParentSample.XmlSamplingValidation == null) 
                        && hideRejected)
                    || !hideRejected)
                {
                    if (resultParentSamples.ContainsKey(r.SampleId))
                    {
                        resultParentSamples[r.SampleId].AddAnalysisResult(r);
                    }
                    else
                    {
                        resultParentSample.AddAnalysisResult(r);
                        resultParentSamples.Add(resultParentSample.Id, resultParentSample);
                    }
                }
            }
            samples.AddRange(resultParentSamples.Values);
            return samples;
        }

        public static void PopulateResultsForSample(ref List<Sample> samples, bool excludeRejections)
        {
            foreach (Sample s in samples)
            {
                if (!s.HasResults)
                {
                    s.AddAnalysisResult(GetAllResultsForSample(s.Id, excludeRejections));
                }
            }
        }

        public static void PopulateResultsForSample(ref Sample sample, bool excludeRejections)
        {
            if (!sample.HasResults)
            {
                sample.AddAnalysisResult(GetAllResultsForSample(sample.Id, excludeRejections));
            }
        }

        public static ICollection<Result> GetAllResultsForSample(int sampleId, bool excludeRejections)
        {
            if (excludeRejections)
            {
                return db.CmdpResults.Where(r => r.SampleId == sampleId 
                    && (r.DataValidation == null || r.DataValidation == true) 
                    && (r.XmlSamplingValidation == null || r.XmlSamplingValidation == true)).ToList<Result>();
            }
            else
            {
                return db.CmdpResults.Where(r => r.SampleId == sampleId).ToList<Result>();
            }
        }

        public static Sample GetParentSampleOfResult(Result child)
        {
             return db.CmdpSamples.SingleOrDefault(s => s.Id == child.SampleId);
        }

        public static ICollection<Sample> GetSamplesForXmlSampling(bool setXmlCompilationDate)
        {
            var samples = db.CmdpSamples.Where(s => s.DataValidation == true && s.XmlSamplingValidation == null && (s.XmlCompilationDate == null || (s.XmlCompilationDate != null && s.StaffValidation == true))).ToList<Sample>();
            //var results = db.CmdpResults.Where(r => r.DataValidation == true && ((r.XmlCompilationDate == null && r.XmlSamplingValidation == null) || (r.XmlSamplingValidation == false && r.StaffValidation == true))).ToList<Result>();
            //var resultsWithParents = GetSamplesForResults(results, true);
            /*foreach(var ps in resultsWithParents)
            {
                bool alreadyThere = false;
                foreach(var s in samples)
                {
                    if(s.Id == ps.Id)
                    {
                        alreadyThere = true;
                        break;
                    }
                }
                if(!alreadyThere)
                {
                    samples.Add(ps);
                }
            }*/
            if (setXmlCompilationDate)
            {
                DateTime xmlCompilationDate = DateTime.Now;
                foreach (Sample s in samples)
                {
                    s.XmlCompilationDate = xmlCompilationDate;
                    
                    if(s.AnalysisResults == null || s.AnalysisResults.Count() == 0)
                        s.AddAnalysisResult(GetAllResultsForSample(s.Id, true));
                    foreach(Result r in s.AnalysisResults)
                    {
                        r.XmlCompilationDate = xmlCompilationDate;
                    }
                }
                db.SaveChanges();
            }
            return samples; //db.CmdpSamples.Where(s => s.DataValidation == true && (s.XmlSamplingValidation == null || (s.XmlSamplingValidation == false && s.StaffValidation == true))).ToList<Sample>();
        }
    }
}
