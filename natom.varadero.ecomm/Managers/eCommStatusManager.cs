using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class eCommStatusManager
    {
        private static eCommStatusManager _instance = null;
        public static eCommStatusManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new eCommStatusManager();
                return _instance;
            }
        }

        private DbEcommerceContext _db = new DbEcommerceContext();

        public void RegisterStartUp()
        {
            var status = _db.eCommerceStatus.First();
            _db.Entry(status).State = System.Data.Entity.EntityState.Modified;
            status.LastStartUp = DateTime.Now;
            _db.SaveChanges();
        }

        public void RegisterStartedSync()
        {
            object statusUpdaterLock = new object();
            lock (statusUpdaterLock)
            {
                var status = this._db.eCommerceStatus.First();
                this._db.Entry(status).State = System.Data.Entity.EntityState.Modified;
                status.RunningSyncsCounter++;
                this._db.SaveChanges();
            }
        }

        public void RegisterFinishedSync()
        {
            object statusUpdaterLock = new object();
            lock (statusUpdaterLock)
            {
                var status = this._db.eCommerceStatus.First();
                this._db.Entry(status).State = System.Data.Entity.EntityState.Modified;
                status.RunningSyncsCounter--;
                this._db.SaveChanges();
            }
        }

        public bool IsRunnningSyncRoutine()
        {
            var status = this._db.eCommerceStatus.First();
            return status.RunningSyncsCounter > 0;
        }
    }
}