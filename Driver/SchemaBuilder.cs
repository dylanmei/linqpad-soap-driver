using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Description;
using LINQPad.Extensibility.DataContext;

namespace Driver
{
	public class SchemaBuilder
	{
		public Schema Build(ServiceDescription description, string bindingName, Assembly assembly)
		{
			var binding = GetSoapBinding(description, bindingName);
			var serviceType = GetServiceType(binding, assembly);
			return new Schema {
				TypeName = serviceType.Name,
				Entities = BuildEntities(serviceType, binding)
			};
		}

		static Type GetServiceType(Binding soapBinding, Assembly assembly)
		{
			foreach (var type in assembly.GetTypes())
			{
				var bindingAttribute = type.GetCustomAttributes(typeof (WebServiceBindingAttribute), false)
					.Cast<WebServiceBindingAttribute>().SingleOrDefault();
				if (bindingAttribute != null && bindingAttribute.Name == soapBinding.Name)
					return type;
			}

			return null;

			//var serviceTypes = new List<Type>();

			//// FIXME: Best way to match the service/binding to the implementing type
			////

			//foreach (var type in assembly.GetTypes())
			//{
			//    var bindingAttributes = type.GetCustomAttributes(typeof (WebServiceBindingAttribute), false);
			//    if (bindingAttributes.Length != 0) serviceTypes.Add(type);
			//}

			//var name = soapBinding.Type.Name;
			//return string.IsNullOrEmpty(name)
			//    ? new Type[0]
			//    : serviceTypes.Where(t => t.Name == name || t.Name.EndsWith(name));
		}

		static List<ExplorerItem> BuildEntities(Type serviceType, Binding soapBinding)
		{
			var list = new List<ExplorerItem>();
			foreach (var operation in GetSoapOperations(soapBinding))
				list.Add(CreateExplorerOperation(serviceType, operation));

			return list;
		}

		static ExplorerItem CreateExplorerOperation(Type serviceType, OperationBinding operation)
		{
			var method = serviceType.GetMethod(operation.Name);
			var parameters = method.GetParameters();
			var description = operation.DocumentationElement == null
				? ""
				: operation.DocumentationElement.InnerText;
			var item = new ExplorerItem(operation.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.StoredProc) {
				ToolTipText = description,
				DragText = GetMethodCallString(method.Name, from p in parameters select p.Name),
				Children = (
					from p in parameters
					select new ExplorerItem(p.Name, ExplorerItemKind.Parameter, ExplorerIcon.Parameter)
				).ToList()
			};

			return item;
		}

		static Binding GetSoapBinding(ServiceDescription description, string bindingName)
		{
			var soapBindings = description.GetSoapBindings()
				.Where(binding => binding.Name == bindingName)
				.OrderByDescending(binding => binding.Type.Name);
			return soapBindings.FirstOrDefault();
		}

		static IEnumerable<OperationBinding> GetSoapOperations(Binding soapBinding)
		{
            return soapBinding.Operations.Cast<OperationBinding>()
				.OrderBy(o => o.Name).ToList();
		}

		static string GetMethodCallString(string methodName, IEnumerable<string> parameterNames)
		{
			return string.Concat(methodName, "(", string.Join(", ", parameterNames), ")");
		}
	}
}
