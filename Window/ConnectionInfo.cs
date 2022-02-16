using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace TestWindow
{
	class ConnectionInfo : IConnectionInfo
	{
		XElement data = new XElement("blah");

		public XElement DriverData
		{
			get { return data; }
			set { data = value; }
		}

		public bool Persist { get; set; }

		public string Encrypt(string data)
		{
			return data;
			//throw new NotImplementedException();
		}

		public string Decrypt(string data)
		{
			return data;
			//throw new NotImplementedException();
		}

		public IDatabaseInfo DatabaseInfo
		{
			get { throw new NotImplementedException(); }
		}

		public ICustomTypeInfo CustomTypeInfo
		{
			get { throw new NotImplementedException(); }
		}

		public IDynamicSchemaOptions DynamicSchemaOptions
		{
			get { throw new NotImplementedException(); }
		}

		public string DisplayName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string AppConfigPath
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public IDictionary<string, object> SessionData
		{
			get { throw new NotImplementedException(); }
		}
	}
}
