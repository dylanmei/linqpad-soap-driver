using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Driver
{
	public class DiscoveryReference
	{
		public CodeCompileUnit CodeDom { get; set; }
		public CodeDomProvider CodeProvider { get; set; }

        public Assembly CompiledAssembly { get; set; }
	    public List<DiscoveryBinding> Bindings { get; private set; }

		readonly List<string> errors = new List<string>();
		readonly List<string> warnings = new List<string>();

		public DiscoveryReference()
		{
			Bindings = new List<DiscoveryBinding>();
		}

		public bool HasErrors { get { return Errors.Count() > 0; } }
		public bool HasWarnings { get { return Warnings.Count() > 0; } }

		public IEnumerable<string> Errors {
			get { return errors; }
		}

		public IEnumerable<string> Warnings {
			get { return warnings; }
		}

		internal void Error(string message)
		{
			errors.Add(message);
		}

		internal void Warn(string message)
		{
			warnings.Add(message);
		}
	}
}