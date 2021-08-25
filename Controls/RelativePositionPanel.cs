namespace DashboardDesigner.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class RelativePositionPanel : Panel
    {
        public static readonly DependencyProperty RelativePositionProperty = DependencyProperty.RegisterAttached("RelativePosition", typeof(Point), typeof(RelativePositionPanel), new FrameworkPropertyMetadata(new Point(0.0, 0.0), new PropertyChangedCallback(RelativePositionPanel.OnRelativePositionChanged)));

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement element in base.InternalChildren)
            {
                if (element != null)
                {
                    Point relativePosition = GetRelativePosition(element);
                    double d = (arrangeSize.Width - element.DesiredSize.Width) * relativePosition.X;
                    double num2 = (arrangeSize.Height - element.DesiredSize.Height) * relativePosition.Y;
                    if (double.IsNaN(d))
                    {
                        d = 0.0;
                    }
                    if (double.IsNaN(num2))
                    {
                        num2 = 0.0;
                    }
                    element.Arrange(new Rect(new Point(d, num2), element.DesiredSize));
                }
            }
            return arrangeSize;
        }

        public static Point GetRelativePosition(UIElement element)
        {
            if (ReferenceEquals(element, null))
            {
                throw new ArgumentNullException("element");
            }
            return (Point) element.GetValue(RelativePositionProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement element in base.InternalChildren)
            {
                if (element != null)
                {
                    element.Measure(size);
                }
            }
            return base.MeasureOverride(availableSize);
        }

        private static void OnRelativePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement reference = d as UIElement;
            if (reference != null)
            {
                RelativePositionPanel parent = VisualTreeHelper.GetParent(reference) as RelativePositionPanel;
                if (parent != null)
                {
                    parent.InvalidateArrange();
                }
            }
        }

        public static void SetRelativePosition(UIElement element, Point value)
        {
            if (ReferenceEquals(element, null))
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RelativePositionProperty, value);
        }
    }
}

