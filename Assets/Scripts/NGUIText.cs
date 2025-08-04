using System.Text;
using UnityEngine;

public static class NGUIText
{
	public enum SymbolStyle
	{
		None,
		Uncolored,
		Colored
	}

	public class GlyphInfo
	{
		public Vector2 v0;

		public Vector2 v1;

		public Vector2 u0;

		public Vector2 u1;

		public float advance;

		public int channel;

		public bool rotatedUVs;
	}

	public static UIFont bitmapFont;

	public static Font dynamicFont;

	public static GlyphInfo glyph = new GlyphInfo();

	public static int fontSize = 16;

	public static float fontScale = 1f;

	public static float pixelDensity = 1f;

	public static FontStyle fontStyle = FontStyle.Normal;

	public static TextAlignment alignment = TextAlignment.Left;

	public static Color tint = Color.white;

	public static int rectWidth = 1000000;

	public static int rectHeight = 1000000;

	public static int maxLines = 0;

	public static bool gradient = false;

	public static Color gradientBottom = Color.white;

	public static Color gradientTop = Color.white;

	public static bool encoding = false;

	public static float spacingX = 0f;

	public static float spacingY = 0f;

	public static bool premultiply = false;

	public static SymbolStyle symbolStyle;

	public static int finalSize = 0;

	public static float finalSpacingX = 0f;

	public static float finalLineHeight = 0f;

	public static float baseline = 0f;

	public static bool useSymbols = false;

	private static Color mInvisible = new Color(0f, 0f, 0f, 0f);

	private static BetterList<Color> mColors = new BetterList<Color>();

	private static CharacterInfo mTempChar;

	private static BetterList<float> mSizes = new BetterList<float>();

	private static Color32 s_c0;

	private static Color32 s_c1;

	public static void Update()
	{
		Update(request: true);
	}

	public static void Update(bool request)
	{
		finalSize = Mathf.RoundToInt((float)fontSize / pixelDensity);
		finalSpacingX = spacingX * fontScale;
		finalLineHeight = ((float)fontSize + spacingY) * fontScale;
		useSymbols = (bitmapFont != null && bitmapFont.hasSymbols && encoding && symbolStyle != SymbolStyle.None);
		if (!(dynamicFont != null) || !request)
		{
			return;
		}
		dynamicFont.RequestCharactersInTexture(")", finalSize, fontStyle);
		if (!dynamicFont.GetCharacterInfo(')', out mTempChar, finalSize, fontStyle))
		{
			dynamicFont.RequestCharactersInTexture("A", finalSize, fontStyle);
			if (!dynamicFont.GetCharacterInfo('A', out mTempChar, finalSize, fontStyle))
			{
				baseline = 0f;
				return;
			}
		}
		float yMax = mTempChar.vert.yMax;
		float yMin = mTempChar.vert.yMin;
		baseline = Mathf.Round(yMax + ((float)finalSize - yMax + yMin) * 0.5f);
	}

	public static void Prepare(string text)
	{
		if (dynamicFont != null)
		{
			dynamicFont.RequestCharactersInTexture(text, finalSize, fontStyle);
		}
	}

	public static BMSymbol GetSymbol(string text, int index, int textLength)
	{
		return (!(bitmapFont != null)) ? null : bitmapFont.MatchSymbol(text, index, textLength);
	}

	public static float GetGlyphWidth(int ch, int prev)
	{
		if (bitmapFont != null)
		{
			BMGlyph bMGlyph = bitmapFont.bmFont.GetGlyph(ch);
			if (bMGlyph != null)
			{
				return fontScale * (float)((prev == 0) ? bMGlyph.advance : (bMGlyph.advance + bMGlyph.GetKerning(prev)));
			}
		}
		else if (dynamicFont != null && dynamicFont.GetCharacterInfo((char)ch, out mTempChar, finalSize, fontStyle))
		{
			return Mathf.Round(mTempChar.width * fontScale * pixelDensity);
		}
		return 0f;
	}

	public static GlyphInfo GetGlyph(int ch, int prev)
	{
		if (bitmapFont != null)
		{
			BMGlyph bMGlyph = bitmapFont.bmFont.GetGlyph(ch);
			if (bMGlyph != null)
			{
				int num = (prev != 0) ? bMGlyph.GetKerning(prev) : 0;
				glyph.v0.x = ((prev == 0) ? bMGlyph.offsetX : (bMGlyph.offsetX + num));
				glyph.v1.y = -bMGlyph.offsetY;
				glyph.v1.x = glyph.v0.x + (float)bMGlyph.width;
				glyph.v0.y = glyph.v1.y - (float)bMGlyph.height;
				glyph.u0.x = bMGlyph.x;
				glyph.u0.y = bMGlyph.y + bMGlyph.height;
				glyph.u1.x = bMGlyph.x + bMGlyph.width;
				glyph.u1.y = bMGlyph.y;
				glyph.advance = bMGlyph.advance + num;
				glyph.channel = bMGlyph.channel;
				glyph.rotatedUVs = false;
				if (fontScale != 1f)
				{
					glyph.v0 *= fontScale;
					glyph.v1 *= fontScale;
					glyph.advance *= fontScale;
				}
				return glyph;
			}
		}
		else if (dynamicFont != null && dynamicFont.GetCharacterInfo((char)ch, out mTempChar, finalSize, fontStyle))
		{
			glyph.v0.x = mTempChar.vert.xMin;
			glyph.v1.x = glyph.v0.x + mTempChar.vert.width;
			glyph.v0.y = mTempChar.vert.yMax - baseline;
			glyph.v1.y = glyph.v0.y - mTempChar.vert.height;
			glyph.u0.x = mTempChar.uv.xMin;
			glyph.u0.y = mTempChar.uv.yMin;
			glyph.u1.x = mTempChar.uv.xMax;
			glyph.u1.y = mTempChar.uv.yMax;
			glyph.advance = mTempChar.width;
			glyph.channel = 0;
			glyph.rotatedUVs = mTempChar.flipped;
			float num2 = fontScale * pixelDensity;
			if (num2 != 1f)
			{
				glyph.v0 *= num2;
				glyph.v1 *= num2;
				glyph.advance *= num2;
			}
			glyph.advance = Mathf.Round(glyph.advance);
			return glyph;
		}
		return null;
	}

	public static Color ParseColor(string text, int offset)
	{
		int num = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int num2 = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int num3 = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		float num4 = 0.003921569f;
		return new Color(num4 * (float)num, num4 * (float)num2, num4 * (float)num3);
	}

	public static string EncodeColor(Color c)
	{
		int num = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
		return NGUIMath.DecimalToHex(num);
	}

	public static int ParseSymbol(string text, int index)
	{
		int length = text.Length;
		if (index + 2 < length && text[index] == '[')
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
				{
					return 3;
				}
			}
			else if (index + 7 < length && text[index + 7] == ']')
			{
				Color c = ParseColor(text, index + 1);
				if (EncodeColor(c) == text.Substring(index + 1, 6).ToUpper())
				{
					return 8;
				}
			}
		}
		return 0;
	}

	public static bool ParseSymbol(string text, ref int index)
	{
		int num = ParseSymbol(text, index);
		if (num != 0)
		{
			index += num;
			return true;
		}
		return false;
	}

	public static bool ParseSymbol(string text, ref int index, BetterList<Color> colors, bool premultiply)
	{
		if (colors == null)
		{
			return ParseSymbol(text, ref index);
		}
		int length = text.Length;
		if (index + 2 < length && text[index] == '[')
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
				{
					if (colors != null && colors.size > 1)
					{
						colors.RemoveAt(colors.size - 1);
					}
					index += 3;
					return true;
				}
			}
			else if (index + 7 < length && text[index + 7] == ']')
			{
				if (colors != null)
				{
					Color color = ParseColor(text, index + 1);
					if (EncodeColor(color) != text.Substring(index + 1, 6).ToUpper())
					{
						return false;
					}
					Color color2 = colors[colors.size - 1];
					color.a = color2.a;
					if (premultiply && color.a != 1f)
					{
						color = Color.Lerp(mInvisible, color, color.a);
					}
					colors.Add(color);
				}
				index += 8;
				return true;
			}
		}
		return false;
	}

	public static string StripSymbols(string text)
	{
		if (text != null)
		{
			int num = 0;
			int length = text.Length;
			while (num < length)
			{
				char c = text[num];
				if (c == '[')
				{
					int num2 = ParseSymbol(text, num);
					if (num2 != 0)
					{
						text = text.Remove(num, num2);
						length = text.Length;
						continue;
					}
				}
				num++;
			}
		}
		return text;
	}

	public static void Align(BetterList<Vector3> verts, int indexOffset, float offset)
	{
		if (alignment == TextAlignment.Left)
		{
			return;
		}
		float num = 0f;
		if (alignment == TextAlignment.Right)
		{
			num = (float)rectWidth - offset;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		else
		{
			num = ((float)rectWidth - offset) * 0.5f;
			if (num < 0f)
			{
				num = 0f;
			}
			int num2 = Mathf.RoundToInt((float)rectWidth - offset);
			int num3 = Mathf.RoundToInt(rectWidth);
			bool flag = (num2 & 1) == 1;
			bool flag2 = (num3 & 1) == 1;
			if ((flag && !flag2) || (!flag && flag2))
			{
				num += 0.5f / fontScale;
			}
		}
		for (int i = indexOffset; i < verts.size; i++)
		{
			verts.buffer[i] = verts.buffer[i];
			verts.buffer[i].x += num;
		}
	}

	public static int GetClosestCharacter(BetterList<Vector3> verts, Vector2 pos)
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		int result = 0;
		for (int i = 0; i < verts.size; i++)
		{
			float y = pos.y;
			Vector3 vector = verts[i];
			float num3 = Mathf.Abs(y - vector.y);
			if (!(num3 > num2))
			{
				float x = pos.x;
				Vector3 vector2 = verts[i];
				float num4 = Mathf.Abs(x - vector2.x);
				if (num3 < num2)
				{
					num2 = num3;
					num = num4;
					result = i;
				}
				else if (num4 < num)
				{
					num = num4;
					result = i;
				}
			}
		}
		return result;
	}

	public static void EndLine(ref StringBuilder s)
	{
		int num = s.Length - 1;
		if (num > 0 && s[num] == ' ')
		{
			s[num] = '\n';
		}
		else
		{
			s.Append('\n');
		}
	}

	public static Vector2 CalculatePrintedSize(string text)
	{
		Vector2 zero = Vector2.zero;
		if (!string.IsNullOrEmpty(text))
		{
			if (encoding)
			{
				text = StripSymbols(text);
			}
			Prepare(text);
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			int length = text.Length;
			int num4 = 0;
			int prev = 0;
			for (int i = 0; i < length; i++)
			{
				num4 = text[i];
				if (num4 == 10)
				{
					if (num > num3)
					{
						num3 = num;
					}
					num = 0f;
					num2 += finalLineHeight;
				}
				else
				{
					if (num4 < 32)
					{
						continue;
					}
					BMSymbol bMSymbol = (!useSymbols) ? null : GetSymbol(text, i, length);
					if (bMSymbol == null)
					{
						num4 = text[i];
						float glyphWidth = GetGlyphWidth(num4, prev);
						if (glyphWidth != 0f)
						{
							num += finalSpacingX + glyphWidth;
						}
						prev = num4;
					}
					else
					{
						num += finalSpacingX + (float)bMSymbol.advance * fontScale;
						i += bMSymbol.sequence.Length - 1;
						prev = 0;
					}
				}
			}
			zero.x = ((!(num > num3)) ? num3 : num);
			zero.y = num2 + finalLineHeight;
		}
		return zero;
	}

	public static int CalculateOffsetToFit(string text)
	{
		if (string.IsNullOrEmpty(text) || rectWidth < 1)
		{
			return 0;
		}
		Prepare(text);
		int length = text.Length;
		int num = 0;
		int prev = 0;
		int i = 0;
		for (int length2 = text.Length; i < length2; i++)
		{
			BMSymbol bMSymbol = (!useSymbols) ? null : GetSymbol(text, i, length);
			if (bMSymbol == null)
			{
				num = text[i];
				float glyphWidth = GetGlyphWidth(num, prev);
				if (glyphWidth != 0f)
				{
					mSizes.Add(finalSpacingX + glyphWidth);
				}
				prev = num;
				continue;
			}
			mSizes.Add(finalSpacingX + (float)bMSymbol.advance * fontScale);
			int j = 0;
			for (int num2 = bMSymbol.sequence.Length - 1; j < num2; j++)
			{
				mSizes.Add(0f);
			}
			i += bMSymbol.sequence.Length - 1;
			prev = 0;
		}
		float num3 = rectWidth;
		int num4 = mSizes.size;
		while (num4 > 0 && num3 > 0f)
		{
			num3 -= mSizes[--num4];
		}
		mSizes.Clear();
		if (num3 < 0f)
		{
			num4++;
		}
		return num4;
	}

	public static string GetEndOfLineThatFits(string text)
	{
		int length = text.Length;
		int num = CalculateOffsetToFit(text);
		return text.Substring(num, length - num);
	}

	public static void RequestCharactersInTexture(Font font, string text)
	{
		if (font != null)
		{
			font.RequestCharactersInTexture(text, finalSize, fontStyle);
		}
	}

	public static bool WrapText(string text, out string finalText)
	{
		if (rectWidth < 1 || rectHeight < 1 || finalLineHeight < 1f)
		{
			finalText = string.Empty;
			return false;
		}
		float num = (maxLines <= 0) ? ((float)rectHeight) : Mathf.Min(rectHeight, finalLineHeight * (float)maxLines);
		int num2 = (maxLines <= 0) ? 1000000 : maxLines;
		num2 = Mathf.FloorToInt(Mathf.Min(num2, num / finalLineHeight) + 0.01f);
		if (num2 == 0)
		{
			finalText = string.Empty;
			return false;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		StringBuilder s = new StringBuilder();
		int length = text.Length;
		float num3 = rectWidth;
		int i = 0;
		int j = 0;
		int num4 = 1;
		int num5 = 0;
		bool flag = true;
		for (; j < length; j++)
		{
			char c = text[j];
			if (c == '\n')
			{
				if (num4 == num2)
				{
					break;
				}
				num3 = rectWidth;
				if (i < j)
				{
					s.Append(text.Substring(i, j - i + 1));
				}
				else
				{
					s.Append(c);
				}
				flag = true;
				num4++;
				i = j + 1;
				num5 = 0;
				continue;
			}
			if (c == ' ' && num5 != 32 && i < j)
			{
				s.Append(text.Substring(i, j - i + 1));
				flag = false;
				i = j + 1;
				num5 = c;
			}
			if (encoding && ParseSymbol(text, ref j))
			{
				j--;
				continue;
			}
			BMSymbol bMSymbol = (!useSymbols) ? null : GetSymbol(text, j, length);
			float num6;
			if (bMSymbol == null)
			{
				float glyphWidth = GetGlyphWidth(c, num5);
				if (glyphWidth == 0f)
				{
					continue;
				}
				num6 = finalSpacingX + glyphWidth;
			}
			else
			{
				num6 = finalSpacingX + (float)bMSymbol.advance * fontScale;
			}
			num3 -= num6;
			if (num3 < 0f)
			{
				if (!flag && num4 != num2)
				{
					for (; i < length && text[i] == ' '; i++)
					{
					}
					flag = true;
					num3 = rectWidth;
					j = i - 1;
					num5 = 0;
					if (num4++ == num2)
					{
						break;
					}
					EndLine(ref s);
					continue;
				}
				s.Append(text.Substring(i, Mathf.Max(0, j - i)));
				if (num4++ == num2)
				{
					i = j;
					break;
				}
				EndLine(ref s);
				flag = true;
				if (c == ' ')
				{
					i = j + 1;
					num3 = rectWidth;
				}
				else
				{
					i = j;
					num3 = (float)rectWidth - num6;
				}
				num5 = 0;
			}
			else
			{
				num5 = c;
			}
			if (bMSymbol != null)
			{
				j += bMSymbol.length - 1;
				num5 = 0;
			}
		}
		if (i < j)
		{
			s.Append(text.Substring(i, j - i));
		}
		finalText = s.ToString();
		return j == length || num4 <= Mathf.Min(maxLines, num2);
	}

	public static void Print(string text, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int size = verts.size;
		Prepare(text);
		mColors.Add(Color.white);
		int num = 0;
		int prev = 0;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = finalSize;
		Color a = tint * gradientBottom;
		Color b = tint * gradientTop;
		Color32 color = tint;
		int length = text.Length;
		Rect rect = default(Rect);
		float num6 = 0f;
		float num7 = 0f;
		if (bitmapFont != null)
		{
			rect = bitmapFont.uvRect;
			num6 = rect.width / (float)bitmapFont.texWidth;
			num7 = rect.height / (float)bitmapFont.texHeight;
		}
		for (int i = 0; i < length; i++)
		{
			num = text[i];
			if (num == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (alignment != 0)
				{
					Align(verts, size, num2 - finalSpacingX);
					size = verts.size;
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num < 32)
			{
				prev = num;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i, mColors, premultiply))
			{
				Color color2 = tint * mColors[mColors.size - 1];
				color = color2;
				if (gradient)
				{
					a = gradientBottom * color2;
					b = gradientTop * color2;
				}
				i--;
				continue;
			}
			BMSymbol bMSymbol = (!useSymbols) ? null : GetSymbol(text, i, length);
			if (bMSymbol == null)
			{
				GlyphInfo glyphInfo = GetGlyph(num, prev);
				if (glyphInfo == null)
				{
					continue;
				}
				prev = num;
				if (num == 32)
				{
					num2 += finalSpacingX + glyphInfo.advance;
					continue;
				}
				if (uvs != null)
				{
					if (bitmapFont != null)
					{
						glyphInfo.u0.x = rect.xMin + num6 * glyphInfo.u0.x;
						glyphInfo.u1.x = rect.xMin + num6 * glyphInfo.u1.x;
						glyphInfo.u0.y = rect.yMax - num7 * glyphInfo.u0.y;
						glyphInfo.u1.y = rect.yMax - num7 * glyphInfo.u1.y;
					}
					if (glyphInfo.rotatedUVs)
					{
						uvs.Add(glyphInfo.u0);
						uvs.Add(new Vector2(glyphInfo.u1.x, glyphInfo.u0.y));
						uvs.Add(glyphInfo.u1);
						uvs.Add(new Vector2(glyphInfo.u0.x, glyphInfo.u1.y));
					}
					else
					{
						uvs.Add(glyphInfo.u0);
						uvs.Add(new Vector2(glyphInfo.u0.x, glyphInfo.u1.y));
						uvs.Add(glyphInfo.u1);
						uvs.Add(new Vector2(glyphInfo.u1.x, glyphInfo.u0.y));
					}
				}
				if (cols != null)
				{
					if (glyphInfo.channel == 0 || glyphInfo.channel == 15)
					{
						if (gradient)
						{
							float num8 = num5 + glyphInfo.v0.y;
							float num9 = num5 + glyphInfo.v1.y;
							num8 /= num5;
							num9 /= num5;
							s_c0 = Color.Lerp(a, b, num8);
							s_c1 = Color.Lerp(a, b, num9);
							cols.Add(s_c0);
							cols.Add(s_c1);
							cols.Add(s_c1);
							cols.Add(s_c0);
						}
						else
						{
							for (int j = 0; j < 4; j++)
							{
								cols.Add(color);
							}
						}
					}
					else
					{
						Color color3 = color;
						color3 *= 0.49f;
						switch (glyphInfo.channel)
						{
						case 1:
							color3.b += 0.51f;
							break;
						case 2:
							color3.g += 0.51f;
							break;
						case 4:
							color3.r += 0.51f;
							break;
						case 8:
							color3.a += 0.51f;
							break;
						}
						for (int k = 0; k < 4; k++)
						{
							cols.Add(color3);
						}
					}
				}
				glyphInfo.v0.x += num2;
				glyphInfo.v1.x += num2;
				glyphInfo.v0.y -= num3;
				glyphInfo.v1.y -= num3;
				num2 += finalSpacingX + glyphInfo.advance;
				verts.Add(glyphInfo.v0);
				verts.Add(new Vector3(glyphInfo.v0.x, glyphInfo.v1.y));
				verts.Add(glyphInfo.v1);
				verts.Add(new Vector3(glyphInfo.v1.x, glyphInfo.v0.y));
				continue;
			}
			float num10 = num2 + (float)bMSymbol.offsetX * fontScale;
			float x = num10 + (float)bMSymbol.width * fontScale;
			float num11 = 0f - (num3 + (float)bMSymbol.offsetY * fontScale);
			float y = num11 - (float)bMSymbol.height * fontScale;
			verts.Add(new Vector3(num10, y));
			verts.Add(new Vector3(num10, num11));
			verts.Add(new Vector3(x, num11));
			verts.Add(new Vector3(x, y));
			num2 += finalSpacingX + (float)bMSymbol.advance * fontScale;
			i += bMSymbol.length - 1;
			prev = 0;
			if (uvs != null)
			{
				Rect uvRect = bMSymbol.uvRect;
				float xMin = uvRect.xMin;
				float yMin = uvRect.yMin;
				float xMax = uvRect.xMax;
				float yMax = uvRect.yMax;
				uvs.Add(new Vector2(xMin, yMin));
				uvs.Add(new Vector2(xMin, yMax));
				uvs.Add(new Vector2(xMax, yMax));
				uvs.Add(new Vector2(xMax, yMin));
			}
			if (cols == null)
			{
				continue;
			}
			if (symbolStyle == SymbolStyle.Colored)
			{
				for (int l = 0; l < 4; l++)
				{
					cols.Add(color);
				}
				continue;
			}
			Color32 item = Color.white;
			item.a = color.a;
			for (int m = 0; m < 4; m++)
			{
				cols.Add(item);
			}
		}
		if (alignment != 0 && size < verts.size)
		{
			Align(verts, size, num2 - finalSpacingX);
			size = verts.size;
		}
		mColors.Clear();
	}

	public static void PrintCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices)
	{
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = (float)fontSize * fontScale * 0.5f;
		int length = text.Length;
		int size = verts.size;
		int num5 = 0;
		int prev = 0;
		for (int i = 0; i < length; i++)
		{
			num5 = text[i];
			verts.Add(new Vector3(num, 0f - num2 - num4));
			indices.Add(i);
			if (num5 == 10)
			{
				if (num > num3)
				{
					num3 = num;
				}
				if (alignment != 0)
				{
					Align(verts, size, num - finalSpacingX);
					size = verts.size;
				}
				num = 0f;
				num2 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num5 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = (!useSymbols) ? null : GetSymbol(text, i, length);
			if (bMSymbol == null)
			{
				float glyphWidth = GetGlyphWidth(num5, prev);
				if (glyphWidth != 0f)
				{
					num += glyphWidth + finalSpacingX;
					verts.Add(new Vector3(num, 0f - num2 - num4));
					indices.Add(i + 1);
					prev = num5;
				}
			}
			else
			{
				num += (float)bMSymbol.advance * fontScale + finalSpacingX;
				verts.Add(new Vector3(num, 0f - num2 - num4));
				indices.Add(i + 1);
				i += bMSymbol.sequence.Length - 1;
				prev = 0;
			}
		}
		if (alignment != 0 && size < verts.size)
		{
			Align(verts, size, num - finalSpacingX);
		}
	}

	public static void PrintCaretAndSelection(string text, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)
	{
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		int num = end;
		if (start > end)
		{
			end = start;
			start = num;
		}
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = (float)fontSize * fontScale;
		int indexOffset = caret?.size ?? 0;
		int num6 = highlight?.size ?? 0;
		int length = text.Length;
		int i = 0;
		int num7 = 0;
		int prev = 0;
		bool flag = false;
		bool flag2 = false;
		Vector2 v = Vector2.zero;
		Vector2 v2 = Vector2.zero;
		for (; i < length; i++)
		{
			if (caret != null && !flag2 && num <= i)
			{
				flag2 = true;
				caret.Add(new Vector3(num2 - 1f, 0f - num3 - num5));
				caret.Add(new Vector3(num2 - 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3 - num5));
			}
			num7 = text[i];
			if (num7 == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (caret != null && flag2)
				{
					if (alignment != 0)
					{
						Align(caret, indexOffset, num2 - finalSpacingX);
					}
					caret = null;
				}
				if (highlight != null)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(v2);
						highlight.Add(v);
					}
					else if (start <= i && end > i)
					{
						highlight.Add(new Vector3(num2, 0f - num3 - num5));
						highlight.Add(new Vector3(num2, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
					}
					if (alignment != 0 && num6 < highlight.size)
					{
						Align(highlight, num6, num2 - finalSpacingX);
						num6 = highlight.size;
					}
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num7 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i, mColors, premultiply))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = (!useSymbols) ? null : GetSymbol(text, i, length);
			float num8 = (bMSymbol == null) ? GetGlyphWidth(num7, prev) : ((float)bMSymbol.advance * fontScale);
			if (num8 == 0f)
			{
				continue;
			}
			float x = num2;
			float x2 = num2 + num8;
			float y = 0f - num3 - num5;
			float y2 = 0f - num3;
			num2 += num8 + finalSpacingX;
			if (highlight != null)
			{
				if (start > i || end <= i)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(v2);
						highlight.Add(v);
					}
				}
				else if (!flag)
				{
					flag = true;
					highlight.Add(new Vector3(x, y));
					highlight.Add(new Vector3(x, y2));
				}
			}
			v = new Vector2(x2, y);
			v2 = new Vector2(x2, y2);
			prev = num7;
		}
		if (caret != null)
		{
			if (!flag2)
			{
				caret.Add(new Vector3(num2 - 1f, 0f - num3 - num5));
				caret.Add(new Vector3(num2 - 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3 - num5));
			}
			if (alignment != 0)
			{
				Align(caret, indexOffset, num2 - finalSpacingX);
			}
		}
		if (highlight != null)
		{
			if (flag)
			{
				highlight.Add(v2);
				highlight.Add(v);
			}
			else if (start < i && end == i)
			{
				highlight.Add(new Vector3(num2, 0f - num3 - num5));
				highlight.Add(new Vector3(num2, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
			}
			if (alignment != 0 && num6 < highlight.size)
			{
				Align(highlight, num6, num2 - finalSpacingX);
			}
		}
	}
}
