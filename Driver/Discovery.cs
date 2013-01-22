using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Services.Discovery;
using ServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace Driver
{
    public class Discovery
	{
		readonly string uri;
		readonly ICredentials credentials;
		DiscoveryClientDocumentCollection documents;

		public Discovery(string uri, ICredentials credentials)
		{
			this.uri = uri;
			this.credentials = credentials;
		}

		public IEnumerable<ServiceDescription> GetServices()
		{
			return (
				from DictionaryEntry entry in GetDocuments()
				let description = entry.Value as ServiceDescription
				where description != null && description.Services.Count > 0
				select description
			);
		}

		public DiscoveryClientDocumentCollection GetDocuments()
		{
			return documents ?? (documents = DiscoverDocuments());
		}

		DiscoveryClientDocumentCollection DiscoverDocuments()
		{
			var protocol = new DiscoveryClientProtocol {
				AllowAutoRedirect = true,
				Credentials = credentials ?? CredentialCache.DefaultCredentials
			};

			protocol.DiscoverAny(uri);
			protocol.ResolveAll();

			return protocol.Documents;			
		}
	}
}
