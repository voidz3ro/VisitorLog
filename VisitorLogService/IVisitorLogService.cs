using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace VisitorLogService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IVisitorLogService" in both code and config file together.
    [ServiceContract]
    public interface IVisitorLogService
    {
        [OperationContract]
        string RecordAction(string naction, string ncompany, string ncountry, string ncity, string nsublocation, bool prescreened, string entity, DateTime datetime);

        [OperationContract]
        List<VisitLog> GetLogAll();

    }

    

   
}
