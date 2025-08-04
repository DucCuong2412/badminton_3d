using System;
using System.IO;
using UnityEngine;

public class GGUID
{
	protected static string uid;

	protected static string filePath => Application.persistentDataPath + "/gguid.txt";

	protected static void Save()
	{
		try
		{
			File.WriteAllText(filePath, uid);
		}
		catch
		{
		}
	}

	public static string InstallId()
	{
		if (!string.IsNullOrEmpty(uid))
		{
			return uid;
		}
		string text = Application.persistentDataPath + "/gguid.txt";
		FileInfo fileInfo = new FileInfo(text);
		if (fileInfo == null || !fileInfo.Exists)
		{
			uid = Guid.NewGuid().ToString();
			Save();
		}
		else
		{
			try
			{
				uid = File.ReadAllText(text);
			}
			catch
			{
			}
		}
		UnityEngine.Debug.Log("UID: " + uid);
		return uid;
	}
}
