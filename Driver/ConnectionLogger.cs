using System.Windows.Controls;

namespace Driver
{
	public class ConnectionLogger
	{
		readonly TextBox box;

		public ConnectionLogger(TextBox box)
		{
			this.box = box;
		}

		public void Clear()
		{
			box.Text = "";
		}

		public void Error(string message)
		{
			Write("Error> ", message);
		}

		public void Warn(string message)
		{
			Write("Warning> ", message);
		}

		public void Write(string message)
		{
			Write("> ", message);
		}

		void Write(string tag, string message)
		{
			box.Text += string.Concat(tag, message, "\n");
		}
	}
}