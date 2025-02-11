using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashCards.Shared.Util
{
	public static class ColorUtil
	{
		public static Color ColorGradient(float fade, List<Color> colors)
		{
			if (fade >= 1.0f)
				return colors.Last();
			else if (fade <= 0.0f)
				return colors.First();

			var f = fade * (colors.Count - 1);
			var interval = (int)Math.Truncate(f);
			f = f - interval;
			var color1 = colors[interval];
			var color2 = colors[interval + 1];

			var diffRed = color2.Red - color1.Red;
			var diffGreen = color2.Green - color1.Green;
			var diffBlue = color2.Blue - color1.Blue;

			return new Color()
			{
				Red = (diffRed * f) + color1.Red,
				Green = (diffGreen * f) + color1.Green,
				Blue = (diffBlue * f) + color1.Blue
			};
		}
	}

	public class Color
	{
		public float Red { get; set; }
		public float Green { get; set; }
		public float Blue { get; set; }

		public Color() { }

		public Color(string hex)
		{
			if (!hex.StartsWith('#') || (hex.Length != 7 && hex.Length != 4))
			{
				throw new ArgumentException("Invalid hex string");
			}
			if (hex.Length == 7)
			{
				Red = Convert.ToInt32(hex[1..3], 16) / 255.0f;
				Green = Convert.ToInt32(hex[3..5], 16) / 255.0f;
				Blue = Convert.ToInt32(hex[5..7], 16) / 255.0f;
			}
			else
			{
				var r = char.ToString(hex[1]);
				var g = char.ToString(hex[2]);
				var b = char.ToString(hex[3]);
				Red = Convert.ToInt32(r + r, 16) / 255.0f;
				Green = Convert.ToInt32(g + g, 16) / 255.0f;
				Blue = Convert.ToInt32(b + b, 16) / 255.0f;
			}
		}

		public string ToHex() =>
			$"#{((int)(Math.Clamp(Red, 0.0f, 1.0f) * 255)):X2}{((int)(Math.Clamp(Green, 0.0f, 1.0f) * 255)):X2}{((int)(Math.Clamp(Blue, 0.0f, 1.0f) * 255)):X2}";
	}
}
