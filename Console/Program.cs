using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Driver;

namespace Console
{
    public class Program
    {
        static ContractDiscovery Discovery;

        static void Main(string[] args)
        {
            Discovery = new ContractDiscovery(
                "http://myservice/soap", CredentialCache.DefaultCredentials);

            var assemblyPath = Path.Combine(GetOutputPath(), "output.dll");
            var assemblyName = new AssemblyName {
                Name = "output",
                CodeBase = assemblyPath
            };
            BuildAssembly(assemblyName, "LINQPad.User");

            System.Console.Write("Press any key to exit...");
            System.Console.ReadKey();
        }

		static Assembly BuildAssembly(AssemblyName assemblyName, string @namespace)
		{
            var codeProvider = CodeProvider.Default;

            var compileResults = new ContractCompiler(codeProvider)
                .CompileDiscovery(Discovery, assemblyName, @namespace);

            WriteSource(codeProvider, compileResults.CodeDom);
			return compileResults.CompiledAssembly;
		}

        static void WriteSource(CodeDomProvider codeProvider, CodeCompileUnit compileUnit)
        {
            var sourcePath = Path.Combine(GetOutputPath(), "output.cs");
            using (var sourceWriter = new StreamWriter(sourcePath))
            {
                codeProvider.GenerateCodeFromCompileUnit(
                    compileUnit, sourceWriter, new CodeGeneratorOptions { });
                sourceWriter.Close();
            }
        }

        static string GetOutputPath()
        {
            var rootedPath = AppDomain.CurrentDomain.BaseDirectory; 
            var directory = Path.GetDirectoryName(rootedPath) ?? "";
            if (directory.Contains("bin") && !directory.EndsWith("bin"))
                directory = Path.GetDirectoryName(directory);
            return directory;
        }
    }
}
