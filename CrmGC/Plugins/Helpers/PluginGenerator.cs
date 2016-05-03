using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace EgcsCommon
{
    public enum CrmPluginStepDeployment
    {
        ServerOnly = 0,
        OfflineOnly = 1,
        Both = 2
    };

    public enum CrmPluginStepMode
    {
        Asynchronous = 1,
        Synchronous = 0
    };

    public enum CrmPluginStepStage
    {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40
    };

    public enum SdkMessageName
    {
        Create,
        Update,
        Delete,
        Retrieve,
        Assign,
        GrantAccess,
        ModifyAccess,
        RetrieveMultiple,
        RetrievePrincipalAccess,
        RetrieveSharedPrincipalsAndAccess,
        RevokeAccess,
        SetState,
        SetStateDynamicEntity,
    };


    public class PluginGenerator
    {
        //public exposed vars
        public IOrganizationService _service;
        public string param_PluginClassWithNamespace = "";
        public int param_CrmPluginStepStage = 0;
        public string param_EntityName = "";
        public string param_StepName = "";
        public string param_SdkMessageName = "";
        public string param_PluginFriendlyName = "";
        public string param_PluginDescription = "";

        //private vars
        private int _rank = 0;
        private string _assemblyName = "";

        public PluginGenerator()
        {
        }

        public PluginGenerator(IOrganizationService service, string entityName, string pluginClassWithNamespace, int crmPluginStepStage, string stepName, string sdkMessage, string friendlyName, string pluginDescription)
        {
            _service = service;
            param_EntityName = entityName;
            param_PluginClassWithNamespace = pluginClassWithNamespace;
            param_CrmPluginStepStage = crmPluginStepStage;
            param_StepName = stepName;
            param_SdkMessageName = sdkMessage;
            param_PluginFriendlyName = friendlyName;
            param_PluginDescription = pluginDescription;
        }

        public string hi()
        {
            return "heya";
        }

        public Guid generatePlugin()
        {

            _assemblyName = "CrmGC.Plugins";
          
            _rank = 1;
            var pluginId = GetPluginTypeId();

            Entity step = new Entity("sdkmessageprocessingstep");
            step["asyncautodelete"] = false;
            step["mode"] = new OptionSetValue((int)CrmPluginStepMode.Synchronous);
            step["name"] = param_StepName;
            step["rank"] = _rank;
            step["eventhandler"] = new EntityReference("plugintype", pluginId);
            step["sdkmessageid"] = new EntityReference("sdkmessage", GetMessageId());
            step["stage"] = new OptionSetValue((int)param_CrmPluginStepStage);
            step["supporteddeployment"] = new OptionSetValue((int)CrmPluginStepDeployment.ServerOnly);
            step["sdkmessagefilterid"] = new EntityReference("sdkmessagefilter", GetSdkMessageFilterId());
            return _service.Create(step);

        }

        public Boolean deletePlugin()
        {
            return false;
        }
        private Guid GetSdkMessageFilterId()
        {
            QueryExpression qe = new QueryExpression("sdkmessagefilter");
            qe.Criteria.AddCondition("primaryobjecttypecode", ConditionOperator.Equal, param_EntityName);
            qe.Criteria.AddCondition("sdkmessageid", ConditionOperator.Equal, GetMessageId());
            return _service.RetrieveMultiple(qe).Entities.First().Id;

        }
        private Guid GetMessageId()
        {
            QueryExpression qe = new QueryExpression("sdkmessage");
            qe.Criteria.AddCondition("name", ConditionOperator.Equal, param_SdkMessageName);
            return _service.RetrieveMultiple(qe).Entities.First().Id;
        }
        private Guid GetPluginTypeId()
        {
            QueryExpression qe = new QueryExpression("pluginassembly");
            qe.Criteria.AddCondition("name", ConditionOperator.Equal, _assemblyName);
            Guid assemblyId = _service.RetrieveMultiple(qe).Entities.First().Id;

            //if plugintypename is not found, create it, otherwise fetch it
            QueryExpression qe1 = new QueryExpression("plugintype");
            qe1.Criteria.AddCondition("typename", ConditionOperator.Equal, param_PluginClassWithNamespace);
            var plugin = _service.RetrieveMultiple(qe1).Entities;
            //FaultException ex = new FaultException();
            // throw new InvalidPluginExecutionException(plugin.Count().ToString(), ex);
            if (plugin.Count() > 0)
            {

                return plugin.First().Id;
            }
            else
            {

                Entity pluginType = new Entity("plugintype");
                pluginType["pluginassemblyid"] = new EntityReference("pluginassembly", assemblyId);
                pluginType["typename"] = param_PluginClassWithNamespace;
                pluginType["friendlyname"] = param_PluginFriendlyName;
                pluginType["name"] = param_PluginClassWithNamespace;
                pluginType["description"] = param_PluginDescription;

                return _service.Create(pluginType);
            }




        }

    }
}





