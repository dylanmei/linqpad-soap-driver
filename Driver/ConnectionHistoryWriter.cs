using System;
using System.IO;
using System.Linq;

namespace Driver
{
	public class ConnectionHistoryWriter
	{
		readonly string path;

		public ConnectionHistoryWriter(string path)
		{
			this.path = path;
		}

		public void Append(string item)
		{
			var history = new ConnectionHistoryReader(path).Read();
			if (history.Any(uri => uri.Equals(item, StringComparison.OrdinalIgnoreCase)))
				return;

			using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
			{
				var writer = new StreamWriter(stream);
				foreach (var value in history) writer.WriteLine(value);
				writer.WriteLine(item);
				writer.Flush();
			}
		}
	}
}
