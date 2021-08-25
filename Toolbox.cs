using System;
using System.Windows;
using System.Windows.Controls;

namespace DashboardDesigner
{
	public class Toolbox : ItemsControl
	{
		private Size itemSize = new Size(50, 50);

		public Size ItemSize
		{
			get
			{
				return this.itemSize;
			}
			set
			{
				this.itemSize = value;
			}
		}

		public Toolbox()
		{
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ToolboxItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is ToolboxItem;
		}
	}
}