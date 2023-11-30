using GumpStudio.Elements;
/*
namespace GumpStudio
{
	public class TreeGumpling : TreeItem
	{
		public GroupElement Gumpling;

		public TreeGumpling(string Text, GroupElement Gumpling)
		{
			this.Text = Text;
			this.Gumpling = Gumpling;
		}
	}
}*/

namespace GumpStudio
{
	public class TreeGumpling : TreeItem
	{
		public GroupElement Gumpling;

		public TreeGumpling(string text, GroupElement gumpling) : base(text)
		{
			Gumpling = gumpling;
		}
	}
}
