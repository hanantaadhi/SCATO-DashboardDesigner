using MaterialDesignThemes.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace DashboardDesigner
{
	public partial class Win_About : Window
	{
		private BackgroundWorker bw = new BackgroundWorker();

		private BackgroundWorker bwMsg = new BackgroundWorker();

		private string myver;

		private HttpClient client = new HttpClient();

		public Win_About()
		{
			this.InitializeComponent();
		}

		private void btnCekUpdate_Click(object sender, RoutedEventArgs e)
		{
			this.bw.DoWork += new DoWorkEventHandler(this.Bw_DoWork);
			this.bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.Bw_RunWorkerCompleted);
			this.bw.RunWorkerAsync();
			base.Cursor = Cursors.Wait;
		}



		private void btnUpdateApp_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void Bw_DoWork(object sender, DoWorkEventArgs e)
		{

		}

		private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

		}

		private void BwMsg_DoWork(object sender, DoWorkEventArgs e)
		{
			
		}

		private void BwMsg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			base.Cursor = Cursors.Arrow;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.myver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			this.lblVersion.Text = this.myver;
		}
	}
}