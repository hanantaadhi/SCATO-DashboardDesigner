using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace DashboardDesigner
{
	public partial class DesignerCanvas : Canvas
	{
		public static DesignerCanvas DesCanvas;
		public static RoutedCommand Group;
		public static RoutedCommand Ungroup;
		public static RoutedCommand BringForward;
		public static RoutedCommand BringToFront;
		public static RoutedCommand SendBackward;
		public static RoutedCommand SendToBack;
		public static RoutedCommand AlignTop;
		public static RoutedCommand AlignVerticalCenters;
		public static RoutedCommand AlignBottom;
		public static RoutedCommand AlignLeft;
		public static RoutedCommand AlignHorizontalCenters;
		public static RoutedCommand AlignRight;
		public static RoutedCommand DistributeHorizontal;
		public static RoutedCommand DistributeVertical;
		public static RoutedCommand SelectAll;
		public static RoutedCommand Fullscreen;
		public static RoutedCommand ActivateRuntime;
		public static RoutedCommand DeactivateRuntime;
		public static RoutedCommand Upload;
		public static RoutedCommand Download;
		public static RoutedCommand AutoOpenFile;
		public static bool RunActive;
		public static bool FullscreenActive;
		private Point? rubberbandSelectionStartPoint = null;
		private SelectionService selectionService;
		internal DesignerCanvas MyDesigner;

		internal SelectionService SelectionService
		{
			get
			{
				if (selectionService == null)
				{
					selectionService = new SelectionService(this);
				}
				return selectionService;
			}
		}

		static DesignerCanvas()
		{
			DesignerCanvas.Group = new RoutedCommand();
			DesignerCanvas.Ungroup = new RoutedCommand();
			DesignerCanvas.BringForward = new RoutedCommand();
			DesignerCanvas.BringToFront = new RoutedCommand();
			DesignerCanvas.SendBackward = new RoutedCommand();
			DesignerCanvas.SendToBack = new RoutedCommand();
			DesignerCanvas.AlignTop = new RoutedCommand();
			DesignerCanvas.AlignVerticalCenters = new RoutedCommand();
			DesignerCanvas.AlignBottom = new RoutedCommand();
			DesignerCanvas.AlignLeft = new RoutedCommand();
			DesignerCanvas.AlignHorizontalCenters = new RoutedCommand();
			DesignerCanvas.AlignRight = new RoutedCommand();
			DesignerCanvas.DistributeHorizontal = new RoutedCommand();
			DesignerCanvas.DistributeVertical = new RoutedCommand();
			DesignerCanvas.SelectAll = new RoutedCommand();
			DesignerCanvas.Fullscreen = new RoutedCommand();
			DesignerCanvas.ActivateRuntime = new RoutedCommand();
			DesignerCanvas.DeactivateRuntime = new RoutedCommand();
			DesignerCanvas.Upload = new RoutedCommand();
			DesignerCanvas.Download = new RoutedCommand();
			DesignerCanvas.AutoOpenFile = new RoutedCommand();
			DesignerCanvas.RunActive = false;
			DesignerCanvas.FullscreenActive = false;
		}

		public DesignerCanvas()
		{
			DesignerCanvas.DesCanvas = this;
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, new ExecutedRoutedEventHandler(this.New_Executed)));
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new ExecutedRoutedEventHandler(this.Open_Executed)));
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new ExecutedRoutedEventHandler(this.Save_Executed)));
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, new ExecutedRoutedEventHandler(this.Print_Executed)));
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, new ExecutedRoutedEventHandler(this.Cut_Executed), new CanExecuteRoutedEventHandler(this.Cut_Enabled)));
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(this.Copy_Executed), new CanExecuteRoutedEventHandler(this.Copy_Enabled)));
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, new ExecutedRoutedEventHandler(this.Paste_Executed), new CanExecuteRoutedEventHandler(this.Paste_Enabled)));
			base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, new ExecutedRoutedEventHandler(this.Delete_Executed), new CanExecuteRoutedEventHandler(this.Delete_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.Group, new ExecutedRoutedEventHandler(this.Group_Executed), new CanExecuteRoutedEventHandler(this.Group_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.Ungroup, new ExecutedRoutedEventHandler(this.Ungroup_Executed), new CanExecuteRoutedEventHandler(this.Ungroup_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.BringForward, new ExecutedRoutedEventHandler(this.BringForward_Executed), new CanExecuteRoutedEventHandler(this.Order_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.BringToFront, new ExecutedRoutedEventHandler(this.BringToFront_Executed), new CanExecuteRoutedEventHandler(this.Order_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.SendBackward, new ExecutedRoutedEventHandler(this.SendBackward_Executed), new CanExecuteRoutedEventHandler(this.Order_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.SendToBack, new ExecutedRoutedEventHandler(this.SendToBack_Executed), new CanExecuteRoutedEventHandler(this.Order_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.AlignTop, new ExecutedRoutedEventHandler(this.AlignTop_Executed), new CanExecuteRoutedEventHandler(this.Align_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.AlignVerticalCenters, new ExecutedRoutedEventHandler(this.AlignVerticalCenters_Executed), new CanExecuteRoutedEventHandler(this.Align_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.AlignBottom, new ExecutedRoutedEventHandler(this.AlignBottom_Executed), new CanExecuteRoutedEventHandler(this.Align_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.AlignLeft, new ExecutedRoutedEventHandler(this.AlignLeft_Executed), new CanExecuteRoutedEventHandler(this.Align_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.AlignHorizontalCenters, new ExecutedRoutedEventHandler(this.AlignHorizontalCenters_Executed), new CanExecuteRoutedEventHandler(this.Align_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.AlignRight, new ExecutedRoutedEventHandler(this.AlignRight_Executed), new CanExecuteRoutedEventHandler(this.Align_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.DistributeHorizontal, new ExecutedRoutedEventHandler(this.DistributeHorizontal_Executed), new CanExecuteRoutedEventHandler(this.Distribute_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.DistributeVertical, new ExecutedRoutedEventHandler(this.DistributeVertical_Executed), new CanExecuteRoutedEventHandler(this.Distribute_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.SelectAll, new ExecutedRoutedEventHandler(this.SelectAll_Executed)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.Fullscreen, new ExecutedRoutedEventHandler(this.Fullscreen_Executed)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.ActivateRuntime, new ExecutedRoutedEventHandler(this.ActivateRuntime_Executed), new CanExecuteRoutedEventHandler(this.ActivateRuntime_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.DeactivateRuntime, new ExecutedRoutedEventHandler(this.DeactivateRuntime_Executed), new CanExecuteRoutedEventHandler(this.DeactivateRuntime_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.Upload, new ExecutedRoutedEventHandler(this.Upload_Executed), new CanExecuteRoutedEventHandler(this.Upload_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.Download, new ExecutedRoutedEventHandler(this.Download_Executed), new CanExecuteRoutedEventHandler(this.Download_Enabled)));
			base.CommandBindings.Add(new CommandBinding(DesignerCanvas.AutoOpenFile, new ExecutedRoutedEventHandler(this.AutoOpenFile_Executed)));
			DesignerCanvas.SelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
			base.AllowDrop = true;
			Clipboard.Clear();
		}

		private void ActivateRuntime_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!DesignerCanvas.RunActive)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		private void ActivateRuntime_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow.AppWindow.ActivateRuntime();
		}

		private void Align_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void AlignBottom_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double bottom = Canvas.GetTop(selectedItems.First<DesignerItem>()) + selectedItems.First<DesignerItem>().Height;
				foreach (DesignerItem designerItem in selectedItems)
				{
					double delta = bottom - (Canvas.GetTop(designerItem) + designerItem.Height);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem))
					{
						Canvas.SetTop(di, Canvas.GetTop(di) + delta);
					}
				}
			}
		}

		private void AlignHorizontalCenters_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double center = Canvas.GetLeft(selectedItems.First<DesignerItem>()) + selectedItems.First<DesignerItem>().Width / 2;
				foreach (DesignerItem designerItem in selectedItems)
				{
					double delta = center - (Canvas.GetLeft(designerItem) + designerItem.Width / 2);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem))
					{
						Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
					}
				}
			}
		}

		private void AlignLeft_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double left = Canvas.GetLeft(selectedItems.First<DesignerItem>());
				foreach (DesignerItem designerItem in selectedItems)
				{
					double delta = left - Canvas.GetLeft(designerItem);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem))
					{
						Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
					}
				}
			}
		}

		private void AlignRight_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double right = Canvas.GetLeft(selectedItems.First<DesignerItem>()) + selectedItems.First<DesignerItem>().Width;
				foreach (DesignerItem designerItem in selectedItems)
				{
					double delta = right - (Canvas.GetLeft(designerItem) + designerItem.Width);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem))
					{
						Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
					}
				}
			}
		}

		private void AlignTop_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double top = Canvas.GetTop(selectedItems.First<DesignerItem>());
				foreach (DesignerItem designerItem in selectedItems)
				{
					double delta = top - Canvas.GetTop(designerItem);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem))
					{
						Canvas.SetTop(di, Canvas.GetTop(di) + delta);
					}
				}
			}
		}

		private void AlignVerticalCenters_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double bottom = Canvas.GetTop(selectedItems.First<DesignerItem>()) + selectedItems.First<DesignerItem>().Height / 2;
				foreach (DesignerItem designerItem in selectedItems)
				{
					double delta = bottom - (Canvas.GetTop(designerItem) + designerItem.Height / 2);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem))
					{
						Canvas.SetTop(di, Canvas.GetTop(di) + delta);
					}
				}
			}
		}

		public void AutoOpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.OpenFile(XElement.Load(MainWindow.AppWindow.Set_Startup_AutoOpenFile));
		}

		private bool BelongToSameGroup(IGroupable item1, IGroupable item2)
		{
			IGroupable root1 = this.SelectionService.GetGroupRoot(item1);
			IGroupable root2 = this.SelectionService.GetGroupRoot(item2);
			return root1.ID == root2.ID;
		}

		private void BringForward_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List<UIElement> ordered = (
				from item in this.SelectionService.CurrentSelection
				orderby Panel.GetZIndex(item as UIElement) descending
				select item as UIElement).ToList<UIElement>();
			int count = base.Children.Count;
			for (int i = 0; i < ordered.Count; i++)
			{
				int currentIndex = Panel.GetZIndex(ordered[i]);
				int num = Math.Min(count - 1 - i, currentIndex + 1);
				if (currentIndex != num)
				{
					Panel.SetZIndex(ordered[i], num);
					IEnumerable<UIElement> it =
						from item in base.Children.OfType<UIElement>()
						where Panel.GetZIndex(item) == num
						select item;
					foreach (UIElement elm in it)
					{
						if (elm != ordered[i])
						{
							Panel.SetZIndex(elm, currentIndex);
							break;
						}
					}
				}
			}
		}

		private void BringToFront_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List<UIElement> selectionSorted = (
				from item in this.SelectionService.CurrentSelection
				orderby Panel.GetZIndex(item as UIElement)
				select item as UIElement).ToList<UIElement>();
			List<UIElement> childrenSorted = (
				from UIElement item in base.Children
				orderby Panel.GetZIndex(item)
				select item).ToList<UIElement>();
			int i = 0;
			int j = 0;
			foreach (UIElement uIElement in childrenSorted)
			{
				if (!selectionSorted.Contains(uIElement))
				{
					int num = i;
					i = num + 1;
					Panel.SetZIndex(uIElement, num);
				}
				else
				{
					Panel.GetZIndex(uIElement);
					int num1 = j;
					j = num1 + 1;
					Panel.SetZIndex(uIElement, childrenSorted.Count - selectionSorted.Count + num1);
				}
			}
		}

		public void ClearSels()
		{
			this.SelectionService.ClearSelection();
		}

		private void Copy_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.SelectionService.CurrentSelection.Count<ISelectable>() > 0;
		}

		private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.CopyCurrentSelection();
		}

		private void CopyCurrentSelection()
		{
			IEnumerable<DesignerItem> selectedDesignerItems = this.SelectionService.CurrentSelection.OfType<DesignerItem>();
			XElement designerItemsXML = this.SerializeDesignerItems(selectedDesignerItems);
			XElement root = new XElement("Root");
			root.Add(designerItemsXML);
			root.Add(new XAttribute("OffsetX", (object)10));
			root.Add(new XAttribute("OffsetY", (object)10));
			Clipboard.Clear();
			Clipboard.SetData(DataFormats.Xaml, root);
		}

		private void Cut_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.SelectionService.CurrentSelection.Count<ISelectable>() > 0;
		}

		private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.CopyCurrentSelection();
			this.DeleteCurrentSelection();
		}

		private void DeactivateRuntime_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = DesignerCanvas.RunActive;
		}

		private void DeactivateRuntime_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow.AppWindow.DeactivateRuntime();
		}

		private void Delete_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.SelectionService.CurrentSelection.Count<ISelectable>() > 0;
		}

		private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.DeleteCurrentSelection();
		}

		private void DeleteCurrentSelection()
		{
			foreach (DesignerItem item in this.SelectionService.CurrentSelection.OfType<DesignerItem>())
			{
				base.Children.Remove(item);
			}
			this.SelectionService.ClearSelection();
			this.UpdateZIndex();
		}

		private static DesignerItem DeserializeDesignerItem(XElement itemXML, Guid id, double OffsetX, double OffsetY)
		{
			DesignerItem item = new DesignerItem(id)
			{
				Width = double.Parse(itemXML.Element("Width").Value, CultureInfo.InvariantCulture),
				Height = double.Parse(itemXML.Element("Height").Value, CultureInfo.InvariantCulture),
				ParentID = new Guid(itemXML.Element("ParentID").Value),
				IsGroup = bool.Parse(itemXML.Element("IsGroup").Value)
			};
			Canvas.SetLeft(item, double.Parse(itemXML.Element("Left").Value, CultureInfo.InvariantCulture) + OffsetX);
			Canvas.SetTop(item, double.Parse(itemXML.Element("Top").Value, CultureInfo.InvariantCulture) + OffsetY);
			Panel.SetZIndex(item, int.Parse(itemXML.Element("zIndex").Value));
			object content = XamlReader.Load(XmlReader.Create(new StringReader(itemXML.Element("Content").Value)));
			item.Content = content;
			return item;
		}

		private void Distribute_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void DistributeHorizontal_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				let itemLeft = Canvas.GetLeft(item)
				orderby itemLeft
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double left = double.MaxValue;
				double right = double.MinValue;
				double sumWidth = 0;
				foreach (DesignerItem designerItem in selectedItems)
				{
					left = Math.Min(left, Canvas.GetLeft(designerItem));
					right = Math.Max(right, Canvas.GetLeft(designerItem) + designerItem.Width);
					sumWidth += designerItem.Width;
				}
				double distance = Math.Max(0, (right - left - sumWidth) / (double)(selectedItems.Count<DesignerItem>() - 1));
				double offset = Canvas.GetLeft(selectedItems.First<DesignerItem>());
				foreach (DesignerItem designerItem1 in selectedItems)
				{
					double delta = offset - Canvas.GetLeft(designerItem1);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem1))
					{
						Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
					}
					offset = offset + designerItem1.Width + distance;
				}
			}
		}

		private void DistributeVertical_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> selectedItems =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				let itemTop = Canvas.GetTop(item)
				orderby itemTop
				select item;
			if (selectedItems.Count<DesignerItem>() > 1)
			{
				double top = double.MaxValue;
				double bottom = double.MinValue;
				double sumHeight = 0;
				foreach (DesignerItem designerItem in selectedItems)
				{
					top = Math.Min(top, Canvas.GetTop(designerItem));
					bottom = Math.Max(bottom, Canvas.GetTop(designerItem) + designerItem.Height);
					sumHeight += designerItem.Height;
				}
				double distance = Math.Max(0, (bottom - top - sumHeight) / (double)(selectedItems.Count<DesignerItem>() - 1));
				double offset = Canvas.GetTop(selectedItems.First<DesignerItem>());
				foreach (DesignerItem designerItem1 in selectedItems)
				{
					double delta = offset - Canvas.GetTop(designerItem1);
					foreach (DesignerItem di in this.SelectionService.GetGroupMembers(designerItem1))
					{
						Canvas.SetTop(di, Canvas.GetTop(di) + delta);
					}
					offset = offset + designerItem1.Height + distance;
				}
			}
		}

		private void Download_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Download_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow.AppWindow.OpenWindowDownload();
		}

		private void Fullscreen_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (DesignerCanvas.FullscreenActive)
			{
				MainWindow.AppWindow.gridHeader.Visibility = System.Windows.Visibility.Visible;
				MainWindow.AppWindow.colToolbox.Width = new GridLength(280);
				MainWindow.AppWindow.colProperty.Width = new GridLength(280);
				MainWindow.AppWindow.tabMain.Margin = new Thickness(0, 0, 0, 0);
				MainWindow.AppWindow.WindowStyle = WindowStyle.SingleBorderWindow;
				MainWindow.AppWindow.pnlTopRight.Visibility = System.Windows.Visibility.Visible;
				MainWindow.AppWindow.pnlMenu.Visibility = System.Windows.Visibility.Visible;
				DesignerCanvas.FullscreenActive = false;
			}
			else
			{
				MainWindow.AppWindow.gridHeader.Visibility = System.Windows.Visibility.Collapsed;
				MainWindow.AppWindow.colToolbox.Width = new GridLength(0);
				MainWindow.AppWindow.colProperty.Width = new GridLength(0);
				MainWindow.AppWindow.tabMain.Margin = new Thickness(-7, -95, -7, -7);
				MainWindow.AppWindow.WindowStyle = WindowStyle.None;
				MainWindow.AppWindow.pnlTopRight.Visibility = System.Windows.Visibility.Collapsed;
				MainWindow.AppWindow.pnlMenu.Visibility = System.Windows.Visibility.Collapsed;
				DesignerCanvas.FullscreenActive = true;
			}
		}

		private static Rect GetBoundingRectangle(IEnumerable<DesignerItem> items)
		{
			double x1 = double.MaxValue;
			double y1 = double.MaxValue;
			double x2 = double.MinValue;
			double y2 = double.MinValue;
			foreach (DesignerItem item in items)
			{
				x1 = Math.Min(Canvas.GetLeft(item), x1);
				y1 = Math.Min(Canvas.GetTop(item), y1);
				x2 = Math.Max(Canvas.GetLeft(item) + item.Width, x2);
				y2 = Math.Max(Canvas.GetTop(item) + item.Height, y2);
			}
			Rect rect = new Rect(new Point(x1, y1), new Point(x2, y2));
			return rect;
		}

		private void Group_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			int count = (
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item).Count<DesignerItem>();
			e.CanExecute = count > 1;
		}

		private void Group_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> items =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID == Guid.Empty
				select item;
			Rect rect = DesignerCanvas.GetBoundingRectangle(items);
			DesignerItem groupItem = new DesignerItem()
			{
				IsGroup = true,
				Width = rect.Width,
				Height = rect.Height
			};
			Canvas.SetLeft(groupItem, rect.Left);
			Canvas.SetTop(groupItem, rect.Top);
			groupItem.Content = new Canvas();
			Panel.SetZIndex(groupItem, base.Children.Count);
			base.Children.Add(groupItem);
			foreach (DesignerItem d in items)
			{
				d.ParentID = groupItem.ID;
			}
			this.SelectionService.SelectItem(groupItem);
		}

		private XElement LoadSerializedDataFromClipBoard()
		{
			XElement xElement;
			if (Clipboard.ContainsData(DataFormats.Xaml))
			{
				string clipboardData = Clipboard.GetData(DataFormats.Xaml) as string;
				if (!string.IsNullOrEmpty(clipboardData))
				{
					try
					{
						xElement = XElement.Load(new StringReader(clipboardData));
						return xElement;
					}
					catch (Exception exception)
					{
						Exception e = exception;
						MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Hand);
					}
				}
				else
				{
					xElement = null;
					return xElement;
				}
			}
			xElement = null;
			return xElement;
		}

		private XElement LoadSerializedDataFromFile(string filename)
		{
			XElement xElement;
			if (filename != "")
			{
				try
				{
					xElement = XElement.Load(filename);
					return xElement;
				}
				catch (Exception exception)
				{
					Exception e = exception;
					MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
			xElement = null;
			return xElement;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			Size size = new Size();
			foreach (UIElement element in base.InternalChildren)
			{
				double left = Canvas.GetLeft(element);
				double top = Canvas.GetTop(element);
				left = (double.IsNaN(left) ? 0 : left);
				top = (double.IsNaN(top) ? 0 : top);
				element.Measure(constraint);
				Size desiredSize = element.DesiredSize;
				if ((double.IsNaN(desiredSize.Width) ? false : !double.IsNaN(desiredSize.Height)))
				{
					size.Width = Math.Max(size.Width, left + desiredSize.Width);
					size.Height = Math.Max(size.Height, top + desiredSize.Height);
				}
			}
			size.Width = size.Width + 10;
			size.Height = size.Height + 10;
			return size;
		}

		private void New_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			base.Children.Clear();
			this.SelectionService.ClearSelection();
			if (MainWindow.AppWindow.RuntimeisActive)
			{
				MainWindow.AppWindow.DeactivateRuntime();
			}
			foreach (MainWindow.clsConnection con in MainWindow.AppWindow.MyConnections)
			{
				Page_TagMan.PageTagMan.trvConModTCP.Items.Clear();
				Page_TagMan.PageTagMan.trvConModRTU.Items.Clear();
				Page_TagMan.PageTagMan.trvConMQTT.Items.Clear();
				Page_TagMan.PageTagMan.trvConFirebase.Items.Clear();
			}
			Page_TagMan.PageTagMan.dgTags.Items.Clear();
			MainWindow.AppWindow.MyConnections.Clear();
			MainWindow.AppWindow.MyTags.Clear();
			MainWindow.AppWindow.cboItems.Items.Clear();
			MainWindow.AppWindow.MyItems.Clear();
			MainWindow.AppWindow.MyDesigner.Background = Brushes.White;
			MainWindow.AppWindow.ResetProperty();
			MainWindow.AppWindow.Title = "SCATO";
		}

		protected override void OnDrop(DragEventArgs e)
		{
			//error from here
			base.OnDrop(e);
			DragObject dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
			if (dragObject == null ? false : !string.IsNullOrEmpty(dragObject.Xaml))
			{
				DesignerItem newItem = null;
				object content = null;
				content = XamlReader.Load(XmlReader.Create(new StringReader(dragObject.Xaml)));
				if (content != null)
				{
					newItem = new DesignerItem()
					{
						Content = content
					};
					string sCont = content.ToString();
					if (sCont == "LiveCharts.Wpf.AngularGauge")
					{
						newItem.Tag = "Angular Gauge";
					}
					else if (sCont == "LiveCharts.Wpf.PieChart")
					{
						newItem.Tag = "Pie Chart";
					}
					else if (sCont == "LiveCharts.Wpf.CartesianChart")
					{
						newItem.Tag = "Cartesian Chart";
					}
					if (sCont.Contains("System.Windows.Controls.ProgressBar"))
					{
						sCont = "System.Windows.Controls.ProgressBar";
					}
					Point position = e.GetPosition(this);
					if (!dragObject.DesiredSize.HasValue)
					{
						Canvas.SetLeft(newItem, Math.Max(0, position.X));
						Canvas.SetTop(newItem, Math.Max(0, position.Y));
					}
					else
					{
						Size desiredSize = dragObject.DesiredSize.Value;
						newItem.Width = desiredSize.Width;
						newItem.Height = desiredSize.Height;
						Canvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
						Canvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
					}
					Panel.SetZIndex(newItem, base.Children.Count);
					base.Children.Add(newItem);
					this.SelectionService.SelectItem(newItem);
					newItem.Focus();
					string sid = newItem.ID.ToString();
					MainWindow.AppWindow.cboItems.Items.Add(sid);
					MainWindow.MyItem myItem = new MainWindow.MyItem()
					{
						DesItem = newItem,
						ID = sid
					};
					MainWindow.AppWindow.MyItems.Add(myItem);
					MainWindow.AppWindow.cboItems.SelectedItem = sid;
					//error until here
				}
				e.Handled = true;
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Source == this)
			{
				this.rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));
				this.SelectionService.ClearSelection();
				base.Focus();
				MainWindow.AppWindow.cboItems.SelectedItem = null;
				MainWindow.AppWindow.ResetProperty();
				e.Handled = true;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.LeftButton != MouseButtonState.Pressed)
			{
				this.rubberbandSelectionStartPoint = null;
			}
			if (this.rubberbandSelectionStartPoint.HasValue)
			{
				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
				if (adornerLayer != null)
				{
					RubberbandAdorner adorner = new RubberbandAdorner(this, this.rubberbandSelectionStartPoint);
					if (adorner != null)
					{
						adornerLayer.Add(adorner);
					}
				}
			}
			e.Handled = true;
		}

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (MainWindow.AppWindow.RuntimeisActive)
			{
				MainWindow.AppWindow.DeactivateRuntime();
			}
			OpenFileDialog openFile = new OpenFileDialog()
			{
				Filter = "Designer Files (*.xml)|*.xml|All Files (*.*)|*.*"
			};
			bool? nullable = openFile.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				try
				{
					this.New_Executed(null, null);
					MainWindow.AppWindow.Title = string.Concat("Scato - ", openFile.FileName);
					this.OpenFile(this.LoadSerializedDataFromFile(openFile.FileName));
				}
				catch (Exception exception)
				{
					Exception ex = exception;
					MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
		}

		public void OpenFile(XElement root)
		{
			if (root != null)
			{
				base.Children.Clear();
				this.SelectionService.ClearSelection();
				MainWindow.AppWindow.cboItems.Items.Clear();
				MainWindow.AppWindow.MyItems.Clear();
				try
				{
					XElement xback = root.Element("Canvas").Element("Background");
					string sBack = xback.Value.ToString();
					SolidColorBrush brush = (SolidColorBrush)(new BrushConverter()).ConvertFromString(sBack);
					MainWindow.AppWindow.MyDesigner.Background = brush;
				}
				catch (Exception exception)
				{
					exception.ToString();
				}
				foreach (XElement itemXML in root.Elements("DesignerItems").Elements<XElement>("DesignerItem"))
				{
					Guid id = new Guid(itemXML.Element("ID").Value);
					DesignerItem item = DesignerCanvas.DeserializeDesignerItem(itemXML, id, 0, 0);
					base.Children.Add(item);
					string sid = id.ToString();
					MainWindow.AppWindow.cboItems.Items.Add(sid);
					MainWindow.MyItem myItem = new MainWindow.MyItem()
					{
						DesItem = item,
						ID = sid
					};
					MainWindow.AppWindow.MyItems.Add(myItem);
				}
				try
				{
					MainWindow.AppWindow.MyConnections.Clear();
					XElement xcon = root.Element("Connections");
					using (IEnumerator<XElement> enumerator = xcon.Elements().GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							XElement con = enumerator.Current;
							MainWindow.clsConnection ncon = new MainWindow.clsConnection()
							{
								Name = con.Name.ToString()
							};
							foreach (XElement conn in con.Elements())
							{
								string str = conn.Name.ToString();
								switch (str)
								{
									case "Type":
										{
											ncon.Type = conn.Value.ToString();
											break;
										}
									case "Interval":
										{
											ncon.Interval = conn.Value.ToString();
											break;
										}
									case "Host":
										{
											ncon.Host = conn.Value.ToString();
											break;
										}
									case "Server":
										{
											ncon.Server = conn.Value.ToString();
											break;
										}
									case "IP":
										{
											ncon.IP = conn.Value.ToString();
											break;
										}
									case "Port":
										{
											ncon.Port = conn.Value.ToString();
											break;
										}
									case "Baud":
										{
											ncon.Baud = conn.Value.ToString();
											break;
										}
									case "Parity":
										{
											ncon.Parity = conn.Value.ToString();
											break;
										}
									case "Slave":
										{
											ncon.Slave = conn.Value.ToString();
											break;
										}
									case "FBPath":
										{
											ncon.FBPath = conn.Value.ToString();
											break;
										}
									case "FBSecret":
										{
											ncon.FBSecret = conn.Value.ToString();
											break;
										}
									case "User":
										{
											ncon.User = conn.Value.ToString();
											break;
										}
									case "Pass":
										{
											ncon.Pass = conn.Value.ToString();
											break;
										}
									case "Database":
										{
											ncon.Database = conn.Value.ToString();
											break;
										}
									case "Target":
										{
											ncon.Target = conn.Value.ToString();
											break;
										}
									case "FileType":
										{
											ncon.FileType = conn.Value.ToString();
											break;
										}
									case "FileSeparator":
										{
											ncon.FileSeparator = conn.Value.ToString();
											break;
										}
									case "FileName":
										{
											ncon.FileName = conn.Value.ToString();
											break;
										}
								}
							}
							MainWindow.AppWindow.MyConnections.Add(ncon);
						}
					}
					MainWindow.AppWindow.MyTags.Clear();
					foreach (XElement tag in root.Element("Tags").Elements())
					{
						MainWindow.clsTag ntag = new MainWindow.clsTag()
						{
							Name = tag.Name.ToString()
						};
						string svals = tag.Value;
						string[] arvals = svals.Split(new char[] { '#' });
						if ((int)arvals.Length >= 3)
						{
							ntag.Connection = arvals[0];
							ntag.Address = arvals[1];
							ntag.DataType = arvals[2];
						}
						MainWindow.AppWindow.MyTags.Add(ntag);
					}
				}
				catch (Exception exception1)
				{
					exception1.ToString();
				}
				try
				{
					foreach (XElement items in root.Element("Dynamics").Elements())
					{
						string value = items.Element("ID").Value;
						int iditem = MainWindow.AppWindow.MyItems.FindIndex((MainWindow.MyItem a) => a.ID == value);
						if (iditem >= 0)
						{
							MainWindow.AppWindow.MyItems[iditem].Dynamics = new List<MainWindow.clsDynamic>();
							foreach (XElement item in items.Elements())
							{
								if (item.Name != "ID")
								{
									string key = item.Name.ToString();
									string tag = item.Value;
									MainWindow.AppWindow.MyItems[iditem].Dynamics.Add(new MainWindow.clsDynamic()
									{
										Key = key,
										Tag = tag
									});
								}
							}
						}
					}
				}
				catch (Exception exception2)
				{
					exception2.ToString();
				}
				MainWindow.AppWindow.ResetProperty();
				base.InvalidateVisual();
			}
		}

		private void Order_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Paste_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = Clipboard.ContainsData(DataFormats.Xaml);
		}

		private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			XElement root = this.LoadSerializedDataFromClipBoard();
			if (root != null)
			{
				Dictionary<Guid, Guid> mappingOldToNewIDs = new Dictionary<Guid, Guid>();
				List<ISelectable> newItems = new List<ISelectable>();
				IEnumerable<XElement> itemsXML = root.Elements("DesignerItems").Elements<XElement>("DesignerItem");
				double offsetX = double.Parse(root.Attribute("OffsetX").Value, CultureInfo.InvariantCulture);
				double offsetY = double.Parse(root.Attribute("OffsetY").Value, CultureInfo.InvariantCulture);
				foreach (XElement itemXML in itemsXML)
				{
					Guid oldID = new Guid(itemXML.Element("ID").Value);
					Guid newID = Guid.NewGuid();
					mappingOldToNewIDs.Add(oldID, newID);
					DesignerItem item = DesignerCanvas.DeserializeDesignerItem(itemXML, newID, offsetX, offsetY);
					base.Children.Add(item);
					newItems.Add(item);
					string sid = item.ID.ToString();
					MainWindow.AppWindow.cboItems.Items.Add(sid);
					MainWindow.MyItem myItem = new MainWindow.MyItem()
					{
						DesItem = item,
						ID = sid
					};
					MainWindow.AppWindow.MyItems.Add(myItem);
					MainWindow.AppWindow.cboItems.SelectedItem = sid;
				}
				this.SelectionService.ClearSelection();
				foreach (DesignerItem el in newItems)
				{
					if (el.ParentID != Guid.Empty)
					{
						el.ParentID = mappingOldToNewIDs[el.ParentID];
					}
				}
				foreach (DesignerItem item in newItems)
				{
					if (item.ParentID == Guid.Empty)
					{
						this.SelectionService.AddToSelection(item);
					}
				}
				DesignerCanvas.BringToFront.Execute(null, this);
				double num = offsetX + 10;
				root.Attribute("OffsetX").Value = num.ToString();
				num = offsetY + 10;
				root.Attribute("OffsetY").Value = num.ToString();
				Clipboard.Clear();
				Clipboard.SetData(DataFormats.Xaml, root);
			}
		}

		private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.SelectionService.ClearSelection();
			PrintDialog printDialog = new PrintDialog();
			if (true & printDialog.ShowDialog().HasValue)
			{
				printDialog.PrintVisual(this, "WPF Diagram");
			}
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> designerItems = base.Children.OfType<DesignerItem>();
			XElement designerItemsXML = this.SerializeDesignerItems(designerItems);
			XElement root = new XElement("Root");
			XElement canvas = new XElement("Canvas");
			XElement cback = new XElement("Background", MainWindow.AppWindow.MyDesigner.Background.ToString());
			canvas.Add(cback);
			root.Add(canvas);
			root.Add(designerItemsXML);
			XElement xcon = new XElement("Connections");
			foreach (MainWindow.clsConnection conn in MainWindow.AppWindow.MyConnections)
			{
				XElement ncon = new XElement(conn.Name);
				if (conn.Type == "OPC")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("Host", conn.Host));
					ncon.Add(new XElement("Server", conn.Server));
				}
				else if (conn.Type == "Firebase")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("FBPath", conn.FBPath));
					ncon.Add(new XElement("FBSecret", conn.FBSecret));
				}
				else if (conn.Type == "ModbusTCP")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("IP", conn.IP));
					ncon.Add(new XElement("Port", conn.Port));
				}
				else if (conn.Type == "ModbusRTU")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("Port", conn.Port));
					ncon.Add(new XElement("Baud", conn.Baud));
					ncon.Add(new XElement("Parity", conn.Parity));
					ncon.Add(new XElement("Slave", conn.Slave));
				}
				else if (conn.Type == "IEC104")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("IP", conn.IP));
					ncon.Add(new XElement("Port", conn.Port));
				}
				else if (conn.Type == "MQTT")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("Host", conn.Host));
					ncon.Add(new XElement("Port", conn.Port));
					ncon.Add(new XElement("User", conn.User));
					ncon.Add(new XElement("Pass", conn.Pass));
				}
				else if (conn.Type == "SQL")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("Target", conn.Target));
					ncon.Add(new XElement("Host", conn.Host));
					ncon.Add(new XElement("User", conn.User));
					ncon.Add(new XElement("Pass", conn.Pass));
					ncon.Add(new XElement("Database", conn.Database));
				}
				else if (conn.Type == "File")
				{
					ncon.Add(new XElement("Type", conn.Type));
					ncon.Add(new XElement("Interval", conn.Interval));
					ncon.Add(new XElement("FileType", conn.FileType));
					ncon.Add(new XElement("FileSeparator", conn.FileSeparator));
					ncon.Add(new XElement("FileName", conn.FileName));
				}
				xcon.Add(ncon);
			}
			XElement xtag = new XElement("Tags");
			foreach (MainWindow.clsTag tag in MainWindow.AppWindow.MyTags)
			{
				string svals = string.Concat(new string[] { tag.Connection, "#", tag.Address, "#", tag.DataType });
				xtag.Add(new XElement(tag.Name, svals));
			}
			root.Add(xcon);
			root.Add(xtag);
			XElement xdyn = new XElement("Dynamics");
			foreach (MainWindow.MyItem nitem in MainWindow.AppWindow.MyItems)
			{
				if (nitem.Dynamics != null)
				{
					XElement xitem = new XElement("item");
					xitem.Add(new XElement("ID", nitem.ID));
					foreach (MainWindow.clsDynamic dyn in nitem.Dynamics)
					{
						xitem.Add(new XElement(dyn.Key, dyn.Tag));
					}
					xdyn.Add(xitem);
				}
			}
			root.Add(xdyn);
			this.SaveFile(root);
		}

		private void SaveFile(XElement xElement)
		{
			SaveFileDialog saveFile = new SaveFileDialog()
			{
				Filter = "Files (*.xml)|*.xml|All Files (*.*)|*.*"
			};
			bool? nullable = saveFile.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				try
				{
					xElement.Save(saveFile.FileName);
				}
				catch (Exception exception)
				{
					Exception ex = exception;
					MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
		}

		private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.SelectionService.SelectAll();
		}

		private void SendBackward_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List<UIElement> ordered = (
				from item in this.SelectionService.CurrentSelection
				orderby Panel.GetZIndex(item as UIElement)
				select item as UIElement).ToList<UIElement>();
			int count = base.Children.Count;
			for (int i = 0; i < ordered.Count; i++)
			{
				int currentIndex = Panel.GetZIndex(ordered[i]);
				int num = Math.Max(i, currentIndex - 1);
				if (currentIndex != num)
				{
					Panel.SetZIndex(ordered[i], num);
					IEnumerable<UIElement> it =
						from item in base.Children.OfType<UIElement>()
						where Panel.GetZIndex(item) == num
						select item;
					foreach (UIElement elm in it)
					{
						if (elm != ordered[i])
						{
							Panel.SetZIndex(elm, currentIndex);
							break;
						}
					}
				}
			}
		}

		private void SendToBack_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List<UIElement> selectionSorted = (
				from item in this.SelectionService.CurrentSelection
				orderby Panel.GetZIndex(item as UIElement)
				select item as UIElement).ToList<UIElement>();
			List<UIElement> childrenSorted = (
				from UIElement item in base.Children
				orderby Panel.GetZIndex(item)
				select item).ToList<UIElement>();
			int i = 0;
			int j = 0;
			foreach (UIElement uIElement in childrenSorted)
			{
				if (!selectionSorted.Contains(uIElement))
				{
					int num = i;
					i = num + 1;
					Panel.SetZIndex(uIElement, selectionSorted.Count + num);
				}
				else
				{
					Panel.GetZIndex(uIElement);
					int num1 = j;
					j = num1 + 1;
					Panel.SetZIndex(uIElement, num1);
				}
			}
		}

		private XElement SerializeDesignerItems(IEnumerable<DesignerItem> designerItems)
		{
			double value;
			LegendLocation legendLocation;
			bool dataLabels;
			XElement serializedItems2 = new XElement("DesignerItems");
			foreach (DesignerItem item in designerItems)
			{
				string sCont = item.Content.ToString();
				string contentXaml2 = "";
				if (sCont != null)
				{
					if (sCont == "LiveCharts.Wpf.AngularGauge")
					{
						AngularGauge gauge = item.Content as AngularGauge;
						string[] str = new string[] { "<lvc:AngularGauge Tag=\"Angular Gauge\" IsHitTestVisible=\"False\" Value=\"", null, null, null, null, null, null, null, null };
						value = gauge.Value;
						str[1] = value.ToString();
						str[2] = "\" FromValue=\"";
						value = gauge.FromValue;
						str[3] = value.ToString();
						str[4] = "\" ToValue=\"";
						value = gauge.ToValue;
						str[5] = value.ToString();
						str[6] = "\" SectionsInnerRadius=\"";
						value = gauge.SectionsInnerRadius;
						str[7] = value.ToString();
						str[8] = "\"  ";
						contentXaml2 = string.Concat(str);
						contentXaml2 = string.Concat(new string[] { contentXaml2, "Foreground=\"", gauge.Foreground.ToString(), "\" LabelsEffect=\"{x:Null}\" NeedleFill=\"", gauge.NeedleFill.ToString(), "\" TicksForeground=\"", gauge.TicksForeground.ToString(), "\" xmlns:lvc=\"clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" });
						contentXaml2 = string.Concat(contentXaml2, "<lvc:AngularGauge.Sections>");
						foreach (AngularSection section in gauge.Sections)
						{
							string[] strArrays = new string[] { contentXaml2, "<lvc:AngularSection FromValue=\"", null, null, null, null, null, null };
							value = section.FromValue;
							strArrays[2] = value.ToString();
							strArrays[3] = "\" ToValue=\"";
							value = section.ToValue;
							strArrays[4] = value.ToString();
							strArrays[5] = "\" Fill=\"";
							strArrays[6] = section.Fill.ToString();
							strArrays[7] = "\"/>";
							contentXaml2 = string.Concat(strArrays);
						}
						contentXaml2 = string.Concat(contentXaml2, "</lvc:AngularGauge.Sections></lvc:AngularGauge>");
					}
					else if (sCont == "LiveCharts.Wpf.PieChart")
					{
						PieChart pie = item.Content as PieChart;
						string[] str1 = new string[] { "<lvc:PieChart Tag=\"Pie Chart\" IsHitTestVisible=\"False\" xmlns:lvc=\"clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf\" LegendLocation=\"", null, null, null, null };
						legendLocation = pie.LegendLocation;
						str1[1] = legendLocation.ToString();
						str1[2] = "\" Foreground=\"";
						str1[3] = pie.Foreground.ToString();
						str1[4] = "\" ><lvc:PieChart.Series>";
						contentXaml2 = string.Concat(str1);
						foreach (PieSeries serie in pie.Series)
						{
							contentXaml2 = string.Concat(new string[] { contentXaml2, "<lvc:PieSeries Title=\"", serie.Title, "\" Values=\"", serie.Values[0].ToString(), "\" Fill=\"", serie.Fill.ToString(), "\"/>" });
						}
						contentXaml2 = string.Concat(contentXaml2, "</lvc:PieChart.Series></lvc:PieChart> ");
					}
					else if (sCont != "LiveCharts.Wpf.CartesianChart")
					{
						contentXaml2 = XamlWriter.Save(item.Content);
					}
					else
					{
						CartesianChart chart = item.Content as CartesianChart;
						string[] strArrays1 = new string[] { "<lvc:CartesianChart Tag=\"Cartesian Chart\" IsHitTestVisible=\"False\" DisableAnimations=\"True\" xmlns:lvc=\"clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" LegendLocation=\"", null, null, null, null };
						legendLocation = chart.LegendLocation;
						strArrays1[1] = legendLocation.ToString();
						strArrays1[2] = "\" Foreground=\"";
						strArrays1[3] = chart.Foreground.ToString();
						strArrays1[4] = "\" >";
						contentXaml2 = string.Concat(strArrays1);
						contentXaml2 = string.Concat(contentXaml2, "<lvc:CartesianChart.Series >");
						foreach (Series serie in chart.Series)
						{
							string stype = serie.ToString();
							string svals = "";
							for (int i = 0; i < serie.Values.Count; i++)
							{
								svals = string.Concat(svals, serie.Values[i].ToString());
								if (i < serie.Values.Count - 1)
								{
									svals = string.Concat(svals, ",");
								}
							}
							if (stype != "LiveCharts.Wpf.LineSeries")
							{
								string[] title = new string[] { contentXaml2, "<lvc:ColumnSeries Title=\"", serie.Title, "\" Values=\"", svals, "\" Fill=\"", serie.Fill.ToString(), "\" Stroke=\"Transparent\" DataLabels=\"", null, null };
								dataLabels = serie.DataLabels;
								title[8] = dataLabels.ToString();
								title[9] = "\"/>";
								contentXaml2 = string.Concat(title);
							}
							else
							{
								string[] title1 = new string[] { contentXaml2, "<lvc:LineSeries Title=\"", serie.Title, "\" Values=\"", svals, "\" Stroke=\"", serie.Stroke.ToString(), "\" Fill=\"Transparent\" DataLabels=\"", null, null };
								dataLabels = serie.DataLabels;
								title1[8] = dataLabels.ToString();
								title1[9] = "\" PointGeometry=\"{x:Null}\" LineSmoothness=\"0\" />";
								contentXaml2 = string.Concat(title1);
							}
						}
						contentXaml2 = string.Concat(contentXaml2, "</lvc:CartesianChart.Series>");
						contentXaml2 = string.Concat(contentXaml2, "<lvc:CartesianChart.AxisY><lvc:Axis");
						if (chart.AxisY[0].MaxValue.ToString() != "NaN")
						{
							value = chart.AxisY[0].MaxValue;
							contentXaml2 = string.Concat(contentXaml2, " MaxValue=\"", value.ToString(), "\"");
						}
						if (chart.AxisY[0].MinValue.ToString() != "NaN")
						{
							value = chart.AxisY[0].MinValue;
							contentXaml2 = string.Concat(contentXaml2, " MinValue=\"", value.ToString(), "\"");
						}
						dataLabels = chart.AxisY[0].ShowLabels;
						contentXaml2 = string.Concat(contentXaml2, " ShowLabels=\"", dataLabels.ToString(), "\" /></lvc:CartesianChart.AxisY>");
						contentXaml2 = string.Concat(contentXaml2, "<lvc:CartesianChart.AxisX ><lvc:Axis ");
						if (chart.AxisX[0].MaxValue.ToString() != "NaN")
						{
							value = chart.AxisX[0].MaxValue;
							contentXaml2 = string.Concat(contentXaml2, " MaxValue=\"", value.ToString(), "\"");
						}
						dataLabels = chart.AxisX[0].ShowLabels;
						contentXaml2 = string.Concat(contentXaml2, " ShowLabels=\"", dataLabels.ToString(), "\" /></lvc:CartesianChart.AxisX>");
						contentXaml2 = string.Concat(contentXaml2, "</lvc:CartesianChart >");
					}
				}
				serializedItems2.Add(new XElement("DesignerItem", new object[] { new XElement("Left", (object)Canvas.GetLeft(item)), new XElement("Top", (object)Canvas.GetTop(item)), new XElement("Width", (object)item.Width), new XElement("Height", (object)item.Height), new XElement("ID", (object)item.ID), new XElement("zIndex", (object)Panel.GetZIndex(item)), new XElement("IsGroup", (object)item.IsGroup), new XElement("ParentID", (object)item.ParentID), new XElement("Content", contentXaml2) }));
			}
			return serializedItems2;
		}

		private void Ungroup_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			IEnumerable<DesignerItem> groupedItem =
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where item.ParentID != Guid.Empty
				select item;
			e.CanExecute = groupedItem.Count<DesignerItem>() > 0;
		}

		private void Ungroup_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DesignerItem[] array = (
				from item in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
				where (!item.IsGroup ? false : item.ParentID == Guid.Empty)
				select item).ToArray<DesignerItem>();
			for (int i = 0; i < (int)array.Length; i++)
			{
				DesignerItem designerItem = array[i];
				IEnumerable<DesignerItem> children =
					from child in this.SelectionService.CurrentSelection.OfType<DesignerItem>()
					where child.ParentID == designerItem.ID
					select child;
				foreach (DesignerItem empty in children)
				{
					empty.ParentID = Guid.Empty;
				}
				this.SelectionService.RemoveFromSelection(designerItem);
				base.Children.Remove(designerItem);
				this.UpdateZIndex();
			}
		}

		private void UpdateZIndex()
		{
			List<UIElement> ordered = (
				from UIElement item in base.Children
				orderby Panel.GetZIndex(item)
				select item).ToList<UIElement>();
			for (int i = 0; i < ordered.Count; i++)
			{
				Panel.SetZIndex(ordered[i], i);
			}
		}

		private void Upload_Enabled(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Upload_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow.AppWindow.UploadFile();
		}
	}
}