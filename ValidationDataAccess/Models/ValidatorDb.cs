using System.Data.Entity;

namespace ValidationDataAccess.Models
{
    public class ValidatorDb : DbContext
    {
        public ValidatorDb() : base(AddOnConString)
        {

        }
        public DbSet<Sample> CmdpSamples { get; set; }
        public DbSet<Result> CmdpResults { get; set; }

        public static string SdwisConString
        {
            get
            {
                if (DbHelper.IsProductionInstance())
                {
                    return Properties.Settings.Default.SdwisDbProd;
                }
                else
                {
                    return Properties.Settings.Default.SdwisDbConnectionString;
                }
            }
        }
        public static string AddOnConString
        {
            get
            {
                if (DbHelper.IsProductionInstance())
                {
                    return Properties.Settings.Default.SdwisAddOnProd;
                }
                else
                {
                    return Properties.Settings.Default.SdwisAddOnConnectionString;
                }
            }
        }
    }
}
