using GGUnityUtilWP;
using System;
using System.IO;
using UnityEngine;

public class GGFileIOWP : GGFileIO
{
	public override void Write(string path, string text)
	{
		GGUnityUtilWP.GGFileIOWP.Write(path, text);
	}

	public override void Write(string path, byte[] bytes)
	{
		GGUnityUtilWP.GGFileIOWP.Write(path, bytes);
	}

	public override string ReadText(string path)
	{
		return GGUnityUtilWP.GGFileIOWP.ReadText(path);
	}

	public override byte[] Read(string path)
	{
		return GGUnityUtilWP.GGFileIOWP.Read(path);
	}

	public override bool FileExists(string path)
	{
		return GGUnityUtilWP.GGFileIOWP.FileExists(path);
	}

	public override DateTime LastWriteTimeUTC(string path)
	{
		return GGUnityUtilWP.GGFileIOWP.LastWriteTimeUTC(path);
	}

	public override Stream FileReadStream(string path)
	{
		UnityEngine.Debug.Log("Read Stream From Path: " + path);
		byte[] array = Read(path);
		return new MemoryStream(array, 0, array.Length);
	}
}
