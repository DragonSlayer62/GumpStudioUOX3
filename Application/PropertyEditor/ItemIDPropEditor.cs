using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Ultima;

namespace GumpStudio
{
	public class ItemIDPropEditor : UITypeEditor
	{
		protected IWindowsFormsEditorService edSvc;
		protected int ReturnValue;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

			if (edSvc == null)
			{
				return value;
			}

			var staticArtBrowser = new StaticArtBrowser { ItemID = Convert.ToInt32(value) };

			if (edSvc.ShowDialog(staticArtBrowser) != DialogResult.OK)
			{
				return value;
			}

			if (Art.GetStatic(staticArtBrowser.ItemID) != null)
			{
				ReturnValue = staticArtBrowser.ItemID;
				staticArtBrowser.Dispose();
				return ReturnValue;
			}

			MessageBox.Show(@"Invalid ItemID");

			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
	}
}
