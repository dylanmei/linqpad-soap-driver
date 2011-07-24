using System.Collections.Generic;
using System.IO;

namespace Driver
{
	public class ConnectionHistoryReader
	{
		readonly string path;
		public ConnectionHistoryReader(string path)
		{
			this.path = path;
		}

		public IEnumerable<string> Read()
		{
			if (!File.Exists(path)) return new string[0];

			using (var stream = new FileStream(path, FileMode.Open))
				return ReadStream(new StreamReader(stream));
		}

		static IEnumerable<string> ReadStream(StreamReader reader)
		{
			var lines = new List<string>();
			while (!reader.EndOfStream)
				lines.Add(reader.ReadLine());
			return lines;
		}
	}
}