using System.Collections;

/*namespace GumpStudio
{
	public class TreeFolder : TreeItem
	{
		protected ArrayList Children = new ArrayList();

		public TreeFolder(string Text)
		{
			this.Text = Text;
		}

		public void AddItem(TreeItem Item)
		{
			Children.Add(Item);
			Item.Parent = this;
		}

		public ArrayList GetChildren()
		{
			return Children;
		}

		public void RemoveItem(TreeItem Item)
		{
			Children.Remove(Item);
			Item.Parent = null;
		}
	}
}*/

using System.Collections.Generic;

namespace GumpStudio
{
	public class TreeFolder : TreeItem
	{
		private List<TreeItem> _children = new List<TreeItem>();

		public TreeFolder(string text) : base(text)
		{
		}

		public IReadOnlyList<TreeItem> Children => _children.AsReadOnly();

		public void AddItem(TreeItem item)
		{
			_children.Add(item);
			item.Parent = this;
		}

		public void RemoveItem(TreeItem item)
		{
			_children.Remove(item);
			item.Parent = null;
		}
	}
}

