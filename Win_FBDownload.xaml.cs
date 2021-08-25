using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace DashboardDesigner
{
	public partial class Win_FBDownload : Window
	{
		private static FirebaseClient FB_Client;

		private List<MainWindow.clsFBStorage> MyFiles = new List<MainWindow.clsFBStorage>();

		public Win_FBDownload()
		{
			this.InitializeComponent();
		}

		private void btnDownload_Click(object sender, RoutedEventArgs e)
		{
			int id = this.dgFiles.SelectedIndex;
			if (id >= 0)
			{
				string sfile = "";
				SaveFileDialog op = new SaveFileDialog()
				{
					Title = "Save to file",
					Filter = "XML file|*.xml"
				};
				bool? nullable = op.ShowDialog();
				if (nullable.GetValueOrDefault() & nullable.HasValue)
				{
					sfile = op.FileName;
					string slink = this.MyFiles[id].Link;
					try
					{
						(new WebClient()).DownloadFile(slink, sfile);
						MessageBox.Show("Done.", "Download", MessageBoxButton.OK, MessageBoxImage.Asterisk);
					}
					catch (Exception exception)
					{
						Exception ex = exception;
						MessageBox.Show(string.Concat("Error! \r\n", ex.ToString()), "Download", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
				}
			}
		}

		private void dgFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.dgFiles.SelectedIndex < 0)
			{
				this.btnDownload.IsEnabled = false;
			}
			else
			{
				this.btnDownload.IsEnabled = true;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.MyFiles.Clear();
			if (Win_FBDownload.FB_Client == null)
			{
				FirebaseConfig firebaseConfig = new FirebaseConfig();
				firebaseConfig.BasePath = MainWindow.AppWindow.Set_FBStorage_FBPath;
				firebaseConfig.AuthSecret = MainWindow.AppWindow.Set_FBStorage_FBSecret;
				Win_FBDownload.FB_Client = new FirebaseClient(firebaseConfig);
			}
			string sDatas = "";
			try
			{
				FirebaseResponse FB_Response = Win_FBDownload.FB_Client.Get("Dashboard/");
				if (!FB_Response.Body.Equals("null"))
				{
					sDatas = FB_Response.Body;
				}
			}
			catch (Exception exception)
			{
				exception.ToString();
			}
			if (!sDatas.Equals(""))
			{
				this.dgFiles.Items.Clear();
				string[] arDatas = sDatas.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < (int)arDatas.Length; i++)
				{
					if (!arDatas[i].Equals(""))
					{
						if ((!arDatas[i].Contains("Name") || !arDatas[i].Contains("Link") ? false : arDatas[i].Contains("Date")))
						{
							MainWindow.clsFBStorage nfb = new MainWindow.clsFBStorage();
							try
							{
								nfb = JsonConvert.DeserializeObject<MainWindow.clsFBStorage>(string.Concat("{", arDatas[i], "}"));
							}
							catch (Exception exception1)
							{
								exception1.ToString();
							}
							if (nfb.Name != null)
							{
								this.MyFiles.Add(nfb);
								this.dgFiles.Items.Add(nfb);
							}
						}
					}
				}
			}
		}
	}
}