using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualBasic.CompilerServices;

namespace GumpStudio
{
	public class LargeTextPropEditor : UITypeEditor
	{
		protected IWindowsFormsEditorService edSvc;
		protected int ReturnValue;

		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (edSvc == null)
			{
				return value;
			}

			var largeTextEditor = new LargeTextEditor();
			largeTextEditor.txtText.Text = Conversions.ToString(value);
			if (edSvc.ShowDialog(largeTextEditor) == DialogResult.OK)
			{
				return largeTextEditor.txtText.Text;
			}

			return value;
		}

		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
	}
}
