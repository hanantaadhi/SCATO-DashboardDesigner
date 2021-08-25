using System;

namespace DashboardDesigner
{
	public interface IGroupable
	{
		Guid ID
		{
			get;
		}

		bool IsGroup
		{
			get;
			set;
		}

		Guid ParentID
		{
			get;
			set;
		}
	}
}