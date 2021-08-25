using DashboardDesigner.Properties;
using EasyModbus;
using Firebase.Storage;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using LiveCharts;
using LiveCharts.Definitions.Series;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Xceed.Wpf.Toolkit;

namespace DashboardDesigner
{
	public partial class MainWindow : Window
	{
		public static MainWindow AppWindow;

		private DesignerCanvas designerCanvas;

		private Win_About wAbout;

		private readonly BackgroundWorker bWorkerFB = new BackgroundWorker();

		private static FirebaseClient FB_Client;

		private static FirebaseClient FBStorage_Client;

		public int Set_Runtime_PlotInterval;

		public bool Set_Runtime_AutoMaximize;

		public bool Set_Runtime_AutoFullscreen;

		public string Set_Startup_AutoOpenFile;

		public bool Set_Startup_AutoActivate;

		public string Set_FBStorage_Path;

		public string Set_FBStorage_FBPath;

		public string Set_FBStorage_FBSecret;

		public string Set_CSV_Separator;

		private DispatcherTimer timerRun;

		public List<MyItem> MyItems = new List<MainWindow.MyItem>();

		public DesignerItem SelItem;

		public List<clsConnection> MyConnections = new List<MainWindow.clsConnection>();

		public List<clsTag> MyTags = new List<MainWindow.clsTag>();

		public bool RuntimeisActive = false;

		public bool FullscreenisActive = false;

		private DispatcherTimer timerPlot;

		private DispatcherTimer timerFB;

		public MainWindow()
		{
			this.InitializeComponent();
			MainWindow.AppWindow = this;
		}

		//Runtime Application button
		public void ActivateRuntime()
		{
			int i = 0;
			foreach (clsConnection con in MyConnections)
			{
				if (con.Type == "Firebase")
				{
					con.State = "Connected";
					this.StartFirebaseReading();
				}
				else if ((con.Type == "ModbusTCP" ? true : con.Type == "ModbusRTU"))
				{
					if (con.Type != "ModbusTCP")
					{
						con.ModClient = new ModbusClient(con.Port);
						con.ModClient.Baudrate = int.Parse(con.Baud);
						if (con.Parity == "None")
						{
							con.ModClient.Parity = Parity.None;
						}
						if (con.Parity == "Even")
						{
							con.ModClient.Parity = Parity.Even;
						}
						if (con.Parity == "Odd")
						{
							con.ModClient.Parity = Parity.Odd;
						}
						if (con.Parity == "Mark")
						{
							con.ModClient.Parity = Parity.Mark;
						}
						if (con.Parity == "Space")
						{
							con.ModClient.Parity = Parity.Space;
						}
						con.ModClient.UnitIdentifier = (byte)int.Parse(con.Slave);
					}
					else
					{
						con.ModClient = new ModbusClient(con.IP, int.Parse(con.Port));
					}

					try
					{
						con.TimerPoll = new DispatcherTimer()
						{
							Interval = TimeSpan.FromMilliseconds((double)int.Parse(con.Interval))
						};
						con.TimerPoll.Tick += new EventHandler(this.TimerPoll_Tick);
						con.ModClient.Connect();
						con.TimerPoll.Start();
						con.State = "Connected";
					}
					catch (Exception exception1)
					{
						exception1.ToString();
						con.State = "Disconnected";
						if (con.TimerReconnect == null)
						{
							con.TimerReconnect = new DispatcherTimer()
							{
								Tag = i,
								Interval = TimeSpan.FromSeconds(3)
							};
							con.TimerReconnect.Tick += new EventHandler(this.TimerReconnect_Tick);
							con.TimerReconnect.Start();
						}
					}
				}
				
				//Under Construction
				else if (con.Type == "MQTT")
				{
					con.MqttClient = new MqttClient(con.Host);
					MqttClient mqttClient = con.MqttClient;
					Guid guid = Guid.NewGuid();
					mqttClient.Connect(guid.ToString(), con.User, con.Pass);
					if (!con.MqttClient.IsConnected)
					{
						con.MqttClient = null;
						con.State = "Disconnected";
					}
					else
					{
						con.MqttClient.MqttMsgPublishReceived += new MqttClient.MqttMsgPublishEventHandler(MqttClient_MqttMsgPublishReceived);
						string stopics = "";
						foreach (MainWindow.clsTag tag in this.MyTags)
						{
							if (tag.Connection == con.Name)
							{
								if (stopics != "")
								{
									stopics = string.Concat(stopics, ",");
								}
								stopics = string.Concat(stopics, tag.Address);
							}
						}
						if (stopics != "")
						{
							string[] topics = stopics.Split(new char[] { ',' });
							byte[] mbase = new byte[(int)topics.Length];
							for (int j = 0; j < topics.Length; j++)
							{
								mbase[j] = 1;
							}
							con.MqttClient.Subscribe(topics, mbase);
						}
						con.State = "Connected";
					}
				}
				else if (con.Type == "SQL")
				{
					string connString = string.Concat(new string[] { "SERVER=", con.Host, ";DATABASE=", con.Database, ";UID=", con.User, ";PASSWORD=", con.Pass, ";" });
					if (con.Target != "MySQL")
					{
						try
						{
							con.SqlCon = new SqlConnection(connString);
							con.SqlCon.Open();
							con.State = "Connected";
						}
						catch (Exception exception3)
						{
							Exception ex2 = exception3;
							con.SqlCon = null;
							ex2.ToString();
							con.State = "Error";
						}
					}
					else
					{
						try
						{
							con.MySqlCon = new MySqlConnection(connString);
							con.MySqlCon.Open();
							con.State = "Connected";
						}
						catch (Exception exception4)
						{
							Exception ex1 = exception4;
							con.MySqlCon = null;
							ex1.ToString();
							con.State = "Error";
						}
					}
				}
			}
			this.RuntimeisActive = true;
			DesignerCanvas.RunActive = true;
			foreach (MainWindow.MyItem item in this.MyItems)
			{
				item.DesItem.IsSelected = false;
			}
			this.cboItems.SelectedItem = null;
			int iSec = this.Set_Runtime_PlotInterval;
			if (iSec <= 0)
			{
				iSec = 2;
			}
			this.timerRun = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds((double)iSec)
			};
			this.timerRun.Tick += new EventHandler(this.TimerRun_Tick);
			this.timerRun.Start();
			this.timerPlot = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds((double)iSec)
			};
			this.timerPlot.Tick += new EventHandler((object sender, EventArgs args) => {
				foreach (MainWindow.MyItem item in this.MyItems)
				{
					if (item.Dynamics != null)
					{
						foreach (MainWindow.clsDynamic dynamic in item.Dynamics)
						{
							try
							{
								int itag = this.MyTags.FindIndex((MainWindow.clsTag a) => a.Name == dynamic.Tag);
								if (itag >= 0)
								{
									string sval = this.MyTags[itag].Value;
									if (!dynamic.Key.Contains("Serie"))
									{
										string key = dynamic.Key;
										if (key == "Text")
										{
											(item.DesItem.Content as TextBlock).Text = sval;
										}
										else if (key != "Visible")
										{
											if (key == "Value")
											{
												string sCont = item.DesItem.Content.ToString();
												if (sCont.Contains("System.Windows.Controls.ProgressBar"))
												{
													(item.DesItem.Content as ProgressBar).Value = double.Parse(sval);

													//notifications
													if ((item.DesItem.Content as ProgressBar).Value >= 85) {
														//do action
														//DialogHost.OpenDialogCommand.Execute(null, null);
														warn.ShowDialog(warn.DialogContent);
													}
												}
												else if (sCont == "LiveCharts.Wpf.AngularGauge")
												{
													(item.DesItem.Content as AngularGauge).Value = double.Parse(sval);
													if ((item.DesItem.Content as AngularGauge).Value >= 85)
													{
														warn.ShowDialog(warn.DialogContent);
                                                    }
												}
											}
										}
										else if ((sval == "1" ? false : sval != "True"))
										{
											item.DesItem.Visibility = System.Windows.Visibility.Hidden;
										}
										else
										{
											item.DesItem.Visibility = System.Windows.Visibility.Visible;
										}
									}
									else
									{
										int idserie = int.Parse(dynamic.Key.Substring(5, dynamic.Key.Length - 5)) - 1;
										if (idserie >= 0)
										{
											string sCont = item.DesItem.Content.ToString();
											if (sCont == "LiveCharts.Wpf.PieChart")
											{
												(item.DesItem.Content as PieChart).Series[idserie].Values[0] = double.Parse(sval);
											}
											else if (sCont == "LiveCharts.Wpf.CartesianChart")
											{
												CartesianChart chart = item.DesItem.Content as CartesianChart;
												if (chart.AxisX[0].MaxValue.ToString() != "NaN")
												{
													double maxdata = chart.AxisX[0].MaxValue;
													if (maxdata <= 1)
													{
														chart.Series[idserie].Values[0] = double.Parse(sval);
													}
													else if ((double)chart.Series[idserie].Values.Count > maxdata)
													{
														chart.Series[idserie].Values.RemoveAt(0);
														chart.Series[idserie].Values.Add(double.Parse(sval));
													}
													else
													{
														chart.Series[idserie].Values.Add(double.Parse(sval));
													}
												}
												else
												{
													chart.Series[idserie].Values.Add(double.Parse(sval));
												}
											}
										}
									}
								}
							}
							catch (Exception exception)
							{
								exception.ToString();
							}
						}
					}
				}
			});
			this.timerPlot.Start();
			MyDesigner.IsHitTestVisible = false;
			this.gridProperty.IsEnabled = false;
			this.gridTool.IsEnabled = false;
			this.rectRuntime.Visibility = System.Windows.Visibility.Visible;
			if (this.Set_Runtime_AutoMaximize)
			{
				base.WindowState = System.Windows.WindowState.Maximized;
			}
			if ((this.tabMain.SelectedIndex != 0 ? false : this.Set_Runtime_AutoFullscreen))
			{
				this.FullscreenisActive = false;
				this.Fullscreen_Toggle();
			}
		}

		private void AddDynamic(TextBox txb, string skey)
		{
			if (txb.Text.Trim() != "")
			{
				if (this.SelItem != null)
				{
					MainWindow.clsDynamic dyn = new MainWindow.clsDynamic()
					{
						Key = skey,
						Tag = txb.Text
					};
					int id = this.MyItems.FindIndex((MainWindow.MyItem a) => a.ID == this.SelItem.ID.ToString());
					if (this.MyItems[id].Dynamics != null)
					{
						int id2 = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == skey);
						if (id2 < 0)
						{
							this.MyItems[id].Dynamics.Add(dyn);
						}
						else
						{
							this.MyItems[id].Dynamics[id2] = dyn;
						}
					}
					else
					{
						this.MyItems[id].Dynamics = new List<MainWindow.clsDynamic>()
						{
							dyn
						};
					}
				}
			}
			else if (this.SelItem != null)
			{
				int id = this.MyItems.FindIndex((MainWindow.MyItem a) => a.ID == this.SelItem.ID.ToString());
				if (this.MyItems[id].Dynamics != null)
				{
					int id2 = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == skey);
					if (id2 >= 0)
					{
						this.MyItems[id].Dynamics.RemoveAt(id2);
					}
				}
			}
		}

		
		private void BackColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.BackColorChooser.SelectedColor.HasValue)
			{
				SolidColorBrush brush = new SolidColorBrush(this.BackColorChooser.SelectedColor.Value);
				if ((this.SelItem != null ? false : this.txtType.Text == "Canvas"))
				{
					MyDesigner.Background = brush;
				}
				else if (this.txtType.Text == "System.Windows.Shapes.Ellipse")
				{
					(this.SelItem.Content as Ellipse).Fill = brush;
				}
				else if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
				{
					(this.SelItem.Content as ProgressBar).Background = brush;
				}
			}
		}

		private void BorderColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.BorderColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.BorderColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
					{
						(this.SelItem.Content as ProgressBar).BorderBrush = brush;
					}
				}
			}
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

		private void btnActivate_Click(object sender, RoutedEventArgs e)
		{
			this.ActivateRuntime();
			if (this.RuntimeisActive)
			{
				this.btnActivate.IsEnabled = false;
				this.btnDeactivate.IsEnabled = true;
				Page_TagMan.PageTagMan.btnShowValue.Visibility = System.Windows.Visibility.Visible;
				this.lblWMark.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void btnAddChartSerie_Click(object sender, RoutedEventArgs e)
		{
			this.btnAddChartSerie.Background = Brushes.LightYellow;
			this.radSerieLine.IsChecked = new bool?(true);
			this.radSerieBar.IsEnabled = true;
			this.radSerieLine.IsEnabled = true;
			this.txtChartSerieTitle.Text = "";
			this.txtChartSerieValues.Text = "0";
			this.StrokeSerieColorChooser.SelectedColor = new Color?(Brushes.White.Color);
			this.grdAddChartSerie.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnAddChartSerieCancel_Click(object sender, RoutedEventArgs e)
		{
			this.btnAddChartSerie.Background = Brushes.Transparent;
			this.btnEditChartSerie.Background = Brushes.Transparent;
			this.btnDeleteChartSerie.Background = Brushes.Transparent;
			this.dgChartSeries.IsEnabled = true;
			this.grdAddChartSerie.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void btnAddChartSerieOk_Click(object sender, RoutedEventArgs e)
		{
			bool? isChecked;
			Color? selectedColor;
			if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				CartesianChart chart = this.SelItem.Content as CartesianChart;
				if (this.btnAddChartSerie.Background == Brushes.LightYellow)
				{
					this.btnAddChartSerie.Background = Brushes.Transparent;
					string stype = "";
					isChecked = this.radSerieLine.IsChecked;
					stype = (!(isChecked.GetValueOrDefault() & isChecked.HasValue) ? "Bar" : "Line");
					string title = this.txtChartSerieTitle.Text;
					selectedColor = this.StrokeSerieColorChooser.SelectedColor;
					SolidColorBrush brush1 = new SolidColorBrush(selectedColor.Value);
					this.GetColorName(brush1);
					string[] svals = this.txtChartSerieValues.Text.Split(new char[] { ',' });
					if (stype == "Line")
					{
						LineSeries serie = new LineSeries();
						serie.Title = title;
						serie.Stroke = brush1;
						serie.Fill = Brushes.Transparent;
						serie.LineSmoothness = 0;
						serie.PointGeometry = null;
						isChecked = this.radDataLabelShow.IsChecked;
						if (!(isChecked.GetValueOrDefault() & isChecked.HasValue))
						{
							serie.DataLabels = false;
						}
						else
						{
							serie.DataLabels = true;
						}
						serie.Values = new ChartValues<double>();
						string[] strArrays = svals;
						for (int i = 0; i < (int)strArrays.Length; i++)
						{
							double dval = double.Parse(strArrays[i]);
							serie.Values.Add(dval);
						}
						chart.Series.Add(serie);
					}
					else if (stype == "Bar")
					{
						ColumnSeries serie = new ColumnSeries();
						serie.Title = title;
						serie.Stroke = Brushes.Transparent;
						serie.Fill = brush1;
						isChecked = this.radDataLabelShow.IsChecked;
						if (!(isChecked.GetValueOrDefault() & isChecked.HasValue))
						{
							serie.DataLabels = false;
						}
						else
						{
							serie.DataLabels = true;
						}
						serie.Values = new ChartValues<double>();
						string[] strArrays1 = svals;
						for (int j = 0; j < (int)strArrays1.Length; j++)
						{
							double dval = double.Parse(strArrays1[j]);
							serie.Values.Add(dval);
						}
						chart.Series.Add(serie);
					}
					this.dgChartSeries.Items.Add(new MainWindow.ChartSerie()
					{
						Type = stype,
						Title = title
					});
				}
				else if (this.btnEditChartSerie.Background == Brushes.LightYellow)
				{
					int id = this.dgChartSeries.SelectedIndex;
					if (id >= 0)
					{
						this.btnEditChartSerie.Background = Brushes.Transparent;
						string stype = "";
						isChecked = this.radSerieLine.IsChecked;
						stype = (!(isChecked.GetValueOrDefault() & isChecked.HasValue) ? "Bar" : "Line");
						string title = this.txtChartSerieTitle.Text;
						selectedColor = this.StrokeSerieColorChooser.SelectedColor;
						SolidColorBrush brush1 = new SolidColorBrush(selectedColor.Value);
						this.GetColorName(brush1);
						string[] svals = this.txtChartSerieValues.Text.Split(new char[] { ',' });
						if (stype == "Line")
						{
							LineSeries serie = new LineSeries();
							serie.Title = title;
							serie.Stroke = brush1;
							serie.Fill = Brushes.Transparent;
							serie.LineSmoothness = 0.0;
							serie.PointGeometry = null;
							isChecked = this.radDataLabelShow.IsChecked;
							if (!(isChecked.GetValueOrDefault() & isChecked.HasValue))
							{
								serie.DataLabels = false;
							}
							else
							{
								serie.DataLabels = true;
							}
							serie.Values = new ChartValues<double>();
							string[] strArrays2 = svals;
							for (int k = 0; k < (int)strArrays2.Length; k++)
							{
								double dval = double.Parse(strArrays2[k]);
								serie.Values.Add(dval);
							}
							//chart.Series[id, serie];
							chart.Series.Add(serie);
						}
						else if (stype == "Bar")
						{
							ColumnSeries serie = new ColumnSeries();
							serie.Title = title;
							serie.Fill = brush1;
							isChecked = this.radDataLabelShow.IsChecked;
							if (!(isChecked.GetValueOrDefault() & isChecked.HasValue))
							{
								serie.DataLabels = false;
							}
							else
							{
								serie.DataLabels = true;
							}
							serie.Values = new ChartValues<double>();
							string[] strArrays3 = svals;
							for (int l = 0; l < (int)strArrays3.Length; l++)
							{
								double dval = double.Parse(strArrays3[l]);
								serie.Values.Add(dval);
							}
							//chart.Series.Item(id, serie);
							chart.Series.Add(serie);
						}
						this.dgChartSeries.Items[id] = new MainWindow.ChartSerie()
						{
							Type = stype,
							Title = title
						};
						this.dgChartSeries.IsEnabled = true;
					}
				}
				else if (this.btnDeleteChartSerie.Background == Brushes.LightYellow)
				{
					int id = this.dgChartSeries.SelectedIndex;
					if (id >= 0)
					{
						this.btnDeleteChartSerie.Background = Brushes.Transparent;
						chart.Series.RemoveAt(id);
						this.dgChartSeries.Items.RemoveAt(id);
						this.dgChartSeries.IsEnabled = true;
					}
				}
				this.grdAddChartSerie.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void btnAddSectionCancel_Click(object sender, RoutedEventArgs e)
		{
			this.btnAddSectionGauge.Background = Brushes.Transparent;
			this.btnEditSection.Background = Brushes.Transparent;
			this.btnDeleteSection.Background = Brushes.Transparent;
			this.dgSectionGauge.IsEnabled = true;
			this.grdAddSection.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void btnAddSectionGauge_Click(object sender, RoutedEventArgs e)
		{
			this.btnAddSectionGauge.Background = Brushes.LightYellow;
			this.txtFromSection.Text = "0";
			this.txtToSection.Text = "0";
			this.SectionColorChooser.SelectedColor = new Color?(Brushes.Transparent.Color);
			this.grdAddSection.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnAddSectionOk_Click(object sender, RoutedEventArgs e)
		{
			Color? selectedColor;
			if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
			{
				AngularGauge gauge = this.SelItem.Content as AngularGauge;
				if (this.btnAddSectionGauge.Background == Brushes.LightYellow)
				{
					this.btnAddSectionGauge.Background = Brushes.Transparent;
					double fromval = double.Parse(this.txtFromSection.Text);
					double toval = double.Parse(this.txtToSection.Text);
					selectedColor = this.SectionColorChooser.SelectedColor;
					SolidColorBrush brush = new SolidColorBrush(selectedColor.Value);
					AngularSection section = new AngularSection();
					section.FromValue = fromval;
					section.ToValue = toval;
					section.Fill = brush;
					gauge.Sections.Add(section);
					this.dgSectionGauge.Items.Add(new MainWindow.GaugeSection()
					{
						FromValue = fromval.ToString(),
						ToValue = toval.ToString(),
						Fill = this.GetColorName(brush)
					});
				}
				else if (this.btnEditSection.Background == Brushes.LightYellow)
				{
					int id = this.dgSectionGauge.SelectedIndex;
					if (id >= 0)
					{
						this.btnEditSection.Background = Brushes.Transparent;
						double fromval = double.Parse(this.txtFromSection.Text);
						double toval = double.Parse(this.txtToSection.Text);
						selectedColor = this.SectionColorChooser.SelectedColor;
						SolidColorBrush brush = new SolidColorBrush(selectedColor.Value);
						AngularSection section = gauge.Sections[id];
						section.FromValue = fromval;
						section.ToValue = toval;
						section.Fill = brush;
						this.dgSectionGauge.Items[id] = new MainWindow.GaugeSection()
						{
							FromValue = fromval.ToString(),
							ToValue = toval.ToString(),
							Fill = this.GetColorName(brush)
						};
						this.dgSectionGauge.IsEnabled = true;
					}
				}
				else if (this.btnDeleteSection.Background == Brushes.LightYellow)
				{
					int id = this.dgSectionGauge.SelectedIndex;
					if (id >= 0)
					{
						this.btnDeleteSection.Background = Brushes.Transparent;
						gauge.Sections.RemoveAt(id);
						this.dgSectionGauge.Items.RemoveAt(id);
						this.dgSectionGauge.IsEnabled = true;
					}
				}
				int num = int.Parse(this.txtWidth.Text) - 1;
				this.txtWidth.Text = num.ToString();
				this.txtWidth_LostFocus(null, null);
				this.grdAddSection.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void btnAddSerie_Click(object sender, RoutedEventArgs e)
		{
			this.btnAddSerie.Background = Brushes.LightYellow;
			this.txtSerieTitle.Text = "";
			this.txtSerieValues.Text = "0";
			this.SerieColorChooser.SelectedColor = new Color?(Brushes.White.Color);
			this.grdAddSerie.Visibility = System.Windows.Visibility.Visible;
		}

		private void btnAddSerieCancel_Click(object sender, RoutedEventArgs e)
		{
			this.btnAddSerie.Background = Brushes.Transparent;
			this.btnEditSerie.Background = Brushes.Transparent;
			this.btnDeleteSerie.Background = Brushes.Transparent;
			this.dgSeries.IsEnabled = true;
			this.grdAddSerie.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void btnAddSerieOk_Click(object sender, RoutedEventArgs e)
		{
			Color? selectedColor;
			if (this.txtType.Text == "LiveCharts.Wpf.PieChart")
			{
				PieChart pie = this.SelItem.Content as PieChart;
				if (this.btnAddSerie.Background == Brushes.LightYellow)
				{
					this.btnAddSerie.Background = Brushes.Transparent;
					string title = this.txtSerieTitle.Text;
					double dval = double.Parse(this.txtSerieValues.Text);
					selectedColor = this.SerieColorChooser.SelectedColor;
					SolidColorBrush brush = new SolidColorBrush(selectedColor.Value);
					PieSeries serie = new PieSeries();
					serie.Title = title;
					ChartValues<double> chartValue = new ChartValues<double>();
					chartValue.Add(dval);
					serie.Values = chartValue;
					serie.Fill = brush;
					pie.Series.Add(serie);
					this.dgSeries.Items.Add(new MainWindow.DataSerie()
					{
						Title = title,
						Values = dval.ToString(),
						Fill = this.GetColorName(brush)
					});
				}
				else if (this.btnEditSerie.Background == Brushes.LightYellow)
				{
					int id = this.dgSeries.SelectedIndex;
					if (id >= 0)
					{
						this.btnEditSerie.Background = Brushes.Transparent;
						string title = this.txtSerieTitle.Text;
						double dval = double.Parse(this.txtSerieValues.Text);
						selectedColor = this.SerieColorChooser.SelectedColor;
						SolidColorBrush brush = new SolidColorBrush(selectedColor.Value);
						PieSeries serie = pie.Series[id] as PieSeries;
						serie.Title = title;
						serie.Values[0] = dval;
						serie.Fill = brush;
						this.dgSeries.Items[id] = new MainWindow.DataSerie()
						{
							Title = title,
							Values = dval.ToString(),
							Fill = this.GetColorName(brush)
						};
						this.dgSeries.IsEnabled = true;
					}
				}
				else if (this.btnDeleteSerie.Background == Brushes.LightYellow)
				{
					int id = this.dgSeries.SelectedIndex;
					if (id >= 0)
					{
						this.btnDeleteSerie.Background = Brushes.Transparent;
						pie.Series.RemoveAt(id);
						this.dgSeries.Items.RemoveAt(id);
						this.dgSeries.IsEnabled = true;
					}
				}
				this.grdAddSerie.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void btnBrowseImage_Click(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.Image")
				{
					Image image = this.SelItem.Content as Image;
					OpenFileDialog op = new OpenFileDialog()
					{
						Title = "Select a picture",
						Filter = "All supported graphics|*.jpg;*.jpeg;*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|Portable Network Graphic (*.png)|*.png"
					};
					bool? nullable = op.ShowDialog();
					if (nullable.GetValueOrDefault() & nullable.HasValue)
					{
						image.Source = new BitmapImage(new Uri(op.FileName));
						this.txtImage.Text = image.Source.ToString();
					}
				}
			}
		}

		private void btnDeactivate_Click(object sender, RoutedEventArgs e)
		{
			this.DeactivateRuntime();
			if (!this.RuntimeisActive)
			{
				this.btnActivate.IsEnabled = true;
				this.btnDeactivate.IsEnabled = false;
				Page_TagMan.PageTagMan.btnShowValue.Visibility = System.Windows.Visibility.Collapsed;
				Page_TagMan.PageTagMan.colTagValue.Visibility = System.Windows.Visibility.Collapsed;
				this.lblWMark.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void btnDeleteChartSerie_Click(object sender, RoutedEventArgs e)
		{
			this.btnEditChartSerie_Click(null, null);
			if (this.btnEditChartSerie.Background == Brushes.LightYellow)
			{
				this.btnEditChartSerie.Background = Brushes.Transparent;
				this.btnDeleteChartSerie.Background = Brushes.LightYellow;
			}
		}

		private void btnDeleteSection_Click(object sender, RoutedEventArgs e)
		{
			if (this.dgSectionGauge.SelectedItem != null)
			{
				int id = this.dgSectionGauge.SelectedIndex;
				if (id >= 0)
				{
					if (this.SelItem != null)
					{
						if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
						{
							this.btnDeleteSection.Background = Brushes.LightYellow;
							this.dgSectionGauge.IsEnabled = false;
							AngularGauge gauge = this.SelItem.Content as AngularGauge;
							TextBox str = this.txtFromSection;
							double fromValue = gauge.Sections[id].FromValue;
							str.Text = fromValue.ToString();
							TextBox textBox = this.txtToSection;
							fromValue = gauge.Sections[id].ToValue;
							textBox.Text = fromValue.ToString();
							SolidColorBrush brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(gauge.Sections[id].Fill.ToString());
							this.SectionColorChooser.SelectedColor = new Color?(brush.Color);
							this.grdAddSection.Visibility = System.Windows.Visibility.Visible;
						}
					}
				}
			}
		}

		private void btnDeleteSerie_Click(object sender, RoutedEventArgs e)
		{
			this.btnEditSerie_Click(null, null);
			if (this.btnEditSerie.Background == Brushes.LightYellow)
			{
				this.btnEditSerie.Background = Brushes.Transparent;
				this.btnDeleteSerie.Background = Brushes.LightYellow;
			}
		}

		private void btnEditChartSerie_Click(object sender, RoutedEventArgs e)
		{
			if (this.dgChartSeries.SelectedItem != null)
			{
				if (this.dgChartSeries.SelectedIndex >= 0)
				{
					if (this.SelItem != null)
					{
						this.btnEditChartSerie.Background = Brushes.LightYellow;
						this.dgChartSeries.IsEnabled = false;
						this.grdAddChartSerie.IsEnabled = true;
					}
				}
			}
		}

		private void btnEditDynSerie_Click(object sender, RoutedEventArgs e)
		{
			int selectedIndex = this.dgDynSeries.SelectedIndex;
			if (selectedIndex >= 0)
			{
				if (this.SelItem != null)
				{
					int id = this.MyItems.FindIndex((MainWindow.MyItem a) => a.ID == this.SelItem.ID.ToString());
					int id3 = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == string.Concat("Serie", (selectedIndex + 1).ToString()));
					if (id3 < 0)
					{
						this.txtSeriesTag.Text = "";
					}
					else
					{
						this.txtSeriesTag.Text = this.MyItems[id].Dynamics[id3].Tag;
					}
					this.grdEditDynSeries.Visibility = System.Windows.Visibility.Visible;
					this.btnEditDynSerie.Background = Brushes.LightYellow;
				}
			}
		}

		private void btnEditDynSeriesCancel_Click(object sender, RoutedEventArgs e)
		{
			this.btnEditDynSerie.Background = Brushes.Transparent;
			this.dgDynSeries.IsEnabled = true;
			this.grdEditDynSeries.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void btnEditDynSeriesOk_Click(object sender, RoutedEventArgs e)
		{
			int num;
			if (this.btnEditDynSerie.Background == Brushes.LightYellow)
			{
				int id = this.MyItems.FindIndex((MainWindow.MyItem a) => a.ID == this.SelItem.ID.ToString());
				int selectedIndex = this.dgDynSeries.SelectedIndex;
				if (id >= 0)
				{
					string stag = this.txtSeriesTag.Text;
					int id3 = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == string.Concat("Serie", (selectedIndex + 1).ToString()));
					if (stag.Trim() == "")
					{
						if (id3 >= 0)
						{
							this.MyItems[id].Dynamics.RemoveAt(id3);
						}
					}
					else if (id3 < 0)
					{
						List<MainWindow.clsDynamic> dynamics = this.MyItems[id].Dynamics;
						MainWindow.clsDynamic _clsDynamic = new MainWindow.clsDynamic();
						num = selectedIndex + 1;
						_clsDynamic.Key = string.Concat("Serie", num.ToString());
						_clsDynamic.Tag = stag;
						dynamics.Add(_clsDynamic);
					}
					else
					{
						this.MyItems[id].Dynamics[id3].Tag = stag;
					}
					ItemCollection items = this.dgDynSeries.Items;
					int num1 = selectedIndex;
					MainWindow.DynSerie dynSerie = new MainWindow.DynSerie();
					num = selectedIndex + 1;
					dynSerie.Serie = string.Concat("Serie", num.ToString());
					dynSerie.Tag = stag;
					items[num1] = dynSerie;
					this.dgDynSeries.IsEnabled = true;
				}
			}
			this.btnEditDynSeriesCancel_Click(null, null);
		}

		private void btnEditSection_Click(object sender, RoutedEventArgs e)
		{
			if (this.dgSectionGauge.SelectedItem != null)
			{
				int id = this.dgSectionGauge.SelectedIndex;
				if (id >= 0)
				{
					if (this.SelItem != null)
					{
						if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
						{
							this.btnEditSection.Background = Brushes.LightYellow;
							this.dgSectionGauge.IsEnabled = false;
							AngularGauge gauge = this.SelItem.Content as AngularGauge;
							TextBox str = this.txtFromSection;
							double fromValue = gauge.Sections[id].FromValue;
							str.Text = fromValue.ToString();
							TextBox textBox = this.txtToSection;
							fromValue = gauge.Sections[id].ToValue;
							textBox.Text = fromValue.ToString();
							SolidColorBrush brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(gauge.Sections[id].Fill.ToString());
							this.SectionColorChooser.SelectedColor = new Color?(brush.Color);
							this.grdAddSection.Visibility = System.Windows.Visibility.Visible;
						}
					}
				}
			}
		}

		private void btnEditSerie_Click(object sender, RoutedEventArgs e)
		{
			if (this.dgSeries.SelectedItem != null)
			{
				int id = this.dgSeries.SelectedIndex;
				if (id >= 0)
				{
					if (this.SelItem != null)
					{
						this.btnEditSerie.Background = Brushes.LightYellow;
						this.dgSeries.IsEnabled = false;
						PieChart pie = this.SelItem.Content as PieChart;
						Series serie = pie.Series[id] as Series;
						this.txtSerieTitle.Text = serie.Title;
						this.txtSerieValues.Text = serie.Values[0].ToString();
						SolidColorBrush brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(serie.Fill.ToString());
						this.SerieColorChooser.SelectedColor = new Color?(brush.Color);
						this.grdAddSerie.Visibility = System.Windows.Visibility.Visible;
					}
				}
			}
		}

		private void cboItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SolidColorBrush brush;
			int iddyn;
			Predicate<MainWindow.clsDynamic> predicate;
			Color color;
			Predicate<MainWindow.clsDynamic> predicate1 = null;
			Predicate<MainWindow.clsDynamic> predicate2 = null;
			if (this.cboItems.SelectedItem == null)
			{
				this.ResetProperty();
			}
			else if (this.cboItems.SelectedItem.ToString() != "Canvas")
			{
				int id = this.MyItems.FindIndex((MainWindow.MyItem a) => a.ID == this.cboItems.SelectedItem.ToString());
				if (id >= 0)
				{
					this.SelItem = this.MyItems[id].DesItem;
					foreach (Grid grid in this.stkProp.Children)
					{
						grid.Visibility = System.Windows.Visibility.Collapsed;
					}
					foreach (Grid grid in this.stkDyn.Children)
					{
						grid.Visibility = System.Windows.Visibility.Collapsed;
					}
					string sCont = this.SelItem.Content.ToString();
					if (sCont.Contains("System.Windows.Controls.ProgressBar"))
					{
						sCont = "System.Windows.Controls.ProgressBar";
					}
					this.txtType.Text = sCont;
					TextBox str = this.txtLeft;
					double left = Canvas.GetLeft(this.SelItem);
					str.Text = left.ToString();
					TextBox textBox = this.txtTop;
					left = Canvas.GetTop(this.SelItem);
					textBox.Text = left.ToString();
					TextBox str1 = this.txtWidth;
					left = this.SelItem.Width;
					str1.Text = left.ToString();
					TextBox textBox1 = this.txtHeight;
					left = this.SelItem.Height;
					textBox1.Text = left.ToString();
					this.grdPropLeft.Visibility = System.Windows.Visibility.Visible;
					this.grdPropTop.Visibility = System.Windows.Visibility.Visible;
					this.grdPropWidth.Visibility = System.Windows.Visibility.Visible;
					this.grdPropHeight.Visibility = System.Windows.Visibility.Visible;
					string text = this.txtType.Text;
					if (text == "System.Windows.Controls.TextBlock")
					{
						TextBlock ntxt = this.SelItem.Content as TextBlock;
						this.txtText.Text = ntxt.Text;
						this.txtFontSize.Text = ntxt.FontSize.ToString();
						this.FontChooser.Text = ntxt.FontFamily.ToString();
						if (ntxt.FontStyle != FontStyles.Italic)
						{
							this.chkItalic.IsChecked = new bool?(false);
						}
						else
						{
							this.chkItalic.IsChecked = new bool?(true);
						}
						if (ntxt.FontWeight != FontWeights.Bold)
						{
							this.chkBold.IsChecked = new bool?(false);
						}
						else
						{
							this.chkBold.IsChecked = new bool?(true);
						}
						this.grdPropText.Visibility = System.Windows.Visibility.Visible;
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(ntxt.Foreground.ToString());
						this.ForeColorChooser.SelectedColor = new Color?(brush.Color);
						this.grdPropForeground.Visibility = System.Windows.Visibility.Visible;
						this.txtDynText.Text = "";
						this.txtDynVisible.Text = "";
						if (this.MyItems[id].Dynamics != null)
						{
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Text");
							if (iddyn >= 0)
							{
								this.txtDynText.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
							if (iddyn >= 0)
							{
								this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
						}
						this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
						this.grdDynText.Visibility = System.Windows.Visibility.Visible;
					}
					else if (text == "System.Windows.Controls.Image")
					{
						Image image = this.SelItem.Content as Image;
						if ((image == null ? false : image.Source != null))
						{
							this.txtImage.Text = image.Source.ToString();
							this.grdPropImage.Visibility = System.Windows.Visibility.Visible;
							this.txtDynVisible.Text = "";
							if (this.MyItems[id].Dynamics != null)
							{
								iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
								if (iddyn >= 0)
								{
									this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
								}
							}
							this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
						}
					}
					else if (text == "LiveCharts.Wpf.AngularGauge")
					{
						AngularGauge gauge = this.SelItem.Content as AngularGauge;
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(gauge.Foreground.ToString());
						this.ForeColorChooser.SelectedColor = new Color?(brush.Color);
						this.grdPropForeground.Visibility = System.Windows.Visibility.Visible;
						this.txtValue.Text = gauge.Value.ToString();
						this.txtFromValue.Text = gauge.FromValue.ToString();
						this.txtToValue.Text = gauge.ToValue.ToString();
						this.txtSectionRadius.Text = gauge.SectionsInnerRadius.ToString();
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(gauge.NeedleFill.ToString());
						this.NeedleColorChooser.SelectedColor = new Color?(brush.Color);
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(gauge.TicksForeground.ToString());
						this.TicksColorChooser.SelectedColor = new Color?(brush.Color);
						this.dgSectionGauge.Items.Clear();
						foreach (AngularSection section in gauge.Sections)
						{
							brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(section.Fill.ToString());
							this.dgSectionGauge.Items.Add(new MainWindow.GaugeSection()
							{
								FromValue = section.FromValue.ToString(),
								ToValue = section.ToValue.ToString(),
								Fill = this.GetColorName(brush)
							});
						}
						this.grdPropGauge.Visibility = System.Windows.Visibility.Visible;
						this.txtDynValue.Text = "";
						this.txtDynVisible.Text = "";
						if (this.MyItems[id].Dynamics != null)
						{
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Value");
							if (iddyn >= 0)
							{
								this.txtDynValue.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
							if (iddyn >= 0)
							{
								this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
						}
						this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
						this.grdDynValue.Visibility = System.Windows.Visibility.Visible;
					}
					else if (text == "LiveCharts.Wpf.CartesianChart")
					{
						CartesianChart chart = this.SelItem.Content as CartesianChart;
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(chart.Foreground.ToString());
						this.ForeColorChooser.SelectedColor = new Color?(brush.Color);
						this.grdPropForeground.Visibility = System.Windows.Visibility.Visible;
						if (chart.AxisX[0].MaxValue.ToString() != "NaN")
						{
							TextBox str2 = this.txtMaxData;
							left = chart.AxisX[0].MaxValue;
							str2.Text = left.ToString();
						}
						else
						{
							this.txtMaxData.Text = "Auto";
						}
						if (chart.AxisY[0].MaxValue.ToString() != "NaN")
						{
							TextBox textBox2 = this.txtMaxValue;
							left = chart.AxisY[0].MaxValue;
							textBox2.Text = left.ToString();
						}
						else
						{
							this.txtMaxValue.Text = "Auto";
						}
						if (chart.AxisY[0].MinValue.ToString() != "NaN")
						{
							TextBox str3 = this.txtMinValue;
							left = chart.AxisY[0].MinValue;
							str3.Text = left.ToString();
						}
						else
						{
							this.txtMinValue.Text = "Auto";
						}
						if (!chart.AxisX[0].ShowLabels)
						{
							this.cekShowAxisX.IsChecked = new bool?(false);
						}
						else
						{
							this.cekShowAxisX.IsChecked = new bool?(true);
						}
						if (!chart.AxisY[0].ShowLabels)
						{
							this.cekShowAxisY.IsChecked = new bool?(false);
						}
						else
						{
							this.cekShowAxisY.IsChecked = new bool?(true);
						}
						this.dgChartSeries.Items.Clear();
						this.dgChartSeries.IsEnabled = true;
						this.dgDynSeries.Items.Clear();
						if (this.MyItems[id].Dynamics == null)
						{
							this.MyItems[id].Dynamics = new List<MainWindow.clsDynamic>();
						}
						int num = 0;
						foreach (Series serie in chart.Series)
						{
							num++;
							string stype = "";
							if (serie.ToString() == "LiveCharts.Wpf.LineSeries")
							{
								stype = "Line";
							}
							else if (serie.ToString() == "LiveCharts.Wpf.ColumnSeries")
							{
								stype = "Bar";
							}
							this.dgChartSeries.Items.Add(new MainWindow.ChartSerie()
							{
								Type = stype,
								Title = serie.Title
							});
							List<MainWindow.clsDynamic> dynamics = this.MyItems[id].Dynamics;
							Predicate<MainWindow.clsDynamic> predicate3 = predicate1;
							if (predicate3 == null)
							{
								Predicate<MainWindow.clsDynamic> key = (MainWindow.clsDynamic a) => a.Key == string.Concat("Serie", num.ToString());
								predicate = key;
								predicate1 = key;
								predicate3 = predicate;
							}
							int id2 = dynamics.FindIndex(predicate3);
							if (id2 < 0)
							{
								this.dgDynSeries.Items.Add(new MainWindow.DynSerie()
								{
									Serie = string.Concat("Serie", num.ToString()),
									Tag = ""
								});
							}
							else
							{
								this.dgDynSeries.Items.Add(new MainWindow.DynSerie()
								{
									Serie = string.Concat("Serie", num.ToString()),
									Tag = this.MyItems[id].Dynamics[id2].Tag
								});
							}
						}
						this.grdPropChart.Visibility = System.Windows.Visibility.Visible;
						string str4 = chart.LegendLocation.ToString();
						if (str4 == "None")
						{
							this.radLegendNone.IsChecked = new bool?(true);
						}
						else if (str4 == "Bottom")
						{
							this.radLegendBottom.IsChecked = new bool?(true);
						}
						else if (str4 == "Right")
						{
							this.radLegendRight.IsChecked = new bool?(true);
						}
						else if (str4 == "Left")
						{
							this.radLegendLeft.IsChecked = new bool?(true);
						}
						else if (str4 == "Top")
						{
							this.radLegendTop.IsChecked = new bool?(true);
						}
						this.grdLegend.Visibility = System.Windows.Visibility.Visible;
						this.txtDynVisible.Text = "";
						if (this.MyItems[id].Dynamics != null)
						{
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
							if (iddyn >= 0)
							{
								this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
						}
						this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
						this.grdDynSeries.Visibility = System.Windows.Visibility.Visible;
					}
					else if (text == "LiveCharts.Wpf.PieChart")
					{
						PieChart pie = this.SelItem.Content as PieChart;
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(pie.Foreground.ToString());
						this.ForeColorChooser.SelectedColor = new Color?(brush.Color);
						this.grdPropForeground.Visibility = System.Windows.Visibility.Visible;
						this.dgSeries.Items.Clear();
						this.dgSeries.IsEnabled = true;
						this.dgDynSeries.Items.Clear();
						if (this.MyItems[id].Dynamics == null)
						{
							this.MyItems[id].Dynamics = new List<MainWindow.clsDynamic>();
						}
						int num1 = 0;
						foreach (PieSeries serie in pie.Series)
						{
							num1++;
							brush = (SolidColorBrush) new BrushConverter().ConvertFromString(serie.Fill.ToString());
							this.dgSeries.Items.Add(new MainWindow.DataSerie()
							{
								Title = serie.Title,
								Values = serie.Values[0].ToString(),
								Fill = this.GetColorName(brush)
							});
							List<MainWindow.clsDynamic> clsDynamics = this.MyItems[id].Dynamics;
							Predicate<MainWindow.clsDynamic> predicate4 = predicate2;
							if (predicate4 == null)
							{
								Predicate<MainWindow.clsDynamic> key1 = (MainWindow.clsDynamic a) => a.Key == string.Concat("Serie", num1.ToString());
								predicate = key1;
								predicate2 = key1;
								predicate4 = predicate;
							}
							int id2 = clsDynamics.FindIndex(predicate4);
							if (id2 < 0)
							{
								this.dgDynSeries.Items.Add(new MainWindow.DynSerie()
								{
									Serie = string.Concat("Serie", num1.ToString()),
									Tag = ""
								});
							}
							else
							{
								this.dgDynSeries.Items.Add(new MainWindow.DynSerie()
								{
									Serie = string.Concat("Serie", num1.ToString()),
									Tag = this.MyItems[id].Dynamics[id2].Tag
								});
							}
						}
						this.grdPropPie.Visibility = System.Windows.Visibility.Visible;
						string str5 = pie.LegendLocation.ToString();
						if (str5 == "None")
						{
							this.radLegendNone.IsChecked = new bool?(true);
						}
						else if (str5 == "Bottom")
						{
							this.radLegendBottom.IsChecked = new bool?(true);
						}
						else if (str5 == "Right")
						{
							this.radLegendRight.IsChecked = new bool?(true);
						}
						else if (str5 == "Left")
						{
							this.radLegendLeft.IsChecked = new bool?(true);
						}
						else if (str5 == "Top")
						{
							this.radLegendTop.IsChecked = new bool?(true);
						}
						this.grdLegend.Visibility = System.Windows.Visibility.Visible;
						this.txtDynVisible.Text = "";
						if (this.MyItems[id].Dynamics != null)
						{
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
							if (iddyn >= 0)
							{
								this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
						}
						this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
						this.grdDynSeries.Visibility = System.Windows.Visibility.Visible;
					}
					else if (text == "System.Windows.Shapes.Ellipse")
					{
						Ellipse ellipse = this.SelItem.Content as Ellipse;
						if (ellipse.Fill != null)
						{
							if (ellipse.Fill.ToString() != "System.Windows.Media.LinearGradientBrush")
							{
								brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(ellipse.Fill.ToString());
								this.FillColorChooser.SelectedColor = new Color?(brush.Color);
								this.grdPropFill.Visibility = System.Windows.Visibility.Visible;
							}
							else
							{
								LinearGradientBrush grad = (LinearGradientBrush)ellipse.Fill;
								BrushConverter brushConverter = new BrushConverter();
								color = grad.GradientStops[0].Color;
								brush = (SolidColorBrush)brushConverter.ConvertFromString(color.ToString());
								this.Grad1ColorChooser.SelectedColor = new Color?(brush.Color);
								BrushConverter brushConverter1 = new BrushConverter();
								color = grad.GradientStops[1].Color;
								brush = (SolidColorBrush)brushConverter1.ConvertFromString(color.ToString());
								this.Grad2ColorChooser.SelectedColor = new Color?(brush.Color);
								this.grdPropGrad1.Visibility = System.Windows.Visibility.Visible;
								this.grdPropGrad2.Visibility = System.Windows.Visibility.Visible;
							}
						}
						this.txtStrokeThick.Text = ellipse.StrokeThickness.ToString();
						this.grdPropStrokeThick.Visibility = System.Windows.Visibility.Visible;
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(ellipse.Stroke.ToString());
						this.StrokeColorChooser.SelectedColor = new Color?(brush.Color);
						this.grdPropStroke.Visibility = System.Windows.Visibility.Visible;
						this.txtDynVisible.Text = "";
						if (this.MyItems[id].Dynamics != null)
						{
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
							if (iddyn >= 0)
							{
								this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
						}
						this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
					}
					else if (text == "System.Windows.Shapes.Rectangle")
					{
						Rectangle rectangle = this.SelItem.Content as Rectangle;
						if (rectangle.Fill != null)
						{
							if (rectangle.Fill.ToString() != "System.Windows.Media.LinearGradientBrush")
							{
								brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(rectangle.Fill.ToString());
								this.FillColorChooser.SelectedColor = new Color?(brush.Color);
								this.grdPropFill.Visibility = System.Windows.Visibility.Visible;
							}
							else
							{
								LinearGradientBrush grad = (LinearGradientBrush)rectangle.Fill;
								BrushConverter brushConverter2 = new BrushConverter();
								color = grad.GradientStops[0].Color;
								brush = (SolidColorBrush)brushConverter2.ConvertFromString(color.ToString());
								this.Grad1ColorChooser.SelectedColor = new Color?(brush.Color);
								BrushConverter brushConverter3 = new BrushConverter();
								color = grad.GradientStops[1].Color;
								brush = (SolidColorBrush)brushConverter3.ConvertFromString(color.ToString());
								this.Grad2ColorChooser.SelectedColor = new Color?(brush.Color);
								this.grdPropGrad1.Visibility = System.Windows.Visibility.Visible;
								this.grdPropGrad2.Visibility = System.Windows.Visibility.Visible;
							}
						}
						this.txtStrokeThick.Text = rectangle.StrokeThickness.ToString();
						this.grdPropStrokeThick.Visibility = System.Windows.Visibility.Visible;
						brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(rectangle.Stroke.ToString());
						this.StrokeColorChooser.SelectedColor = new Color?(brush.Color);
						this.grdPropStroke.Visibility = System.Windows.Visibility.Visible;
						this.txtRadiusX.Text = rectangle.RadiusX.ToString();
						this.grdPropRadiusX.Visibility = System.Windows.Visibility.Visible;
						this.txtRadiusY.Text = rectangle.RadiusY.ToString();
						this.grdPropRadiusY.Visibility = System.Windows.Visibility.Visible;
						this.txtDynVisible.Text = "";
						if (this.MyItems[id].Dynamics != null)
						{
							iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
							if (iddyn >= 0)
							{
								this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
							}
						}
						this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
					}
					else if (text != "System.Windows.Shapes.Path")
					{
						switch (text)
						{
							case "System.Windows.Controls.Grid":
								{
									Grid grid = this.SelItem.Content as Grid;
									if (grid.Children.Count > 0)
									{
										if (grid.Children[0].ToString() == "MaterialDesignThemes.Wpf.PackIcon")
										{
											PackIcon icon = grid.Children[0] as PackIcon;
											brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(icon.Foreground.ToString());
											this.ForeColorChooser.SelectedColor = new Color?(brush.Color);
											this.grdPropForeground.Visibility = System.Windows.Visibility.Visible;
											this.txtKind.Text = icon.Kind.ToString();
											this.grdPropKind.Visibility = System.Windows.Visibility.Visible;
											this.txtDynVisible.Text = "";
											if (this.MyItems[id].Dynamics != null)
											{
												iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
												if (iddyn >= 0)
												{
													this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
												}
											}
											this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
										}
									}
									break;
								}
							case "System.Windows.Controls.ProgressBar":
								{
									ProgressBar prog = this.SelItem.Content as ProgressBar;
									brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(prog.Foreground.ToString());
									this.ForeColorChooser.SelectedColor = new Color?(brush.Color);
									this.grdPropForeground.Visibility = System.Windows.Visibility.Visible;
									brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(prog.Background.ToString());
									this.BackColorChooser.SelectedColor = new Color?(brush.Color);
									this.grdPropBack.Visibility = System.Windows.Visibility.Visible;
									brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(prog.BorderBrush.ToString());
									this.BorderColorChooser.SelectedColor = new Color?(brush.Color);
									this.grdPropBorder.Visibility = System.Windows.Visibility.Visible;
									this.txtProgValue.Text = prog.Value.ToString();
									this.txtProgMin.Text = prog.Minimum.ToString();
									this.txtProgMax.Text = prog.Maximum.ToString();
									if (prog.Orientation != Orientation.Vertical)
									{
										this.radProgHorizontal.IsChecked = new bool?(true);
									}
									else
									{
										this.radProgVertical.IsChecked = new bool?(true);
									}
									this.grdPropProg.Visibility = System.Windows.Visibility.Visible;
									this.txtDynValue.Text = "";
									this.txtDynVisible.Text = "";
									if (this.MyItems[id].Dynamics != null)
									{
										iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Value");
										if (iddyn >= 0)
										{
											this.txtDynValue.Text = this.MyItems[id].Dynamics[iddyn].Tag;
										}
										iddyn = this.MyItems[id].Dynamics.FindIndex((MainWindow.clsDynamic a) => a.Key == "Visible");
										if (iddyn >= 0)
										{
											this.txtDynVisible.Text = this.MyItems[id].Dynamics[iddyn].Tag;
										}
									}
									this.grdDynVisible.Visibility = System.Windows.Visibility.Visible;
									this.grdDynValue.Visibility = System.Windows.Visibility.Visible;
									break;
								}
						}
					}
				}
			}
			else
			{
				this.ResetProperty();
			}
		}

		private void cekShowAxisX_Checked(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				CartesianChart chart = SelItem.Content as CartesianChart;
				chart.AxisX[0].ShowLabels = cekShowAxisX.IsChecked.Value;
			}
		}

		private void cekShowAxisY_Checked(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				CartesianChart chart = SelItem.Content as CartesianChart;
				chart.AxisY[0].ShowLabels = cekShowAxisY.IsChecked.Value;
			}
		}

		private void chkBold_Checked(object sender, RoutedEventArgs e)
		{
			(this.SelItem.Content as TextBlock).FontWeight = FontWeights.Bold;
		}

		private void chkBold_Unchecked(object sender, RoutedEventArgs e)
		{
			(this.SelItem.Content as TextBlock).FontWeight = FontWeights.Normal;
		}

		private void chkItalic_Checked(object sender, RoutedEventArgs e)
		{
			(this.SelItem.Content as TextBlock).FontStyle = FontStyles.Italic;
		}

		private void chkItalic_Unchecked(object sender, RoutedEventArgs e)
		{
			(this.SelItem.Content as TextBlock).FontStyle = FontStyles.Normal;
		}

		private double ConvertDW2Double(short int1, short int2)
		{
			byte[] intBytes1 = BitConverter.GetBytes(int1);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(intBytes1);
			}
			byte[] intBytes2 = BitConverter.GetBytes(int2);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(intBytes2);
			}
			byte[] _bytes = new byte[] { intBytes2[1], intBytes2[0], intBytes1[1], intBytes1[0] };
			double _val = (double)BitConverter.ToSingle(_bytes, 0);
			return Math.Round(_val, 2);
		}

		public void DeactivateRuntime()
		{
			foreach (MainWindow.clsConnection con in this.MyConnections)
			{
				if ((con.Type == "ModbusTCP" ? true : con.Type == "ModbusRTU"))
				{
					if (con.TimerReconnect != null)
					{
						con.TimerReconnect.Stop();
					}
					con.TimerPoll.Stop();
					con.TimerPoll.Tick -= new EventHandler(this.TimerPoll_Tick);
					con.TimerPoll = null;
					con.ModClient.Disconnect();
					con.ModClient = null;
				}
				else if (con.Type == "Firebase")
				{
					this.StopFirebaseReading();
				}
				else if (con.Type == "MQTT")
				{
					if (con.MqttClient == null ? false : con.MqttClient.IsConnected)
					{
						con.MqttClient.Disconnect();
						con.MqttClient.MqttMsgPublishReceived += new MqttClient.MqttMsgPublishEventHandler(MqttClient_MqttMsgPublishReceived);
					}
					con.MqttClient = null;
				}
				else if (con.Type == "SQL")
				{
					if (con.MySqlCon != null)
					{
						con.MySqlCon.Close();
					}
					if (con.SqlCon != null)
					{
						con.SqlCon.Close();
					}
					con.MySqlCon = null;
					con.SqlCon = null;
				}
			}
			
			this.RuntimeisActive = false;
			DesignerCanvas.RunActive = false;

			if (this.timerPlot != null)
			{
				this.timerPlot.Stop();
			}
			this.MyDesigner.IsHitTestVisible = true;
			this.gridProperty.IsEnabled = true;
			this.gridTool.IsEnabled = true;
			this.rectRuntime.Visibility = System.Windows.Visibility.Collapsed;
			foreach (MainWindow.MyItem item in this.MyItems)
			{
				item.DesItem.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void dgChartSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int id = this.dgChartSeries.SelectedIndex;
			if (id < 0)
			{
				this.grdAddChartSerie.Visibility = System.Windows.Visibility.Collapsed;
			}
			else
			{
				CartesianChart chart = this.SelItem.Content as CartesianChart;
				string ctype = chart.Series[id].ToString();
				string scolor = "";
				string svals = "";
				if (ctype == "LiveCharts.Wpf.LineSeries")
				{
					LineSeries serie = chart.Series[id] as LineSeries;
					this.radSerieLine.IsChecked = new bool?(true);
					this.txtChartSerieTitle.Text = serie.Title;
					int totVal = serie.Values.Count;
					if (totVal > 5)
					{
						totVal = 5;
					}
					for (int i = 0; i < totVal; i++)
					{
						svals = string.Concat(svals, serie.Values[i].ToString());
						if (i < totVal - 1)
						{
							svals = string.Concat(svals, ",");
						}
					}
					scolor = serie.Stroke.ToString();
					if (!serie.DataLabels)
					{
						this.radDataLabelNone.IsChecked = new bool?(true);
					}
					else
					{
						this.radDataLabelShow.IsChecked = new bool?(true);
					}
				}
				else if (ctype == "LiveCharts.Wpf.ColumnSeries")
				{
					ColumnSeries serie = chart.Series[id] as ColumnSeries;
					this.radSerieBar.IsChecked = new bool?(true);
					this.txtChartSerieTitle.Text = serie.Title;
					int totVal = serie.Values.Count;
					if (totVal > 5)
					{
						totVal = 5;
					}
					for (int i = 0; i < totVal; i++)
					{
						svals = string.Concat(svals, serie.Values[i].ToString());
						if (i < totVal - 1)
						{
							svals = string.Concat(svals, ",");
						}
					}
					scolor = serie.Fill.ToString();
					if (!serie.DataLabels)
					{
						this.radDataLabelNone.IsChecked = new bool?(true);
					}
					else
					{
						this.radDataLabelShow.IsChecked = new bool?(true);
					}
				}
				this.txtChartSerieValues.Text = svals;
				SolidColorBrush brush1 = (SolidColorBrush)(new BrushConverter()).ConvertFromString(scolor);
				this.StrokeSerieColorChooser.SelectedColor = new Color?(brush1.Color);
				this.grdAddChartSerie.IsEnabled = false;
				this.grdAddChartSerie.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void FillColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.FillColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.FillColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "System.Windows.Shapes.Ellipse")
					{
						(this.SelItem.Content as Ellipse).Fill = brush;
					}
					else if (this.txtType.Text == "System.Windows.Shapes.Rectangle")
					{
						(this.SelItem.Content as Rectangle).Fill = brush;
					}
				}
			}
		}

		private void FillSerieColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
		}

		private void FontChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.FontChooser.SelectedItem != null)
			{
				if (this.SelItem != null)
				{
					TextBlock ntxt = this.SelItem.Content as TextBlock;
					ntxt.FontFamily = (System.Windows.Media.FontFamily)this.FontChooser.SelectedItem;
				}
			}
		}

		private void ForeColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.ForeColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.ForeColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "System.Windows.Controls.TextBlock")
					{
						(this.SelItem.Content as TextBlock).Foreground = brush;
					}
					else if (this.txtType.Text == "System.Windows.Controls.Grid")
					{
						Grid grid = this.SelItem.Content as Grid;
						if (grid.Children.Count > 0)
						{
							if (grid.Children[0].ToString() == "MaterialDesignThemes.Wpf.PackIcon")
							{
								(grid.Children[0] as PackIcon).Foreground = brush;
							}
						}
					}
					else if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
					{
						(this.SelItem.Content as AngularGauge).Foreground = brush;
					}
					else if (this.txtType.Text == "LiveCharts.Wpf.PieChart")
					{
						(this.SelItem.Content as PieChart).Foreground = brush;
					}
					else if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
					{
						(this.SelItem.Content as CartesianChart).Foreground = brush;
					}
					else if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
					{
						(this.SelItem.Content as ProgressBar).Foreground = brush;
					}
				}
			}
		}

		private void Fullscreen_Toggle()
		{
			if (this.FullscreenisActive)
			{
				MainWindow.AppWindow.gridHeader.Visibility = System.Windows.Visibility.Visible;
				MainWindow.AppWindow.colToolbox.Width = new GridLength(280);
				MainWindow.AppWindow.colProperty.Width = new GridLength(280);
				MainWindow.AppWindow.tabMain.Margin = new Thickness(0, 0, 0, 0);
				MainWindow.AppWindow.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
				MainWindow.AppWindow.pnlTopRight.Visibility = System.Windows.Visibility.Visible;
				MainWindow.AppWindow.pnlMenu.Visibility = System.Windows.Visibility.Visible;
				this.FullscreenisActive = false;
			}
			else
			{
				MainWindow.AppWindow.gridHeader.Visibility = System.Windows.Visibility.Collapsed;
				MainWindow.AppWindow.colToolbox.Width = new GridLength(0);
				MainWindow.AppWindow.colProperty.Width = new GridLength(0);
				MainWindow.AppWindow.tabMain.Margin = new Thickness(-7, -95, -7, -7);
				MainWindow.AppWindow.WindowStyle = System.Windows.WindowStyle.None;
				MainWindow.AppWindow.pnlTopRight.Visibility = System.Windows.Visibility.Collapsed;
				MainWindow.AppWindow.pnlMenu.Visibility = System.Windows.Visibility.Collapsed;
				this.FullscreenisActive = true;
			}
		}

		private string GetColorName(SolidColorBrush brush)
		{
			IEnumerable<string> results =
				from p in typeof(Colors).GetProperties()
				where (Color)p.GetValue(null, null) == brush.Color
				select p.Name;
			return (results.Count<string>() > 0 ? results.First<string>() : string.Empty);
		}

		private void Grad1ColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.Grad1ColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.Grad1ColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "System.Windows.Shapes.Ellipse")
					{
						LinearGradientBrush grad = (LinearGradientBrush)(this.SelItem.Content as Ellipse).Fill;
						grad.GradientStops[0].Color = brush.Color;
					}
					else if (this.txtType.Text == "System.Windows.Shapes.Rectangle")
					{
						LinearGradientBrush grad = (LinearGradientBrush)(this.SelItem.Content as Rectangle).Fill;
						grad.GradientStops[0].Color = brush.Color;
					}
				}
			}
		}

		private void Grad2ColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.Grad2ColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.Grad2ColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "System.Windows.Shapes.Ellipse")
					{
						LinearGradientBrush grad = (LinearGradientBrush)(this.SelItem.Content as Ellipse).Fill;
						grad.GradientStops[1].Color = brush.Color;
					}
					else if (this.txtType.Text == "System.Windows.Shapes.Rectangle")
					{
						LinearGradientBrush grad = (LinearGradientBrush)(this.SelItem.Content as Rectangle).Fill;
						grad.GradientStops[1].Color = brush.Color;
					}
				}
			}
		}

		private void gridProperty_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.grdPropImage.Visibility == System.Windows.Visibility.Visible)
			{
				this.txtImage.Text = "";
			}
		}

		private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			if (this.grdPropImage.Visibility == System.Windows.Visibility.Visible)
			{
				Image image = this.SelItem.Content as Image;
				this.txtImage.Text = image.Source.ToString();
			}
		}

		private void GridSplitter_MouseUp(object sender, MouseButtonEventArgs e)
		{
		}

		public void LoadSettings()
		{
			this.Set_Runtime_PlotInterval = Settings.Default.Set_Runtime_PlotInterval;
			this.Set_Runtime_AutoMaximize = Settings.Default.Set_Runtime_AutoMaximize;
			this.Set_Runtime_AutoFullscreen = Settings.Default.Set_Runtime_AutoFullscreen;
			this.Set_Startup_AutoOpenFile = Settings.Default.Set_Startup_AutoOpenFile;
			this.Set_Startup_AutoActivate = Settings.Default.Set_Startup_AutoActivate;
			this.Set_FBStorage_Path = Settings.Default.Set_FBStorage_Path;
			this.Set_FBStorage_FBPath = Settings.Default.Set_FBStorage_FBPath;
			this.Set_FBStorage_FBSecret = Settings.Default.Set_FBStorage_FBSecret;
			this.Set_CSV_Separator = Settings.Default.Set_CSV_Separator;
		}

		private void lsvMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int id = this.lsvMenu.SelectedIndex;
			if (id >= 0)
			{
				this.tabMain.SelectedIndex = id;
				this.mnuDes.Width = 60;
				this.mnuDes.Background = Brushes.Gray;
				this.kindMnuDes.Foreground = Brushes.Silver;
				this.lblMnuDes.Visibility = System.Windows.Visibility.Collapsed;
				this.mnuTag.Width = 60;
				this.mnuTag.Background = Brushes.Gray;
				this.kindMnuTag.Foreground = Brushes.Silver;
				this.lblMnuTag.Visibility = System.Windows.Visibility.Collapsed;
				this.mnuSet.Width = 60;
				this.mnuSet.Background = Brushes.Gray;
				this.kindMnuSet.Foreground = Brushes.Silver;
				this.lblMnuSet.Visibility = System.Windows.Visibility.Collapsed;
				switch (id)
				{
					case 0:
						{
							this.mnuDes.Width = 200;
							this.mnuDes.Background = Brushes.White;
							this.kindMnuDes.Foreground = Brushes.IndianRed;
							this.lblMnuDes.Visibility = System.Windows.Visibility.Visible;
							break;
						}
					case 1:
						{
							this.mnuTag.Width = 200;
							this.mnuTag.Background = Brushes.White;
							this.kindMnuTag.Foreground = Brushes.Blue;
							this.lblMnuTag.Visibility = System.Windows.Visibility.Visible;
							break;
						}
					case 2:
						{
							this.mnuSet.Width = 200;
							this.mnuSet.Background = Brushes.White;
							this.kindMnuSet.Foreground = Brushes.Orange;
							this.lblMnuSet.Visibility = System.Windows.Visibility.Visible;
							break;
						}
				}
			}
		}

		private void menuFullscreen_Click(object sender, RoutedEventArgs e)
		{
			this.Fullscreen_Toggle();
		}

		private void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			string topic = e.Topic;
			string sdata = Encoding.UTF8.GetString(e.Message);
			int num = this.MyTags.FindIndex((MainWindow.clsTag a) => a.Address == topic);
			if (num >= 0)
			{
				this.MyTags[num].Value = sdata;
				Application.Current.Dispatcher.Invoke(() => this.UpdateDgTagValue(num));
			}
		}

		private void NeedleColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.NeedleColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.NeedleColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
					{
						(this.SelItem.Content as AngularGauge).NeedleFill = brush;
					}
				}
			}
		}

		public void OpenWindowDownload()
		{
			(new Win_FBDownload()).Show();
		}

		private void radLegendBottom_Checked(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.PieChart")
			{
				(this.SelItem.Content as PieChart).LegendLocation = LegendLocation.Bottom;
			}
			else if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				(this.SelItem.Content as CartesianChart).LegendLocation = LegendLocation.Bottom;
			}
		}

		private void radLegendLeft_Checked(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.PieChart")
			{
				(this.SelItem.Content as PieChart).LegendLocation = LegendLocation.Left;
			}
			else if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				(this.SelItem.Content as CartesianChart).LegendLocation = LegendLocation.Left;
			}
		}

		private void radLegendNone_Checked(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.PieChart")
			{
				(this.SelItem.Content as PieChart).LegendLocation = LegendLocation.None;
			}
			else if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				(this.SelItem.Content as CartesianChart).LegendLocation = LegendLocation.None;
			}
		}

		private void radLegendRight_Checked(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.PieChart")
			{
				(this.SelItem.Content as PieChart).LegendLocation = LegendLocation.Right;
			}
			else if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				(this.SelItem.Content as CartesianChart).LegendLocation = LegendLocation.Right;
			}
		}

		private void radLegendTop_Checked(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.PieChart")
			{
				(this.SelItem.Content as PieChart).LegendLocation = LegendLocation.Top;
			}
			else if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				(this.SelItem.Content as CartesianChart).LegendLocation = LegendLocation.Top ;
			}
		}

		private void radProgHorizontal_Checked(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
				{
					(this.SelItem.Content as ProgressBar).Orientation = Orientation.Horizontal;
				}
			}
		}

		private void radProgVertical_Checked(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
				{
					(this.SelItem.Content as ProgressBar).Orientation = Orientation.Vertical;
				}
			}
		}

		public void ResetProperty()
		{
			this.SelItem = null;
			this.txtType.Text = "Canvas";
			foreach (Grid grid in this.stkProp.Children)
			{
				grid.Visibility = System.Windows.Visibility.Collapsed;
			}
			foreach (Grid grid in this.stkDyn.Children)
			{
				grid.Visibility = System.Windows.Visibility.Collapsed;
			}
			SolidColorBrush brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(this.MyDesigner.Background.ToString());
			this.BackColorChooser.SelectedColor = new Color?(brush.Color);
			this.grdPropBack.Visibility = System.Windows.Visibility.Visible;
		}

		public void SaveSettings()
		{
			Settings.Default.Set_Runtime_PlotInterval = this.Set_Runtime_PlotInterval;
			Settings.Default.Set_Runtime_AutoMaximize = this.Set_Runtime_AutoMaximize;
			Settings.Default.Set_Runtime_AutoFullscreen = this.Set_Runtime_AutoFullscreen;
			Settings.Default.Set_Startup_AutoOpenFile = this.Set_Startup_AutoOpenFile;
			Settings.Default.Set_Startup_AutoActivate = this.Set_Startup_AutoActivate;
			Settings.Default.Set_FBStorage_Path = this.Set_FBStorage_Path;
			Settings.Default.Set_FBStorage_FBPath = this.Set_FBStorage_FBPath;
			Settings.Default.Set_FBStorage_FBSecret = this.Set_FBStorage_FBSecret;
			Settings.Default.Set_CSV_Separator = this.Set_CSV_Separator;
			Settings.Default.Save();
		}

		private void SectionColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
		}

		private void SerieColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
		}

		public void SetFirebase()
		{
			try
			{
				string FB_Path = "";
				string FB_Secret = "";
				foreach (MainWindow.clsConnection con in this.MyConnections)
				{
					if (con.Type == "Firebase")
					{
						FB_Path = con.FBPath;
						FB_Secret = con.FBSecret;
						break;
					}
				}
				FirebaseConfig firebaseConfig = new FirebaseConfig();
				firebaseConfig.BasePath = FB_Path;
				firebaseConfig.AuthSecret = FB_Secret;
				MainWindow.FB_Client = new FirebaseClient(firebaseConfig);
			}
			catch (Exception exception)
			{
				exception.ToString();
			}
		}

		private void StartFirebaseReading()
		{
			try
			{
				this.bWorkerFB.DoWork -= new DoWorkEventHandler(this.workerFB_DoWork);
				this.bWorkerFB.DoWork += new DoWorkEventHandler(this.workerFB_DoWork);
				this.bWorkerFB.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
				this.bWorkerFB.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
				int iSec = 0;
				MainWindow.clsConnection con = this.MyConnections.Find((MainWindow.clsConnection a) => a.Type == "Firebase");
				if (con != null)
				{
					iSec = int.Parse(con.Interval);
				}
				if (iSec <= 0)
				{
					iSec = 5;
				}
				this.timerFB = new DispatcherTimer()
				{
					Interval = TimeSpan.FromSeconds((double)iSec)
				};
				this.timerFB.Tick += new EventHandler((object sender, EventArgs args) => {
					this.timerFB.Stop();
					if (!this.bWorkerFB.IsBusy)
					{
						this.bWorkerFB.RunWorkerAsync();
					}
				});
				this.timerFB.Start();
			}
			catch (Exception exception)
			{
				exception.ToString();
			}
		}

		private void StopFirebaseReading()
		{
			try
			{
				if (this.timerFB != null)
				{
					this.timerFB.Stop();
				}
				MainWindow.FB_Client = null;
			}
			catch (Exception exception)
			{
				exception.ToString();
			}
		}

		private void StrokeColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.StrokeColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.StrokeColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "System.Windows.Shapes.Ellipse")
					{
						(this.SelItem.Content as Ellipse).Stroke = brush;
					}
					else if (this.txtType.Text == "System.Windows.Shapes.Rectangle")
					{
						(this.SelItem.Content as Rectangle).Stroke = brush;
					}
				}
			}
		}

		private void StrokeSerieColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
		}

		private void TicksColorChooser_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			if (this.TicksColorChooser.SelectedColor.HasValue)
			{
				if (this.SelItem != null)
				{
					SolidColorBrush brush = new SolidColorBrush(this.TicksColorChooser.SelectedColor.Value);
					if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
					{
						(this.SelItem.Content as AngularGauge).TicksForeground = brush;
					}
				}
			}
		}

		private void TimerPoll_Tick(object sender, EventArgs e)
		{
			//Parsing data registry modbus 10001-60001
			int[] vals;
			int idtag = 0;
			foreach (MainWindow.clsTag tag in this.MyTags)
			{
				string connection = tag.Connection;
				int idc = this.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == connection);
				if (idc >= 0)
				{
					MainWindow.clsConnection con = this.MyConnections[idc];
					if ((con.Type == "ModbusTCP" ? true : con.Type == "ModbusRTU"))
					{
						string sadr = tag.Address;
						int iadr = int.Parse(sadr);
						if (sadr.Length != 5)
						{
							bool[] valsflag = con.ModClient.ReadCoils(iadr, 1);
							tag.Value = valsflag[0].ToString();
						}
						else if (sadr.Substring(0, 1) == "4")
						{
							int adr = iadr - 40001;
							if (tag.DataType != "Float")
							{
								vals = con.ModClient.ReadHoldingRegisters(adr, 1);
								tag.Value = vals[0].ToString();
							}
							else
							{
								vals = con.ModClient.ReadHoldingRegisters(adr, 2);
								double kw = this.ConvertDW2Double((short)vals[0], (short)vals[1]);
								tag.Value = kw.ToString();
							}
						}
						else if (sadr.Substring(0, 1) == "3")
						{
							int adr = iadr - 30001;
							if (tag.DataType != "Float")
							{
								vals = con.ModClient.ReadInputRegisters(adr, 1);
								tag.Value = vals[0].ToString();
							}
							else
							{
								vals = con.ModClient.ReadInputRegisters(adr, 2);
								double kw = this.ConvertDW2Double((short)vals[0], (short)vals[1]);
								tag.Value = kw.ToString();
							}
						}
						else if (sadr.Substring(0, 1) == "1")
						{
							int adr = iadr - 10001;
							bool[] valsflag = con.ModClient.ReadDiscreteInputs(adr, 1);
							tag.Value = valsflag[0].ToString();
						}
						else if (sadr.Substring(0, 1) == "0")
						{
							bool[] valsflag = con.ModClient.ReadCoils(iadr, 1);
							tag.Value = valsflag[0].ToString();
						}
					}
					this.UpdateDgTagValue(idtag);
				}
				idtag++;
			}
		}

		private void TimerReconnect_Tick(object sender, EventArgs e)
		{
			DispatcherTimer timer = (DispatcherTimer)sender;
			int i = int.Parse(timer.Tag.ToString());
			timer.Stop();
			MainWindow.clsConnection nInput = this.MyConnections[i];
			try
			{
				if ((nInput.Type.Equals("ModbusTCP") ? true : nInput.Type.Equals("ModbusRTU")))
				{
					nInput.ModClient.Connect();
					nInput.TimerPoll.Start();
					nInput.State = "Connected";
				}
			}
			catch (Exception exception)
			{
				exception.ToString();
				nInput.State = "Disconnected";
				timer.Start();
			}
		}

		private void TimerRun_Tick(object sender, EventArgs e)
		{
			int value;
			float single;
			char sp;
			int i = 0;
			foreach (MainWindow.clsConnection myConnection in this.MyConnections)
			{
				i++;
				if (myConnection.Type == "SQL")
				{
					if ((myConnection.MySqlCon != null ? true : myConnection.SqlCon != null))
					{
						int idtag = 0;
						foreach (MainWindow.clsTag tag in this.MyTags)
						{
							if (this.RuntimeisActive)
							{
								try
								{
									string[] adrs = tag.Address.Split(new char[] { '/' });
									if ((int)adrs.Length >= 2)
									{
										string table = adrs[0];
										string col = adrs[1];
										if (tag.Connection == myConnection.Name)
										{
											if (myConnection.Target != "MySQL")
											{
												SqlCommand cmd = myConnection.SqlCon.CreateCommand();
												cmd.CommandText = string.Concat("SELECT ", col, " FROM ", table);
												SqlDataReader reader = cmd.ExecuteReader();
												while (reader.Read())
												{
													if (tag.DataType == "String")
													{
														tag.Value = (string)reader.GetValue(0);
													}
													else if (tag.DataType == "Integer")
													{
														value = (int)reader.GetValue(0);
														tag.Value = value.ToString();
													}
													else if (tag.DataType == "Float")
													{
														single = (float)reader.GetValue(0);
														tag.Value = single.ToString();
													}
												}
												reader.Close();
											}
											else
											{
												MySqlCommand cmd = myConnection.MySqlCon.CreateCommand();
												cmd.CommandText = string.Concat("SELECT ", col, " FROM ", table);
												MySqlDataReader reader = cmd.ExecuteReader();
												while (reader.Read())
												{
													if (tag.DataType == "String")
													{
														tag.Value = (string)reader.GetValue(0);
													}
													else if (tag.DataType == "Integer")
													{
														value = (int)reader.GetValue(0);
														tag.Value = value.ToString();
													}
													else if (tag.DataType == "Float")
													{
														single = (float)reader.GetValue(0);
														tag.Value = single.ToString();
													}
												}
												reader.Close();
											}
											this.UpdateDgTagValue(idtag);
										}
									}
								}
								catch (Exception exception)
								{
									exception.ToString();
								}
								idtag++;
							}
							else
							{
								return;
							}
						}
					}
					else
					{
						return;
					}
				}
				else if (myConnection.Type == "File")
				{
					bool bok = true;
					try
					{
						if (!File.Exists(myConnection.FileName))
						{
							bok = false;
						}
						else
						{
							sp = (myConnection.FileSeparator.ToUpper() != "{TAB}" ? char.Parse(myConnection.FileSeparator) : '\t');
							if (myConnection.FileType != "Table")
							{
								string[] strArrays = File.ReadAllLines(myConnection.FileName);
								for (int num = 0; num < (int)strArrays.Length; num++)
								{
									string line = strArrays[num];
									string[] ardata = line.Split(new char[] { sp });
									if ((int)ardata.Length >= 2)
									{
										string str = ardata[0];
										string sval = ardata[1];
										int idtag = this.MyTags.FindIndex((MainWindow.clsTag a) => (a.Connection != myConnection.Name ? false : a.Address == str));
										if (idtag >= 0)
										{
											this.MyTags[idtag].Value = sval;
											this.UpdateDgTagValue(idtag);
										}
									}
								}
							}
							else
							{
								int iline = 0;
								string[] arheader = new string[0];
								string[] strArrays1 = File.ReadAllLines(myConnection.FileName);
								int num1 = 0;
								while (num1 < (int)strArrays1.Length)
								{
									string line = strArrays1[num1];
									if (iline != 0)
									{
										string[] ardata = line.Split(new char[] { sp });
										if ((int)ardata.Length >= (int)arheader.Length)
										{
											int idata = 0;
											string[] strArrays2 = ardata;
											for (int j = 0; j < (int)strArrays2.Length; j++)
											{
												string sval = strArrays2[j];
												string str1 = arheader[idata];
												int idtag = this.MyTags.FindIndex((MainWindow.clsTag a) => (a.Connection != myConnection.Name ? false : a.Address == str1));
												if (idtag >= 0)
												{
													this.MyTags[idtag].Value = sval;
													this.UpdateDgTagValue(idtag);
												}
												idata++;
											}
										}
									}
									else
									{
										arheader = line.Split(new char[] { sp });
									}
									iline++;
									if (iline <= 1)
									{
										num1++;
									}
									else
									{
										break;
									}
								}
							}
						}
					}
					catch (Exception exception1)
					{
						exception1.ToString();
						bok = false;
					}
					if (!bok)
					{
						myConnection.State = "Error";
					}
					else
					{
						myConnection.State = "Connected";
					}
				}
			}
		}

		private void txtBorderThick_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtBorderThick_LostFocus(null, null);
			}
		}

		private void txtBorderThick_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
				{
				}
			}
		}

		private void txtDynText_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtDynText_LostFocus(null, null);
			}
		}

		private void txtDynText_LostFocus(object sender, RoutedEventArgs e)
		{
			this.AddDynamic(this.txtDynText, "Text");
		}

		private void txtDynValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtDynValue_LostFocus(null, null);
			}
		}

		private void txtDynValue_LostFocus(object sender, RoutedEventArgs e)
		{
			this.AddDynamic(this.txtDynValue, "Value");
		}

		private void txtDynVisible_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtDynVisible_LostFocus(null, null);
			}
		}

		private void txtDynVisible_LostFocus(object sender, RoutedEventArgs e)
		{
			this.AddDynamic(this.txtDynVisible, "Visible");
		}

		private void txtFontSize_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtFontSize_LostFocus(null, null);
			}
		}

		private void txtFontSize_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBlock ntxt = this.SelItem.Content as TextBlock;
			if (ntxt != null)
			{
				ntxt.FontSize = (double)int.Parse(this.txtFontSize.Text);
			}
		}

		private void txtFromValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtFromValue_LostFocus(null, null);
			}
		}

		private void txtFromValue_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
				{
					AngularGauge gauge = this.SelItem.Content as AngularGauge;
					gauge.FromValue = double.Parse(this.txtFromValue.Text);
				}
			}
		}

		private void txtHeight_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtHeight_LostFocus(null, null);
			}
		}

		private void txtHeight_LostFocus(object sender, RoutedEventArgs e)
		{
			this.designerCanvas = this.MyDesigner;
			foreach (Control item in this.designerCanvas.Children)
			{
				DesignerItem di = item as DesignerItem;
				if (di.ID.ToString() == this.cboItems.SelectedItem.ToString())
				{
					di.Height = double.Parse(this.txtHeight.Text);
					break;
				}
			}
		}

		private void txtKind_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtKind_LostFocus(null, null);
			}
		}

		private void txtKind_LostFocus(object sender, RoutedEventArgs e)
		{
			PackIconKind kind;
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.Grid")
				{
					Grid grid = this.SelItem.Content as Grid;
					if (grid.Children.Count > 0)
					{
						if (grid.Children[0].ToString() == "MaterialDesignThemes.Wpf.PackIcon")
						{
							try
							{
								PackIcon icon = grid.Children[0] as PackIcon;
								Enum.TryParse<PackIconKind>(this.txtKind.Text, true, out kind);
								if (kind.ToString().ToLower() == this.txtKind.Text.ToLower())
								{
									icon.Kind = kind;
								}
								this.txtKind.Text = icon.Kind.ToString();
							}
							catch (Exception exception)
							{
								exception.ToString();
							}
						}
					}
				}
			}
		}

		private void txtLeft_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtLeft_LostFocus(null, null);
			}
		}

		private void txtLeft_LostFocus(object sender, RoutedEventArgs e)
		{
			this.designerCanvas = this.MyDesigner;
			foreach (Control item in this.designerCanvas.Children)
			{
				DesignerItem di = item as DesignerItem;
				if (di.ID.ToString() == this.cboItems.SelectedItem.ToString())
				{
					Canvas.SetLeft(di, double.Parse(this.txtLeft.Text));
					break;
				}
			}
		}

		private void txtMaxData_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtMaxData_LostFocus(null, null);
			}
		}

		private void txtMaxData_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				CartesianChart chart = this.SelItem.Content as CartesianChart;
				if (this.txtMaxData.Text.ToLower() != "auto")
				{
					chart.AxisX[0].MaxValue = double.Parse(this.txtMaxData.Text);
				}
				else
				{
					chart.AxisX[0].MaxValue = double.NaN;
					this.txtMaxData.Text = "Auto";
				}
			}
		}

		private void txtMaxValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtMaxValue_LostFocus(null, null);
			}
		}

		private void txtMaxValue_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				CartesianChart chart = this.SelItem.Content as CartesianChart;
				if (this.txtMaxValue.Text.ToLower() != "auto")
				{
					chart.AxisY[0].MaxValue = double.Parse(this.txtMaxValue.Text);
				}
				else
				{
					chart.AxisY[0].MaxValue = double.NaN;
					this.txtMaxValue.Text = "Auto";
				}
			}
		}

		private void txtMinValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtMinValue_LostFocus(null, null);
			}
		}

		private void txtMinValue_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.txtType.Text == "LiveCharts.Wpf.CartesianChart")
			{
				CartesianChart chart = this.SelItem.Content as CartesianChart;
				if (this.txtMinValue.Text.ToLower() != "auto")
				{
					chart.AxisY[0].MinValue = double.Parse(this.txtMinValue.Text);
				}
				else
				{
					chart.AxisY[0].MinValue = double.NaN;
					this.txtMinValue.Text = "Auto";
				}
			}
		}

		private void txtNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
		}

		private void txtProgMax_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtProgMax_LostFocus(null, null);
			}
		}

		private void txtProgMax_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
				{
					ProgressBar prog = this.SelItem.Content as ProgressBar;
					prog.Maximum = double.Parse(this.txtProgMax.Text);
				}
			}
		}

		private void txtProgMin_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtProgMin_LostFocus(null, null);
			}
		}

		private void txtProgMin_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
				{
					ProgressBar prog = this.SelItem.Content as ProgressBar;
					prog.Minimum = double.Parse(this.txtProgMin.Text);
				}
			}
		}

		private void txtProgValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtProgValue_LostFocus(null, null);
			}
		}

		private void txtProgValue_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Controls.ProgressBar")
				{
					ProgressBar prog = this.SelItem.Content as ProgressBar;
					prog.Value = double.Parse(this.txtProgValue.Text);
				}
			}
		}

		private void txtRadiusX_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtRadiusX_LostFocus(null, null);
			}
		}

		private void txtRadiusX_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Shapes.Rectangle")
				{
					Rectangle rectangle = this.SelItem.Content as Rectangle;
					rectangle.RadiusX = double.Parse(this.txtRadiusX.Text);
				}
			}
		}

		private void txtRadiusY_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtRadiusY_LostFocus(null, null);
			}
		}

		private void txtRadiusY_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Shapes.Rectangle")
				{
					Rectangle rectangle = this.SelItem.Content as Rectangle;
					rectangle.RadiusY = double.Parse(this.txtRadiusY.Text);
				}
			}
		}

		private void txtSectionRadius_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtSectionRadius_LostFocus(null, null);
			}
		}

		private void txtSectionRadius_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
				{
					AngularGauge gauge = this.SelItem.Content as AngularGauge;
					gauge.SectionsInnerRadius = double.Parse(this.txtSectionRadius.Text);
				}
			}
		}

		private void txtSectionRadius_TextChanged(object sender, TextChangedEventArgs e)
		{
			double dval;
			string result = "";
			char[] validChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '.' };
			string text = this.txtSectionRadius.Text;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (Array.IndexOf<char>(validChars, c) != -1)
				{
					if (double.TryParse(string.Concat(result, c.ToString()), out dval))
					{
						result = string.Concat(result, c.ToString());
					}
				}
			}
			this.txtSectionRadius.Text = result;
		}

		private void txtStrokeThick_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtStrokeThick_LostFocus(null, null);
			}
		}

		private void txtStrokeThick_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "System.Windows.Shapes.Ellipse")
				{
					Ellipse ellipse = this.SelItem.Content as Ellipse;
					ellipse.StrokeThickness = double.Parse(this.txtStrokeThick.Text);
				}
				else if (this.txtType.Text == "System.Windows.Shapes.Rectangle")
				{
					Rectangle rectangle = this.SelItem.Content as Rectangle;
					rectangle.StrokeThickness = double.Parse(this.txtStrokeThick.Text);
				}
			}
		}

		private void txtText_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtText_LostFocus(null, null);
			}
		}

		private void txtText_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBlock ntxt = this.SelItem.Content as TextBlock;
			if (ntxt != null)
			{
				ntxt.Text = this.txtText.Text;
			}
		}

		private void txtTop_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtTop_LostFocus(null, null);
			}
		}

		private void txtTop_LostFocus(object sender, RoutedEventArgs e)
		{
			this.designerCanvas = this.MyDesigner;
			foreach (Control item in this.designerCanvas.Children)
			{
				DesignerItem di = item as DesignerItem;
				if (di.ID.ToString() == this.cboItems.SelectedItem.ToString())
				{
					Canvas.SetTop(di, double.Parse(this.txtTop.Text));
					break;
				}
			}
		}

		private void txtToValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtToValue_LostFocus(null, null);
			}
		}

		private void txtToValue_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
				{
					AngularGauge gauge = this.SelItem.Content as AngularGauge;
					gauge.ToValue = double.Parse(this.txtToValue.Text);
				}
			}
		}

		private void txtValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtValue_LostFocus(null, null);
			}
		}

		private void txtValue_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this.SelItem != null)
			{
				if (this.txtType.Text == "LiveCharts.Wpf.AngularGauge")
				{
					AngularGauge gauge = this.SelItem.Content as AngularGauge;
					gauge.Value = double.Parse(this.txtValue.Text);
				}
			}
		}

		private void txtWidth_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.txtWidth_LostFocus(null, null);
			}
		}

		private void txtWidth_LostFocus(object sender, RoutedEventArgs e)
		{
			designerCanvas = MyDesigner;
			foreach (Control item in this.designerCanvas.Children)
			{
				DesignerItem di = item as DesignerItem;
				if (di.ID.ToString() == this.cboItems.SelectedItem.ToString())
				{
					di.Width = double.Parse(this.txtWidth.Text);
					break;
				}
			}
		}

		private void UpdateDgTagValue(int idtag)
		{
			if (this.tabTagMan.IsSelected)
			{
				Page_TagMan.PageTagMan.dgTags.Items[idtag] = new MainWindow.clsTag()
				{
					Name = this.MyTags[idtag].Name,
					Connection = this.MyTags[idtag].Connection,
					Address = this.MyTags[idtag].Address,
					Value = this.MyTags[idtag].Value
				};
			}
		}

		public async void UploadFile()
		{
			bool flag;
			flag = (this.Set_FBStorage_Path == null ? false : this.Set_FBStorage_Path != "");
			if (flag)
			{
				string fileName = "";
				string safeFileName = "";
				OpenFileDialog openFileDialog = new OpenFileDialog()
				{
					Title = "Select a picture",
					Filter = "XML file|*.xml"
				};
				bool? nullable = openFileDialog.ShowDialog();
				if (nullable.GetValueOrDefault() & nullable.HasValue)
				{
					fileName = openFileDialog.FileName;
					safeFileName = openFileDialog.SafeFileName;
				}
				FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				FirebaseStorageTask firebaseStorageTask = (new FirebaseStorage(this.Set_FBStorage_Path, null)).Child("Dashboard").Child(safeFileName).PutAsync(fileStream);
				string str = await firebaseStorageTask;
				string str1 = str;
				str = null;
				string str2 = str1;
				if (MainWindow.FBStorage_Client == null)
				{
					FirebaseConfig firebaseConfig = new FirebaseConfig();
					firebaseConfig.BasePath = this.Set_FBStorage_FBPath;
					firebaseConfig.AuthSecret = this.Set_FBStorage_FBSecret;
					IFirebaseConfig firebaseConfig1 = firebaseConfig;
					MainWindow.FBStorage_Client = new FirebaseClient(firebaseConfig1);
					firebaseConfig1 = null;
				}
				MainWindow.clsFBStorage _clsFBStorage = new MainWindow.clsFBStorage()
				{
					Name = safeFileName,
					Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
					Link = str2
				};
				string str3 = string.Concat("Dashboard/", safeFileName.Replace(".", "_"));
				FirebaseResponse firebaseResponse = MainWindow.FBStorage_Client.Set<MainWindow.clsFBStorage>(str3, _clsFBStorage);
				fileName = null;
				safeFileName = null;
				openFileDialog = null;
				fileStream = null;
				firebaseStorageTask = null;
				str1 = null;
				str2 = null;
				_clsFBStorage = null;
				str3 = null;
				firebaseResponse = null;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (this.RuntimeisActive)
			{
				this.btnDeactivate_Click(null, null);
			}
			Environment.Exit(0);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			
			this.cboItems.Items.Add("Canvas");
			this.ResetProperty();
			this.LoadSettings();
			if ((this.Set_Startup_AutoOpenFile == null ? false : this.Set_Startup_AutoOpenFile != ""))
			{
				DesignerCanvas.DesCanvas.AutoOpenFile_Executed(null, null);
				if (this.Set_Startup_AutoActivate)
				{
					this.ActivateRuntime();
				}
			}
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			int idtag = 0;
			foreach (MainWindow.clsTag tag in this.MyTags)
			{
				string connection = tag.Connection;
				int id = this.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == connection);
				if (id >= 0)
				{
					if (this.MyConnections[id].Type == "Firebase")
					{
						this.UpdateDgTagValue(idtag);
					}
				}
				idtag++;
			}
			this.timerFB.Start();
		}

		private void workerFB_DoWork(object sender, DoWorkEventArgs e)
		{
			foreach (MainWindow.clsTag tag in this.MyTags)
			{
				string connection = tag.Connection;
				int id = this.MyConnections.FindIndex((MainWindow.clsConnection a) => a.Name == connection);
				if (id >= 0)
				{
					if (this.MyConnections[id].Type == "Firebase")
					{
						if (this.RuntimeisActive)
						{
							if (MainWindow.FB_Client == null)
							{
								this.SetFirebase();
							}
							string sPath = tag.Address;
							try
							{
								FirebaseResponse FB_Response = MainWindow.FB_Client.Get(sPath);
								if (FB_Response.Body != null)
								{
									tag.Value = FB_Response.Body.Replace('\"', ' ').Trim();
								}
								this.MyConnections[id].State = "Connected";
							}
							catch (Exception exception)
							{
								exception.ToString();
								this.MyConnections[id].State = "Error";
							}
						}
						else
						{
							return;
						}
					}
				}
			}
		}

		public class ChartSerie
		{
			public string Color
			{
				get;
				set;
			}

			public string Title
			{
				get;
				set;
			}

			public string Type
			{
				get;
				set;
			}

			public string Values
			{
				get;
				set;
			}

			public ChartSerie()
			{
			}
		}

		public class clsConnection
		{
			public short[] AnalogInputs
			{
				get;
				set;
			}

			public string Baud
			{
				get;
				set;
			}

			public short[] CoilOutputs
			{
				get;
				set;
			}

			public string Database
			{
				get;
				set;
			}

			public short[] DigitalInputs
			{
				get;
				set;
			}

			public string FBPath
			{
				get;
				set;
			}

			public string FBSecret
			{
				get;
				set;
			}

			public string FileName
			{
				get;
				set;
			}

			public string FileSeparator
			{
				get;
				set;
			}

			public string FileType
			{
				get;
				set;
			}

			public short[] HoldingRegisters
			{
				get;
				set;
			}

			public string Host
			{
				get;
				set;
			}
						
			public string Interval
			{
				get;
				set;
			}

			public string IP
			{
				get;
				set;
			}

			public ModbusClient ModClient
			{
				get;
				set;
			}

			public MqttClient MqttClient
			{
				get;
				set;
			}

			public MySqlConnection MySqlCon
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}

			public string Parity
			{
				get;
				set;
			}

			public string Pass
			{
				get;
				set;
			}

			public string Port
			{
				get;
				set;
			}

			public string Server
			{
				get;
				set;
			}

			public string Slave
			{
				get;
				set;
			}

			public SqlConnection SqlCon
			{
				get;
				set;
			}

			public string State
			{
				get;
				set;
			}

			public string Table
			{
				get;
				set;
			}

			public string Target
			{
				get;
				set;
			}

			public DispatcherTimer TimerPoll
			{
				get;
				set;
			}

			public DispatcherTimer TimerReconnect
			{
				get;
				set;
			}

			public string Type
			{
				get;
				set;
			}

			public string User
			{
				get;
				set;
			}

			public clsConnection()
			{
			}
		}

		public class clsDynamic
		{
			public string Key
			{
				get;
				set;
			}

			public string Tag
			{
				get;
				set;
			}

			public clsDynamic()
			{
			}
		}

		public class clsFBStorage
		{
			public string Date
			{
				get;
				set;
			}

			public string Link
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}

			public clsFBStorage()
			{
			}
		}

		public class clsModGroup
		{
			public string LengthReg
			{
				get;
				set;
			}

			public string StartReg
			{
				get;
				set;
			}

			public clsModGroup()
			{
			}
		}

		public class clsTag
		{
			public string Address
			{
				get;
				set;
			}

			public string Connection
			{
				get;
				set;
			}

			public string DataType
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}

			public string Value
			{
				get;
				set;
			}

			public clsTag()
			{
			}
		}

		public class DataSerie
		{
			public string Fill
			{
				get;
				set;
			}

			public string Title
			{
				get;
				set;
			}

			public string Values
			{
				get;
				set;
			}

			public DataSerie()
			{
			}
		}

		public class DynSerie
		{
			public string Serie
			{
				get;
				set;
			}

			public string Tag
			{
				get;
				set;
			}

			public DynSerie()
			{
			}
		}

		public class GaugeSection
		{
			public string Fill
			{
				get;
				set;
			}

			public string FromValue
			{
				get;
				set;
			}

			public string ToValue
			{
				get;
				set;
			}

			public GaugeSection()
			{
			}
		}

		public class MyItem
		{
			public DesignerItem DesItem
			{
				get;
				set;
			}

			public List<MainWindow.clsDynamic> Dynamics
			{
				get;
				set;
			}

			public string ID
			{
				get;
				set;
			}

			public MyItem()
			{
			}
		}
	}
}