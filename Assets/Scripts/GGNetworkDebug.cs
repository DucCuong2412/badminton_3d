using System.IO;
using UnityEngine;

public class GGNetworkDebug : GGNetwork
{
	protected MemoryStream readBuffer;

	protected MemoryStream writeBuffer;

	protected BinaryReader reader;

	protected BinaryWriter writer;

	public GGNetworkDebug()
	{
		readBuffer = new MemoryStream(new byte[1024]);
		writeBuffer = new MemoryStream(new byte[1024]);
		reader = new BinaryReader(readBuffer);
		writer = new BinaryWriter(writeBuffer);
	}

	public override void BeginWrite(int messageType)
	{
		writeBuffer.SetLength(0L);
		AddInt(NextIndex());
		AddInt(messageType);
	}

	public override void AddInt(int i)
	{
		writer.Write(i);
	}

	public override void AddFloat(float f)
	{
		writer.Write(f);
	}

	public override void AddVector3(Vector3 v)
	{
		AddFloat(v.x);
		AddFloat(v.y);
		AddFloat(v.z);
	}

	public override void EndWrite()
	{
		writer.Flush();
	}

	public override void BeginRead()
	{
		readBuffer.Seek(0L, SeekOrigin.Begin);
	}

	public override int GetInt()
	{
		return reader.ReadInt32();
	}

	public override float GetFloat()
	{
		return reader.ReadSingle();
	}

	public override Vector3 GetVector3()
	{
		return new Vector3(GetFloat(), GetFloat(), GetFloat());
	}

	public override void EndRead()
	{
	}

	public override void Send()
	{
		readBuffer.SetLength(0L);
		writeBuffer.WriteTo(readBuffer);
	}
}
