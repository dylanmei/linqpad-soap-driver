using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Description;
using System.Web.Services;
using System.Web.Services.Description;
using System.Xml.Serialization;
using Binding = System.Web.Services.Description.Binding;
using ServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace Driver
{
    public class ContractCompiler
    {
        readonly CodeDomProvider codeProvider;
        const string ErrorHeading = "Cannot compile service proxy. ";

        public ContractCompiler(CodeDomProvider codeProvider)
		{
			this.codeProvider = codeProvider ?? CodeProvider.Default;
		}

        public DiscoveryReference CompileDiscovery(ContractDiscovery discovery, AssemblyName assemblyName, string @namespace)
        {
            var generator = new ServiceContractGenerator {
                Options = ServiceContractGenerationOptions.None,
            };
            generator.NamespaceMappings.Add("*", @namespace);

            foreach (var contract in discovery.GetContracts())
            {
                generator.GenerateServiceContractType(contract);
            }

            var reference = new DiscoveryReference();
            CheckImportValidations(generator.Errors, reference);

            if (!reference.HasErrors)
            {
                reference.Bindings.AddRange(
                    GetSoapBindings(discovery.GetServices()));
                reference.CodeDom = generator.TargetCompileUnit;

                var results = Compile(generator.TargetCompileUnit, assemblyName);
                CheckCompileResults(results.Errors);
                reference.CompiledAssembly = results.CompiledAssembly;
            }

            return reference;
        }

        CompilerResults Compile(CodeCompileUnit compileUnit, AssemblyName assemblyName)
        {
            var options = new CompilerParameters(
                "System.dll System.Core.dll System.Xml.dll System.Web.Services.dll System.ServiceModel.dll System.Runtime.Serialization.dll".Split(),
                assemblyName.CodeBase, true);

            return codeProvider.CompileAssemblyFromDom(options, compileUnit);
        }

        static IEnumerable<DiscoveryBinding> GetSoapBindings(IEnumerable<ServiceDescription> serviceDescriptions)
        {
            var description = serviceDescriptions.First();
            return description.Bindings.Cast<Binding>()
                .Where(binding => binding.Extensions.OfType<SoapBinding>().Any())
                .OrderByDescending(binding => binding.Type.Name)
                .Select(sb => new DiscoveryBinding(sb));
        }

        static void CheckImportValidations(IEnumerable<MetadataConversionError> validations, DiscoveryReference result)
        {
            foreach (var validation in validations)
            {
                if (validation.IsWarning) result.Warn(validation.Message);
                else result.Error(validation.Message);
            }
        }

        static void CheckCompileResults(CompilerErrorCollection errors)
        {
            if (errors.Count > 0)
            {
                throw new Exception(ErrorHeading +
                    errors[0].ErrorText + " (line " + errors[0].Line + ")");
            }
        }
    }

    public class DiscoveryCompiler
	{
		readonly Discovery discovery;
		readonly CodeDomProvider codeProvider;

        public DiscoveryCompiler(Discovery discovery)
            : this(discovery, CodeProvider.Default)
        {
        }

	    public DiscoveryCompiler(Discovery discovery, CodeDomProvider codeProvider)
		{
			this.discovery = discovery;
			this.codeProvider = codeProvider ?? CodeProvider.Default;
		}

		public DiscoveryReference GenerateReference(string nameSpace)
		{
			var codeNamespace = new CodeNamespace(nameSpace);
			var compileUnit = new CodeCompileUnit();
			compileUnit.Namespaces.Add(codeNamespace);

			return GenerateWebReferences(
				new WebReference(discovery.GetDocuments(), codeNamespace, "Soap", null, null),
				compileUnit
			);
		}

		DiscoveryReference GenerateWebReferences(WebReference reference, CodeCompileUnit compileUnit)
		{
			var result = new DiscoveryReference();
			CheckInteroperabilityConformance(reference, result);
            
			var webReferences = new WebReferenceCollection {reference};
			var options = new WebReferenceOptions {
                CodeGenerationOptions = CodeGenerationOptions.GenerateProperties,
			};

            ServiceDescriptionImporter.GenerateWebReferences(
				webReferences, codeProvider, compileUnit, options);
			CheckDescriptionImportValidations(reference, result);

			if (!result.HasErrors)
			{
                //result.CodeDom = compileUnit;
                //result.CodeProvider = codeProvider;
				result.Bindings.AddRange(
					GetSoapBindings(discovery.GetServices()));
			}

			return result;
		}

		static IEnumerable<DiscoveryBinding> GetSoapBindings(IEnumerable<ServiceDescription> serviceDescriptions)
		{
			var description = serviceDescriptions.First();
			return description.Bindings.Cast<Binding>()
				.Where(binding => binding.Extensions.OfType<SoapBinding>().Any())
				.OrderByDescending(binding => binding.Type.Name)
				.Select(sb => new DiscoveryBinding(sb));
		}

		static void CheckInteroperabilityConformance(WebReference reference, DiscoveryReference result)
		{
			var violations = new BasicProfileViolationCollection();
			WebServicesInteroperability.CheckConformance(WsiProfiles.BasicProfile1_1, reference, violations);
			if (violations.Count > 0)
			{
				result.Warn("This web reference does not conform to WS-I Basic Profile v1.1.");
				foreach (var v in violations)
					result.Warn(v.ToString());
			}
		}

		static void CheckDescriptionImportValidations(WebReference reference, DiscoveryReference result)
		{
			foreach (var validationMessage in reference.ValidationWarnings) result.Warn(validationMessage);
			var flags = reference.Warnings;

			if ((flags & ServiceDescriptionImportWarnings.SchemaValidation) != 0)
				result.Warn("Schema could not be validated. Class generation may fail or may produce incorrect results.");

			if ((flags & ServiceDescriptionImportWarnings.OptionalExtensionsIgnored) != 0)
				result.Warn("One or more optional WSDL extension elements were ignored.");

			if ((flags & ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored) != 0)
				result.Warn("One or more operations were skipped.");

			if ((flags & ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored) != 0)
				result.Warn("One or more bindings were skipped.");

			if ((flags & ServiceDescriptionImportWarnings.RequiredExtensionsIgnored) != 0)
				result.Warn("One or more required WSDL extension elements were ignored.");

			if ((flags & ServiceDescriptionImportWarnings.NoMethodsGenerated) != 0)
				result.Error("No methods were generated.");

			if ((flags & ServiceDescriptionImportWarnings.NoCodeGenerated) != 0)
				result.Error("No classes were generated.");
		}
	}
}
