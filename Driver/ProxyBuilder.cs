using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Driver
{
	public class ProxyBuilder
	{
		readonly Discovery discovery;
        readonly string driverPath;

		public ProxyBuilder(string driverPath, string url)
		{
            this.driverPath = driverPath;
			discovery = new Discovery(url, CredentialCache.DefaultCredentials);
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

            //WriteSource(codeProvider, reference.CodeDom);
			return results.CompiledAssembly;
		}

        void WriteSource(CodeDomProvider codeProvider, CodeCompileUnit compileUnit)
        {
            var path = System.IO.Path.Combine(driverPath, "gen.cs");
            using (var sourceWriter = new System.IO.StreamWriter(path))
            {
                codeProvider.GenerateCodeFromCompileUnit(
                    compileUnit, sourceWriter, new CodeGeneratorOptions { });
            }
        }
	}
}