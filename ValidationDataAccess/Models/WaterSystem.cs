using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationDataAccess.Models
{
    public class WaterSystem : IComparable<WaterSystem>
    {
        private bool _sortOnPwsid;
        private bool _sortOnPwsName;
        public string Pwsid { get; set; }
        public string PwsName { get; set; }

        public WaterSystem(string Pwsid)
        {
            InitValues();
            this.Pwsid = Pwsid;
            this.PwsName = DbHelper.GetPwsName(Pwsid);
        }

        private void InitValues()
        {
            _sortOnPwsid = false;
            _sortOnPwsName = true;
        }

        public void SortOnPwsid()
        {
            _sortOnPwsid = true;
            _sortOnPwsName = false;
        }

        public void SortOnPwsName()
        {
            _sortOnPwsName = true;
            _sortOnPwsid = false;
        }

        public int CompareTo(WaterSystem that)
        {
            if (_sortOnPwsName)
                return this.PwsName.ToUpper().CompareTo(that.PwsName.ToUpper());
            else
                return this.Pwsid.ToUpper().CompareTo(that.Pwsid.ToUpper());
        }

        public override bool Equals(object obj)
        {
            if (obj is WaterSystem)
                return this.Pwsid.ToUpper().Equals(((WaterSystem)obj).Pwsid.ToUpper());
            else
                return base.Equals(obj);
        }
    }
}
