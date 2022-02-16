using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Xml.Serialization;

namespace Driver
{
	public class ProxyBuilder
	{
		readonly Discovery discovery;

		public ProxyBuilder(string url)
		{
			discovery = new Discovery(url, CredentialCache.DefaultCredentials, false);
		}

		public Proxy Build(AssemblyName assemblyName, string nameSpace)
		{
			var description = discovery.GetServices().First();
			var assembly = BuildAssembly(assemblyName, nameSpace);

			return new Proxy {
				Namespace = nameSpace,
				Description = description,
				Assembly = assembly
			};
		}

		Assembly BuildAssembly(AssemblyName assemblyName, string nameSpace)
		{
			var codeProvider = CodeProvider.Default;
			var reference = new DiscoveryCompiler(discovery, codeProvider)
				.GenerateReference(nameSpace);

			var options = new CompilerParameters(
				"System.dll System.Core.dll System.Xml.dll System.Web.Services.dll".Split(),
				assemblyName.CodeBase, true);
			var results = codeProvider.CompileAssemblyFromDom(options, new[] {reference.CodeDom});

			if (results.Errors.Count > 0)
				throw new Exception("Cannot compile service proxy: " +
					results.Errors[0].ErrorText + " (line " + results.Errors[0].Line + ")");

			return results.CompiledAssembly;
		}
	}
}