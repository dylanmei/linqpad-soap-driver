using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;

namespace Driver
{
    public static class WsdlExtensions
    {
        public static IEnumerable<Binding> GetSoapBindings(this ServiceDescription description)
        {
            IEnumerable<Binding> bindings;

            if (description.Bindings.Count != 0)
                bindings = description.Bindings.Cast<Binding>();
            else
            {
                var importedDescriptions = description.ServiceDescriptions
                    .Cast<ServiceDescription>();
                bindings = importedDescriptions.SelectMany(d => d.Bindings.Cast<Binding>());
            }

            return bindings
                .Where(binding => binding.Extensions.OfType<SoapBinding>().Any());
        }
    }
}
