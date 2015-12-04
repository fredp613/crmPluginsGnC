// <copyright file="PreFundCentreRetrieve.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>10/28/2015 4:27:44 PM</date>
// <summary>Implements the PreFundCentreRetrieve Plugin.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>
namespace CrmGC.Plugins
{
  //  using System;
   // using System.ServiceModel;
   // using Microsoft.Xrm.Sdk;

    /// <summary>
    /// PreFundCentreRetrieve Plugin.
    /// </summary>    
   // public class PreFundCentreRetrieve: Plugin
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Sdk.Discovery;
    using Microsoft.Xrm.Sdk.Messages;
    using System.Collections.Generic;
    using System.Linq;

    public class PreFundCentreRetrieve : IPlugin
    {
        /// <summary>
        /// A plug-in that creates a follow-up task activity when a new account is created.
        /// </summary>
        /// <remarks>Register this plug-in on the Create message, account entity,
        /// and asynchronous mode.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            OrganizationServiceContext ctx = new OrganizationServiceContext(service);
            // The InputParameters collection contains all the data passed in the message request.
            FaultException ex1 = new FaultException();
            if (context.Depth == 1) {
               
                // Obtain the target entity from the input parameters.
                //Entity entity1 = (Entity)context.InputParameters["Target"];

                EntityReference entity = (EntityReference)context.InputParameters["Target"];
                //throw new InvalidPluginExecutionException(entity.LogicalName.ToString(), ex1);
                if (entity.LogicalName.ToString() != "gcbase_fundcentre")
                    return;
               
                ColumnSet cols = new ColumnSet(
                                     new String[] {"gcbase_name", "statuscode"});

                var fundCentre = service.Retrieve("gcbase_fundcentre", entity.Id, cols);
                    try
                    {
                      //  throw new InvalidPluginExecutionException("test2", ex1);
                        if (fundCentre != null)
                        {
                            var statuscode = fundCentre.GetAttributeValue<OptionSetValue>("statuscode").Value;
                           // throw new InvalidPluginExecutionException(fundCentre.GetAttributeValue<OptionSetValue>("statuscode").Value.ToString(), ex1);
                            if (statuscode.ToString() != "1413213")
                            {
                                //entity.Attributes.Add("address1_name", "first time value: " + rndgen.Next().ToString());
                               // throw new InvalidPluginExecutionException("test", ex1);
                                RetrieveEntityRequest request = new RetrieveEntityRequest();
                                request.LogicalName = "gcbase_fundcentre";

                                RetrieveEntityResponse response = (RetrieveEntityResponse)service.Execute(request);
                                int objecttypecode = response.EntityMetadata.ObjectTypeCode.Value;
                                var query = new QueryExpression("userentityuisettings");
                                query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, context.UserId);
                                query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, objecttypecode);
                                query.ColumnSet = new ColumnSet("lastviewedformxml");

                                var settings = service.RetrieveMultiple(query).Entities;
                                var setting = settings[0];
                                //foreach (Entity ent in settings)
                                //{
                                //    foreach (KeyValuePair<String, Object> attribute in ent.Attributes)
                                //    {
                                //      //  Console.WriteLine(attribute.Key + ": " + attribute.Value);
                                //        throw new InvalidPluginExecutionException(attribute.Key.ToString(), ex1);
                                //       // tracingService.Trace("key: {0}, value: {1}", attribute.Key.ToString(), attribute.Value.ToString());
                                //    }
                                //} 
                               
                            }
                            else
                            {
                                //contact["address1_name"] = "i already exist";
                                //throw new InvalidPluginExecutionException("test1", ex1);
                            }
                           // service.Update(contact);
                        }

                    }

                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        throw new InvalidPluginExecutionException("An error occurred in the IOS Setup plug-in.", ex);
                    }

                    catch (Exception ex)
                    {
                        tracingService.Trace("FollowupPlugin: {0}", ex.ToString());
                        throw;
                    }

            }
        }
    }
}
