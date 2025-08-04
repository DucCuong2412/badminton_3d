using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GGFormat
{
	public static string RandomFrom(params string[] variants)
	{
		return variants[UnityEngine.Random.Range(0, variants.Length) % variants.Length];
	}

	public static string FormatPrice(int price)
	{
		if (price >= 1000)
		{
			string text = price / 1000 + " " + price % 1000;
			while (price.ToString().Length >= text.Length)
			{
				text += "0";
			}
			return text;
		}
		return price.ToString();
	}

	public static float WinPercent(int wins, int loses)
	{
		int num = wins + loses;
		if (num == 0)
		{
			return 0f;
		}
		return (float)wins / (float)num;
	}

	public static string FormatPercent(float p)
	{
		return ((int)(p * 100f)).ToString();
	}

	public static string Implode(IEnumerable<string> list, string glue)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in list)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(glue);
			}
			stringBuilder.Append(item);
		}
		return stringBuilder.ToString();
	}

	public static string FormatTime(int time)
	{
		if (time < 10)
		{
			return "0" + time;
		}
		return time.ToString();
	}

	public static string FormatTimeSpan(TimeSpan span)
	{
		string str = string.Empty;
		if (span.TotalHours >= 1.0)
		{
			str = str + FormatTime(span.Hours) + ":";
		}
		return str + FormatTime(span.Minutes) + ":" + FormatTime(span.Seconds);
	}

	public static string FormatTimeSpanHuman(TimeSpan span, string hours = "h", string minutes = "m", string seconds = "s", bool useAnd = false)
	{
		string text = string.Empty;
		string text2 = "d";
		if (span.Days > 0)
		{
			string text3 = text;
			text = text3 + span.Days + string.Empty + text2 + " ";
		}
		if (span.Hours > 0)
		{
			string text3 = text;
			text = text3 + span.Hours + string.Empty + hours + " ";
		}
		if (span.Minutes > 0)
		{
			string text3 = text;
			text = text3 + span.Minutes + string.Empty + minutes + " ";
		}
		if (span.Seconds > 0)
		{
			if (text != string.Empty && useAnd)
			{
				text += "and ";
			}
			string text3 = text;
			text = text3 + span.Seconds + string.Empty + seconds;
		}
		return text;
	}
}
