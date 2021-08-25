using DashboardDesigner.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DashboardDesigner
{
	public class DesignerItem : ContentControl, ISelectable, IGroupable
	{
		private Guid id;

		public readonly static DependencyProperty ParentIDProperty;

		public readonly static DependencyProperty IsGroupProperty;

		public readonly static DependencyProperty IsSelectedProperty;

		public readonly static DependencyProperty DragThumbTemplateProperty;

		public Guid ID
		{
			get
			{
				return id;
			}
		}

		public bool IsGroup
		{
			get
			{
				return (bool)base.GetValue(DesignerItem.IsGroupProperty);
			}
			set
			{
				base.SetValue(DesignerItem.IsGroupProperty, value);
			}
		}

		public bool IsSelected
		{
			get
			{
				return (bool)base.GetValue(DesignerItem.IsSelectedProperty);
			}
			set
			{
				base.SetValue(DesignerItem.IsSelectedProperty, value);
			}
		}

		public Guid ParentID
		{
			get
			{
				return (Guid)base.GetValue(DesignerItem.ParentIDProperty);
			}
			set
			{
				base.SetValue(DesignerItem.ParentIDProperty, value);
			}
		}

		static DesignerItem()
		{
			DesignerItem.ParentIDProperty = DependencyProperty.Register("ParentID", typeof(Guid), typeof(DesignerItem));
			DesignerItem.IsGroupProperty = DependencyProperty.Register("IsGroup", typeof(bool), typeof(DesignerItem));
			DesignerItem.IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));
			DesignerItem.DragThumbTemplateProperty = DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
		}

		public DesignerItem(Guid id)
		{
			this.id = id;
			base.Loaded += new RoutedEventHandler(this.DesignerItem_Loaded);
		}

		public DesignerItem() : this(Guid.NewGuid())
		{
		}

		private void DesignerItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (base.Template != null)
			{
				ContentPresenter contentPresenter = base.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
				if (contentPresenter != null)
				{
					UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
					if (contentVisual != null)
					{
						DragThumb thumb = base.Template.FindName("PART_DragThumb", this) as DragThumb;
						if (thumb != null)
						{
							ControlTemplate template = DesignerItem.GetDragThumbTemplate(contentVisual);
							if (template != null)
							{
								thumb.Template = template;
							}
						}
					}
				}
			}
		}

		public static ControlTemplate GetDragThumbTemplate(UIElement element)
		{
			return (ControlTemplate)element.GetValue(DesignerItem.DragThumbTemplateProperty);
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);
			DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;
			if (designer != null)
			{
				if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != ModifierKeys.None)
				{
					if (!this.IsSelected)
					{
						designer.SelectionService.AddToSelection(this);
					}
					else
					{
						designer.SelectionService.RemoveFromSelection(this);
					}
				}
				else if (!this.IsSelected)
				{
					designer.SelectionService.SelectItem(this);
					MainWindow.AppWindow.cboItems.SelectedItem = this.id.ToString();
				}
				base.Focus();
			}
			e.Handled = false;
		}

		public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
		{
			element.SetValue(DesignerItem.DragThumbTemplateProperty, value);
		}
	}
}