using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGNetworkUnityManager : BehaviourSingleton<GGNetworkUnityManager>
{
	public delegate void OnMessageReceived(byte[] b);

	public class DelaySendMsg
	{
		public byte[] bytes;

		public float time;

		public float delay;
	}

	protected static string gameType = "com.giraffegames.realtabletennis";

	protected float delay;

	protected float delayRangePercent = 0.25f;

	private List<DelaySendMsg> delayList = new List<DelaySendMsg>();

	//private NetworkView networkView_;

	public GGNetworkState state
	{
		get;
		protected set;
	}

	//protected NetworkView cachedNetworkView
	//{
	//	get
	//	{
	//		if (networkView_ == null)
	//		{
	//			networkView_ = GetComponent<NetworkView>();
	//			if (networkView_ == null)
	//			{
	//				networkView_ = AddNetworkView();
	//			}
	//		}
	//		return networkView_;
	//	}
	//}

	public event OnMessageReceived onMessage;

	public void Send(byte[] b)
	{
		if (delay > 0f)
		{
			delayList.Add(new DelaySendMsg
			{
				bytes = b,
				time = Time.realtimeSinceStartup,
				delay = delay * (1f + Random.Range(0f - delayRangePercent, delayRangePercent))
			});
		}
		else
		{
			//cachedNetworkView.RPC("ReceiveRPCMessage", RPCMode.Others, b);
		}
	}

	//[RPC]
	public void ReceiveRPCMessage(byte[] b)
	{
		if (this.onMessage != null)
		{
			this.onMessage(b);
		}
	}

	//private NetworkView AddNetworkView()
	//{
	//	NetworkView networkView = base.gameObject.AddComponent<NetworkView>();
	//	networkView.observed = null;
	//	networkView.stateSynchronization = NetworkStateSynchronization.Off;
	//	return networkView;
	//}

	private void Awake()
	{
		//networkView_ = AddNetworkView();
	}

	public void StartServer(string name)
	{
		gameType = name;
		state = GGNetworkState.NotStarted;
		//if (Network.isClient || Network.isServer)
		//{
		//	StartCoroutine(StopThanStartServer());
		//}
		//else
		//{
		//	DoStartServer();
		//}
	}

	private void DoStartServer()
	{
		//bool useNat = !Network.HavePublicAddress();
		int num = 0;
		int num2 = 3;
		int num3 = 25000;
		while (true)
		{
			//NetworkConnectionError networkConnectionError = Network.InitializeServer(2, num3, useNat);
			//if (networkConnectionError == NetworkConnectionError.CreateSocketOrThreadFailure && num < num2)
			//{
			//	num3++;
			//	num++;
			//	continue;
			//}
			break;
		}
	}

	//private IEnumerator StopThanStartServer()
	//{
	//	Time.timeScale = 1f;
	//	StopServer();
	//	//while (Network.isClient || Network.isServer)
	//	//{
	//	//	yield return null;
	//	//}
	//	DoStartServer();
	//}

	public void StopServer()
	{
		state = GGNetworkState.NotStarted;
		//Network.Disconnect();
		//MasterServer.UnregisterHost();
	}

	//public void StartClient(string name)
	//{
	//	gameType = name;
	//	state = GGNetworkState.NotStarted;
	//	MasterServer.ClearHostList();
	//	state = GGNetworkState.ClientLookingForHosts;
	//	UnityEngine.Debug.Log("Requesting host list");
	//	MasterServer.RequestHostList(name);
	//}

	//public void UnregisterHost()
	//{
	//	MasterServer.UnregisterHost();
	//}

	//private void OnMasterServerEvent(MasterServerEvent e)
	//{
	//	switch (e)
	//	{
	//	case MasterServerEvent.RegistrationSucceeded:
	//		UnityEngine.Debug.Log("Registration Success");
	//		state = GGNetworkState.ServerStarted;
	//		break;
	//	case MasterServerEvent.RegistrationFailedNoServer:
	//		UnityEngine.Debug.Log("No Server");
	//		state = GGNetworkState.Error;
	//		break;
	//	case MasterServerEvent.RegistrationFailedGameName:
	//		UnityEngine.Debug.Log("Registration Failed Game Name");
	//		state = GGNetworkState.ServerErrorNameExists;
	//		break;
	//	case MasterServerEvent.HostListReceived:
	//		UnityEngine.Debug.Log("Received Hosts");
	//		state = GGNetworkState.ReceivedHosts;
	//		break;
	//	}
	//}

	//private void OnFailedToConnectToMasterServer(NetworkConnectionError error)
	//{
	//	UnityEngine.Debug.Log("Failed to Connect To Master Server");
	//	state = GGNetworkState.Error;
	//}

	private void OnConnectedToServer()
	{
		UnityEngine.Debug.Log("OnConnected to server");
		state = GGNetworkState.ClientConnected;
	}

	private void OnFailedToConnect()
	{
		UnityEngine.Debug.Log("OnFailedToConnect");
		state = GGNetworkState.Error;
	}

	//private void OnServerInitialized()
	//{
	//	UnityEngine.Debug.Log("Server Started");
	//	state = GGNetworkState.ServerRegistering;
	//	MasterServer.RegisterHost(gameType, "Test Server");
	//}

	private void OnDisconnectedFromServer()
	{
		UnityEngine.Debug.Log("OnDisconected");
		state = GGNetworkState.NotStarted;
	}

	private void OnPlayerConnected()
	{
	}

	private void OnPlayerDisconected()
	{
	}

	//public void ConnectClient(HostData hd)
	//{
	//	UnityEngine.Debug.Log("Error " + Network.Connect(hd).ToString());
	//}

	private void Update()
	{
		if (state == GGNetworkState.ClientLookingForHosts || state == GGNetworkState.ReceivedHosts)
		{
			//if (MasterServer.PollHostList().Length > 0)
			//{
			//	state = GGNetworkState.ClientFoundHosts;
			//	HostData[] array = MasterServer.PollHostList();
			//	UnityEngine.Debug.Log("Host Data available " + array.Length + " 0 " + array[0].gameType + " game type " + gameType);
			//	ConnectClient(array[0]);
			//	MasterServer.ClearHostList();
			//}
			//else if (state == GGNetworkState.ReceivedHosts)
			//{
			//	state = GGNetworkState.HostsListEmpty;
			//}
		}
		if (delayList.Count > 0)
		{
			List<DelaySendMsg> list = new List<DelaySendMsg>();
			foreach (DelaySendMsg delay2 in delayList)
			{
				if (delay2.time + delay2.delay < Time.realtimeSinceStartup)
				{
					list.Add(delay2);
					//cachedNetworkView.RPC("ReceiveRPCMessage", RPCMode.Others, delay2.bytes);
				}
			}
			foreach (DelaySendMsg item in list)
			{
				delayList.Remove(item);
			}
			list.Clear();
		}
	}
}
