using System.Reflection;
using System.Web.Services.Description;

namespace Driver
{
	public class Proxy
	{
		public string Namespace { get; set; }
		public Assembly Assembly { get; set; }
		public ServiceDescription Description { get; set; }
	}
}