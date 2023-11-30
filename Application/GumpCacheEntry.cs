using System;
using System.Drawing;

namespace GumpStudio
{
	[Serializable]
	public class GumpCacheEntry
	{
		public int ID;
		[NonSerialized]
		public Image ImageCache;
		public string Name;
		public Size Size;

		public override string ToString()
		{
			return ID.ToString();
		}
	}
}
