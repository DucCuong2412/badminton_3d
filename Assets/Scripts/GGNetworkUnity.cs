using System.IO;
using UnityEngine;

public class GGNetworkUnity : GGNetwork
{
	protected MemoryStream readBuffer;

	protected MemoryStream writeBuffer;

	protected BinaryReader reader;

	protected BinaryWriter writer;

	protected OnOverflow onOverflow;

	private GGNetworkUnityManager manager;

	protected long beginPosition;

	protected int curMessageLength;

	protected long curMessageReadPos;

	protected override void Init()
	{
		base.Init();
		readBuffer = new MemoryStream(new byte[1024]);
		writeBuffer = new MemoryStream(new byte[1024]);
		reader = new BinaryReader(readBuffer);
		writer = new BinaryWriter(writeBuffer);
		ClearBuffers();
		manager = BehaviourSingleton<GGNetworkUnityManager>.instance;
		manager.onMessage += onMessage;
	}

	private void onMessage(byte[] b)
	{
		if (readBuffer.Capacity - readBuffer.Length < b.Length + 1)
		{
			UnityEngine.Debug.Log("Overflow");
			if (onOverflow != 0)
			{
				return;
			}
			readBuffer.SetLength(0L);
		}
		long position = readBuffer.Position;
		readBuffer.Write(b, 0, b.Length);
		readBuffer.Seek(position, SeekOrigin.Begin);
		CallOnMessageReceived();
	}

	//public override int ConnectedPlayers()
	//{
	//	//return Network.connections.Length;
	//}

	public override bool isInError()
	{
		return manager.state == GGNetworkState.Error || manager.state == GGNetworkState.ErrorFindingHosts || manager.state == GGNetworkState.ServerErrorNameExists;
	}

	public override void StartServer(string name)
	{
		manager.StartServer(name);
	}

	public override void StopServer()
	{
		manager.StopServer();
	}

	public override void StartClient(string name)
	{
		//manager.StartClient(name);
	}

	public override bool isClientServerCapable()
	{
		return true;
	}

	public override void BeginWrite(int messageType)
	{
		writeBuffer.SetLength(0L);
		beginPosition = writeBuffer.Position;
		AddInt(0);
		AddInt(base.protocolVersion);
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

	public override void AddString(string s)
	{
		writer.Write(s);
	}

	public override void AddBool(bool b)
	{
		writer.Write(b);
	}

	public override void AddVector3(Vector3 v)
	{
		AddFloat(v.x);
		AddFloat(v.y);
		AddFloat(v.z);
	}

	public override void EndWrite()
	{
		long position = writeBuffer.Position;
		writeBuffer.Position = beginPosition;
		writer.Write((int)(position - beginPosition));
		writeBuffer.Position = position;
		writer.Flush();
	}

	public override void BeginRead()
	{
		curMessageReadPos = readBuffer.Position;
		curMessageLength = GetInt();
		base.curMessageProtocolVersion = GetInt();
		curMessageType = GetInt();
	}

	public override int GetInt()
	{
		return reader.ReadInt32();
	}

	public override float GetFloat()
	{
		return reader.ReadSingle();
	}

	public override string GetString()
	{
		return reader.ReadString();
	}

	public override bool GetBool()
	{
		return reader.ReadBoolean();
	}

	public override Vector3 GetVector3()
	{
		return new Vector3(GetFloat(), GetFloat(), GetFloat());
	}

	public override void EndRead()
	{
		long num = readBuffer.Position - curMessageReadPos;
		if (num < curMessageLength)
		{
			UnityEngine.Debug.Log("Message unread " + (curMessageLength - num));
			reader.ReadBytes((int)(curMessageLength - num));
		}
	}

	public override void ClearBuffers()
	{
		readBuffer.Seek(0L, SeekOrigin.Begin);
		readBuffer.SetLength(0L);
		writeBuffer.Seek(0L, SeekOrigin.Begin);
		writeBuffer.SetLength(0L);
	}

	public override void SetOnOverflow(OnOverflow o)
	{
		onOverflow = o;
	}

	public override bool HasData()
	{
		return readBuffer.Position < readBuffer.Length;
	}

	public override void Send()
	{
		writer.Flush();
		manager.Send(writeBuffer.ToArray());
	}
}
