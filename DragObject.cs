using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DashboardDesigner
{
	public class DragObject
	{
		public Size? DesiredSize
		{
			get;
			set;
		}

		public string Xaml
		{
			get;
			set;
		}

		public DragObject()
		{
		}
	}
}