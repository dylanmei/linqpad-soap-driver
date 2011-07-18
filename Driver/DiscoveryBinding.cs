using System.Web.Services.Description;

namespace Driver
{
	public class DiscoveryBinding
	{
		internal DiscoveryBinding(Binding b)
		{
			Name = b.Name;
			TypeName = b.Type.Name;
		}

		public string Name { get; set; }
		public string TypeName { get; set; }
	}
}