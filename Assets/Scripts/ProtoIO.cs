using ProtoBuf.Meta;
using System.IO;

public class ProtoIO
{
	public static bool LoadFromFileCloudSync<T>(string filename, out T model) where T : class
	{
		return LoadFromFile<ProtoSerializer, T>(filename, GGFileIOCloudSync.instance.GetDefaultFileIO(), out model);
	}

	public static bool LoadFromFile<S, T>(string filename, GGFileIO fileIO, out T model) where S : TypeModel, new()where T : class
	{
		model = (T)null;
		if (!fileIO.FileExists(filename))
		{
			return false;
		}
		try
		{
			S val = new S();
			using (Stream source = fileIO.FileReadStream(filename))
			{
				model = (val.Deserialize(source, null, typeof(T)) as T);
			}
			return true;
		}
		catch
		{
		}
		return false;
	}

	public static bool SaveToFileCloudSync<T>(string filename, T model) where T : new()
	{
		return SaveToFile<ProtoSerializer, T>(filename, GGFileIOCloudSync.instance.GetDefaultFileIO(), model);
	}

	public static bool SaveToFile<S, T>(string filename, GGFileIO fileIO, T model) where S : TypeModel, new()where T : new()
	{
		S val = new S();
		if (model == null)
		{
			model = new T();
		}
		try
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				val.Serialize(memoryStream, model);
				memoryStream.Flush();
				fileIO.Write(filename, memoryStream.ToArray());
			}
		}
		catch
		{
			return false;
		}
		return true;
	}
}
