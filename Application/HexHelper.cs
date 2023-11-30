using System;

using Microsoft.VisualBasic;

namespace GumpStudio
{
	public class HexHelper
	{
		protected const string Numbers = "0123456789ABCDEF";

		public static int HexToDec(string Value)
		{
			Value = Strings.UCase(Value);
			if (Strings.Len(Value) <= 2 | Strings.Len(Value) > 6 || Strings.Left(Value, 2) != "0X")
			{
				return 0;
			}

			var num1 = 0;
			var length = Value.Length;
			while (true)
			{
				var num2 = 3;
				if (length >= num2)
				{
					var num3 = Strings.InStr("0123456789ABCDEF", Strings.Mid(Value, length, 1), CompareMethod.Binary) - 1;
					if (num3 != -1)
					{
						num1 += (int)Math.Round(Math.Pow(16.0, Value.Length - length) * num3);
						length += -1;
					}
					else
					{
						break;
					}
				}
				else
				{
					goto label_7;
				}
			}
			return 0;
		label_7:
			return num1;
		}
	}
}
