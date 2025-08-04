using UnityEngine;

public class GGNetwork
{
	public enum OnOverflow
	{
		DeleteAllUnread,
		DismissNewMessage
	}

	public delegate void NetworkEvent(GGNetwork network);

	protected static GGNetwork instance_;

	protected int curMessageType;

	protected int index_;

	public int curMessageProtocolVersion
	{
		get;
		protected set;
	}

	public int protocolVersion => 1;

	public static GGNetwork instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new GGNetworkPhoton();
				instance_.Init();
			}
			return instance_;
		}
	}

	public event NetworkEvent onMessageReceived;

	protected virtual void Init()
	{
	}

	protected void CallOnMessageReceived()
	{
		if (this.onMessageReceived != null)
		{
			this.onMessageReceived(this);
		}
	}

	public int NextIndex()
	{
		return index_++;
	}

	public virtual int ConnectedPlayers()
	{
		return 0;
	}

	public virtual bool isInError()
	{
		return false;
	}

	public virtual bool Start(string name)
	{
		return true;
	}

	public virtual bool isReady()
	{
		return true;
	}

	public virtual bool isClientServerCapable()
	{
		return false;
	}

	public virtual void StartServer(string name)
	{
	}

	public virtual void StartClient(string name)
	{
	}

	public virtual void StopServer()
	{
	}

	public virtual void BeginWrite(int messageType)
	{
	}

	public virtual void AddInt(int i)
	{
	}

	public virtual void AddFloat(float f)
	{
	}

	public virtual void AddString(string s)
	{
	}

	public virtual void AddBool(bool b)
	{
	}

	public virtual bool CreateRoom(string name, string levelName, int maxPlayers)
	{
		return false;
	}

	public virtual bool JoinRandomRoom(string levelName)
	{
		return false;
	}

	public virtual void AddVector3(Vector3 v)
	{
		AddFloat(v.x);
		AddFloat(v.y);
		AddFloat(v.z);
	}

	public virtual void EndWrite()
	{
	}

	public virtual void BeginRead()
	{
	}

	public virtual int GetInt()
	{
		return 0;
	}

	public virtual float GetFloat()
	{
		return 0f;
	}

	public virtual string GetString()
	{
		return null;
	}

	public virtual bool GetBool()
	{
		return false;
	}

	public virtual Vector3 GetVector3()
	{
		return Vector3.zero;
	}

	public virtual void EndRead()
	{
	}

	public virtual void ClearBuffers()
	{
	}

	public virtual void SetOnOverflow(OnOverflow o)
	{
	}

	public virtual bool HasData()
	{
		return false;
	}

	public virtual void Send(int type)
	{
		BeginWrite(type);
		EndWrite();
		Send();
	}

	public virtual void Send()
	{
	}

	public virtual bool PollMessage(GGNetworkDeserializer deserializer)
	{
		if (HasData())
		{
			try
			{
				BeginRead();
				deserializer.Deserialize(curMessageType, this);
			}
			finally
			{
				EndRead();
			}
		}
		return HasData();
	}
}
