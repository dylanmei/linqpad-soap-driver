using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace Driver
{
	public class DiscoveryCompleteEventArgs : EventArgs
	{
		public DiscoveryReference Reference { get; set; }
	}

	public class DiscoveryFailureEventArgs : EventArgs
	{
		public string Reason { get; set; }
	}

	public class DiscoveryWorker
	{
		class DiscoveryResult
		{
			public DiscoveryReference Reference { get; set; }
			public string FailureReason { get; set; }
		}

		Action<DiscoveryCompleteEventArgs> completeHandler;
		Action<DiscoveryFailureEventArgs> failureHandler;
		readonly BackgroundWorker worker = new BackgroundWorker();

		public DiscoveryWorker()
		{
			worker.DoWork += OnDoWork;
			worker.RunWorkerCompleted += OnRunWorkerCompleted;
		}

		public bool Busy
		{
			get
			{
				return worker.IsBusy;
			}
		}

		public DiscoveryWorker Failure(Action<DiscoveryFailureEventArgs> failureHandler)
		{
			this.failureHandler = failureHandler;
			return this;
		}

		public DiscoveryWorker Complete(Action<DiscoveryCompleteEventArgs> completeHandler)
		{
			this.completeHandler = completeHandler;
			return this;
		}

		public void Connect(string uri, ICredentials credentials, bool isBasicAuth)
		{
			worker.RunWorkerAsync(new Discovery(uri, credentials, isBasicAuth));
		}

		static void OnDoWork(object sender, DoWorkEventArgs args)
		{
			var discovery = args.Argument as Discovery;
			var result = new DiscoveryResult();
			args.Result = result;

			if (discovery != null)
			{
				try
				{
					result.Reference = new DiscoveryCompiler(discovery, CodeProvider.Default)
						.GenerateReference("temp");
				}
				catch (InvalidOperationException ioe)
				{
					Debug.WriteLine(ioe.Message);
					result.FailureReason = ioe.Message;
					if (ioe.InnerException != null)
                    {
						result.FailureReason += "\n" + ioe.InnerException.Message;
                    }
				}
			}
		}

		void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var result = (DiscoveryResult)e.Result;
			if (result.Reference != null && completeHandler != null)
			{
				completeHandler(new DiscoveryCompleteEventArgs {
					Reference = result.Reference
				});
			}
			else if (failureHandler != null)
			{
				failureHandler(new DiscoveryFailureEventArgs {
					Reason = result.FailureReason
				});
			}
		}
	}
}
