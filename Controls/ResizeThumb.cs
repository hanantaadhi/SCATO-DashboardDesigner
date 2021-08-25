namespace DashboardDesigner.Controls
{
    using DashboardDesigner;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void CalculateDragLimits(IEnumerable<DesignerItem> selectedItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;
            foreach (DesignerItem item in selectedItems)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);
                minLeft = double.IsNaN(left) ? 0.0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0.0 : Math.Min(top, minTop);
                minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
            }
        }

        private void DragBottom(double scale, DesignerItem item, SelectionService selectionService)
        {
            double top = Canvas.GetTop(item);
            foreach (DesignerItem item2 in selectionService.GetGroupMembers(item).Cast<DesignerItem>())
            {
                double num2 = Canvas.GetTop(item2);
                double num3 = (num2 - top) * (scale - 1.0);
                Canvas.SetTop(item2, num2 + num3);
                item2.Height = item2.ActualHeight * scale;
                string sid = item2.ID.ToString();
                int num4 = MainWindow.AppWindow.MyItems.FindIndex(a => a.ID == sid);
                if (num4 >= 0)
                {
                    MainWindow.AppWindow.txtTop.Text = Canvas.GetTop(item2).ToString();
                    MainWindow.AppWindow.txtHeight.Text = item2.Height.ToString();
                }
            }
        }

        private void DragLeft(double scale, DesignerItem item, SelectionService selectionService)
        {
            double num = Canvas.GetLeft(item) + item.Width;
            foreach (DesignerItem item2 in selectionService.GetGroupMembers(item).Cast<DesignerItem>())
            {
                double left = Canvas.GetLeft(item2);
                double num3 = (num - left) * (scale - 1.0);
                Canvas.SetLeft(item2, left - num3);
                item2.Width = item2.ActualWidth * scale;
                string sid = item2.ID.ToString();
                int num4 = MainWindow.AppWindow.MyItems.FindIndex(a => a.ID == sid);
                if (num4 >= 0)
                {
                    MainWindow.AppWindow.txtLeft.Text = Canvas.GetLeft(item2).ToString();
                    MainWindow.AppWindow.txtWidth.Text = item2.Width.ToString();
                }
            }
        }

        private void DragRight(double scale, DesignerItem item, SelectionService selectionService)
        {
            double left = Canvas.GetLeft(item);
            foreach (DesignerItem item2 in selectionService.GetGroupMembers(item).Cast<DesignerItem>())
            {
                double num2 = Canvas.GetLeft(item2);
                double num3 = (num2 - left) * (scale - 1.0);
                Canvas.SetLeft(item2, num2 + num3);
                item2.Width = item2.ActualWidth * scale;
                string sid = item2.ID.ToString();
                int num4 = MainWindow.AppWindow.MyItems.FindIndex(a => a.ID == sid);
                if (num4 >= 0)
                {
                    MainWindow.AppWindow.txtLeft.Text = Canvas.GetLeft(item2).ToString();
                    MainWindow.AppWindow.txtWidth.Text = item2.Width.ToString();
                }
            }
        }

        private void DragTop(double scale, DesignerItem item, SelectionService selectionService)
        {
            double num = Canvas.GetTop(item) + item.Height;
            foreach (DesignerItem item2 in selectionService.GetGroupMembers(item).Cast<DesignerItem>())
            {
                double top = Canvas.GetTop(item2);
                double num3 = (num - top) * (scale - 1.0);
                Canvas.SetTop(item2, top - num3);
                item2.Height = item2.ActualHeight * scale;
                string sid = item2.ID.ToString();
                int num4 = MainWindow.AppWindow.MyItems.FindIndex(a => a.ID == sid);
                if (num4 >= 0)
                {
                    MainWindow.AppWindow.txtTop.Text = Canvas.GetTop(item2).ToString();
                    MainWindow.AppWindow.txtHeight.Text = item2.Height.ToString();
                }
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItem dataContext = base.DataContext as DesignerItem;
            DesignerCanvas parent = VisualTreeHelper.GetParent(dataContext) as DesignerCanvas;
            if (((dataContext != null) && (parent != null)) && dataContext.IsSelected)
            {
                double num;
                double num2;
                double num3;
                double num4;
                IEnumerable<DesignerItem> selectedItems = parent.SelectionService.CurrentSelection.OfType<DesignerItem>();
                this.CalculateDragLimits(selectedItems, out num, out num2, out num3, out num4);
                foreach (DesignerItem item2 in selectedItems)
                {
                    if ((item2 != null) && (item2.ParentID == Guid.Empty))
                    {
                        double num5;
                        double num6;
                        double num7;
                        VerticalAlignment verticalAlignment = base.VerticalAlignment;
                        if (verticalAlignment == VerticalAlignment.Top)
                        {
                            double top = Canvas.GetTop(item2);
                            num5 = Math.Min(Math.Max(-num2, e.VerticalChange), num4);
                            num7 = (item2.ActualHeight - num5) / item2.ActualHeight;
                            this.DragTop(num7, item2, parent.SelectionService);
                        }
                        else if (verticalAlignment == VerticalAlignment.Bottom)
                        {
                            num5 = Math.Min(-e.VerticalChange, num4);
                            num7 = (item2.ActualHeight - num5) / item2.ActualHeight;
                            this.DragBottom(num7, item2, parent.SelectionService);
                        }
                        HorizontalAlignment horizontalAlignment = base.HorizontalAlignment;
                        if (horizontalAlignment == HorizontalAlignment.Left)
                        {
                            double left = Canvas.GetLeft(item2);
                            num6 = Math.Min(Math.Max(-num, e.HorizontalChange), num3);
                            num7 = (item2.ActualWidth - num6) / item2.ActualWidth;
                            this.DragLeft(num7, item2, parent.SelectionService);
                        }
                        else if (horizontalAlignment == HorizontalAlignment.Right)
                        {
                            num6 = Math.Min(-e.HorizontalChange, num3);
                            num7 = (item2.ActualWidth - num6) / item2.ActualWidth;
                            this.DragRight(num7, item2, parent.SelectionService);
                        }
                    }
                }
                e.Handled = true;
            }
        }
    }
}

