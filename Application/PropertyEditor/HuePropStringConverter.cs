using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.VisualBasic.CompilerServices;
using Ultima;

namespace GumpStudio
{
	public class HuePropStringConverter : StringConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			var flag = false;
			if (sourceType == typeof(string))
			{
				flag = true;
			}

			return flag;
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(Hue);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (Versioned.IsNumeric(Conversions.ToString(value)))
			{
				return Hues.GetHue(Conversions.ToInteger(value));
			}

			return Hues.GetHue(HexHelper.HexToDec(Conversions.ToString(value)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return value.ToString();
		}
	}
}
