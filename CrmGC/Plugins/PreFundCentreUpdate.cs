// <copyright file="PreFundCentreUpdate.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>8/6/2015 3:23:19 PM</date>
// <summary>Implements the PreFundCentreUpdate Plugin.</summary>
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
    /// PreFundCentreUpdate Plugin.
    /// Fires when the following attributes are updated:
    /// All Attributes
    /// </summary> 
    /// 
    public struct FundCentreParamaters 
    {
        public DateTime? startdate;
        public DateTime? enddate;
        public Money amount;
        public Guid id;
        public string name;
    }

   

    public class PreFundCentreUpdate: Plugin
    {

    
        /// <summary>
        /// Initializes a new instance of the <see cref="PreFundCentreUpdate"/> class.
        /// </summary>
        public PreFundCentreUpdate()
            : base(typeof(PreFundCentreUpdate))
        {
            base.RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(20, "Update", "gcbase_fundcentre", new Action<LocalPluginContext>(ExecutePreFundCentreUpdate)));

            // Note : you can register for more events here if this plugin is not specific to an individual entity and message combination.
            // You may also need to update your RegisterFile.crmregister plug-in registration file to reflect any change.
        }

        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="localContext">The <see cref="LocalPluginContext"/> which contains the
        /// <see cref="IPluginExecutionContext"/>,
        /// <see cref="IOrganizationService"/>
        /// and <see cref="ITracingService"/>
        /// </param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics CRM caches plug-in instances.
        /// The plug-in's Execute method should be written to be stateless as the constructor
        /// is not called for every invocation of the plug-in. Also, multiple system threads
        /// could execute the plug-in at the same time. All per invocation state information
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        protected void ExecutePreFundCentreUpdate(LocalPluginContext localContext)
        //protected void ExecutePreFundCentreUpdate(IServiceProvider serviceProvider)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException("localContext");
            }

           

            // TODO: Implement your custom Plug-in business logic.
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = localContext.PluginExecutionContext;
            IOrganizationService service = localContext.OrganizationService;

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
            context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parmameters.
                Entity entity = (Entity)context.InputParameters["Target"];
                //this.updateBudgetLineItems(context, service, entity);
                FaultException ex1 = new FaultException();
                
                if (entity.LogicalName == "gcbase_fundcentre")
                {
                   
                    //string fundTitle = "Freds first JUS plugin";
                    try
                    {
                        
                        DateTime startDate;
                        DateTime endDate;
                        var preEntity = (Entity)context.PreEntityImages["fundcentre"];
                     
                        // if not budget items exc
                        // if start or end date changes check if they fall with a new timeline, if yes excude below
                        // if not , throw plugin exception and show which budget lines are affected (custom methods here)
                        string statusReason = getCurrentStatus((int)preEntity.GetAttributeValue<OptionSetValue>("statuscode").Value);
                        //throw new InvalidPluginExecutionException("geez" + statusReason.ToString(), ex1);
                       
                        
                        if ((entity.Attributes.Contains("gcbase_estimatedannualbudget") || entity.Attributes.Contains("gcbase_startdate") || entity.Attributes.Contains("gcbase_enddate")) 
                            /**&& (statusReason == "open")**/)
                        {
                            FundCentreParamaters param = new FundCentreParamaters();
                            
                           // throw new InvalidPluginExecutionException(preEntity.GetAttributeValue<OptionSetValue>("statuscode").Value.ToString(), ex1);

                            if (entity.Attributes.Contains("gcbase_startdate"))
                            {
                                startDate = entity.GetAttributeValue<DateTime>("gcbase_startdate");
                                param.startdate = startDate;
                            }
                            else
                            {
                                if (preEntity.Contains("gcbase_startdate"))
                                {
                                    startDate = preEntity.GetAttributeValue<DateTime>("gcbase_startdate");
                                    param.startdate = startDate;
                                }
                                
                                
                            }
                            if (entity.Attributes.Contains("gcbase_enddate"))
                            {
                                endDate = entity.GetAttributeValue<DateTime>("gcbase_enddate");
                                param.enddate = endDate;
                            }
                            else
                            {
                                if (preEntity.Contains("gcbase_enddate"))
                                {
                                    endDate = preEntity.GetAttributeValue<DateTime>("gcbase_enddate");
                                    param.enddate = endDate;
                                }


                            }
                           
                            if (entity.Attributes.Contains("gcbase_estimatedannualbudget"))
                            {
                                param.amount = entity.GetAttributeValue<Money>("gcbase_estimatedannualbudget");
                            }
                            else {
                                if (preEntity.Contains("gcbase_estimatedannualbudget")) {
                                    param.amount = preEntity.GetAttributeValue<Money>("gcbase_estimatedannualbudget");
                                }  
                            }
                            param.id = entity.Id;
                           
                            var fundEntity = service.Retrieve("gcbase_fund", preEntity.GetAttributeValue<EntityReference>("gcbase_fund").Id, new ColumnSet("gcbase_name", "gcbase_fundid", "gcbase_fundtype"));
                            var fundTypeEntity = service.Retrieve("gcbase_fundtype", fundEntity.GetAttributeValue<EntityReference>("gcbase_fundtype").Id, new ColumnSet("gcbase_name"));
                            param.name = preEntity.GetAttributeValue<string>("gcbase_name") + "-" + fundEntity.GetAttributeValue<string>("gcbase_name") + "-" + fundTypeEntity.GetAttributeValue<string>("gcbase_name");
                                                     
                            createOrUpdateBudgetLineItems(param, service, preEntity, entity);
                        }           
                    }


                    catch (FaultException ex)
                    {
                        throw new InvalidPluginExecutionException("An error occurred in the plug-in.", ex);
                    }
                }
            }
        }

        public DataCollection<Entity> fundCentreBudgets(FundCentreParamaters param, IOrganizationService service) {
            QueryExpression existingFundCentreBudgets = new QueryExpression
            {
                EntityName = "gcbase_fundcentrebudget",
                ColumnSet = new ColumnSet("gcbase_fundcentre"),
                Criteria = new FilterExpression
                {
                    Conditions = {
                     new ConditionExpression {
                        AttributeName = "gcbase_fundcentre",
                        Operator = ConditionOperator.Equal,
                        Values = { param.id }
                     }
                   }
                }
            };
            
            DataCollection<Entity> fundCentreBudgetCollection = service.RetrieveMultiple(existingFundCentreBudgets).Entities;
            return fundCentreBudgetCollection;
        }

        private void createOrUpdateBudgetLineItems(FundCentreParamaters param, IOrganizationService service, Entity preEntity, Entity entity)
        {
            //using (var ctx = new OrganizationServiceContext(service)) {
                OrganizationServiceContext ctx = new OrganizationServiceContext(service);
                FaultException ex1 = new FaultException();

             //DELETE existing budget lines
                QueryExpression existingFundCentreBudgets = new QueryExpression
                {
                    EntityName = "gcbase_fundcentrebudget",
                    ColumnSet = new ColumnSet("gcbase_fundcentre", "gcbase_fiscalyear"),
                    Criteria = new FilterExpression
                    {
                        Conditions = {
                     new ConditionExpression {
                        AttributeName = "gcbase_fundcentre",
                        Operator = ConditionOperator.Equal,
                        Values = { param.id }
                     }
                   }
                    }
                };

                                    

                if (param.startdate.HasValue && param.enddate.HasValue) {
                    
                    int[] fiscalYears = new FiscalYear(param.startdate.Value, param.enddate.Value).getFiscalYears();
                 
        
                    DataCollection<Entity> fundCentreBudgetsToDelete = service.RetrieveMultiple(existingFundCentreBudgets).Entities;
                    if (fundCentreBudgetsToDelete.Count > 0)
                    {
                        // here we should validate if we have projects pending instead of deleting budgets

                       var currentYears = fundCentreBudgetsToDelete.Select(s => (int)s.GetAttributeValue<OptionSetValue>("gcbase_fiscalyear").Value).ToArray();
                        var newYears = fiscalYears.ToArray();
                        //newYears.Except(currentYears);

                        var illegalYears = currentYears.Except(newYears);

                        if (illegalYears.Count() > 0)
                        {
                            throw new InvalidPluginExecutionException(@"Cannot save your new start and end dates because there are budgets entered in
                        fiscal years that fall outside of those dates. If you want to revise the dates please first delete the budgets in 
                        fiscal years: " + string.Join("-", illegalYears) + " and try again!", ex1);
                        }
                        else
                        { 
                            foreach (Entity fcb in fundCentreBudgetsToDelete)
                            {
                                service.Delete("gcbase_fundcentrebudget", fcb.Id);
                            }
                        }
                    }

                    Array values = Enum.GetValues(typeof(goal_fiscalyear));
                    string[] fys = new string[fiscalYears.Count()];
                    int index = 0;

                    //QueryExpression fundTypeQry = new QueryExpression
                    //{
                    //    EntityName = "gcbase_fundtype",
                    //    ColumnSet = new ColumnSet("gcbase_name"),
                    //    Criteria = new FilterExpression
                    //    {
                    //        Conditions = { 
                    //            new ConditionExpression("gcbase_name", ConditionOperator.Equal, "Contribution")
                    //        }
                    //    }
                    //};
                    //DataCollection<Entity> fundTypes = service.RetrieveMultiple(fundTypeQry).Entities;
                    //Guid contribution = new Guid();
                   
                    //foreach (var ft in fundTypes)
                    //{
                    //    if (ft.Attributes["gcbase_name"].ToString() != "Grant")
                    //    {
                    //        contribution = ft.Id;
                    //    }
                    //}   
                   
               
                    // throw new InvalidPluginExecutionException(grant.ToString() + contribution.ToString(), ex1);


                    foreach (int year in fiscalYears)
                    {
                        if (param.amount.Value > 0)
                        {
                            Entity fundCentreBudget = new Entity("gcbase_fundcentrebudget");
                            // fundCentreBudget.Id = Guid.NewGuid();
                            fundCentreBudget["gcbase_amount"] = (Money)param.amount;
                            fys[index] = (string)Enum.GetName(typeof(goal_fiscalyear), year);
                            OptionSetValue fy = new OptionSetValue();
                            fy.Value = year;
                            fundCentreBudget["gcbase_fiscalyear"] = fy;
                            
                            EntityReference fundCentre = new EntityReference("gcbase_fundcentre", param.id);
                            fundCentreBudget["gcbase_fundcentre"] = fundCentre;
                            fundCentreBudget["gcbase_name"] = param.name + "-" + fy.Value;
                            // ctx.Attach(fundCentreBudget)
                            ctx.AddObject(fundCentreBudget);
                            ctx.SaveChanges();
                        }
                   
                        index++;
                    }
                
                }

              }
                
               //update rollup
                /**CalculateRollupFieldRequest crfr = new CalculateRollupFieldRequest
                {
                    Target = new EntityReference("new_class", classId),
                    FieldName = "new_totalstudents"
                }; **/
              
               // string test = string.Join("-", fys);
                //throw new InvalidPluginExecutionException(test, ex1);
           // }

            private string getCurrentStatus(int statusReason) {
                //FaultException ex1 = new FaultException();
                //throw new InvalidPluginExecutionException("wtf" + statusReason.ToString(), ex1);
                switch (statusReason) { 
                    case 148030011:
                    case 148030010:
                    case 148030009:
                        return "closed";
                }

                return "open";
            }

        }

        

}

    

