using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Ultima
{
	public sealed class Multis
	{
		private static MultiComponentList[] m_Components = new MultiComponentList[0x2000];
		private static FileIndex m_FileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x2000, 14);

		public enum ImportType
		{
			TXT,
			UOA,
			UOAB,
			WSC,
			MULTICACHE,
			UOADESIGN
		}

		public static bool PostHSFormat { get; set; }

		/// <summary>
		/// ReReads multi.mul
		/// </summary>
		public static void Reload()
		{
			m_FileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x2000, 14);
			m_Components = new MultiComponentList[0x2000];
		}

		/// <summary>
		/// Gets <see cref="MultiComponentList"/> of multi
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static MultiComponentList GetComponents(int index)
		{
			MultiComponentList mcl;

			index &= 0x1FFF;

			if (index >= 0 && index < m_Components.Length)
			{
				mcl = m_Components[index];

				if (mcl == null)
				{
					m_Components[index] = mcl = Load(index);
				}
			}
			else
			{
				mcl = MultiComponentList.Empty;
			}

			return mcl;
		}

		public static MultiComponentList Load(int index)
		{
			try
			{
				var stream = m_FileIndex.Seek(index, out var length, out var extra, out var patched);

				if (stream == null)
				{
					return MultiComponentList.Empty;
				}

				if (PostHSFormat || Art.IsUOAHS())
				{
					return new MultiComponentList(new BinaryReader(stream), length / 16);
				}
				else
				{
					return new MultiComponentList(new BinaryReader(stream), length / 12);
				}
			}
			catch
			{
				return MultiComponentList.Empty;
			}
		}

		public static void Remove(int index)
		{
			m_Components[index] = MultiComponentList.Empty;
		}

		public static void Add(int index, MultiComponentList comp)
		{
			m_Components[index] = comp;
		}

		public static MultiComponentList ImportFromFile(int index, string FileName, Multis.ImportType type)
		{
			try
			{
				return m_Components[index] = new MultiComponentList(FileName, type);
			}
			catch
			{
				return m_Components[index] = MultiComponentList.Empty;
			}
		}

		public static MultiComponentList LoadFromFile(string FileName, Multis.ImportType type)
		{
			try
			{
				return new MultiComponentList(FileName, type);
			}
			catch
			{
				return MultiComponentList.Empty;
			}
		}

		public static List<MultiComponentList> LoadFromCache(string FileName)
		{
			var multilist = new List<MultiComponentList>();
			using (var ip = new StreamReader(FileName))
			{
				string line;
				while ((line = ip.ReadLine()) != null)
				{
					var split = Regex.Split(line, @"\s+");
					if (split.Length == 7)
					{
						var count = Convert.ToInt32(split[2]);
						multilist.Add(new MultiComponentList(ip, count));
					}
				}
			}
			return multilist;
		}

		public static string ReadUOAString(BinaryReader bin)
		{
			var flag = bin.ReadByte();

			if (flag == 0)
			{
				return null;
			}
			else
			{
				return bin.ReadString();
			}
		}
		public static List<object[]> LoadFromDesigner(string FileName)
		{
			var multilist = new List<object[]>();
			var root = Path.GetFileNameWithoutExtension(FileName);
			var idx = String.Format("{0}.idx", root);
			var bin = String.Format("{0}.bin", root);
			if ((!File.Exists(idx)) || (!File.Exists(bin)))
			{
				return multilist;
			}

			using (FileStream idxfs = new FileStream(idx, FileMode.Open, FileAccess.Read, FileShare.Read),
							  binfs = new FileStream(bin, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (BinaryReader idxbin = new BinaryReader(idxfs),
									binbin = new BinaryReader(binfs))
				{
					var count = idxbin.ReadInt32();
					var version = idxbin.ReadInt32();

					for (var i = 0; i < count; ++i)
					{
						var data = new object[2];
						switch (version)
						{
							case 0:
								data[0] = ReadUOAString(idxbin);
								var arr = new List<MultiComponentList.MultiTileEntry>();
								data[0] += "-" + ReadUOAString(idxbin);
								data[0] += "-" + ReadUOAString(idxbin);
								var width = idxbin.ReadInt32();
								var height = idxbin.ReadInt32();
								var uwidth = idxbin.ReadInt32();
								var uheight = idxbin.ReadInt32();
								var filepos = idxbin.ReadInt64();
								var reccount = idxbin.ReadInt32();

								binbin.BaseStream.Seek(filepos, SeekOrigin.Begin);
								int index, x, y, z, level, hue;
								for (var j = 0; j < reccount; ++j)
								{
									index = x = y = z = level = hue = 0;
									var compVersion = binbin.ReadInt32();
									switch (compVersion)
									{
										case 0:
											index = binbin.ReadInt32();
											x = binbin.ReadInt32();
											y = binbin.ReadInt32();
											z = binbin.ReadInt32();
											level = binbin.ReadInt32();
											break;

										case 1:
											index = binbin.ReadInt32();
											x = binbin.ReadInt32();
											y = binbin.ReadInt32();
											z = binbin.ReadInt32();
											level = binbin.ReadInt32();
											hue = binbin.ReadInt32();
											break;
									}
									var tempitem = new MultiComponentList.MultiTileEntry
									{
										m_ItemID = (ushort)index,
										m_Flags = 1,
										m_OffsetX = (short)x,
										m_OffsetY = (short)y,
										m_OffsetZ = (short)z,
										m_Unk1 = 0
									};
									arr.Add(tempitem);

								}
								data[1] = new MultiComponentList(arr);
								break;

						}
						multilist.Add(data);
					}
				}
				return multilist;
			}
		}

		public static List<MultiComponentList.MultiTileEntry> RebuildTiles(MultiComponentList.MultiTileEntry[] tiles)
		{
			var newtiles = new List<MultiComponentList.MultiTileEntry>();
			newtiles.AddRange(tiles);

			if (newtiles[0].m_OffsetX == 0 && newtiles[0].m_OffsetY == 0 && newtiles[0].m_OffsetZ == 0) // found a centeritem
			{
				if (newtiles[0].m_ItemID != 0x1) // its a "good" one
				{
					for (var j = newtiles.Count - 1; j >= 0; --j) // remove all invis items
					{
						if (newtiles[j].m_ItemID == 0x1)
						{
							newtiles.RemoveAt(j);
						}
					}
					return newtiles;
				}
				else // a bad one
				{
					for (var i = 1; i < newtiles.Count; ++i) // do we have a better one?
					{
						if (newtiles[i].m_OffsetX == 0 && newtiles[i].m_OffsetY == 0
							&& newtiles[i].m_ItemID != 0x1 && newtiles[i].m_OffsetZ == 0)
						{
							var centeritem = newtiles[i];
							newtiles.RemoveAt(i); // jep so save it
							for (var j = newtiles.Count - 1; j >= 0; --j) // and remove all invis
							{
								if (newtiles[j].m_ItemID == 0x1)
								{
									newtiles.RemoveAt(j);
								}
							}
							newtiles.Insert(0, centeritem);
							return newtiles;
						}
					}
					for (var j = newtiles.Count - 1; j >= 1; --j) // nothing found so remove all invis exept the first
					{
						if (newtiles[j].m_ItemID == 0x1)
						{
							newtiles.RemoveAt(j);
						}
					}
					return newtiles;
				}
			}
			for (var i = 0; i < newtiles.Count; ++i) // is there a good one
			{
				if (newtiles[i].m_OffsetX == 0 && newtiles[i].m_OffsetY == 0
					&& newtiles[i].m_ItemID != 0x1 && newtiles[i].m_OffsetZ == 0)
				{
					var centeritem = newtiles[i];
					newtiles.RemoveAt(i); // store it
					for (var j = newtiles.Count - 1; j >= 0; --j) // remove all invis
					{
						if (newtiles[j].m_ItemID == 0x1)
						{
							newtiles.RemoveAt(j);
						}
					}
					newtiles.Insert(0, centeritem);
					return newtiles;
				}
			}
			for (var j = newtiles.Count - 1; j >= 0; --j) // nothing found so remove all invis
			{
				if (newtiles[j].m_ItemID == 0x1)
				{
					newtiles.RemoveAt(j);
				}
			}
			var invisitem = new MultiComponentList.MultiTileEntry
			{
				m_ItemID = 0x1, // and create a new invis
				m_OffsetX = 0,
				m_OffsetY = 0,
				m_OffsetZ = 0,
				m_Flags = 0,
				m_Unk1 = 0
			};
			newtiles.Insert(0, invisitem);
			return newtiles;
		}

		public static void Save(string path)
		{
			var isUOAHS = PostHSFormat || Art.IsUOAHS();
			var idx = Path.Combine(path, "multi.idx");
			var mul = Path.Combine(path, "multi.mul");
			using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
							  fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				using (BinaryWriter binidx = new BinaryWriter(fsidx),
									binmul = new BinaryWriter(fsmul))
				{
					for (var index = 0; index < 0x2000; ++index)
					{
						var comp = GetComponents(index);

						if (comp == MultiComponentList.Empty)
						{
							binidx.Write(-1); // lookup
							binidx.Write(-1); // length
							binidx.Write(-1); // extra
						}
						else
						{
							var tiles = RebuildTiles(comp.SortedTiles);
							binidx.Write((int)fsmul.Position); //lookup
							if (isUOAHS)
							{
								binidx.Write(tiles.Count * 16); //length
							}
							else
							{
								binidx.Write(tiles.Count * 12); //length
							}

							binidx.Write(-1); //extra
							for (var i = 0; i < tiles.Count; ++i)
							{
								binmul.Write(tiles[i].m_ItemID);
								binmul.Write(tiles[i].m_OffsetX);
								binmul.Write(tiles[i].m_OffsetY);
								binmul.Write(tiles[i].m_OffsetZ);
								binmul.Write(tiles[i].m_Flags);
								if (isUOAHS)
								{
									binmul.Write(tiles[i].m_Unk1);
								}
							}
						}
					}
				}
			}
		}
	}

	public sealed class MultiComponentList
	{
		private Point m_Min, m_Max, m_Center;
		private int m_Width;
		private int m_Height;
		private readonly int m_maxHeight;
		private int m_Surface;
		private MTile[][][] m_Tiles;
		private readonly MultiTileEntry[] m_SortedTiles;

		public static readonly MultiComponentList Empty = new MultiComponentList();

		public Point Min => m_Min;
		public Point Max => m_Max;
		public Point Center => m_Center;
		public int Width => m_Width;
		public int Height => m_Height;
		public MTile[][][] Tiles => m_Tiles;
		public int maxHeight => m_maxHeight;
		public MultiTileEntry[] SortedTiles => m_SortedTiles;
		public int Surface => m_Surface;


		public struct MultiTileEntry
		{
			public ushort m_ItemID;
			public short m_OffsetX, m_OffsetY, m_OffsetZ;
			public int m_Flags;
			public int m_Unk1;
		}

		/// <summary>
		/// Returns Bitmap of Multi
		/// </summary>
		/// <returns></returns>
		public Bitmap GetImage()
		{
			return GetImage(300);
		}

		/// <summary>
		/// Returns Bitmap of Multi to maxheight
		/// </summary>
		/// <param name="maxheight"></param>
		/// <returns></returns>
		public Bitmap GetImage(int maxheight)
		{
			if (m_Width == 0 || m_Height == 0)
			{
				return null;
			}

			int xMin = 1000, yMin = 1000;
			int xMax = -1000, yMax = -1000;

			for (var x = 0; x < m_Width; ++x)
			{
				for (var y = 0; y < m_Height; ++y)
				{
					var tiles = m_Tiles[x][y];

					for (var i = 0; i < tiles.Length; ++i)
					{
						var bmp = Art.GetStatic(tiles[i].ID);

						if (bmp == null)
						{
							continue;
						}

						var px = (x - y) * 22;
						var py = (x + y) * 22;

						px -= (bmp.Width / 2);
						py -= tiles[i].Z << 2;
						py -= bmp.Height;

						if (px < xMin)
						{
							xMin = px;
						}

						if (py < yMin)
						{
							yMin = py;
						}

						px += bmp.Width;
						py += bmp.Height;

						if (px > xMax)
						{
							xMax = px;
						}

						if (py > yMax)
						{
							yMax = py;
						}
					}
				}
			}

			var canvas = new Bitmap(xMax - xMin, yMax - yMin);
			var gfx = Graphics.FromImage(canvas);
			gfx.Clear(Color.White);
			for (var x = 0; x < m_Width; ++x)
			{
				for (var y = 0; y < m_Height; ++y)
				{
					var tiles = m_Tiles[x][y];

					for (var i = 0; i < tiles.Length; ++i)
					{

						var bmp = Art.GetStatic(tiles[i].ID);

						if (bmp == null)
						{
							continue;
						}

						if ((tiles[i].Z) > maxheight)
						{
							continue;
						}

						var px = (x - y) * 22;
						var py = (x + y) * 22;

						px -= (bmp.Width / 2);
						py -= tiles[i].Z << 2;
						py -= bmp.Height;
						px -= xMin;
						py -= yMin;

						gfx.DrawImageUnscaled(bmp, px, py, bmp.Width, bmp.Height);
					}

					var tx = (x - y) * 22;
					var ty = (x + y) * 22;
					tx -= xMin;
					ty -= yMin;
				}
			}

			gfx.Dispose();

			return canvas;
		}

		public MultiComponentList(BinaryReader reader, int count)
		{
			var useNewMultiFormat = Multis.PostHSFormat || Art.IsUOAHS();
			m_Min = m_Max = Point.Empty;
			m_SortedTiles = new MultiTileEntry[count];
			for (var i = 0; i < count; ++i)
			{
				m_SortedTiles[i].m_ItemID = Art.GetLegalItemID(reader.ReadUInt16());
				m_SortedTiles[i].m_OffsetX = reader.ReadInt16();
				m_SortedTiles[i].m_OffsetY = reader.ReadInt16();
				m_SortedTiles[i].m_OffsetZ = reader.ReadInt16();
				m_SortedTiles[i].m_Flags = reader.ReadInt32();
				if (useNewMultiFormat)
				{
					m_SortedTiles[i].m_Unk1 = reader.ReadInt32();
				}
				else
				{
					m_SortedTiles[i].m_Unk1 = 0;
				}

				var e = m_SortedTiles[i];

				if (e.m_OffsetX < m_Min.X)
				{
					m_Min.X = e.m_OffsetX;
				}

				if (e.m_OffsetY < m_Min.Y)
				{
					m_Min.Y = e.m_OffsetY;
				}

				if (e.m_OffsetX > m_Max.X)
				{
					m_Max.X = e.m_OffsetX;
				}

				if (e.m_OffsetY > m_Max.Y)
				{
					m_Max.Y = e.m_OffsetY;
				}

				if (e.m_OffsetZ > m_maxHeight)
				{
					m_maxHeight = e.m_OffsetZ;
				}
			}
			ConvertList();
			reader.Close();
		}

		public MultiComponentList(string FileName, Multis.ImportType Type)
		{
			m_Min = m_Max = Point.Empty;
			int itemcount;
			switch (Type)
			{
				case Multis.ImportType.TXT:
					itemcount = 0;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							itemcount++;
						}
					}
					m_SortedTiles = new MultiTileEntry[itemcount];
					itemcount = 0;
					m_Min.X = 10000;
					m_Min.Y = 10000;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							var split = line.Split(' ');

							var tmp = split[0];
							tmp = tmp.Replace("0x", "");

							m_SortedTiles[itemcount].m_ItemID = UInt16.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
							m_SortedTiles[itemcount].m_OffsetX = Convert.ToInt16(split[1]);
							m_SortedTiles[itemcount].m_OffsetY = Convert.ToInt16(split[2]);
							m_SortedTiles[itemcount].m_OffsetZ = Convert.ToInt16(split[3]);
							m_SortedTiles[itemcount].m_Flags = Convert.ToInt32(split[4]);
							m_SortedTiles[itemcount].m_Unk1 = 0;

							var e = m_SortedTiles[itemcount];

							if (e.m_OffsetX < m_Min.X)
							{
								m_Min.X = e.m_OffsetX;
							}

							if (e.m_OffsetY < m_Min.Y)
							{
								m_Min.Y = e.m_OffsetY;
							}

							if (e.m_OffsetX > m_Max.X)
							{
								m_Max.X = e.m_OffsetX;
							}

							if (e.m_OffsetY > m_Max.Y)
							{
								m_Max.Y = e.m_OffsetY;
							}

							if (e.m_OffsetZ > m_maxHeight)
							{
								m_maxHeight = e.m_OffsetZ;
							}

							itemcount++;
						}
						var centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
						var centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

						m_Min = m_Max = Point.Empty;
						var i = 0;
						for (; i < m_SortedTiles.Length; i++)
						{
							m_SortedTiles[i].m_OffsetX -= (short)centerx;
							m_SortedTiles[i].m_OffsetY -= (short)centery;
							if (m_SortedTiles[i].m_OffsetX < m_Min.X)
							{
								m_Min.X = m_SortedTiles[i].m_OffsetX;
							}

							if (m_SortedTiles[i].m_OffsetX > m_Max.X)
							{
								m_Max.X = m_SortedTiles[i].m_OffsetX;
							}

							if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
							{
								m_Min.Y = m_SortedTiles[i].m_OffsetY;
							}

							if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
							{
								m_Max.Y = m_SortedTiles[i].m_OffsetY;
							}
						}
					}
					break;
				case Multis.ImportType.UOA:
					itemcount = 0;

					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							++itemcount;
							if (itemcount == 4)
							{
								var split = line.Split(' ');
								itemcount = Convert.ToInt32(split[0]);
								break;
							}
						}
					}
					m_SortedTiles = new MultiTileEntry[itemcount];
					itemcount = 0;
					m_Min.X = 10000;
					m_Min.Y = 10000;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						var i = -1;
						while ((line = ip.ReadLine()) != null)
						{
							++i;
							if (i < 4)
							{
								continue;
							}

							var split = line.Split(' ');

							m_SortedTiles[itemcount].m_ItemID = Convert.ToUInt16(split[0]);
							m_SortedTiles[itemcount].m_OffsetX = Convert.ToInt16(split[1]);
							m_SortedTiles[itemcount].m_OffsetY = Convert.ToInt16(split[2]);
							m_SortedTiles[itemcount].m_OffsetZ = Convert.ToInt16(split[3]);
							m_SortedTiles[itemcount].m_Flags = Convert.ToInt32(split[4]);
							m_SortedTiles[itemcount].m_Unk1 = 0;

							var e = m_SortedTiles[itemcount];

							if (e.m_OffsetX < m_Min.X)
							{
								m_Min.X = e.m_OffsetX;
							}

							if (e.m_OffsetY < m_Min.Y)
							{
								m_Min.Y = e.m_OffsetY;
							}

							if (e.m_OffsetX > m_Max.X)
							{
								m_Max.X = e.m_OffsetX;
							}

							if (e.m_OffsetY > m_Max.Y)
							{
								m_Max.Y = e.m_OffsetY;
							}

							if (e.m_OffsetZ > m_maxHeight)
							{
								m_maxHeight = e.m_OffsetZ;
							}

							++itemcount;
						}
						var centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
						var centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

						m_Min = m_Max = Point.Empty;
						i = 0;
						for (; i < m_SortedTiles.Length; ++i)
						{
							m_SortedTiles[i].m_OffsetX -= (short)centerx;
							m_SortedTiles[i].m_OffsetY -= (short)centery;
							if (m_SortedTiles[i].m_OffsetX < m_Min.X)
							{
								m_Min.X = m_SortedTiles[i].m_OffsetX;
							}

							if (m_SortedTiles[i].m_OffsetX > m_Max.X)
							{
								m_Max.X = m_SortedTiles[i].m_OffsetX;
							}

							if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
							{
								m_Min.Y = m_SortedTiles[i].m_OffsetY;
							}

							if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
							{
								m_Max.Y = m_SortedTiles[i].m_OffsetY;
							}
						}
					}

					break;
				case Multis.ImportType.UOAB:
					using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
					using (var reader = new BinaryReader(fs))
					{
						if (reader.ReadInt16() != 1) //Version check
						{
							return;
						}

						string tmp;
						tmp = Multis.ReadUOAString(reader); //Name
						tmp = Multis.ReadUOAString(reader); //Category
						tmp = Multis.ReadUOAString(reader); //Subsection
						var width = reader.ReadInt32();
						var height = reader.ReadInt32();
						var uwidth = reader.ReadInt32();
						var uheight = reader.ReadInt32();

						var count = reader.ReadInt32();
						itemcount = count;
						m_SortedTiles = new MultiTileEntry[itemcount];
						itemcount = 0;
						m_Min.X = 10000;
						m_Min.Y = 10000;
						for (; itemcount < count; ++itemcount)
						{
							m_SortedTiles[itemcount].m_ItemID = (ushort)reader.ReadInt16();
							m_SortedTiles[itemcount].m_OffsetX = reader.ReadInt16();
							m_SortedTiles[itemcount].m_OffsetY = reader.ReadInt16();
							m_SortedTiles[itemcount].m_OffsetZ = reader.ReadInt16();
							reader.ReadInt16(); // level
							m_SortedTiles[itemcount].m_Flags = 1;
							reader.ReadInt16(); // hue
							m_SortedTiles[itemcount].m_Unk1 = 0;

							var e = m_SortedTiles[itemcount];

							if (e.m_OffsetX < m_Min.X)
							{
								m_Min.X = e.m_OffsetX;
							}

							if (e.m_OffsetY < m_Min.Y)
							{
								m_Min.Y = e.m_OffsetY;
							}

							if (e.m_OffsetX > m_Max.X)
							{
								m_Max.X = e.m_OffsetX;
							}

							if (e.m_OffsetY > m_Max.Y)
							{
								m_Max.Y = e.m_OffsetY;
							}

							if (e.m_OffsetZ > m_maxHeight)
							{
								m_maxHeight = e.m_OffsetZ;
							}
						}
						var centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
						var centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

						m_Min = m_Max = Point.Empty;
						itemcount = 0;
						for (; itemcount < m_SortedTiles.Length; ++itemcount)
						{
							m_SortedTiles[itemcount].m_OffsetX -= (short)centerx;
							m_SortedTiles[itemcount].m_OffsetY -= (short)centery;
							if (m_SortedTiles[itemcount].m_OffsetX < m_Min.X)
							{
								m_Min.X = m_SortedTiles[itemcount].m_OffsetX;
							}

							if (m_SortedTiles[itemcount].m_OffsetX > m_Max.X)
							{
								m_Max.X = m_SortedTiles[itemcount].m_OffsetX;
							}

							if (m_SortedTiles[itemcount].m_OffsetY < m_Min.Y)
							{
								m_Min.Y = m_SortedTiles[itemcount].m_OffsetY;
							}

							if (m_SortedTiles[itemcount].m_OffsetY > m_Max.Y)
							{
								m_Max.Y = m_SortedTiles[itemcount].m_OffsetY;
							}
						}
					}
					break;

				case Multis.ImportType.WSC:
					itemcount = 0;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							line = line.Trim();
							if (line.StartsWith("SECTION WORLDITEM"))
							{
								++itemcount;
							}
						}
					}
					m_SortedTiles = new MultiTileEntry[itemcount];
					itemcount = 0;
					m_Min.X = 10000;
					m_Min.Y = 10000;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						var tempitem = new MultiTileEntry
						{
							m_ItemID = 0xFFFF,
							m_Flags = 1,
							m_Unk1 = 0
						};
						while ((line = ip.ReadLine()) != null)
						{
							line = line.Trim();
							if (line.StartsWith("SECTION WORLDITEM"))
							{
								if (tempitem.m_ItemID != 0xFFFF)
								{
									m_SortedTiles[itemcount] = tempitem;
									++itemcount;
								}
								tempitem.m_ItemID = 0xFFFF;
							}
							else if (line.StartsWith("ID"))
							{
								line = line.Remove(0, 2);
								line = line.Trim();
								tempitem.m_ItemID = Convert.ToUInt16(line);
							}
							else if (line.StartsWith("X"))
							{
								line = line.Remove(0, 1);
								line = line.Trim();
								tempitem.m_OffsetX = Convert.ToInt16(line);
								if (tempitem.m_OffsetX < m_Min.X)
								{
									m_Min.X = tempitem.m_OffsetX;
								}

								if (tempitem.m_OffsetX > m_Max.X)
								{
									m_Max.X = tempitem.m_OffsetX;
								}
							}
							else if (line.StartsWith("Y"))
							{
								line = line.Remove(0, 1);
								line = line.Trim();
								tempitem.m_OffsetY = Convert.ToInt16(line);
								if (tempitem.m_OffsetY < m_Min.Y)
								{
									m_Min.Y = tempitem.m_OffsetY;
								}

								if (tempitem.m_OffsetY > m_Max.Y)
								{
									m_Max.Y = tempitem.m_OffsetY;
								}
							}
							else if (line.StartsWith("Z"))
							{
								line = line.Remove(0, 1);
								line = line.Trim();
								tempitem.m_OffsetZ = Convert.ToInt16(line);
								if (tempitem.m_OffsetZ > m_maxHeight)
								{
									m_maxHeight = tempitem.m_OffsetZ;
								}
							}
						}
						if (tempitem.m_ItemID != 0xFFFF)
						{
							m_SortedTiles[itemcount] = tempitem;
						}

						var centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
						var centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

						m_Min = m_Max = Point.Empty;
						var i = 0;
						for (; i < m_SortedTiles.Length; i++)
						{
							m_SortedTiles[i].m_OffsetX -= (short)centerx;
							m_SortedTiles[i].m_OffsetY -= (short)centery;
							if (m_SortedTiles[i].m_OffsetX < m_Min.X)
							{
								m_Min.X = m_SortedTiles[i].m_OffsetX;
							}

							if (m_SortedTiles[i].m_OffsetX > m_Max.X)
							{
								m_Max.X = m_SortedTiles[i].m_OffsetX;
							}

							if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
							{
								m_Min.Y = m_SortedTiles[i].m_OffsetY;
							}

							if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
							{
								m_Max.Y = m_SortedTiles[i].m_OffsetY;
							}
						}
					}
					break;
			}
			ConvertList();
		}

		public MultiComponentList(List<MultiTileEntry> arr)
		{
			m_Min = m_Max = Point.Empty;
			var itemcount = arr.Count;
			m_SortedTiles = new MultiTileEntry[itemcount];
			m_Min.X = 10000;
			m_Min.Y = 10000;
			var i = 0;
			foreach (var entry in arr)
			{
				if (entry.m_OffsetX < m_Min.X)
				{
					m_Min.X = entry.m_OffsetX;
				}

				if (entry.m_OffsetY < m_Min.Y)
				{
					m_Min.Y = entry.m_OffsetY;
				}

				if (entry.m_OffsetX > m_Max.X)
				{
					m_Max.X = entry.m_OffsetX;
				}

				if (entry.m_OffsetY > m_Max.Y)
				{
					m_Max.Y = entry.m_OffsetY;
				}

				if (entry.m_OffsetZ > m_maxHeight)
				{
					m_maxHeight = entry.m_OffsetZ;
				}

				m_SortedTiles[i] = entry;

				++i;
			}
			arr.Clear();
			var centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
			var centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

			m_Min = m_Max = Point.Empty;
			for (i = 0; i < m_SortedTiles.Length; ++i)
			{
				m_SortedTiles[i].m_OffsetX -= (short)centerx;
				m_SortedTiles[i].m_OffsetY -= (short)centery;
				if (m_SortedTiles[i].m_OffsetX < m_Min.X)
				{
					m_Min.X = m_SortedTiles[i].m_OffsetX;
				}

				if (m_SortedTiles[i].m_OffsetX > m_Max.X)
				{
					m_Max.X = m_SortedTiles[i].m_OffsetX;
				}

				if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
				{
					m_Min.Y = m_SortedTiles[i].m_OffsetY;
				}

				if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
				{
					m_Max.Y = m_SortedTiles[i].m_OffsetY;
				}
			}
			ConvertList();
		}

		public MultiComponentList(StreamReader stream, int count)
		{
			string line;
			var itemcount = 0;
			m_Min = m_Max = Point.Empty;
			m_SortedTiles = new MultiTileEntry[count];
			m_Min.X = 10000;
			m_Min.Y = 10000;

			while ((line = stream.ReadLine()) != null)
			{
				var split = Regex.Split(line, @"\s+");
				m_SortedTiles[itemcount].m_ItemID = Convert.ToUInt16(split[0]);
				m_SortedTiles[itemcount].m_Flags = Convert.ToInt32(split[1]);
				m_SortedTiles[itemcount].m_OffsetX = Convert.ToInt16(split[2]);
				m_SortedTiles[itemcount].m_OffsetY = Convert.ToInt16(split[3]);
				m_SortedTiles[itemcount].m_OffsetZ = Convert.ToInt16(split[4]);
				m_SortedTiles[itemcount].m_Unk1 = 0;

				var e = m_SortedTiles[itemcount];

				if (e.m_OffsetX < m_Min.X)
				{
					m_Min.X = e.m_OffsetX;
				}

				if (e.m_OffsetY < m_Min.Y)
				{
					m_Min.Y = e.m_OffsetY;
				}

				if (e.m_OffsetX > m_Max.X)
				{
					m_Max.X = e.m_OffsetX;
				}

				if (e.m_OffsetY > m_Max.Y)
				{
					m_Max.Y = e.m_OffsetY;
				}

				if (e.m_OffsetZ > m_maxHeight)
				{
					m_maxHeight = e.m_OffsetZ;
				}

				++itemcount;
				if (itemcount == count)
				{
					break;
				}
			}
			var centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
			var centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

			m_Min = m_Max = Point.Empty;
			var i = 0;
			for (; i < m_SortedTiles.Length; i++)
			{
				m_SortedTiles[i].m_OffsetX -= (short)centerx;
				m_SortedTiles[i].m_OffsetY -= (short)centery;
				if (m_SortedTiles[i].m_OffsetX < m_Min.X)
				{
					m_Min.X = m_SortedTiles[i].m_OffsetX;
				}

				if (m_SortedTiles[i].m_OffsetX > m_Max.X)
				{
					m_Max.X = m_SortedTiles[i].m_OffsetX;
				}

				if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
				{
					m_Min.Y = m_SortedTiles[i].m_OffsetY;
				}

				if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
				{
					m_Max.Y = m_SortedTiles[i].m_OffsetY;
				}
			}
			ConvertList();
		}

		private void ConvertList()
		{
			m_Center = new Point(-m_Min.X, -m_Min.Y);
			m_Width = (m_Max.X - m_Min.X) + 1;
			m_Height = (m_Max.Y - m_Min.Y) + 1;

			var tiles = new MTileList[m_Width][];
			m_Tiles = new MTile[m_Width][][];

			for (var x = 0; x < m_Width; ++x)
			{
				tiles[x] = new MTileList[m_Height];
				m_Tiles[x] = new MTile[m_Height][];

				for (var y = 0; y < m_Height; ++y)
				{
					tiles[x][y] = new MTileList();
				}
			}

			for (var i = 0; i < m_SortedTiles.Length; ++i)
			{
				var xOffset = m_SortedTiles[i].m_OffsetX + m_Center.X;
				var yOffset = m_SortedTiles[i].m_OffsetY + m_Center.Y;

				tiles[xOffset][yOffset].Add(m_SortedTiles[i].m_ItemID, (sbyte)m_SortedTiles[i].m_OffsetZ, (sbyte)m_SortedTiles[i].m_Flags, m_SortedTiles[i].m_Unk1);
			}

			m_Surface = 0;

			for (var x = 0; x < m_Width; ++x)
			{
				for (var y = 0; y < m_Height; ++y)
				{
					m_Tiles[x][y] = tiles[x][y].ToArray();
					for (var i = 0; i < m_Tiles[x][y].Length; ++i)
					{
						m_Tiles[x][y][i].Solver = i;
					}

					if (m_Tiles[x][y].Length > 1)
					{
						Array.Sort(m_Tiles[x][y]);
					}

					if (m_Tiles[x][y].Length > 0)
					{
						++m_Surface;
					}
				}
			}
		}

		public MultiComponentList(MTileList[][] newtiles, int count, int width, int height)
		{
			m_Min = m_Max = Point.Empty;
			m_SortedTiles = new MultiTileEntry[count];
			m_Center = new Point((int)(Math.Round((width / 2.0))) - 1, (int)(Math.Round((height / 2.0))) - 1);
			if (m_Center.X < 0)
			{
				m_Center.X = width / 2;
			}

			if (m_Center.Y < 0)
			{
				m_Center.Y = height / 2;
			}

			m_maxHeight = -128;

			var counter = 0;
			for (var x = 0; x < width; ++x)
			{
				for (var y = 0; y < height; ++y)
				{
					var tiles = newtiles[x][y].ToArray();
					for (var i = 0; i < tiles.Length; ++i)
					{
						m_SortedTiles[counter].m_ItemID = tiles[i].ID;
						m_SortedTiles[counter].m_OffsetX = (short)(x - m_Center.X);
						m_SortedTiles[counter].m_OffsetY = (short)(y - m_Center.Y);
						m_SortedTiles[counter].m_OffsetZ = (short)(tiles[i].Z);
						m_SortedTiles[counter].m_Flags = tiles[i].Flag;
						m_SortedTiles[counter].m_Unk1 = 0;

						if (m_SortedTiles[counter].m_OffsetX < m_Min.X)
						{
							m_Min.X = m_SortedTiles[counter].m_OffsetX;
						}

						if (m_SortedTiles[counter].m_OffsetX > m_Max.X)
						{
							m_Max.X = m_SortedTiles[counter].m_OffsetX;
						}

						if (m_SortedTiles[counter].m_OffsetY < m_Min.Y)
						{
							m_Min.Y = m_SortedTiles[counter].m_OffsetY;
						}

						if (m_SortedTiles[counter].m_OffsetY > m_Max.Y)
						{
							m_Max.Y = m_SortedTiles[counter].m_OffsetY;
						}

						if (m_SortedTiles[counter].m_OffsetZ > m_maxHeight)
						{
							m_maxHeight = m_SortedTiles[counter].m_OffsetZ;
						}

						++counter;
					}
				}
			}
			ConvertList();
		}

		private MultiComponentList()
		{
			m_Tiles = new MTile[0][][];
		}

		public void ExportToTextFile(string FileName)
		{
			using (var Tex = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
			{
				for (var i = 0; i < m_SortedTiles.Length; ++i)
				{
					Tex.WriteLine(String.Format("0x{0:X} {1} {2} {3} {4}",
								m_SortedTiles[i].m_ItemID,
								m_SortedTiles[i].m_OffsetX,
								m_SortedTiles[i].m_OffsetY,
								m_SortedTiles[i].m_OffsetZ,
								m_SortedTiles[i].m_Flags));
				}
			}
		}

		public void ExportToWscFile(string FileName)
		{
			using (var Tex = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
			{
				for (var i = 0; i < m_SortedTiles.Length; ++i)
				{
					Tex.WriteLine(String.Format("SECTION WORLDITEM {0}", i));
					Tex.WriteLine("{");
					Tex.WriteLine(String.Format("\tID\t{0}", m_SortedTiles[i].m_ItemID));
					Tex.WriteLine(String.Format("\tX\t{0}", m_SortedTiles[i].m_OffsetX));
					Tex.WriteLine(String.Format("\tY\t{0}", m_SortedTiles[i].m_OffsetY));
					Tex.WriteLine(String.Format("\tZ\t{0}", m_SortedTiles[i].m_OffsetZ));
					Tex.WriteLine("\tColor\t0");
					Tex.WriteLine("}");

				}
			}
		}

		public void ExportToUOAFile(string FileName)
		{
			using (var Tex = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
			{
				Tex.WriteLine("6 version");
				Tex.WriteLine("1 template id");
				Tex.WriteLine("-1 item version");
				Tex.WriteLine(String.Format("{0} num components", m_SortedTiles.Length));
				for (var i = 0; i < m_SortedTiles.Length; ++i)
				{
					Tex.WriteLine(String.Format("{0} {1} {2} {3} {4}",
								m_SortedTiles[i].m_ItemID,
								m_SortedTiles[i].m_OffsetX,
								m_SortedTiles[i].m_OffsetY,
								m_SortedTiles[i].m_OffsetZ,
								m_SortedTiles[i].m_Flags));
				}
			}
		}
	}
}