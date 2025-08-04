using System;
using System.IO;
using UnityEngine;

public class GGFileIO
{
	public static GGFileIO instance_;

	public static GGFileIO instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new GGFileIOUnity();
			}
			return instance_;
		}
	}

	public virtual void Write(string path, string text)
	{
	}

	public virtual void Write(string path, byte[] bytes)
	{
	}

	public virtual string ReadText(string path)
	{
		UnityEngine.Debug.Log("Read Text Path: " + path);
		return null;
	}

	public virtual byte[] Read(string path)
	{
		UnityEngine.Debug.Log("Read From Path: " + path);
		return null;
	}

	public virtual bool FileExists(string path)
	{
		return false;
	}

	public virtual DateTime LastWriteTimeUTC(string path)
	{
		return DateTime.UtcNow;
	}

	public virtual Stream FileReadStream(string path)
	{
		UnityEngine.Debug.Log("Read Stream From Path: " + path);
		byte[] array = Read(path);
		return new MemoryStream(array, 0, array.Length);
	}
}
