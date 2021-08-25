using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace DashboardDesigner
{
    public class ToolboxItem : ContentControl
	{
		private Point? dragStartPoint = null;

		static ToolboxItem()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
		}

		public ToolboxItem()
		{
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.LeftButton != MouseButtonState.Pressed)
			{
				this.dragStartPoint = null;
			}
			if (this.dragStartPoint.HasValue)
			{
				string xamlString = "";
				if (base.ToolTip == null)
				{
					try
					{
						xamlString = XamlWriter.Save(base.Content);
					}
					catch (Exception exception)
					{
						exception.ToString();
					}
				}
				else if (base.ToolTip.ToString() == "Angular Gauge")
				{
					xamlString = "<lvc:AngularGauge IsHitTestVisible=\"False\" Value=\"50\" FromValue=\"0\" ToValue=\"100\" SectionsInnerRadius=\".7\"  Foreground=\"Gray\" LabelsEffect=\"{x:Null}\" NeedleFill=\"Gray\" TicksForeground=\"White\" xmlns:lvc=\"clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">";
					xamlString = string.Concat(xamlString, "<lvc:AngularGauge.Sections><lvc:AngularSection FromValue=\"0\" ToValue=\"70\" Fill=\"LightBlue\"/><lvc:AngularSection FromValue=\"70\" ToValue=\"90\" Fill=\"Orange\"/><lvc:AngularSection FromValue=\"90\" ToValue=\"100\" Fill=\"Red\"/></lvc:AngularGauge.Sections></lvc:AngularGauge>");
				}
				else if (base.ToolTip.ToString() == "Cartesian Chart")
				{
					xamlString = "<lvc:CartesianChart IsHitTestVisible=\"False\" xmlns:lvc=\"clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" DisableAnimations=\"True\" >";
					xamlString = string.Concat(xamlString, "<lvc:CartesianChart.Series>");
					xamlString = string.Concat(xamlString, "<lvc:LineSeries Values =\"10,20,30,40,50\" Stroke=\"LightBlue\" Fill=\"Transparent\" DataLabels = \"False\" PointGeometry=\"{x:Null}\" LineSmoothness=\"0\" />");
					xamlString = string.Concat(xamlString, "</lvc:CartesianChart.Series>");
					xamlString = string.Concat(xamlString, "<lvc:CartesianChart.AxisY><lvc:Axis /></lvc:CartesianChart.AxisY>");
					xamlString = string.Concat(xamlString, "<lvc:CartesianChart.AxisX><lvc:Axis /></lvc:CartesianChart.AxisX>");
					xamlString = string.Concat(xamlString, "</lvc:CartesianChart>");
				}
				else if (base.ToolTip.ToString() == "Pie Chart")
				{
					xamlString = "<lvc:PieChart IsHitTestVisible=\"False\" xmlns:lvc=\"clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf\" Tag=\"Pie Chart\"><lvc:PieChart.Series><lvc:PieSeries Values=\"40\" Fill=\"LightBlue\" Title=\"Serie1\"/><lvc:PieSeries Values=\"60\" Fill=\"Orange\" Title=\"Serie2\"/></lvc:PieChart.Series></lvc:PieChart> ";
				}
				else if (base.ToolTip.ToString() != "Image")
				{
					try
					{
						xamlString = XamlWriter.Save(base.Content);
					}
					catch (Exception exception1)
					{
						exception1.ToString();
					}
				}
				else
				{
					xamlString = "<Image Source=\"pack://application:Scato/resources/images/image_icon4.png\" ToolTip=\"Image\" IsHitTestVisible=\"False\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />";
				}
				DragObject dataObject = new DragObject()
				{
					Xaml = xamlString
				};
				WrapPanel panel = VisualTreeHelper.GetParent(this) as WrapPanel;
				if (panel != null)
				{
					double scale = 1.3;
					dataObject.DesiredSize = new Size?(new Size(panel.ItemWidth * scale, panel.ItemHeight * scale));
				}
				DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
				e.Handled = true;
			}
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);
			this.dragStartPoint = new Point?(e.GetPosition(this));
		}
	}
}