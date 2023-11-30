using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace GumpStudio.Elements
{
	[Serializable]
	public class AlphaElement : ResizeableElement, ICSharpExportable
	{
		public override string Type => "AddCheckerTrans";

		public AlphaElement()
		{
			_Size = new Size(100, 50);
		}

		protected AlphaElement(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{
			info.GetInt32("AlphaElementVersion");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("AlphaElementVersion", 1);
		}

		public override void RefreshCache()
		{
		}

		public override void Render(Graphics Target)
		{
			var solidBrush = new SolidBrush(Color.FromArgb(50, Color.Red));
			Target.FillRectangle(solidBrush, Bounds);
			Target.DrawRectangle(Pens.Red, Bounds);
			solidBrush.Dispose();
		}

		public string ToCSharpString()
		{
			return $"uox3gump.AddCheckerTrans({X}, {Y}, {Width}, {Height});";
		}
	}
}
