using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using GumpStudio.Forms;

namespace GumpStudio
{
	public class ClilocPropEditor : UITypeEditor
	{
		protected IWindowsFormsEditorService edSvc;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			}

			if (edSvc == null)
			{
				return value;
			}

			var clilocBrowser = new ClilocBrowser();

			return edSvc.ShowDialog(clilocBrowser) == DialogResult.OK ? clilocBrowser.ClilocID : value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
	}
}