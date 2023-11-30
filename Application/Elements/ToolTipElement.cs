using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.Serialization;
using Ultima;

using UnicodeFonts = UOFont.UnicodeFonts;

namespace GumpStudio.Elements
{
	[Serializable]
	public class ToolTipElement : ResizeableElement, ICSharpExportable
	{
		protected Bitmap mCache;
		protected int mID;
		protected string mInitialText;
		protected int mMaxLength;

		[Description("The ID of this text entry element returned to script.")]
		[MergableProperty(false)]
		public int ID
		{
			get => mID;
			set => mID = value;
		}

		[Description("The text in the text entry area when the gump is initially opened.")]
		public string InitialText
		{
			get => mInitialText;
			set
			{
				mInitialText = value;
				RefreshCache();
			}
		}

		[Description("MaxLength sets the maximum number of characters allowed in this TextEntry element. Set to 0 for no limit.")]
		[MergableProperty(true)]
		public int MaxLength
		{
			get => mMaxLength;
			set => mMaxLength = value;
		}

		public override string Type => "AddToolTip";

		public ToolTipElement()
		{
			_Size = new Size(200, 20);
			mInitialText = String.Empty;
		}

		public ToolTipElement(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			var int32 = info.GetInt32("ToolTipElementVersion");
			mInitialText = info.GetString("Text");

			if (int32 >= 2)
			{
				mID = info.GetInt32(nameof(ID));
				mMaxLength = info.GetInt32(nameof(MaxLength));
			}

			RefreshCache();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("ToolTipElementVersion", 2);
			info.AddValue("Text", mInitialText);
			info.AddValue("ID", mID);
			info.AddValue("MaxLength", mMaxLength);
		}

		public override void RefreshCache()
		{
			if (mCache != null)
			{
				mCache.Dispose();
			}

			mCache = UnicodeFonts.GetStringImage(2, mInitialText + " ");
		}

		public override void Render(Graphics Target)
		{
			if (mCache == null)
			{
				RefreshCache();
			}

			var clip = Target.Clip;
			var region = new Region(Bounds);
			Target.Clip = region;
			var solidBrush = new SolidBrush(Color.FromArgb(50, Color.Yellow));
			Target.FillRectangle(solidBrush, Bounds);
			Target.DrawImage(mCache, Location);
			solidBrush.Dispose();
			Target.Clip = clip;
			region.Dispose();
		}

		public string ToCSharpString()
		{
			if (MaxLength > 0)
				return $"uox3gump.AddToolTip(1042971, socket, \"{InitialText?.Replace("\"", "\\\"")}\", {MaxLength});";
				
			return $"uox3gump.AddToolTip(1042971, socket, \"{InitialText?.Replace("\"", "\\\"")}\");";
		}
	}
}
