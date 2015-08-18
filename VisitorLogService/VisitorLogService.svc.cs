using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace VisitorLogService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "VisitorLogService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select VisitorLogService.svc or VisitorLogService.svc.cs at the Solution Explorer and start debugging.
    public class VisitorLogService : IVisitorLogService
    {
        private VisitorLogDataClassesDataContext db = new VisitorLogDataClassesDataContext();
        public string RecordAction(string naction, string ncompany, string ncountry, string ncity, string nsublocation, bool prescreened, string entity, DateTime datetime)
        {
            VisitLog vl = new VisitLog();
            vl.Action = naction;
            vl.Company = ncompany;
            vl.Country= ncountry;
            vl.City= ncity;
            vl.Sublocation = nsublocation; 
            vl.datestamp = datetime;
            vl.Prescreened = prescreened;
            vl.Entity = entity;

            try
            {
                db.VisitLogs.InsertOnSubmit(vl);
                db.SubmitChanges();
                return "Success";
            }
            catch (Exception e)
            {
                return "Failure: " + e.Message;
            }
        }

        

        public List<VisitLog> GetLogAll() {
            string startDate = null; string endDate;
            List<VisitLog> visitList = new List<VisitLog>();
           
            var qryVisitLog = from actions in db.VisitLogs select actions;
            //if(startDate!=null){
            //    qryVisitLog = from actions in qryVisitLog where actions.datestamp > startDate select actions;
            //}

            //if (endDate != null)
            //{
            //    qryVisitLog = from actions in qryVisitLog where actions.datestamp < endDate select actions;
            //}

            foreach (VisitLog action in qryVisitLog)
            {
                visitList.Add(action);
            }

            return visitList;
        }
    }
}
