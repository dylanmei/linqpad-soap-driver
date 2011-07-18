using System;
using System.ComponentModel;
using System.Net;

namespace Driver
{
	public class ConnectionCompleteEventArgs : EventArgs
	{
		public DiscoveryReference Reference { get; set; }
	}

	public class ConnectionWorker
	{
		Discovery discovery;
		Action<ConnectionCompleteEventArgs> handler;
		readonly BackgroundWorker worker = new BackgroundWorker();

		public ConnectionWorker()
		{
			worker.DoWork += Work;
			worker.RunWorkerCompleted += Complete;
		}

		public bool Busy
		{
			get
			{
				return worker.IsBusy;
			}
		}

		public ConnectionWorker Setup(string uri, ICredentials credentials)
		{
			discovery = new Discovery(uri, credentials);
			return this;
		}

		public void Run(Action<ConnectionCompleteEventArgs> completeHandler)
		{
			handler = completeHandler;
			worker.RunWorkerAsync(discovery);
		}

		static void Work(object sender, DoWorkEventArgs e)
		{
			var discovery = e.Argument as Discovery;
			if (discovery != null)
			{
				e.Result = new DiscoveryCompiler(discovery, CodeProvider.Default)
					.GenerateReference("temp");
			}
		}

		void Complete(object sender, RunWorkerCompletedEventArgs e)
		{
			handler(new ConnectionCompleteEventArgs {
				Reference = e.Result as DiscoveryReference
			});
		}
	}
}
