using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace DashboardDesigner
{
	public partial class Page_TagMan : Page
	{
		public static Page_TagMan PageTagMan;
		//private string BranchParent;
		private List<PackIcon> MyIcons = new List<PackIcon>();
		private DispatcherTimer timerCekCon = new DispatcherTimer();
		//internal PackIcon iconBtnShowTagValue;

		public Page_TagMan()
		{
			this.InitializeComponent();
			Page_TagMan.PageTagMan = this;
		}

		private void btnAddCon_Click(object sender, RoutedEventArgs e)
		{
			TreeViewItem strv = this.trvCon.SelectedItem as TreeViewItem;
			if (strv.Tag != null)
			{
				this.btnAddCon.IsEnabled = false;
				this.txtConName.Text = "";
				this.txtConInterval.Text = "1";
				this.txtConHeader.Text = "New Connection";
				this.grpConName.Visibility = System.Windows.Visibility.Visible;
				this.trvCon.IsEnabled = false;
				this.pnlConOk.Visibility = System.Windows.Visibility.Visible;
				string tag = strv.Tag as string;
				if (tag != null)
				{
					switch (tag)
					{
						case "ModTCP":
							{
								this.txtModTCPIP.Text = "127.0.0.1";
								this.txtModTCPPort.Text = "502";
								this.grpConModTCP.Visibility = System.Windows.Visibility.Visible;
								break;
							}
						case "ModRTU":
							{
								string[] ports = SerialPort.GetPortNames();
								this.cboModRTUPort.Items.Clear();
								string[] strArrays = ports;
								for (int i = 0; i < (int)strArrays.Length; i++)
								{
									string com = strArrays[i];
									this.cboModRTUPort.Items.Add(com);
								}
								this.cboModRTUBaud.Text = "9600";
								this.cboModRTUParity.Text = "None";
								this.txtModRTUSlave.Text = "1";
								this.grpConModRTU.Visibility = System.Windows.Visibility.Visible;
								break;
							}
						case "MQTT":
							{
								this.txtMqttHost.Text = "localhost";
								this.txtMqttPort.Text = "1883";
								this.txtMqttUser.Text = "";
								this.txtMqttPass.Password = "";
								this.grpConMQTT.Visibility = System.Windows.Visibility.Visible;
								break;
							}
						case "SQL":
							{
								this.cboSqlTarget.Text = "MySQL";
								this.txtSqlHost.Text = "localhost";
								this.txtSqlUser.Text = "";
								this.txtSqlPass.Password = "";
								this.txtSqlDb.Text = "";
								this.grpConSQL.Visibility = System.Windows.Visibility.Visible;
								break;
							}
						case "Firebase":
							{
								this.txtFBPath.Text = "";
								this.txtFBSecret.Password = "";
								this.grpConFirebase.Visibility = System.Windows.Visibility.Visible;
								break;
							}
						case "File":
							{
								this.cboFileType.Text = "Table";
								this.txtFileSeparator.Text = ",";
								this.txtFileName.Text = "";
								this.grpConFile.Visibility = System.Windows.Visibility.Visible;
								break;
							}
					}
				}
			}
		}

		private void btnAddConCancel_Click(object sender, RoutedEventArgs e)
		{
			this.grpConName.Visibility = System.Windows.Visibility.Collapsed;
			this.trvCon.IsEnabled = true;
			this.trvCon_SelectedItemChanged(null, null);
		}

		private void btnAddConOK_Click(object sender, RoutedEventArgs e)
		{
			if (this.txtConHeader.Text == "New Connection")
			{
				if ((this.txtConName.Text == "" ? false : this.txtConInterval.Text != ""))
				{
					TreeViewItem nitem = new TreeViewItem();
					StackPanel nhead = new StackPanel()
					{
						Orientation = Orientation.Horizontal
					};
					TextBlock ntxt = new TextBlock()
					{
						Text = this.txtConName.Text,
						Margin = new Thickness(10, 0, 10, 0)
					};
					PackIcon nicon = new PackIcon();
					//nicon.set_Kind(2814);
					nicon.Kind = PackIconKind.MenuDown;
					nicon.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					this.MyIcons.Add(nicon);
					nhead.Children.Add(nicon);
					nhead.Children.Add(ntxt);
					nitem.Header = nhead;
					TreeViewItem strv = this.trvCon.SelectedItem as TreeViewItem;
					if (strv.Tag != null)
					{
						string tag = strv.Tag as string;
						if (tag != null)
						{
							if (tag == "ModTCP")
							{
								if ((this.txtModTCPIP.Text == "" ? false : this.txtModTCPPort.Text != ""))
								{
									if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.txtConName.Text) < 0)
									{
										nitem.Tag = "Con_ModTCP";
										this.trvConModTCP.Items.Add(nitem);
										this.trvConModTCP.IsExpanded = true;
										nitem.IsSelected = true;
										MainWindow.clsConnection ncon = new MainWindow.clsConnection()
										{
											Type = "ModbusTCP",
											Name = this.txtConName.Text,
											Interval = this.txtConInterval.Text,
											IP = this.txtModTCPIP.Text,
											Port = this.txtModTCPPort.Text
										};
										MainWindow.AppWindow.MyConnections.Add(ncon);
										this.btnAddConCancel_Click(null, null);
									}
									else
									{
										MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
									}
								}
							}
							else if (tag == "ModRTU")
							{
								if ((!(this.cboModRTUPort.Text != "") || !(this.cboModRTUBaud.Text != "") || !(this.cboModRTUParity.Text != "") ? false : this.txtModRTUSlave.Text != ""))
								{
									if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.txtConName.Text) < 0)
									{
										nitem.Tag = "Con_ModRTU";
										this.trvConModRTU.Items.Add(nitem);
										this.trvConModRTU.IsExpanded = true;
										nitem.IsSelected = true;
										MainWindow.clsConnection ncon = new MainWindow.clsConnection()
										{
											Type = "ModbusRTU",
											Name = this.txtConName.Text,
											Interval = this.txtConInterval.Text,
											Port = this.cboModRTUPort.Text,
											Baud = this.cboModRTUBaud.Text,
											Parity = this.cboModRTUParity.Text,
											Slave = this.txtModRTUSlave.Text
										};
										MainWindow.AppWindow.MyConnections.Add(ncon);
										this.btnAddConCancel_Click(null, null);
									}
									else
									{
										MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
									}
								}
							}
							else if (tag == "MQTT")
							{
								if ((this.txtMqttHost.Text == "" ? false : this.txtMqttPort.Text != ""))
								{
									if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.txtConName.Text) < 0)
									{
										nitem.Tag = "Con_MQTT";
										this.trvConMQTT.Items.Add(nitem);
										this.trvConMQTT.IsExpanded = true;
										nitem.IsSelected = true;
										MainWindow.clsConnection ncon = new MainWindow.clsConnection()
										{
											Type = "MQTT",
											Name = this.txtConName.Text,
											Interval = this.txtConInterval.Text,
											Host = this.txtMqttHost.Text,
											Port = this.txtMqttPort.Text,
											User = this.txtMqttUser.Text,
											Pass = this.txtMqttPass.Password
										};
										MainWindow.AppWindow.MyConnections.Add(ncon);
										this.btnAddConCancel_Click(null, null);
									}
									else
									{
										MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
									}
								}
							}
							else if (tag == "SQL")
							{
								if ((!(this.cboSqlTarget.Text != "") || !(this.txtSqlHost.Text != "") || !(this.txtSqlUser.Text != "") ? false : this.txtSqlDb.Text != ""))
								{
									if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.txtConName.Text) < 0)
									{
										nitem.Tag = "Con_SQL";
										this.trvConSQL.Items.Add(nitem);
										this.trvConSQL.IsExpanded = true;
										nitem.IsSelected = true;
										MainWindow.clsConnection ncon = new MainWindow.clsConnection()
										{
											Type = "SQL",
											Name = this.txtConName.Text,
											Interval = this.txtConInterval.Text,
											Target = this.cboSqlTarget.Text,
											Host = this.txtSqlHost.Text,
											User = this.txtSqlUser.Text,
											Pass = this.txtSqlPass.Password,
											Database = this.txtSqlDb.Text
										};
										MainWindow.AppWindow.MyConnections.Add(ncon);
										this.btnAddConCancel_Click(null, null);
									}
									else
									{
										MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
									}
								}
							}
							else if (tag != "Firebase")
							{
								if (tag == "File")
								{
									if ((this.txtFileSeparator.Text == "" ? false : this.txtFileName.Text != ""))
									{
										if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.txtConName.Text) < 0)
										{
											nitem.Tag = "Con_File";
											this.trvConFile.Items.Add(nitem);
											this.trvConFile.IsExpanded = true;
											nitem.IsSelected = true;
											MainWindow.clsConnection ncon = new MainWindow.clsConnection()
											{
												Type = "File",
												Name = this.txtConName.Text,
												Interval = this.txtConInterval.Text,
												FileType = this.cboFileType.Text,
												FileSeparator = this.txtFileSeparator.Text,
												FileName = this.txtFileName.Text
											};
											MainWindow.AppWindow.MyConnections.Add(ncon);
											this.btnAddConCancel_Click(null, null);
										}
										else
										{
											MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
										}
									}
								}
							}
							else if ((this.txtFBPath.Text == "" ? false : this.txtFBSecret.Password != ""))
							{
								if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.txtConName.Text) < 0)
								{
									nitem.Tag = "Con_Firebase";
									this.trvConFirebase.Items.Add(nitem);
									this.trvConFirebase.IsExpanded = true;
									nitem.IsSelected = true;
									MainWindow.clsConnection ncon = new MainWindow.clsConnection()
									{
										Type = "Firebase",
										Name = this.txtConName.Text,
										Interval = this.txtConInterval.Text,
										FBPath = this.txtFBPath.Text,
										FBSecret = this.txtFBSecret.Password
									};
									MainWindow.AppWindow.MyConnections.Add(ncon);
									this.btnAddConCancel_Click(null, null);
								}
								else
								{
									MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
								}
							}
						}
					}
				}
			}
			else if (this.txtConHeader.Text == "Edit Connection")
			{
				if ((this.txtConName.Text == "" ? false : this.txtConInterval.Text != ""))
				{
					TreeViewItem strv = this.trvCon.SelectedItem as TreeViewItem;
					TextBlock ntxt = (strv.Header as StackPanel).Children[1] as TextBlock;
					string text = ntxt.Text;
					if (strv.Tag != null)
					{
						string str = strv.Tag as string;
						if (str != null)
						{
							if (str == "Con_ModTCP")
							{
								if ((this.txtModTCPIP.Text == "" ? false : this.txtModTCPPort.Text != ""))
								{
									int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
									if (id >= 0)
									{
										string str1 = this.txtConName.Text;
										bool bOk = true;
										if (text != str1)
										{
											if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == str1) >= 0)
											{
												MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
												bOk = false;
											}
										}
										if (bOk)
										{
											ntxt.Text = this.txtConName.Text;
											MainWindow.clsConnection ncon = MainWindow.AppWindow.MyConnections[id];
											ncon.Name = this.txtConName.Text;
											ncon.Interval = this.txtConInterval.Text;
											ncon.IP = this.txtModTCPIP.Text;
											ncon.Port = this.txtModTCPPort.Text;
											this.btnAddConCancel_Click(null, null);
										}
									}
								}
							}
							else if (str == "Con_ModRTU")
							{
								if ((!(this.cboModRTUPort.Text != "") || !(this.cboModRTUBaud.Text != "") || !(this.cboModRTUParity.Text != "") ? false : this.txtModRTUSlave.Text != ""))
								{
									int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
									if (id >= 0)
									{
										string text2 = this.txtConName.Text;
										bool bOk = true;
										if (text != text2)
										{
											if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text2) >= 0)
											{
												MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
												bOk = false;
											}
										}
										if (bOk)
										{
											ntxt.Text = this.txtConName.Text;
											MainWindow.clsConnection ncon = MainWindow.AppWindow.MyConnections[id];
											ncon.Name = this.txtConName.Text;
											ncon.Interval = this.txtConInterval.Text;
											ncon.Port = this.cboModRTUPort.Text;
											ncon.Baud = this.cboModRTUBaud.Text;
											ncon.Parity = this.cboModRTUParity.Text;
											ncon.Slave = this.txtModRTUSlave.Text;
											this.btnAddConCancel_Click(null, null);
										}
									}
								}
							}
							else if (str == "Con_MQTT")
							{
								if ((this.txtMqttHost.Text == "" ? false : this.txtMqttPort.Text != ""))
								{
									int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
									if (id >= 0)
									{
										string text3 = this.txtConName.Text;
										bool bOk = true;
										if (text != text3)
										{
											if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text3) >= 0)
											{
												MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
												bOk = false;
											}
										}
										if (bOk)
										{
											ntxt.Text = this.txtConName.Text;
											MainWindow.clsConnection ncon = MainWindow.AppWindow.MyConnections[id];
											ncon.Name = this.txtConName.Text;
											ncon.Interval = this.txtConInterval.Text;
											ncon.Host = this.txtMqttHost.Text;
											ncon.Port = this.txtMqttPort.Text;
											ncon.User = this.txtMqttUser.Text;
											ncon.Pass = this.txtMqttPass.Password;
											this.btnAddConCancel_Click(null, null);
										}
									}
								}
							}
							else if (str == "Con_SQL")
							{
								if ((!(this.cboSqlTarget.Text != "") || !(this.txtSqlHost.Text != "") || !(this.txtSqlUser.Text != "") ? false : this.txtSqlDb.Text != ""))
								{
									int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
									if (id >= 0)
									{
										string str3 = this.txtConName.Text;
										bool bOk = true;
										if (text != str3)
										{
											if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == str3) >= 0)
											{
												MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
												bOk = false;
											}
										}
										if (bOk)
										{
											ntxt.Text = this.txtConName.Text;
											MainWindow.clsConnection ncon = MainWindow.AppWindow.MyConnections[id];
											ncon.Name = this.txtConName.Text;
											ncon.Interval = this.txtConInterval.Text;
											ncon.Target = this.cboSqlTarget.Text;
											ncon.Host = this.txtSqlHost.Text;
											ncon.User = this.txtSqlUser.Text;
											ncon.Pass = this.txtSqlPass.Password;
											ncon.Database = this.txtSqlDb.Text;
											this.btnAddConCancel_Click(null, null);
										}
									}
								}
							}
							else if (str != "Con_Firebase")
							{
								if (str == "Con_File")
								{
									if ((this.txtFileSeparator.Text == "" ? false : this.txtFileName.Text != ""))
									{
										int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
										if (id >= 0)
										{
											string text4 = this.txtConName.Text;
											bool bOk = true;
											if (text != text4)
											{
												if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text4) >= 0)
												{
													MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
													bOk = false;
												}
											}
											if (bOk)
											{
												ntxt.Text = this.txtConName.Text;
												MainWindow.clsConnection ncon = MainWindow.AppWindow.MyConnections[id];
												ncon.Name = this.txtConName.Text;
												ncon.Interval = this.txtConInterval.Text;
												ncon.FileType = this.cboFileType.Text;
												ncon.FileSeparator = this.txtFileSeparator.Text;
												ncon.FileName = this.txtFileName.Text;
												this.btnAddConCancel_Click(null, null);
											}
										}
									}
								}
							}
							else if ((this.txtFBPath.Text == "" ? false : this.txtFBSecret.Password != ""))
							{
								int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									string str4 = this.txtConName.Text;
									bool bOk = true;
									if (text != str4)
									{
										if (MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == str4) >= 0)
										{
											MessageBox.Show("Duplicate Connection Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
											bOk = false;
										}
									}
									if (bOk)
									{
										ntxt.Text = this.txtConName.Text;
										MainWindow.clsConnection ncon = MainWindow.AppWindow.MyConnections[id];
										ncon.Name = this.txtConName.Text;
										ncon.Interval = this.txtConInterval.Text;
										ncon.FBPath = this.txtFBPath.Text;
										ncon.FBSecret = this.txtFBSecret.Password;
										this.btnAddConCancel_Click(null, null);
									}
								}
							}
						}
					}
				}
			}
			else if (this.txtConHeader.Text == "Delete Connection")
			{
				TreeViewItem strv = this.trvCon.SelectedItem as TreeViewItem;
				StackPanel nhead = strv.Header as StackPanel;
				string text5 = (nhead.Children[1] as TextBlock).Text;
				if (strv.Tag != null)
				{
					int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text5);
					if (id >= 0)
					{
						MainWindow.AppWindow.MyConnections.RemoveAt(id);
						string tag1 = strv.Tag as string;
						if (tag1 != null)
						{
							switch (tag1)
							{
								case "Con_ModTCP":
									{
										this.trvConModTCP.Items.Remove(strv);
										break;
									}
								case "Con_ModRTU":
									{
										this.trvConModRTU.Items.Remove(strv);
										break;
									}
								case "Con_MQTT":
									{
										this.trvConMQTT.Items.Remove(strv);
										break;
									}
								case "Con_SQL":
									{
										this.trvConSQL.Items.Remove(strv);
										break;
									}
								case "Con_Firebase":
									{
										this.trvConFirebase.Items.Remove(strv);
										break;
									}
								case "Con_File":
									{
										this.trvConFile.Items.Remove(strv);
										break;
									}
							}
						}
						this.btnAddConCancel_Click(null, null);
					}
				}
			}
		}

		private void btnAddTag_Click(object sender, RoutedEventArgs e)
		{
			this.txtTagName.Text = "";
			this.cboTagCon.Items.Clear();
			foreach (MainWindow.clsConnection con in MainWindow.AppWindow.MyConnections)
			{
				this.cboTagCon.Items.Add(con.Name);
			}
			this.cboTagCon.SelectedItem = null;
			this.txtTagAddress.Text = "";
			this.pnlDataType.Visibility = System.Windows.Visibility.Collapsed;
			this.txtAddTagMode.Text = "Add";
			this.dgTags.IsEnabled = false;
			this.btnAddTag.IsEnabled = false;
			this.btnCopyTag.IsEnabled = false;
			this.btnEditTag.IsEnabled = false;
			this.btnDelTag.IsEnabled = false;
			this.pnlAddTag.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnAddTagCancel_Click(object sender, RoutedEventArgs e)
		{
			this.dgTags.IsEnabled = true;
			this.btnAddTag.IsEnabled = true;
			this.pnlAddTag.Visibility = System.Windows.Visibility.Collapsed;
			this.dgTags.SelectedIndex = -1;
		}

		private void btnAddTagOK_Click(object sender, RoutedEventArgs e)
		{
			if ((!(this.txtTagName.Text != "") || this.cboTagCon.SelectedItem == null ? false : this.txtTagAddress.Text != ""))
			{
				if (this.txtAddTagMode.Text == "Add")
				{
					string text = this.txtTagName.Text;
					if (MainWindow.AppWindow.MyTags.FindIndex((MainWindow.clsTag a) => a.Name == text) < 0)
					{
						MainWindow.clsTag tag = new MainWindow.clsTag()
						{
							Name = this.txtTagName.Text,
							Connection = this.cboTagCon.SelectedValue.ToString(),
							Address = this.txtTagAddress.Text,
							DataType = this.cboDataType.Text
						};
						MainWindow.AppWindow.MyTags.Add(tag);
						this.dgTags.Items.Add(tag);
						this.btnAddTagCancel_Click(null, null);
					}
					else
					{
						MessageBox.Show("Duplicate Tag Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
				}
				else if (this.txtAddTagMode.Text == "Edit")
				{
					DataGridCellInfo cellInfo = this.dgTags.SelectedCells[0];
					string name = (cellInfo.Item as MainWindow.clsTag).Name;
					string str = this.txtTagName.Text;
					bool bOk = true;
					if (name != str)
					{
						if (MainWindow.AppWindow.MyTags.FindIndex((MainWindow.clsTag a) => a.Name == str) >= 0)
						{
							MessageBox.Show("Duplicate Tag Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
							bOk = false;
						}
					}
					if (bOk)
					{
						int id = MainWindow.AppWindow.MyTags.FindIndex((MainWindow.clsTag a) => a.Name == name);
						if (id >= 0)
						{
							MainWindow.clsTag tag = MainWindow.AppWindow.MyTags[id];
							tag.Name = str;
							tag.Connection = this.cboTagCon.SelectedValue.ToString();
							tag.Address = this.txtTagAddress.Text;
							tag.DataType = this.cboDataType.Text;
							int currentRowIndex = this.dgTags.Items.IndexOf(this.dgTags.SelectedItem);
							this.dgTags.Items[currentRowIndex] = new MainWindow.clsTag()
							{
								Name = tag.Name,
								Connection = tag.Connection,
								Address = tag.Address
							};
							this.btnAddTagCancel_Click(null, null);
						}
					}
				}
				else if (this.txtAddTagMode.Text == "Delete")
				{
					DataGridCellInfo cellInfo = this.dgTags.SelectedCells[0];
					string name1 = (cellInfo.Item as MainWindow.clsTag).Name;
					int id = MainWindow.AppWindow.MyTags.FindIndex((MainWindow.clsTag a) => a.Name == name1);
					if (id >= 0)
					{
						MainWindow.AppWindow.MyTags.RemoveAt(id);
						this.dgTags.Items.Remove(this.dgTags.SelectedItem);
						this.btnAddTagCancel_Click(null, null);
					}
				}
			}
		}

		//private void btnBrowseOPC_Click(object sender, RoutedEventArgs e)
		//{
		//	object serverList;
		//	if (this.txtOPCHost.Text.Trim() != "")
		//	{
		//		Mouse.OverrideCursor = Cursors.Wait;
		//		try
		//		{
		//			OPCServer opcsvr = (OPCServer)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("28E68F9A-8D75-11D1-8DC3-3C302A000000")));
		//			serverList = ((this.txtOPCHost.Text.Trim().ToLower() == "localhost" ? false : this.txtOPCHost.Text.Trim().ToLower() != "127.0.0.1") ? opcsvr.GetOPCServers(this.txtOPCHost.Text.Trim()) : opcsvr.GetOPCServers(Type.Missing));
		//			this.lstOPCServers.Items.Clear();
		//			foreach (string server in (Array)serverList)
		//			{
		//				this.lstOPCServers.Items.Add(server);
		//			}
		//			this.pnlOPCBrowser.Visibility = System.Windows.Visibility.Visible;
		//			this.pnlOPCTagBrowser.Visibility = System.Windows.Visibility.Collapsed;
		//			this.pnlTag.Visibility = System.Windows.Visibility.Collapsed;
		//		}
		//		catch (Exception exception)
		//		{
		//			Exception ex = exception;
		//			MessageBox.Show(string.Concat("Error Browse OPC Server! \n\r", ex.Message));
		//		}
		//		Mouse.OverrideCursor = Cursors.Arrow;
		//	}
		//}

		//private void btnBrowseOPCTag_Click(object sender, RoutedEventArgs e)
		//{
		//	if (this.cboTagCon.Text.Trim() != "")
		//	{
		//		Mouse.OverrideCursor = Cursors.Wait;
		//		try
		//		{
		//			int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.cboTagCon.Text);
		//			if (id >= 0)
		//			{
		//				this.trvOPCServer.Header = MainWindow.AppWindow.MyConnections[id].Server;
		//				TreeViewItem tItem = (TreeViewItem)this.trvServer.Items[0];
		//				this.MyOPCServer = (OPCServer)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("28E68F9A-8D75-11D1-8DC3-3C302A000000")));
		//				this.MyOPCServer.Connect(MainWindow.AppWindow.MyConnections[id].Server, Type.Missing);
		//				if (this.MyOPCServer.ServerState != 1)
		//				{
		//					MessageBox.Show("Error Browse OPC Tags! \r\n");
		//				}
		//				else
		//				{
		//					this.MyOPCBrowser = this.MyOPCServer.CreateBrowser();
		//					this.MyOPCBrowser.MoveToRoot();
		//					this.ShowBranchsAndLeafs(this.MyOPCServer, tItem);
		//				}
		//				this.pnlOPCTagBrowser.Visibility = System.Windows.Visibility.Visible;
		//				this.pnlOPCBrowser.Visibility = System.Windows.Visibility.Collapsed;
		//				this.pnlTag.Visibility = System.Windows.Visibility.Collapsed;
		//			}
		//		}
		//		catch (Exception exception)
		//		{
		//			Exception ex = exception;
		//			MessageBox.Show(string.Concat("Error Browse OPC Tags! \n\r", ex.Message));
		//		}
		//		Mouse.OverrideCursor = Cursors.Arrow;
		//	}
		//}

		//private void btnCancelBrowseOPC_Click(object sender, RoutedEventArgs e)
		//{
		//	this.pnlOPCBrowser.Visibility = System.Windows.Visibility.Collapsed;
		//	this.pnlTag.Visibility = System.Windows.Visibility.Visible;
		//}

		//private void btnCancelBrowseOPCTag_Click(object sender, RoutedEventArgs e)
		//{
		//	if (this.MyOPCServer.ServerState == 1)
		//	{
		//		this.MyOPCServer.Disconnect();
		//		this.MyOPCServer = null;
		//		this.BranchParent = "";
		//	}
		//	this.pnlOPCTagBrowser.Visibility = System.Windows.Visibility.Collapsed;
		//	this.pnlTag.Visibility = System.Windows.Visibility.Visible;
		//}

		private void btnCopyTag_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.clsTag ntag = this.dgTags.SelectedCells[0].Item as MainWindow.clsTag;
			string name = ntag.Name;
			string slast = name.Substring(name.Length - 1, 1);
			if ((slast == "0" || slast == "1" || slast == "2" || slast == "3" || slast == "4" || slast == "5" || slast == "6" || slast == "7" || slast == "8" ? false : slast != "9"))
			{
				name = string.Concat(name, "1");
			}
			else
			{
				int ilast = int.Parse(slast) + 1;
				name = string.Concat(name.Substring(0, name.Length - 1), ilast.ToString());
			}

			if (MainWindow.AppWindow.MyTags.FindIndex((MainWindow.clsTag a) => a.Name == name) < 0)
			{
				MainWindow.clsTag tag = new MainWindow.clsTag()
				{
					Name = name,
					Connection = ntag.Connection,
					Address = ntag.Address,
					DataType = ntag.DataType
				};
				MainWindow.AppWindow.MyTags.Add(tag);
				this.dgTags.Items.Add(tag);
				this.btnAddTagCancel_Click(null, null);
			}
			else
			{
				MessageBox.Show("Duplicate Tag Name !", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		private void btnDelCon_Click(object sender, RoutedEventArgs e)
		{
			this.txtConHeader.Text = "Delete Connection";
			this.pnlConOk.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnDelTag_Click(object sender, RoutedEventArgs e)
		{
			this.btnEditTag_Click(null, null);
			this.txtAddTagMode.Text = "Delete";
		}

		private void btnEditCon_Click(object sender, RoutedEventArgs e)
		{
			this.txtConHeader.Text = "Edit Connection";
			this.pnlConOk.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnEditTag_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.clsTag item = this.dgTags.SelectedCells[0].Item as MainWindow.clsTag;
			this.txtTagName.Text = item.Name;
			this.cboTagCon.Items.Clear();
			foreach (MainWindow.clsConnection con in MainWindow.AppWindow.MyConnections)
			{
				this.cboTagCon.Items.Add(con.Name);
			}
			this.cboTagCon.SelectedItem = item.Connection;
			int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == item.Connection);
			if (id >= 0)
			{
				string stype = MainWindow.AppWindow.MyConnections[id].Type;
				if ((stype == "ModbusTCP" ? true : stype == "ModbusRTU"))
				{
					this.pnlDataType.Visibility = System.Windows.Visibility.Visible;
					this.cboDataType.Items.Clear();
					this.cboDataType.Items.Add("Boolean");
					this.cboDataType.Items.Add("Integer");
					this.cboDataType.Items.Add("Float");
				}
				/*
				else if (stype == "IEC104")
				{
					this.pnlDataType.Visibility = System.Windows.Visibility.Visible;
					this.cboDataType.Items.Clear();
					this.cboDataType.Items.Add("Single Point");
					this.cboDataType.Items.Add("Double Point");
					this.cboDataType.Items.Add("Measurement Float");
				}
				*/
				else if (stype != "SQL")
				{
					this.pnlDataType.Visibility = System.Windows.Visibility.Collapsed;
				}
				else
				{
					this.pnlDataType.Visibility = System.Windows.Visibility.Visible;
					this.cboDataType.Items.Clear();
					this.cboDataType.Items.Add("String");
					this.cboDataType.Items.Add("Integer");
					this.cboDataType.Items.Add("Float");
				}
			}
			this.txtTagAddress.Text = item.Address;
			this.cboDataType.Text = item.DataType;
			this.txtAddTagMode.Text = "Edit";
			this.dgTags.IsEnabled = false;
			this.btnAddTag.IsEnabled = false;
			this.btnCopyTag.IsEnabled = false;
			this.btnEditTag.IsEnabled = false;
			this.btnDelTag.IsEnabled = false;
			this.pnlAddTag.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnExportTag_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sv = new SaveFileDialog()
			{
				Filter = "CSV file (*.csv)|*.csv"
			};
			bool? nullable = sv.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				string sfile = sv.FileName;
				string sp = MainWindow.AppWindow.Set_CSV_Separator;
				try
				{
					string sprint = string.Concat(new string[] { "Name", sp, "Connection", sp, "Address,DataType\r\n" });
					foreach (MainWindow.clsTag tag in MainWindow.AppWindow.MyTags)
					{
						sprint = string.Concat(new string[] { sprint, tag.Name, sp, tag.Connection, sp, tag.Address, sp, tag.DataType, "\r\n" });
					}
					File.WriteAllText(sfile, sprint);
				}
				catch (Exception exception)
				{
					Exception ex = exception;
					MessageBox.Show(string.Concat("Error Export !\r\n", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
		}

		private void btnFileBrowse_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog op = new OpenFileDialog()
			{
				Title = "Select a file",
				Filter = "CSV file (*.csv)|*.csv| Text file (*.txt)|*.txt| All file (*.*)|*.*"
			};
			bool? nullable = op.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				this.txtFileName.Text = op.FileName;
			}
		}

		private void btnImportTag_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog op = new OpenFileDialog()
			{
				Title = "Select a file",
				Filter = "CSV file (*.csv)|*.csv"
			};
			bool? nullable = op.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				try
				{
					string sfile = op.FileName;
					string sduplicates = "";
					char sp = char.Parse(MainWindow.AppWindow.Set_CSV_Separator);
					foreach (string line in File.ReadAllLines(sfile).Skip<string>(1))
					{
						string[] ardata = line.Split(new char[] { sp });
						if ((int)ardata.Length >= 4)
						{
							string str = ardata[0];
							if (MainWindow.AppWindow.MyTags.FindIndex((MainWindow.clsTag a) => a.Name == str) < 0)
							{
								MainWindow.clsTag tag = new MainWindow.clsTag()
								{
									Name = ardata[0],
									Connection = ardata[1],
									Address = ardata[2],
									DataType = ardata[3]
								};
								MainWindow.AppWindow.MyTags.Add(tag);
								this.dgTags.Items.Add(tag);
							}
							else
							{
								sduplicates = string.Concat(sduplicates, str, ",");
							}
						}
					}
					if (sduplicates != "")
					{
						MessageBox.Show(string.Concat("Duplicate Tag name !\r\n", sduplicates), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
				}
				catch (Exception exception)
				{
					Exception ex = exception;
					MessageBox.Show(string.Concat("Error Import !\r\n", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
		}

		//private void btnSelectOPCServer_Click(object sender, RoutedEventArgs e)
		//{
		//	int idx = this.lstOPCServers.SelectedIndex;
		//	if (idx >= 0)
		//	{
		//		this.txtOPCServer.Text = this.lstOPCServers.Items[idx].ToString();
		//		this.btnCancelBrowseOPC_Click(null, null);
		//	}
		//}

		//private void btnSelectOPCTags_Click(object sender, RoutedEventArgs e)
		//{
		//	TreeViewItem tItem = (TreeViewItem)this.trvServer.SelectedItem;
		//	if (tItem != null)
		//	{
		//		Mouse.SetCursor(Cursors.Wait);
		//		string sTag = tItem.Tag.ToString();
		//		string[] arTags = sTag.Split(new char[] { '#' });
		//		string sType = arTags[0];
		//		string sName = arTags[1];
		//		if (sType == "Tag")
		//		{
		//			string itemName = arTags[2];
		//			this.txtTagAddress.Text = itemName;
		//		}
		//		this.btnCancelBrowseOPCTag_Click(null, null);
		//	}
		//}

		private void btnShowValue_Click(object sender, RoutedEventArgs e)
		{
			if (this.colTagValue.Visibility != System.Windows.Visibility.Collapsed)
			{
				this.colTagValue.Visibility = System.Windows.Visibility.Collapsed;
				//this.iconBtnShowTagValue.Kind = 1801;
				iconBtnShowTagValue.Kind = PackIconKind.FileDocumentBoxMultipleOutline;			}
			else
			{
				this.colTagValue.Visibility = System.Windows.Visibility.Visible;
				//this.iconBtnShowTagValue.Kind(1800);
				iconBtnShowTagValue.Kind = PackIconKind.FileDocumentBoxMultiple;
			}
		}

		private void cboTagCon_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.cboTagCon.SelectedIndex >= 0)
			{
				int id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == this.cboTagCon.SelectedItem.ToString());
				if (id < 0)
				{
					//this.btnBrowseOPCTag.Visibility = System.Windows.Visibility.Collapsed;
				}
				else
				{
					string stype = MainWindow.AppWindow.MyConnections[id].Type;

					if ((stype == "ModbusTCP" ? true : stype == "ModbusRTU"))
					{
						this.pnlDataType.Visibility = System.Windows.Visibility.Visible;
						this.cboDataType.Items.Clear();
						this.cboDataType.Items.Add("Boolean");
						this.cboDataType.Items.Add("Integer");
						this.cboDataType.Items.Add("Float");
						this.cboDataType.Text = "Integer";
					}
					/*
					else if (stype == "IEC104")
					{
						this.pnlDataType.Visibility = System.Windows.Visibility.Visible;
						this.cboDataType.Items.Clear();
						this.cboDataType.Items.Add("Single Point");
						this.cboDataType.Items.Add("Double Point");
						this.cboDataType.Items.Add("Measurement Float");
					}
					*/

					else if (stype != "SQL")
					{
						this.pnlDataType.Visibility = System.Windows.Visibility.Collapsed;
						this.cboDataType.Text = "";
						this.cboDataType.Items.Clear();
					}
					else
					{
						this.pnlDataType.Visibility = System.Windows.Visibility.Visible;
						this.cboDataType.Items.Clear();
						this.cboDataType.Items.Add("String");
						this.cboDataType.Items.Add("Integer");
						this.cboDataType.Items.Add("Float");
					}
				}
			}
		}

		private void dgTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.dgTags.SelectedIndex < 0)
			{
				this.btnCopyTag.IsEnabled = false;
				this.btnEditTag.IsEnabled = false;
				this.btnDelTag.IsEnabled = false;
			}
			else
			{
				this.btnCopyTag.IsEnabled = true;
				this.btnEditTag.IsEnabled = true;
				this.btnDelTag.IsEnabled = true;
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			//this.trvConOPC.Items.Clear();
			this.trvConModRTU.Items.Clear();
			this.trvConModTCP.Items.Clear();
			//this.trvConIEC.Items.Clear();
			this.trvConMQTT.Items.Clear();
			this.trvConFirebase.Items.Clear();
			this.trvConSQL.Items.Clear();
			this.trvConFile.Items.Clear();
			this.MyIcons.Clear();
			foreach (MainWindow.clsConnection con in MainWindow.AppWindow.MyConnections)
			{
				TreeViewItem nitem = new TreeViewItem();
				StackPanel nhead = new StackPanel()
				{
					Orientation = Orientation.Horizontal
				};
				TextBlock ntxt = new TextBlock()
				{
					Text = con.Name,
					Margin = new Thickness(10, 0, 10, 0)
				};
				PackIcon nicon = new PackIcon();
				//nicon.Kind(2814);
				nicon.Kind = PackIconKind.MenuDown;
				nicon.VerticalAlignment = VerticalAlignment.Center;
				this.MyIcons.Add(nicon);
				nhead.Children.Add(nicon);
				nhead.Children.Add(ntxt);
				nitem.Header = nhead;
				//if (con.Type == "OPC")
				//{
				//	nitem.Tag = "Con_OPC";
				//	//this.trvConOPC.Items.Add(nitem);
				//}
				if (con.Type == "ModbusTCP")
				{
					nitem.Tag = "Con_ModTCP";
					this.trvConModTCP.Items.Add(nitem);
				}
				else if (con.Type == "ModbusRTU")
				{
					nitem.Tag = "Con_ModRTU";
					this.trvConModRTU.Items.Add(nitem);
				}
				//else if (con.Type == "IEC104")
				//{
				//	nitem.Tag = "Con_IEC104";
				//	this.trvConIEC.Items.Add(nitem);
				//}
				else if (con.Type == "MQTT")
				{
					nitem.Tag = "Con_MQTT";
					this.trvConMQTT.Items.Add(nitem);
				}
				else if (con.Type == "SQL")
				{
					nitem.Tag = "Con_SQL";
					this.trvConSQL.Items.Add(nitem);
				}
				else if (con.Type == "Firebase")
				{
					nitem.Tag = "Con_Firebase";
					this.trvConFirebase.Items.Add(nitem);
				}
				else if (con.Type == "File")
				{
					nitem.Tag = "Con_File";
					this.trvConFile.Items.Add(nitem);
				}
			}
			this.dgTags.Items.Clear();
			foreach (MainWindow.clsTag tag in MainWindow.AppWindow.MyTags)
			{
				this.dgTags.Items.Add(new MainWindow.clsTag()
				{
					Name = tag.Name,
					Connection = tag.Connection,
					Address = tag.Address,
					DataType = tag.DataType,
					Value = tag.Value
				});
			}
			string[] ports = SerialPort.GetPortNames();
			this.cboModRTUPort.Items.Clear();
			string[] strArrays = ports;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string com = strArrays[i];
				this.cboModRTUPort.Items.Add(com);
			}
			this.timerCekCon.Interval = TimeSpan.FromSeconds(1);
			this.timerCekCon.Tick += new EventHandler(this.TimerCekCon_Tick);
			this.timerCekCon.Start();
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			this.timerCekCon.Stop();
			this.timerCekCon.Tick -= new EventHandler(this.TimerCekCon_Tick);
		}

		//private void ShowBranchsAndLeafs(OPCServer MyServer, TreeViewItem tItem)
		//{
		//	this.MyOPCBrowser.ShowBranches();
		//	PackIcon picon = new PackIcon();
		//	tItem.Items.Clear();
		//	foreach (object turn in this.MyOPCBrowser)
		//	{
		//		string sBranch = turn.ToString();
		//		TreeViewItem nItem = new TreeViewItem()
		//		{
		//			Tag = string.Concat(new string[] { "Folder#", sBranch, "#", this.BranchParent, "#", MyServer.ServerName })
		//		};
		//		StackPanel stackPanel1 = new StackPanel()
		//		{
		//			Orientation = Orientation.Horizontal
		//		};
		//		PackIcon packIcon = new PackIcon();
		//		packIcon.Kind(2099);
		//		picon = packIcon;
		//		TextBlock textBox1 = new TextBlock()
		//		{
		//			Text = sBranch,
		//			Margin = new Thickness(5, 0, 10, 0)
		//		};
		//		stackPanel1.Children.Add(picon);
		//		stackPanel1.Children.Add(textBox1);
		//		nItem.Header = stackPanel1;
		//		tItem.Items.Add(nItem);
		//	}
		//	this.MyOPCBrowser.ShowLeafs(false);
		//	foreach (object turn in this.MyOPCBrowser)
		//	{
		//		string sBranch = turn.ToString();
		//		string sItemID = this.MyOPCBrowser.GetItemID(turn.ToString());
		//		TreeViewItem nItem = new TreeViewItem()
		//		{
		//			Tag = string.Concat(new string[] { "Tag#", sBranch, "#", sItemID, "#", MyServer.ServerName })
		//		};
		//		StackPanel stackPanel1 = new StackPanel()
		//		{
		//			Orientation = Orientation.Horizontal
		//		};
		//		PackIcon packIcon1 = new PackIcon();
		//		packIcon1.Kind(4226);
		//		picon = packIcon1;
		//		TextBlock textBox1 = new TextBlock()
		//		{
		//			Text = sBranch,
		//			Margin = new Thickness(5, 0, 10, 0)
		//		};
		//		stackPanel1.Children.Add(picon);
		//		stackPanel1.Children.Add(textBox1);
		//		nItem.Header = stackPanel1;
		//		tItem.Items.Add(nItem);
		//	}
		//}

		private void TimerCekCon_Tick(object sender, EventArgs e)
		{
			if (!MainWindow.AppWindow.RuntimeisActive)
			{
				this.btnAddTag.Visibility = System.Windows.Visibility.Visible;
				this.btnCopyTag.Visibility = System.Windows.Visibility.Visible;
				this.btnEditTag.Visibility = System.Windows.Visibility.Visible;
				this.btnDelTag.Visibility = System.Windows.Visibility.Visible;
				this.btnAddCon.Visibility = System.Windows.Visibility.Visible;
				this.btnEditCon.Visibility = System.Windows.Visibility.Visible;
				this.btnDelCon.Visibility = System.Windows.Visibility.Visible;
				this.btnExportTag.Visibility = System.Windows.Visibility.Visible;
				this.btnImportTag.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				this.btnAddTag.Visibility = System.Windows.Visibility.Hidden;
				this.btnCopyTag.Visibility = System.Windows.Visibility.Hidden;
				this.btnEditTag.Visibility = System.Windows.Visibility.Hidden;
				this.btnDelTag.Visibility = System.Windows.Visibility.Hidden;
				this.btnAddCon.Visibility = System.Windows.Visibility.Hidden;
				this.btnEditCon.Visibility = System.Windows.Visibility.Hidden;
				this.btnDelCon.Visibility = System.Windows.Visibility.Hidden;
				this.btnExportTag.Visibility = System.Windows.Visibility.Hidden;
				this.btnImportTag.Visibility = System.Windows.Visibility.Hidden;
			}
			for (int i = 0; i < MainWindow.AppWindow.MyConnections.Count; i++)
			{
				if (!MainWindow.AppWindow.RuntimeisActive)
				{
					//this.MyIcons[i].set_Kind(2814);
					MyIcons[i].Kind = PackIconKind.MenuDown;
					MyIcons[i].ClearValue(Control.ForegroundProperty);
				}
				else if (MainWindow.AppWindow.MyConnections[i].State == "Connected")
				{
					MyIcons[i].Kind = PackIconKind.MenuDown;
					MyIcons[i].Foreground = Brushes.Green;
				}
				else
				{
					MyIcons[i].Kind = PackIconKind.AlertOctagon;
					MyIcons[i].Foreground = Brushes.Red;
				}
				//else if (MainWindow.AppWindow.MyConnections[i].State != "Connected")
				//{
				//	this.MyIcons[i].set_Kind(118);
				//	this.MyIcons[i].Foreground = Brushes.Red;
				//}
				//else
				//{
				//	this.MyIcons[i].set_Kind(2814);
				//	this.MyIcons[i].Foreground = Brushes.Green;
				//}
			}
		}

		private void trvCon_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			StackPanel header;
			TextBlock ntxt;
			int id;
			string text;
			this.btnAddCon.IsEnabled = false;
			this.btnEditCon.IsEnabled = false;
			this.btnDelCon.IsEnabled = false;
			this.grpConName.Visibility = System.Windows.Visibility.Collapsed;
			//this.grpConOPC.Visibility = System.Windows.Visibility.Collapsed;
			this.grpConModTCP.Visibility = System.Windows.Visibility.Collapsed;
			this.grpConModRTU.Visibility = System.Windows.Visibility.Collapsed;
			//this.grpConIEC.Visibility = System.Windows.Visibility.Collapsed;
			this.grpConMQTT.Visibility = System.Windows.Visibility.Collapsed;
			this.grpConSQL.Visibility = System.Windows.Visibility.Collapsed;
			this.grpConFirebase.Visibility = System.Windows.Visibility.Collapsed;
			this.grpConFile.Visibility = System.Windows.Visibility.Collapsed;
			this.pnlEditCon.Visibility = System.Windows.Visibility.Collapsed;
			this.pnlConOk.Visibility = System.Windows.Visibility.Collapsed;
			TreeViewItem strv = this.trvCon.SelectedItem as TreeViewItem;
			if (strv.Tag != null)
			{
				string tag = strv.Tag as string;
				if (tag != null)
				{
					switch (tag)
					{
						//case "OPC":
						case "ModTCP":
						case "ModRTU":
						case "Firebase":
						//case "IEC104":
						case "MQTT":
						case "SQL":
						case "File":
							{
								if (MainWindow.AppWindow.MyConnections.Count < 1)
								{
									this.btnAddCon.IsEnabled = true;
								}
								break;
							}
							/*
						case "Con_OPC":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.txtOPCHost.Text = MainWindow.AppWindow.MyConnections[id].Host;
									this.txtOPCServer.Text = MainWindow.AppWindow.MyConnections[id].Server;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConOPC.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
							*/
						case "Con_ModTCP":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.txtModTCPIP.Text = MainWindow.AppWindow.MyConnections[id].IP;
									this.txtModTCPPort.Text = MainWindow.AppWindow.MyConnections[id].Port;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConModTCP.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
						case "Con_ModRTU":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.cboModRTUPort.Text = MainWindow.AppWindow.MyConnections[id].Port;
									this.cboModRTUBaud.Text = MainWindow.AppWindow.MyConnections[id].Baud;
									this.cboModRTUParity.Text = MainWindow.AppWindow.MyConnections[id].Parity;
									this.txtModRTUSlave.Text = MainWindow.AppWindow.MyConnections[id].Slave;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConModRTU.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
							/*
						case "Con_IEC104":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.txtIECIP.Text = MainWindow.AppWindow.MyConnections[id].IP;
									this.txtIECPort.Text = MainWindow.AppWindow.MyConnections[id].Port;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConIEC.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
							*/

						case "Con_MQTT":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.txtMqttHost.Text = MainWindow.AppWindow.MyConnections[id].Host;
									this.txtMqttPort.Text = MainWindow.AppWindow.MyConnections[id].Port;
									this.txtMqttUser.Text = MainWindow.AppWindow.MyConnections[id].User;
									this.txtMqttPass.Password = MainWindow.AppWindow.MyConnections[id].Pass;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConMQTT.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
						case "Con_SQL":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.cboSqlTarget.Text = MainWindow.AppWindow.MyConnections[id].Target;
									this.txtSqlHost.Text = MainWindow.AppWindow.MyConnections[id].Host;
									this.txtSqlUser.Text = MainWindow.AppWindow.MyConnections[id].User;
									this.txtSqlPass.Password = MainWindow.AppWindow.MyConnections[id].Pass;
									this.txtSqlDb.Text = MainWindow.AppWindow.MyConnections[id].Database;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConSQL.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
						case "Con_Firebase":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.txtFBPath.Text = MainWindow.AppWindow.MyConnections[id].FBPath;
									this.txtFBSecret.Password = MainWindow.AppWindow.MyConnections[id].FBSecret;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConFirebase.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
						case "Con_File":
							{
								header = strv.Header as StackPanel;
								ntxt = header.Children[1] as TextBlock;
								text = ntxt.Text;
								id = MainWindow.AppWindow.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == text);
								if (id >= 0)
								{
									this.txtConName.Text = MainWindow.AppWindow.MyConnections[id].Name;
									this.txtConInterval.Text = MainWindow.AppWindow.MyConnections[id].Interval;
									this.cboFileType.Text = MainWindow.AppWindow.MyConnections[id].FileType;
									this.txtFileSeparator.Text = MainWindow.AppWindow.MyConnections[id].FileSeparator;
									this.txtFileName.Text = MainWindow.AppWindow.MyConnections[id].FileName;
									this.txtConHeader.Text = "Connection";
									this.grpConName.Visibility = System.Windows.Visibility.Visible;
									this.grpConFile.Visibility = System.Windows.Visibility.Visible;
									this.btnEditCon.IsEnabled = true;
									this.btnDelCon.IsEnabled = true;
									this.pnlEditCon.Visibility = System.Windows.Visibility.Visible;
								}
								break;
							}
					}
				}
			}
		}

		private void trvConFirebase_Selected(object sender, RoutedEventArgs e)
		{
		}

		private void trvConFirebase_Unselected(object sender, RoutedEventArgs e)
		{
		}


		private void trvConModRTU_Selected(object sender, RoutedEventArgs e)
		{
		}

		private void trvConModRTU_Unselected(object sender, RoutedEventArgs e)
		{
		}

		private void trvConModTCP_Selected(object sender, RoutedEventArgs e)
		{
		}

		private void trvConModTCP_Unselected(object sender, RoutedEventArgs e)
		{
		}


		//private void TrvServer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		//{
		//	TreeViewItem tItem = (TreeViewItem)this.trvServer.SelectedItem;
		//	if (tItem != null)
		//	{
		//		Mouse.SetCursor(Cursors.Wait);
		//		string sTag = tItem.Tag.ToString();
		//		string[] arTags = sTag.Split(new char[] { '#' });
		//		string sType = arTags[0];
		//		string sName = arTags[1];
		//		if (sType != "Station")
		//		{
		//			if (sType != "OPCServer")
		//			{
		//				if (sType == "Folder")
		//				{
		//					//try
		//					//{
		//					//	string serverName = arTags[3];
		//					//	PackIcon picon = (PackIcon)((StackPanel)tItem.Header).Children[0];
		//					//	string sBranchID = arTags[2];
		//					//	this.MyOPCBrowser.MoveToRoot();
		//					//	if (sBranchID != "")
		//					//	{
		//					//		string[] arParents = sBranchID.Split(new char[] { '.' });
		//					//		for (int i = 0; i < (int)arParents.Length; i++)
		//					//		{
		//					//			this.MyOPCBrowser.MoveDown(arParents[i]);
		//					//		}
		//					//		this.MyOPCBrowser.MoveDown(sName);
		//					//		this.BranchParent = string.Concat(sBranchID, ".", sName);
		//					//	}
		//					//	else
		//					//	{
		//					//		this.MyOPCBrowser.MoveDown(sName);
		//					//		this.BranchParent = sName;
		//					//	}
		//					//	this.ShowBranchsAndLeafs(this.MyOPCServer, tItem);
		//					//}
		//					//catch (Exception exception)
		//					//{
		//					//	exception.ToString();
		//					//}
		//				}
		//				else if (sType == "Tag")
		//				{
		//					try
		//					{
		//					}
		//					catch (Exception exception1)
		//					{
		//						exception1.ToString();
		//					}
		//				}
		//			}
		//		}
		//		Mouse.SetCursor(Cursors.Arrow);
		//	}
		//}
	}
}