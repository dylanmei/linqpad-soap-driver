using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Driver
{
	public class ProxyBuilder
	{
        readonly string driverPath;
	    readonly ContractDiscovery discovery;

		public ProxyBuilder(string driverPath, string url)
		{
            this.driverPath = driverPath;
			discovery = new ContractDiscovery(url, CredentialCache.DefaultCredentials);
		}

        public Proxy Build(AssemblyName assemblyName, string @namespace)
		{
			var description = discovery.GetServices().First();
            var assembly = BuildAssembly(assemblyName, @namespace);

			return new Proxy {
                Namespace = @namespace,
				Description = description,
				Assembly = assembly
			};
		}

		Assembly BuildAssembly(AssemblyName assemblyName, string @namespace)
		{
            var codeProvider = CodeProvider.Default;

            var compileResults = new ContractCompiler(codeProvider)
                .CompileDiscovery(discovery, assemblyName, @namespace);

            WriteSource(codeProvider, compileResults.CodeDom);
			return compileResults.CompiledAssembly;
		}

        [Conditional("DEBUG")]
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