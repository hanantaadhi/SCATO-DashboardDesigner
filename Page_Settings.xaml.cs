using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace DashboardDesigner
{
	public partial class Page_Settings : Page
	{
		private Win_About wAbout;
		public Page_Settings()
		{
			this.InitializeComponent();
		}

		private void btnBrowseStartupFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog op = new OpenFileDialog()
			{
				Title = "Select a file",
				Filter = "XML file|*.xml"
			};
			bool? nullable = op.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				this.txtStartupAutoOpenFile.Text = op.FileName;
			}
		}

		private void btnSaveSet_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.AppWindow.Set_Runtime_PlotInterval = int.Parse(this.txtRuntimePlotInterval.Text);
			MainWindow appWindow = MainWindow.AppWindow;
			bool? isChecked = this.chkRuntimeAutoMaximize.IsChecked;
			appWindow.Set_Runtime_AutoMaximize = isChecked.Value;
			MainWindow value = MainWindow.AppWindow;
			isChecked = this.chkRuntimeAutoFullscreen.IsChecked;
			value.Set_Runtime_AutoFullscreen = isChecked.Value;
			MainWindow.AppWindow.Set_Startup_AutoOpenFile = this.txtStartupAutoOpenFile.Text;
			MainWindow mainWindow = MainWindow.AppWindow;
			isChecked = this.chkStartupAutoActivate.IsChecked;
			mainWindow.Set_Startup_AutoActivate = isChecked.Value;
			MainWindow.AppWindow.Set_FBStorage_Path = this.txtFBStoragePath.Text;
			MainWindow.AppWindow.Set_FBStorage_FBPath = this.txtFBStorageFBPath.Text;
			MainWindow.AppWindow.Set_FBStorage_FBSecret = this.txtFBStorageFBSecret.Password;
			MainWindow.AppWindow.Set_CSV_Separator = this.txtSeparator.Text.Trim();
			MainWindow.AppWindow.SaveSettings();
		}

		private void btnAbout_Click(object sender, RoutedEventArgs e)
		{
			if (this.wAbout != null)
			{
				this.wAbout.Close();
			}
			this.wAbout = new Win_About();
			this.wAbout.Show();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			this.txtRuntimePlotInterval.Text = MainWindow.AppWindow.Set_Runtime_PlotInterval.ToString();
			this.chkRuntimeAutoMaximize.IsChecked = new bool?(MainWindow.AppWindow.Set_Runtime_AutoMaximize);
			this.chkRuntimeAutoFullscreen.IsChecked = new bool?(MainWindow.AppWindow.Set_Runtime_AutoFullscreen);
			this.txtStartupAutoOpenFile.Text = MainWindow.AppWindow.Set_Startup_AutoOpenFile;
			this.chkStartupAutoActivate.IsChecked = new bool?(MainWindow.AppWindow.Set_Startup_AutoActivate);
			this.txtFBStoragePath.Text = MainWindow.AppWindow.Set_FBStorage_Path;
			this.txtFBStorageFBPath.Text = MainWindow.AppWindow.Set_FBStorage_FBPath;
			this.txtFBStorageFBSecret.Password = MainWindow.AppWindow.Set_FBStorage_FBSecret;
			this.txtSeparator.Text = MainWindow.AppWindow.Set_CSV_Separator;
		}
	}
}