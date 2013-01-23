using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Windows;

namespace Driver
{
	public class Discovery
	{
		readonly string uri;
		readonly ICredentials credentials;
		DiscoveryClientDocumentCollection documents;

        // Handle SSL cert errors (see http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-ple)
        static Discovery()
        {
            ServicePointManager.ServerCertificateValidationCallback = OnServerCertificateValidationCallback;
        }

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

	    private static bool OnServerCertificateValidationCallback(
            object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	    {
	        if (sslPolicyErrors == SslPolicyErrors.None)
	            return true;

	        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) > 0)
	        {
	            var certStatus = chain.ChainStatus.Last();
	            if (MessageBox.Show(string
	                                    .Format(
	                                        "{0} (Code: {1}).{2}{2}Continue adding the connection?",
	                                        certStatus.StatusInformation.TrimEnd().TrimEnd('.'), certStatus.Status, Environment.NewLine),
	                                "Certificate Validation Error",
	                                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
	            {
	                return true;
	            }
	        }
	        return false;
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
