/*namespace GumpStudio
{
	public abstract class TreeItem
	{
		public TreeItem Parent;
		public string Text;

		public override string ToString()
		{
			return Text;
		}
	}
}*/

namespace GumpStudio
{
	public abstract class TreeItem
	{
		public TreeItem Parent { get; set; }
		public string Text { get; set; }

		public TreeItem(string text)
		{
			Text = text;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
