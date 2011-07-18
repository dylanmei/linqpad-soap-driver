using System.Collections.Generic;
using System.Reflection;
using LINQPad.Extensibility.DataContext;

namespace Driver
{
	public class SoapContextDriver : DynamicDataContextDriver
	{
		public override string Name
		{
			get { return "WCF Data Services (SOAP)"; }
		}

		public override string Author
		{
			get { return "Dylan"; }
		}

		public override string GetConnectionDescription(IConnectionInfo connectionInfo)
		{
			return new ConnectionModel(connectionInfo).Uri;
		}

		public override bool AreRepositoriesEquivalent (IConnectionInfo r1, IConnectionInfo r2)
		{
			return Equals(r1.DriverData.Element("Uri"), r2.DriverData.Element("Uri"));
		}

		public override IEnumerable<string> GetAssembliesToAdd ()
		{
			// We need the following assembly for compiliation and autocompletion:
			return new [] { "System.Web.Services.dll" };
		}

		public override IEnumerable<string> GetNamespacesToAdd ()
		{
			// Import the commonly used namespaces as a courtesy to the user:
			return new [] { "System.Web.Services" };
		}

		public override bool ShowConnectionDialog(IConnectionInfo connectionInfo, bool isNewConnection)
		{
			//// Populate the default URI with a demo value:
			//if (isNewConnection)
			//    new DialogModel (connectionInfo).Uri = "";
			return new Dialog(connectionInfo).ShowDialog() == true;
		}

		//public override void InitializeContext (IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
		//{
		//    if (true) return;
		//}

		public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo connectionInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
		{
			var props = new ConnectionModel(connectionInfo);
			var proxy = new ProxyBuilder(props.Uri)
				.Build(assemblyToBuild, nameSpace);

			var schema = new SchemaBuilder()
				.Build(proxy.Description, props.BindingName, proxy.Assembly);

			nameSpace = proxy.Namespace;
			typeName = schema.TypeName;

			return schema.Entities;
		}
	}
}
