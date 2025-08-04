using ExitGames.Client.Photon;
using System;
using System.IO;
using UnityEngine;

public class GGNetworkPhoton : GGNetwork
{
	protected MemoryStream readBuffer;

	protected MemoryStream writeBuffer;

	protected BinaryReader reader;

	protected BinaryWriter writer;

	protected OnOverflow onOverflow;

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
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Combine(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(onEvent));
	}

	private void onEvent(byte eventCode, object message, int senderId)
	{
		UnityEngine.Debug.Log("On Event Received");
		byte[] array = message as byte[];
		if (array != null)
		{
			onMessage(array);
		}
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

	public override int ConnectedPlayers()
	{
		return PhotonNetwork.otherPlayers.Length;
	}

	public override bool isInError()
	{
		return false;
	}

	public override void StartServer(string name)
	{
		PhotonNetwork.ConnectUsingSettings(name);
	}

	public override void StopServer()
	{
		PhotonNetwork.Disconnect();
	}

	public override void StartClient(string name)
	{
	}

	public override bool Start(string name)
	{
		return PhotonNetwork.ConnectUsingSettings(name);
	}

	public override bool isReady()
	{
		return PhotonNetwork.connectedAndReady;
	}

	public override bool CreateRoom(string name, string levelName, int maxPlayers)
	{
		if (string.IsNullOrEmpty(levelName))
		{
			return PhotonNetwork.CreateRoom(name, isVisible: true, isOpen: true, maxPlayers);
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add("levelName", levelName);
		return PhotonNetwork.CreateRoom(name, isVisible: true, isOpen: true, maxPlayers, hashtable, null);
	}

	public override bool JoinRandomRoom(string levelName)
	{
		if (string.IsNullOrEmpty(levelName))
		{
			return PhotonNetwork.JoinRandomRoom();
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add("levelName", levelName);
		return PhotonNetwork.JoinRandomRoom(hashtable, 0);
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

	public new virtual bool GetBool()
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
		PhotonNetwork.RaiseEvent(1, writeBuffer.ToArray(), sendReliable: true, null);
	}
}
