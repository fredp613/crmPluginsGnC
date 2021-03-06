// <copyright file="PostFundingCaseRiskAssessmentCreate.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2016 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>2/23/2016 4:38:40 PM</date>
// <summary>Implements the PostFundingCaseRiskAssessmentCreate Plugin.</summary>
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
    /// PostFundingCaseRiskAssessmentCreate Plugin.
    /// </summary>    
    public class PostFundingCaseRiskAssessmentCreate: IPlugin
    {
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

                if (entity.LogicalName != "gcbase_fundingcaseriskassessment")
                    return;

                FaultException ex1 = new FaultException();
                //  throw new InvalidPluginExecutionException("test", ex1);

                try
                {
                    //// Obtain the organization service reference.
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    
                    if (entity.Attributes.Contains("gcbase_risktemplate"))
                    {
                        if (!new RiskTemplate(null, service).generateRiskFactorsForTemplate(entity.GetAttributeValue<EntityReference>("gcbase_risktemplate"),entity.Id))
                        {
                            throw new InvalidPluginExecutionException("issue with plugin", ex1);
                        }

                        
                    }                   
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the Funding Case Risk Assessment plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Funding Case Risk Assessment Plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
