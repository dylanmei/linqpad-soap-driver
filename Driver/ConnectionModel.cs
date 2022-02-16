using System.Collections.Generic;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace Driver
{
	public class ConnectionModel
	{
		readonly IConnectionInfo connectionInfo;
		readonly IEnumerable<string> knownUris;

		public ConnectionModel(IConnectionInfo connectionInfo,
			IEnumerable<string> knownUris = null)
		{
			this.connectionInfo = connectionInfo;
			this.knownUris = knownUris ?? new string[0];
		}

		XElement DriverData
		{
			get { return connectionInfo.DriverData; }
		}

		public bool Persist
		{
			get { return connectionInfo.Persist; }
			set { connectionInfo.Persist = value; }
		}

		public string Uri
		{
			get { return (string)DriverData.Element ("Uri") ?? ""; }
			set { DriverData.SetElementValue ("Uri", value); }
		}

		public string UserName
        {
			get { return (string) DriverData.Element("UserName") ?? ""; }
			set { DriverData.SetElementValue("UserName", value); }
        }

		public string Password
        {
			get { return connectionInfo.Decrypt((string)DriverData.Element("Password") ?? ""); }
			set { DriverData.SetElementValue("Password", connectionInfo.Encrypt(value)); }
        }

		public string BindingName
		{
			get { return (string) DriverData.Element("Binding") ?? ""; }
			set { DriverData.SetElementValue ("Binding", value); }
		}

		public IEnumerable<string> KnownUris
		{
			get { return knownUris; }
		}
	}
}
