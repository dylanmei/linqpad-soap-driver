using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace Driver
{
    public class ContractDiscovery
    {
        readonly string uri;
        readonly ICredentials credentials;
        MetadataSet metadataSet;

        public ContractDiscovery(string uri, ICredentials credentials)
        {
            this.uri = uri;
            this.credentials = credentials;
        }

        public IEnumerable<ServiceDescription> GetServices()
        {
            var metadata = GetMetadata();
            return from MetadataSection section in metadata.MetadataSections
                   let description = section.Metadata as ServiceDescription
                   where description != null && description.Services.Count > 0
                   select description;
        }

        public IEnumerable<ContractDescription> GetContracts()
        {
            var metadata = GetMetadata();
            var importer = GetImporter(metadata);

            importer.ImportAllEndpoints();
            return importer.ImportAllContracts();
        }

        MetadataSet GetMetadata()
        {
            if (metadataSet == null)
            {
                var client = new MetadataExchangeClient(CleanEndpoint(), MetadataExchangeClientMode.HttpGet) {
                    ResolveMetadataReferences = true,
                    HttpCredentials = credentials
                };

                metadataSet = client.GetMetadata();
            }
            return metadataSet;
        }

        WsdlImporter GetImporter(MetadataSet metadata)
        {
            var importer = new WsdlImporter(metadata);
            importer.WsdlImportExtensions.Remove<DataContractSerializerMessageContractImporter>();
            importer.WsdlImportExtensions.Remove<XmlSerializerMessageContractImporter>();
            importer.WsdlImportExtensions.Add(new SoapContractImporter());
//            importer.WsdlImportExtensions.Add(new SoapClientRunnerImporter());
//            importer.WsdlImportExtensions.Add(new SoapMessageContractImporter());

            var options = new XmlSerializerImportOptions {
                WebReferenceOptions = {
                    Style = ServiceDescriptionImportStyle.Client,
                    CodeGenerationOptions = CodeGenerationOptions.None
                },
            };

            importer.State.Add(typeof(XmlSerializerImportOptions), options);
            return importer;
        }

        Uri CleanEndpoint()
        {
            var builder = new UriBuilder(uri) { Query = "wsdl" };
            return builder.Uri;
        }
    }

    public class SoapContractImporter : IWsdlImportExtension
    {
        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            context.Contract.Behaviors.Add(new SoapContractGenerator());
        }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }
    }

    public class SoapContractGenerator : IServiceContractGenerationExtension, IContractBehavior
    {
        public void GenerateContract(ServiceContractGenerationContext context)
        {
            context.ContractType.Name = "I" + context.Contract.Name;
            context.ContractType.IsInterface = true;
            var attributes = context.ContractType.CustomAttributes.Cast<CodeAttributeDeclaration>();
            var attribute = attributes.FirstOrDefault(a => a.Name == "System.ServiceModel.ServiceContractAttribute");
            if (attribute != null)
                attribute.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(context.Contract.Name)));

            GenerateClient(context);
        }

        void GenerateClient(ServiceContractGenerationContext context)
        {
            var namespaces = context.ServiceContractGenerator.TargetCompileUnit.Namespaces.Cast<CodeNamespace>();
            var codeNamespace = namespaces.Single();

            var type = new CodeTypeDeclaration
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public,
                Name = context.Contract.Name
            };
            type.BaseTypes.Add(new CodeTypeReference(string.Format("System.ServiceModel.ClientBase<{0}>", context.Contract.Name)));
            type.BaseTypes.Add(new CodeTypeReference("I" + context.Contract.Name));

            codeNamespace.Types.Add(type);
        }

        void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint) {
        }
        void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime) {
        }
        void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime) {
        }
        void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) {
        }
    }
}