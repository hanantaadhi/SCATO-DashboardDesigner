using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DashboardDesigner
{
	internal class SelectionService
	{
		private DesignerCanvas designerCanvas;

		private List<ISelectable> currentSelection;

		internal List<ISelectable> CurrentSelection
		{
			get
			{
				if (this.currentSelection == null)
				{
					this.currentSelection = new List<ISelectable>();
				}
				return this.currentSelection;
			}
		}

		public SelectionService(DesignerCanvas canvas)
		{
			this.designerCanvas = canvas;
		}

		internal void AddToSelection(ISelectable item)
		{
			if (!(item is IGroupable))
			{
				item.IsSelected = true;
				this.CurrentSelection.Add(item);
			}
			else
			{
				foreach (ISelectable groupItem in this.GetGroupMembers(item as IGroupable))
				{
					groupItem.IsSelected = true;
					this.CurrentSelection.Add(groupItem);
				}
			}
		}

		internal void ClearSelection()
		{
			this.CurrentSelection.ForEach((ISelectable item) => item.IsSelected = false);
			this.CurrentSelection.Clear();
		}

		internal List<IGroupable> GetGroupMembers(IGroupable item)
		{
			IEnumerable<IGroupable> list = this.designerCanvas.Children.OfType<IGroupable>();
			IGroupable rootItem = this.GetRoot(list, item);
			return this.GetGroupMembers(list, rootItem);
		}

		private List<IGroupable> GetGroupMembers(IEnumerable<IGroupable> list, IGroupable parent)
		{
			List<IGroupable> groupMembers = new List<IGroupable>()
			{
				parent
			};
			IEnumerable<IGroupable> children =
				from node in list
				where node.ParentID == parent.ID
				select node;
			foreach (IGroupable child in children)
			{
				groupMembers.AddRange(this.GetGroupMembers(list, child));
			}
			return groupMembers;
		}

		internal IGroupable GetGroupRoot(IGroupable item)
		{
			IEnumerable<IGroupable> list = this.designerCanvas.Children.OfType<IGroupable>();
			return this.GetRoot(list, item);
		}

		private IGroupable GetRoot(IEnumerable<IGroupable> list, IGroupable node)
		{
			IGroupable root;
			if ((node == null ? false : node.ParentID != Guid.Empty))
			{
				foreach (IGroupable item in list)
				{
					if (item.ID == node.ParentID)
					{
						root = this.GetRoot(list, item);
						return root;
					}
				}
				root = null;
			}
			else
			{
				root = node;
			}
			return root;
		}

		internal void RemoveFromSelection(ISelectable item)
		{
			if (!(item is IGroupable))
			{
				item.IsSelected = false;
				this.CurrentSelection.Remove(item);
			}
			else
			{
				foreach (ISelectable groupItem in this.GetGroupMembers(item as IGroupable))
				{
					groupItem.IsSelected = false;
					this.CurrentSelection.Remove(groupItem);
				}
			}
		}

		internal void SelectAll()
		{
			this.ClearSelection();
			this.CurrentSelection.AddRange(this.designerCanvas.Children.OfType<ISelectable>());
			this.CurrentSelection.ForEach((ISelectable item) => item.IsSelected = true);
		}

		internal void SelectItem(ISelectable item)
		{
			this.ClearSelection();
			this.AddToSelection(item);
		}
	}
}