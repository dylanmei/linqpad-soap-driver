using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace Driver
{
	public class DiscoveryCompiler
	{
		readonly Discovery discovery;
		readonly CodeDomProvider codeProvider;

		public DiscoveryCompiler(Discovery discovery, CodeDomProvider codeProvider = null)
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
				CodeGenerationOptions = CodeGenerationOptions.GenerateProperties
			};

			ServiceDescriptionImporter.GenerateWebReferences(
				webReferences, codeProvider, compileUnit, options);
			CheckDescriptionImportValidations(reference, result);

			if (!result.HasErrors)
			{
				result.CodeDom = compileUnit;
				result.CodeProvider = codeProvider;
				result.Bindings.AddRange(
					GetSoapBindings(discovery.GetServices()));
			}

			return result;
		}

		static IEnumerable<DiscoveryBinding> GetSoapBindings(IEnumerable<ServiceDescription> serviceDescriptions)
		{
            var description = serviceDescriptions.First();
		    return description.GetSoapBindings()
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
