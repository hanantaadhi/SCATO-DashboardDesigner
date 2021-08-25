using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DashboardDesigner
{
	public class RubberbandAdorner : Adorner
	{
		private Point? startPoint;

		private Point? endPoint;

		private Pen rubberbandPen;

		private DesignerCanvas designerCanvas;

		public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint) : base(designerCanvas)
		{
			this.designerCanvas = designerCanvas;
			this.startPoint = dragStartPoint;
			this.rubberbandPen = new Pen(Brushes.LightSlateGray, 1)
			{
				DashStyle = new DashStyle(new double[] { 2 }, 1)
			};
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (!base.IsMouseCaptured)
				{
					base.CaptureMouse();
				}
				this.endPoint = new Point?(e.GetPosition(this));
				this.UpdateSelection();
				base.InvalidateVisual();
			}
			else if (base.IsMouseCaptured)
			{
				base.ReleaseMouseCapture();
			}
			e.Handled = true;
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (base.IsMouseCaptured)
			{
				base.ReleaseMouseCapture();
			}
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
			if (adornerLayer != null)
			{
				adornerLayer.Remove(this);
			}
			e.Handled = true;
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			dc.DrawRectangle(Brushes.Transparent, null, new Rect(base.RenderSize));
			if ((!this.startPoint.HasValue ? false : this.endPoint.HasValue))
			{
				dc.DrawRectangle(Brushes.Transparent, this.rubberbandPen, new Rect(this.startPoint.Value, this.endPoint.Value));
			}
		}

		private void UpdateSelection()
		{
			this.designerCanvas.SelectionService.ClearSelection();
			Rect rubberBand = new Rect(this.startPoint.Value, this.endPoint.Value);
			foreach (Control item in this.designerCanvas.Children)
			{
				Rect itemRect = VisualTreeHelper.GetDescendantBounds(item);
				if (rubberBand.Contains(item.TransformToAncestor(this.designerCanvas).TransformBounds(itemRect)))
				{
					DesignerItem di = item as DesignerItem;
					if (di.ParentID == Guid.Empty)
					{
						this.designerCanvas.SelectionService.AddToSelection(di);
					}
				}
			}
		}
	}
}