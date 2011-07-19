using System;
using System.Linq;
using System.Net;
using System.Windows;
using Driver;

namespace TestWindow
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		void Go_Click(object sender, EventArgs e)
		{
			var info = new ConnectionInfo();
			var model = new ConnectionModel(info) {
				Uri = ""
			};
			new Dialog(info).ShowDialog();

			ServiceURL.Content = model.Uri;
			BindingName.Content = model.BindingName;
		}
	}
}
