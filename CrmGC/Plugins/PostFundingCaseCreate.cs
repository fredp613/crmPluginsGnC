// <copyright file="PostFundingCaseCreate.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>9/3/2015 2:16:50 PM</date>
// <summary>Implements the PostFundingCaseCreate Plugin.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>
namespace CrmGC.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Sdk.Discovery;
    using Microsoft.Xrm.Sdk.Messages;
    using System.Collections.Generic;
    using System.Linq;
    using EgcsCommon;


    /// <summary>
    /// PostFundingCaseCreate Plugin.
    /// </summary>    
    public class PostFundingCaseCreate: IPlugin
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

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {

                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName != "gcbase_fundingcase")
                    return;

                FaultException ex1 = new FaultException();
              //  throw new InvalidPluginExecutionException("test", ex1);

                try
                {
                     //// Obtain the organization service reference.
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    AutoNumbering autoNumbering = new AutoNumbering(entity, service);
                    String caseNumber = autoNumbering.getAutoNumber();

                    entity["gcbase_name"] = caseNumber;
                    service.Update(entity);
                    if (entity.Attributes.Contains("gcbase_amountsbyfiscalyearserver"))
                    {

                        char[] delimitedChar = { ';' };
                        
                        Entity fundingAmountByFY = new Entity("gcbase_fundingcaseamountbyfy");
                        string[] yearlyAmounts = entity.Attributes["gcbase_amountsbyfiscalyearserver"].ToString().Split(delimitedChar);
                        EntityReference fundCentre = entity.GetAttributeValue<EntityReference>("gcbase_program");
                        foreach (string ya in yearlyAmounts)
                        {

                            fundingAmountByFY["gcbase_fundingcase"] = new EntityReference("gcbase_fundingcase", entity.Id);
                            //fys[index] = (string)Enum.GetName(typeof(goal_fiscalyear), year);
                            OptionSetValue fy = new OptionSetValue();
                            var indexForFY = ya.IndexOf("Y");
                            var indexForAmount = ya.IndexOf("-");
                            var yearStr = ya.Substring(indexForFY + 1,4);
                            var amountStr = ya.Substring(indexForAmount + 1);
                            tracingService.Trace("year is:" + yearStr);
                            tracingService.Trace("amount is:" + amountStr);
                            Money amount = new Money(decimal.Parse(amountStr));
                            fy.Value = Int32.Parse(yearStr);
                            fundingAmountByFY["gcbase_fiscalyear"] = fy;
                            fundingAmountByFY["gcbase_amount"] = amount;
                            fundingAmountByFY["gcbase_fundcentre"] = fundCentre; 
                            tracingService.Trace("PostFundingCasePlugin: Creating the budget item.");
                            service.Create(fundingAmountByFY);                           
                            tracingService.Trace(ya);
                        }                    
                    }
                    if (entity.Attributes.Contains("gcbase_program"))
                    {
                        ColumnSet cols = new ColumnSet
                        (
                                new String[] {"gcbase_fundingcaseriskrequired"}
                        );

                        //program configuration options
                        var program = service.Retrieve("gcbase_fundcentre", entity.GetAttributeValue<EntityReference>("gcbase_program").Id, cols);
                        tracingService.Trace("there");
                        if (program.GetAttributeValue<Boolean>("gcbase_fundingcaseriskrequired"))
                        {
                            var ratype = new OptionSetHelper().getIndexOfLabel("gcbase_fundingcaseriskassessment", "gcbase_fundingcaseriskassessmenttype", "Initial", service);
                            OptionSetValue raTypeOpt = new OptionSetValue(ratype);
                           
                            //create initial risk assessment - should be custom class since this will be reused by other plugins
                            if (!new RiskTemplate(null, service).generateRiskAssessment(entity, raTypeOpt))
                            {
                                throw new InvalidPluginExecutionException("The funding program is not fully configured therefore a funding case cannot be created yet", ex1);
                            }
                            
                            //newRA.generateAssessment();
                        }


                        //var results = service.Retrieve("gcbase_fundcentre", entity.GetAttributeValue<Guid>("gcbase_program"), null);

                    }               
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the FollowupPlugin plug-in.", ex);
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
