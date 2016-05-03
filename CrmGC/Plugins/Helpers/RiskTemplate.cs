using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using System.Linq;

namespace EgcsCommon
{
    public class RiskTemplate
    {

        private EntityReference _riskTemplate;
        private IOrganizationService _service;
        private FaultException ex1;

        public RiskTemplate(EntityReference rt, IOrganizationService serv)
        {
            ex1 = new FaultException();
            _riskTemplate = rt;
            _service = serv;
        }

        public Boolean generateRiskAssessment(Entity fundingCase, OptionSetValue riskAssessmentType)
        {
            //get current entity data

            try
            {
                var fc = _service.Retrieve("gcbase_fundingcase", fundingCase.Id, new ColumnSet("gcbase_program"));
                var fundingCaseRef = new EntityReference("gcbase_fundingcase", fundingCase.Id);

                var fundCentreRef = fc.GetAttributeValue<EntityReference>("gcbase_program");
                QueryExpression qe = new QueryExpression("gcbase_risktemplatefundcentre");
                qe.Criteria.AddCondition("gcbase_fundcentre", ConditionOperator.Equal, fundCentreRef.Id);
                qe.ColumnSet.AddColumns("gcbase_risktemplate", "gcbase_name");
                //var optHelper = new helpers.OptionSetHelper();
                //int indexOfStatus = optHelper.getIndexOfLabel("gcbase_risktemplate", "statuscode", "Completed", _service);
                //qe.Criteria.AddCondition("statuscode", ConditionOperator.Equal, indexOfStatus);

                var rtfc = _service.RetrieveMultiple(qe).Entities.First();
                var riskTemplateRef = rtfc.GetAttributeValue<EntityReference>("gcbase_risktemplate");

                Entity newFundingCaseRiskAssessment = new Entity("gcbase_fundingcaseriskassessment");
                newFundingCaseRiskAssessment["gcbase_fundcentre"] = fundCentreRef;
                newFundingCaseRiskAssessment["gcbase_risktemplate"] = riskTemplateRef;
                newFundingCaseRiskAssessment["gcbase_fundingcase"] = fundingCaseRef;
                newFundingCaseRiskAssessment["gcbase_fundingcaseriskassessmenttype"] = riskAssessmentType;
                //newFundingCaseRiskAssessment["gcbase_name"] = rtfc.GetAttributeValue<EntityReference>("gcbase_risktemplate").Name;
                var newFCRA = _service.Create(newFundingCaseRiskAssessment);

                //the method below already happens when the plugin is called
                //generateRiskFactorsForTemplate(riskTemplateRef, newFCRA);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public Boolean generateRiskFactorsForTemplate(EntityReference riskTemplate, Guid fundingCaseRiskAssessmentId)
        {
            try
            {
                // 
                QueryExpression qe = new QueryExpression("gcbase_risktemplateriskfactor");
                qe.Criteria.AddCondition("gcbase_risktemplate", ConditionOperator.Equal, riskTemplate.Id);
                qe.ColumnSet.AddColumns("gcbase_name", "gcbase_riskfactor", "gcbase_risktemplate");
                var riskFactors = _service.RetrieveMultiple(qe).Entities;

                foreach (var item in riskFactors)
                {
                    Entity riskFactorValue = new Entity("gcbase_riskfactorvalue");
                    riskFactorValue["gcbase_name"] = item.GetAttributeValue<EntityReference>("gcbase_riskfactor").Name;
                    riskFactorValue["gcbase_riskfactor"] = new EntityReference("gcbase_riskfactor", item.GetAttributeValue<EntityReference>("gcbase_riskfactor").Id);
                    riskFactorValue["gcbase_risktemplateriskfactor"] = new EntityReference("gcbase_risktemplateriskfactor", item.Id);
                    riskFactorValue["gcbase_fundingcaseriskassessment"] = new EntityReference("gcbase_fundingcaseriskassessment", fundingCaseRiskAssessmentId);
                    _service.Create(riskFactorValue);
                }


                return true;
            }
            catch
            {
                return false;
            }
        }

        public Boolean generateTotalWeightedRiskScoreForRiskAssessment(Entity riskFactorValue)
        {
            try
            {
                //get all risk template risk factor count
                var fcra = _service.Retrieve("gcbase_fundingcaseriskassessment", riskFactorValue.GetAttributeValue<EntityReference>("gcbase_fundingcaseriskassessment").Id, new ColumnSet("gcbase_risktemplate"));
                QueryExpression qe = new QueryExpression("gcbase_risktemplateriskfactor");
                qe.Criteria.AddCondition("gcbase_risktemplate", ConditionOperator.Equal, fcra.GetAttributeValue<EntityReference>("gcbase_risktemplate").Id);
                qe.ColumnSet.AddColumn("gcbase_risktemplate");
                var qeResult = _service.RetrieveMultiple(qe).Entities;

                int riskTemplateRiskFactorCount = qeResult.Count();

                QueryExpression qe1 = new QueryExpression("gcbase_riskfactorvalue");
                qe1.Criteria.AddCondition("gcbase_fundingcaseriskassessment", ConditionOperator.Equal, riskFactorValue.GetAttributeValue<EntityReference>("gcbase_fundingcaseriskassessment").Id);
                qe1.ColumnSet.AddColumns("statuscode", "gcbase_risktemplateriskfactor", "gcbase_risklevel");
                var relatedRiskFactorValues = _service.RetrieveMultiple(qe1).Entities.ToList();

                int validRiskFactorValueCount = 0;
                decimal totalWeighted = 0;
                decimal totalUnWeighted = 0;

                Guid riskTemplateId = qeResult.First().GetAttributeValue<EntityReference>("gcbase_risktemplate").Id;

                QueryExpression qeMultiplier = new QueryExpression("gcbase_risktemplateriskfactor");
                qeMultiplier.Criteria.AddCondition("gcbase_risktemplate", ConditionOperator.Equal, riskTemplateId);

                int multiplier = _service.RetrieveMultiple(qeMultiplier).Entities.Count();


                foreach (var item in relatedRiskFactorValues)
                {
                    var optHelper = new OptionSetHelper();
                    int indexOfStatus = optHelper.getIndexOfLabel("gcbase_riskfactorvalue", "statuscode", "Completed", _service);

                    //get risk factor weight
                    var rf = _service.Retrieve("gcbase_risktemplateriskfactor", item.GetAttributeValue<EntityReference>("gcbase_risktemplateriskfactor").Id, new ColumnSet("gcbase_weight", "gcbase_multiplier"));



                    if (item.GetAttributeValue<OptionSetValue>("statuscode").Value == indexOfStatus)
                    {
                        validRiskFactorValueCount += 1;
                        var weight = rf.GetAttributeValue<decimal>("gcbase_weight");
                        // var multiplier = rf.GetAttributeValue<decimal>("gcbase_multiplier");

                        var riskLevel = item.GetAttributeValue<OptionSetValue>("gcbase_risklevel");
                        var riskLevelValue = new OptionSetHelper();

                        var riskLevelText = riskLevelValue.getLabelFromField(item, "gcbase_risklevel", _service);
                        int riskVal = 0;
                        if (riskLevelText == "1-Low")
                        {
                            riskVal = 1;
                        }
                        if (riskLevelText == "2-Medium")
                        {
                            riskVal = 2;
                        }
                        if (riskLevelText == "3-High")
                        {
                            riskVal = 3;
                        }

                        totalWeighted += ((riskVal * weight) * multiplier);
                        totalUnWeighted += riskVal;
                    }
                }

                if (riskTemplateRiskFactorCount == validRiskFactorValueCount)
                {
                    //calculate assessed risk based on percentage of total risk.
                    var lowRiskMaxScore = multiplier;
                    var midRiskMaxScore = (multiplier * 3) - multiplier;
                    var highRiskMaxScore = multiplier * 3;

                    OptionSetValue assessedRisk = new OptionSetValue();

                    if (totalUnWeighted <= multiplier)
                    {
                        assessedRisk = new OptionSetValue(new OptionSetHelper().getIndexOfLabel("gcbase_fundingcaseriskassessment", "gcbase_assessedrisk", "1-Low", _service));
                    }
                    if (totalUnWeighted > multiplier && totalUnWeighted <= midRiskMaxScore)
                    {
                        assessedRisk = new OptionSetValue(new OptionSetHelper().getIndexOfLabel("gcbase_fundingcaseriskassessment", "gcbase_assessedrisk", "2-Medium", _service));
                    }
                    if (totalUnWeighted > midRiskMaxScore)
                    {
                        assessedRisk = new OptionSetValue(new OptionSetHelper().getIndexOfLabel("gcbase_fundingcaseriskassessment", "gcbase_assessedrisk", "3-High", _service));
                    }

                    //  var optHelper = new helpers.OptionSetHelper();
                    // int indexOfStatus = optHelper.getIndexOfLabel("gcbase_fundingcaseriskassessment", "statuscode", "Analyst Completed", _service);
                    // fcra["statuscode"] = new OptionSetValue(indexOfStatus);
                    fcra["gcbase_totalweightedscore"] = totalWeighted;
                    fcra["gcbase_totalunweightedscore"] = totalUnWeighted;
                    fcra["gcbase_assessedrisk"] = assessedRisk;
                    _service.Update(fcra);
                    //update funding case risk assessment to analyst completed state.
                }
                else
                {
                    var optHelper = new OptionSetHelper();
                    int indexOfStatus = optHelper.getIndexOfLabel("gcbase_fundingcaseriskassessment", "statuscode", "Incomplete", _service);
                    fcra["statuscode"] = new OptionSetValue(indexOfStatus);
                    fcra["gcbase_totalweightedscore"] = null;
                    fcra["gcbase_totalunweightedscore"] = null;
                    fcra["gcbase_assessedrisk"] = null;
                    _service.Update(fcra);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Boolean generateTotalWeightedRiskScore()
        {
            try
            {
                var riskTemplateId = _riskTemplate.Id;
                QueryExpression qe = new QueryExpression("gcbase_risktemplateriskfactor");
                qe.Criteria.AddCondition("gcbase_risktemplate", ConditionOperator.Equal, riskTemplateId);
                qe.ColumnSet.AddColumns("gcbase_weight");
                var rtrfs = _service.RetrieveMultiple(qe).Entities;
                decimal sumOfWeights = 0;
                foreach (var item in rtrfs)
                {
                    var weight = item.GetAttributeValue<decimal>("gcbase_weight");
                    sumOfWeights += weight;
                }
                if (sumOfWeights == 1)
                {
                    Entity riskTemplate = _service.Retrieve("gcbase_risktemplate", riskTemplateId, new ColumnSet("statuscode"));
                    var optHelper = new OptionSetHelper();
                    int indexOfStatus = optHelper.getIndexOfLabel("gcbase_risktemplate", "statuscode", "Active", _service);
                    riskTemplate["statuscode"] = new OptionSetValue(indexOfStatus);
                    _service.Update(riskTemplate);
                }
                else
                {
                    Entity riskTemplate = _service.Retrieve("gcbase_risktemplate", riskTemplateId, new ColumnSet("statuscode"));
                    var optHelper = new OptionSetHelper();
                    int indexOfStatus = optHelper.getIndexOfLabel("gcbase_risktemplate", "statuscode", "Incomplete", _service);
                    riskTemplate["statuscode"] = new OptionSetValue(indexOfStatus);
                    _service.Update(riskTemplate);
                }
                return true;
            }
            catch
            {
                return false;

            }
        }

        public Boolean templateHasExistingCompletedAssessments()
        {

            QueryExpression qe = new QueryExpression("gcbase_fundingcaseriskassessment");
            qe.Criteria.AddCondition("gcbase_risktemplate", ConditionOperator.Equal, _riskTemplate.Id);

            var optHelper = new OptionSetHelper();
            int indexOfStatus = optHelper.getIndexOfLabel("gcbase_fundingcaseriskassessment", "statuscode", "Analyst Completed", _service);

            qe.Criteria.AddCondition("statuscode", ConditionOperator.Equal, indexOfStatus);
            var existingCompletedTemplates = _service.RetrieveMultiple(qe).Entities;
            // throw new InvalidPluginExecutionException("asfsdafasd", ex1);  
            if (existingCompletedTemplates.Count() > 0)
            {
                return true;
            }
            return false;
        }
    }
}



