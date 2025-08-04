using System;
using System.IO;

public class GGFileIOUnity : GGFileIO
{
	protected string GetInternalPath()
	{
		return GGSupportMenu.instance.GetInternalPath();
	}

	protected string FullPath(string filename)
	{
		return GetInternalPath() + "/" + filename;
	}

	public override void Write(string path, string text)
	{
		File.WriteAllText(FullPath(path), text);
	}

	public override void Write(string path, byte[] bytes)
	{
		File.WriteAllBytes(FullPath(path), bytes);
	}

	public override string ReadText(string path)
	{
		return File.ReadAllText(FullPath(path));
	}

	public override byte[] Read(string path)
	{
		return File.ReadAllBytes(FullPath(path));
	}

	public override bool FileExists(string path)
	{
		return new FileInfo(FullPath(path))?.Exists ?? false;
	}

	public override DateTime LastWriteTimeUTC(string path)
	{
		return new FileInfo(FullPath(path))?.LastWriteTimeUtc ?? DateTime.UtcNow;
	}

	public override Stream FileReadStream(string path)
	{
		return new FileStream(FullPath(path), FileMode.Open, FileAccess.Read);
	}
}
