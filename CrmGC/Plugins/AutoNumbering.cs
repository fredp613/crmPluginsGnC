using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmGC.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;

    class AutoNumbering
    {

        public Entity entity;       
        public IOrganizationService service;

        public AutoNumbering(Entity ent, IOrganizationService serv)
        {
            entity = ent;
            service = serv;
            
        }
        public string getAutoNumber() {
  
            String autoNumber = "";
            if (entity.LogicalName == "gcbase_fundingcase") {
                var fundEntity = service.Retrieve("gcbase_fundcentre", entity.GetAttributeValue<EntityReference>("gcbase_program").Id, new ColumnSet("gcbase_sapfundcentre"));
                var sapEntity = service.Retrieve("gcbase_sapfundcentre", fundEntity.GetAttributeValue<EntityReference>("gcbase_sapfundcentre").Id, new ColumnSet("gcbase_name"));
                autoNumber = generateAutoNumber(sapEntity.GetAttributeValue<String>("gcbase_name").ToString(), getLastNumberFromEntity());
            }
            if (entity.LogicalName == "gcbase_client") {
                autoNumber = generateAutoNumber("", getLastNumberFromEntity());
            }
            return autoNumber;
        }

        private string getLastNumberFromEntity() {
            string entityName = entity.LogicalName;
            QueryExpression lastAutoNumberOfEntity = new QueryExpression
            {
                EntityName = entityName,
                ColumnSet = new ColumnSet("gcbase_name"),                                             
            };
            lastAutoNumberOfEntity.Orders.Add(new OrderExpression("gcbase_name", OrderType.Descending));
            lastAutoNumberOfEntity.PageInfo.Count = 1;
            lastAutoNumberOfEntity.PageInfo.PageNumber = 1;
            var test = service.RetrieveMultiple(lastAutoNumberOfEntity).Entities.FirstOrDefault();
            if (test != null) {
                return test.GetAttributeValue<String>("gcbase_name");
            }
            return "10000";
        }

        private string generateAutoNumber(string prefix, string lastNumber) {           
           // String autoNumber = "";
            long numberForAppending = 0;
            String lastNumberWithoutPrefix = "";
            if (prefix != "") {
                var indexEndOfPrefix = lastNumber.IndexOf("-");
                lastNumberWithoutPrefix = lastNumber.Substring(indexEndOfPrefix + 1);
            } else {
                lastNumberWithoutPrefix = lastNumber;
            }
            
            //string test = lastNumberWithoutPrefix.TrimStart('0');
            //int lastNumberLen = test.Length;
            //FaultException ex1 = new faultexception();
            //throw new invalidPluginExecutionException(test, ex1);
            //if (lastNumber != "00000") {
            
            // if (long.TryParse(test, out numberForAppending)) {
                
            //        switch (lastNumberLen)
            //        {
            //            case 1:
            //                autoNumber = "0000" + (numberForAppending + 1);
            //                break;
            //            case 2:
            //                autoNumber = "000" + (numberForAppending + 1);;
            //                break;
            //            case 3:
            //                autoNumber = "00" + (numberForAppending + 1);;
            //                break;
            //            case 4:
            //                autoNumber = "0" + (numberForAppending + 1);;
            //                break;
            //            case 5:
            //                autoNumber = (numberForAppending + 1).ToString();
            //                break;
            //            default:
            //                //autoNumber = numberForAppending + 1;
            //                break;
            //        }
            //    }

            if (lastNumber != "10000") {
                if (long.TryParse(lastNumberWithoutPrefix, out numberForAppending))
                { 
                    if (prefix != "") {
                        return prefix + "-" + (numberForAppending + 1).ToString();
                    }
                    return (numberForAppending + 1).ToString();
                }
            }
                        
            //first instance of the record
            if (prefix != "") {
                return prefix + "-" + "10001";
            }
            return "10001";
            
        }
        
    }
}
