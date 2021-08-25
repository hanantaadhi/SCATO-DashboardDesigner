namespace DashboardDesigner.Controls
{
    using DashboardDesigner;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(this.DragThumb_DragDelta);
        }

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItem dataContext = base.DataContext as DesignerItem;
            DesignerCanvas parent = VisualTreeHelper.GetParent(dataContext) as DesignerCanvas;
            if (((dataContext != null) && (parent != null)) && dataContext.IsSelected)
            {
                double maxValue = double.MaxValue;
                double num2 = double.MaxValue;
                IEnumerable<DesignerItem> enumerable = parent.SelectionService.CurrentSelection.OfType<DesignerItem>();
                foreach (DesignerItem item2 in enumerable)
                {
                    double left = Canvas.GetLeft(item2);
                    double top = Canvas.GetTop(item2);
                    maxValue = double.IsNaN(left) ? 0.0 : Math.Min(left, maxValue);
                    num2 = double.IsNaN(top) ? 0.0 : Math.Min(top, num2);
                }
                double num3 = Math.Max(-maxValue, e.HorizontalChange);
                double num4 = Math.Max(-num2, e.VerticalChange);
                foreach (DesignerItem item3 in enumerable)
                {
                    double left = Canvas.GetLeft(item3);
                    double top = Canvas.GetTop(item3);
                    if (double.IsNaN(left))
                    {
                        left = 0.0;
                    }
                    if (double.IsNaN(top))
                    {
                        top = 0.0;
                    }
                    Canvas.SetLeft(item3, left + num3);
                    Canvas.SetTop(item3, top + num4);
                    string sid = item3.ID.ToString();
                    if (MainWindow.AppWindow.MyItems.FindIndex(a => a.ID == sid) >= 0)
                    {
                        MainWindow.AppWindow.txtLeft.Text = Canvas.GetLeft(item3).ToString();
                        MainWindow.AppWindow.txtTop.Text = Canvas.GetTop(item3).ToString();
                    }
                }
                parent.InvalidateMeasure();
                e.Handled = true;
            }
        }
    }
}

