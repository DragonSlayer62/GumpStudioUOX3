using System;
using System.Windows.Forms;

using GumpStudio.Forms;

namespace GumpStudio
{
	internal sealed class GumpDesignerMain
	{
		[STAThread]
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.Run(new DesignerForm());
		}
	}
}
