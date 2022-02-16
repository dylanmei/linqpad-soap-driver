using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Driver
{
	public partial class Dialog
	{
		int page;
		readonly ConnectionLogger logger;
		readonly DiscoveryWorker worker;

		public Dialog(ConnectionModel model)
		{
			DataContext = model;
			InitializeComponent();
			worker = new DiscoveryWorker();
			logger = new ConnectionLogger(LogBox);
		}

		ConnectionModel Model
		{
			get { return DataContext as ConnectionModel; }
		}

		void Connect_Click(object sender, EventArgs e)
		{
			if (Uri.IsWellFormedUriString(Model.Uri, UriKind.Absolute) || Model.Uri.StartsWith("file:"))
			{
				SetVisiblePage(1);
				Connect();
			}
			else
            {
				Console.WriteLine("Invalid Url!");
            }
		}

		void Select_Click(object sender, EventArgs e)
		{
			SetVisiblePage(2);
		}

		void Back_Click(object sender, EventArgs e)
		{
			SetVisiblePage(page - 1);
		}

		void Finish_Click(object sender, EventArgs e)
		{
			// Close window
			Model.Persist = true;
			if (this.useBasicAuth.IsChecked == true)
            {
				Model.UserName = this.loginName.Text;
				Model.Password = this.password.Password;
            }
			DialogResult = true;
		}

		void Connect()
		{
			SelectButton.IsEnabled = false;
			RestartButton.IsEnabled = false;
			Progress.IsIndeterminate = true;

			if (!worker.Busy)
			{
				logger.Clear();
				logger.Write("Connecting to " + Model.Uri);

				NetworkCredential cred = CredentialCache.DefaultNetworkCredentials;

				var isBasicAuth = useBasicAuth.IsChecked == true;
				if (isBasicAuth)
				{
					var cred2 = new NetworkCredential()
					{
						UserName = loginName.Text.Trim(),
						Password = password.Password.Trim()
					};
					cred = cred2.GetCredential(new Uri(Model.Uri), "Basic");
				}
                worker
					.Failure(Discovery_Failure)
					.Complete(Discovery_Connect)
					.Connect(Model.Uri, cred, isBasicAuth);
			}
		}

		void Discovery_Failure(DiscoveryFailureEventArgs e)
		{
			logger.Error(e.Reason);
			RestartButton.IsEnabled = true;
		}

		void Discovery_Connect(DiscoveryCompleteEventArgs e)
		{
			var reference = e.Reference;
			foreach (var warning in reference.Warnings) logger.Warn(warning);		
			foreach (var error in reference.Errors) logger.Error(error);

			if (reference.HasErrors)
				logger.Write("Cannot create a connection.");
			else
			{
				logger.Write("Click \"Next\" to continue.");
				SelectButton.IsEnabled = true;

				var reset = string.IsNullOrEmpty(Model.BindingName) ||
					!reference.Bindings.Any(b => b.Name == Model.BindingName);
				if (reset)
				{
					Model.BindingName = reference.Bindings.Count > 0
						? reference.Bindings[0].Name : "";
				}

				BindingBox.ItemsSource = reference.Bindings.Select(b => b.Name);
				BindingBox.SelectedItem = Model.BindingName;
			}

			RestartButton.IsEnabled = true;
			Progress.IsIndeterminate = false;
			LogBox.ScrollToEnd();

			if (!reference.HasErrors && !reference.HasWarnings)
			{
				SetVisiblePage(2);
			}		
		}

		Visibility GetPageVisibility(int which)
		{
		    return which == page ? Visibility.Visible : Visibility.Hidden;
		}

		void SetVisiblePage(int p)
		{
			if (p < 0 || p > 2) return;
			page = p;
			Page1.Visibility = GetPageVisibility(0);
			Page2.Visibility = GetPageVisibility(1);
			Page3.Visibility = GetPageVisibility(2);
		}
	}
}
