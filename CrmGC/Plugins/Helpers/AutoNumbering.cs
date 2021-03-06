﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace EgcsCommon
{
    public class AutoNumbering
    {

        public Entity entity;
        public IOrganizationService service;

        public AutoNumbering(Entity ent, IOrganizationService serv)
        {
            entity = ent;
            service = serv;

        }
        public string getAutoNumber()
        {

            String autoNumber = "";
            if (entity.LogicalName == "gcbase_fundingcase")
            {
                var fundEntity = service.Retrieve("gcbase_fundcentre", entity.GetAttributeValue<EntityReference>("gcbase_program").Id, new ColumnSet("gcbase_sapfundcentre"));
                var sapEntity = service.Retrieve("gcbase_sapfundcentre", fundEntity.GetAttributeValue<EntityReference>("gcbase_sapfundcentre").Id, new ColumnSet("gcbase_name"));
                autoNumber = generateAutoNumberGUIDpartial(sapEntity.GetAttributeValue<String>("gcbase_name").ToString());
            }
            if (entity.LogicalName == "gcbase_client")
            {
                autoNumber = generateAutoNumber("", getLastNumberFromEntity());
            }
            return autoNumber;
        }

        public string generateAutoNumberGUIDpartial(string prefix)
        {
            var guid = Guid.NewGuid();
            string finalStr;
            if (prefix != null)
            {
                finalStr = prefix + "-" + guid.ToString().Substring(0, 7);
            }
            else
            {
                finalStr = guid.ToString().Substring(0, 7);
            }
            return finalStr.ToUpper();
        }

        //this needs refactoring
        private string getLastNumberFromEntity()
        {


            string entityName = entity.LogicalName;
            QueryExpression lastAutoNumberOfEntity = new QueryExpression
            {
                EntityName = entityName,
                ColumnSet = new ColumnSet("gcbase_name"),
            };
            lastAutoNumberOfEntity.Orders.Add(new OrderExpression("gcbase_name", OrderType.Descending));
            var test = service.RetrieveMultiple(lastAutoNumberOfEntity).Entities.FirstOrDefault();
            if (test != null)
            {
                return test.GetAttributeValue<String>("gcbase_name");
            }
            return "10000";
        }

        //needs to be refactored
        private string generateAutoNumber(string prefix, string lastNumber)
        {
            // String autoNumber = "";
            long numberForAppending = 0;
            String lastNumberWithoutPrefix = "";
            if (prefix != "")
            {
                var indexEndOfPrefix = lastNumber.IndexOf("-");
                lastNumberWithoutPrefix = lastNumber.Substring(indexEndOfPrefix + 1);
            }
            else
            {
                lastNumberWithoutPrefix = lastNumber;
            }

            if (lastNumber != "10000")
            {
                if (long.TryParse(lastNumberWithoutPrefix, out numberForAppending))
                {
                    if (prefix != "")
                    {
                        return prefix + "-" + (numberForAppending + 1).ToString();
                    }
                    return (numberForAppending + 1).ToString();
                }
            }

            //first instance of the record
            if (prefix != "")
            {
                return prefix + "-" + "10001";
            }
            return "10001";

        }

    }
}



