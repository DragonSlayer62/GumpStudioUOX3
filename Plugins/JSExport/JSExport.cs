using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GumpStudio.Elements;

namespace GumpStudio.Plugins
{
	public class JSExport : BasePlugin
	{
		private static readonly string Template = @"

function ~gump_type~(pUser)
{
	var socket = pUser.socket;
	var uox3gump = new Gump;
	uox3gump.AddPage(0);
	
	~gump_layout~
	
	uox3gump.Send(pUser);
	uox3gump.Free();
}

function onGumpPress(pSock, pButton, gumpData)
{
	var pUser = pSock.currentChar;
	switch (pButton)
	{
		case 0:
			break;
	}
}";

		private readonly Settings _Config = new Settings();

		public override BaseConfig Config => _Config;

		public override PluginInfo Info { get; } = new PluginInfo("JS Exporter", "1.0", "Dragon Slayer", "Arthenson67@gmail.com", "Exports a JS file compatible with UOX3 targeting JavaScript");

		private MenuItem _MenuFileExport;

		protected override void OnLoaded()
		{
			base.OnLoaded();

			Designer.MenuFileExport.Enabled = true;

			if (_MenuFileExport == null)
			{
				_MenuFileExport = new MenuItem("JS", new[]
				{
					new MenuItem("All Elements", ExportFileClick),
					new MenuItem("Selected Elements", ExportSelectionClick)
				});
			}

			Designer.MenuFileExport.MenuItems.Add(_MenuFileExport);
		}

		protected override void OnUnloaded()
		{
			base.OnUnloaded();
			
			Designer.MenuFileExport.MenuItems.Remove(_MenuFileExport);

			if (Designer.MenuFileExport.MenuItems.Count == 0)
			{
				Designer.MenuFileExport.Enabled = false;
			}
		}

		private void ExportFileClick(object sender, EventArgs e)
		{
			ExportFile(false);
		}

		private void ExportSelectionClick(object sender, EventArgs e)
		{
			ExportFile(true);
		}

		private void ExportFile(bool selected)
		{
			var fullPath = $"{Path.GetTempFileName()}.txt";

			var indent = new StringBuilder();

			var layoutBegin = Template.IndexOf("~gump_layout~");

			while (--layoutBegin >= 0)
			{
				if (Template[layoutBegin] == '\r' || Template[layoutBegin] == '\n')
				{
					break;
				}

				if (!Char.IsWhiteSpace(Template, layoutBegin))
				{
					break;
				}

				indent.Insert(0, Template[layoutBegin]);
			}

			var tabs = indent.ToString();

			var template = new StringBuilder(Template);

			template = template.Replace("~gump_type~", "CustomGump");

			var stacks = new Dictionary<GroupElement, ICSharpExportable[]>();

			if (selected)
			{
				var elements = Designer.ElementStack.SelectedElements.OfType<ICSharpExportable>().ToArray();

				if (elements.Length > 0)
				{
					stacks[Designer.ElementStack] = elements;
				}
			}
			else
			{
				foreach (var stack in Designer.Stacks)
				{
					var elements = stack.AllElements.OfType<ICSharpExportable>().ToArray();

					if (elements.Length > 0)
					{
						stacks[stack] = elements;
					}
				}
			}

			var location = Point.Empty;

			if (_Config.RelativeOffsets)
			{
				location.X = Int32.MaxValue;
				location.Y = Int32.MaxValue;

				foreach (var element in stacks.Values.SelectMany(o => o.OfType<BaseElement>()))
				{
					location.X = Math.Min(location.X, element.X);
					location.Y = Math.Min(location.Y, element.Y);
				}

				if (location.X == Int32.MaxValue)
				{
					location.X = 0;
				}

				if (location.Y == Int32.MaxValue)
				{
					location.Y = 0;
				}
			}

			template = template.Replace("~gump_location~", $"{location.X}, {location.Y}");

			var layout = new StringBuilder();

			var page = -1;

			foreach (var entry in stacks)
			{
				if (++page >= 1)
				{
					layout.AppendLine($"{tabs}uox3gump.AddPage({page});");
				}

				foreach (var exportable in entry.Value)
				{
					if (exportable is BaseElement element)
					{
						if (_Config.RelativeOffsets)
						{
							element.X -= location.X;
							element.Y -= location.Y;
						}

						var csharp = exportable.ToCSharpString();

						if (_Config.NoComments)
						{
							var index = csharp.IndexOf("//");

							if (index >= 0)
							{
								csharp = csharp.Substring(0, index);
							}
						}

						layout.AppendLine($"{tabs}{csharp}");

						if (_Config.RelativeOffsets)
						{
							element.X += location.X;
							element.Y += location.Y;
						}
					}
				}
			}

			template = template.Replace("~gump_layout~", layout.ToString().Trim());

			layout.Clear();
			indent.Clear();

			layoutBegin = Template.IndexOf("~gump_controls~");

			while (--layoutBegin >= 0)
			{
				if (Template[layoutBegin] == '\r' || Template[layoutBegin] == '\n')
				{
					break;
				}

				if (!Char.IsWhiteSpace(Template, layoutBegin))
				{
					break;
				}

				indent.Insert(0, Template[layoutBegin]);
			}

			tabs = indent.ToString();

			if (layout.Length > 0)
			{
				template = template.Replace("~gump_controls~", $"{Environment.NewLine}{tabs}{layout.ToString().Trim()}{Environment.NewLine}");
			}
			else
			{
				template = template.Replace("~gump_controls~", String.Empty);
			}

			try
			{
				File.WriteAllText(fullPath, template.ToString().Trim());

				Process.Start(new ProcessStartInfo(fullPath)
				{
					UseShellExecute = true
				});
			}
			catch { }
		}

		[Serializable]
		public class Settings : BaseConfig
		{
			public override string Name => "JS Exporter";

			public bool RelativeOffsets { get; set; } = false;

			public bool NoComments { get; set; } = true;
		}
	}
}
