using System;
using System.Net;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace Driver
{
	public class ConnectionModel
	{
		readonly IConnectionInfo _connectionInfo;
		readonly XElement _driverData;

		public ConnectionModel(IConnectionInfo connectionInfo)
		{
			_connectionInfo = connectionInfo;
			_driverData = connectionInfo.DriverData;
		}

		public bool Persist
		{
			get { return _connectionInfo.Persist; }
			set { _connectionInfo.Persist = value; }
		}

		public string Uri
		{
			get { return (string)_driverData.Element ("Uri") ?? ""; }
			set { _driverData.SetElementValue ("Uri", value); }
		}

		public string BindingName
		{
			get { return (string) _driverData.Element("Binding") ?? ""; }
			set { _driverData.SetElementValue ("Binding", value); }
		}
	}
}
