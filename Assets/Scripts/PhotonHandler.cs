using ExitGames.Client.Photon;
using Photon;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

internal class PhotonHandler : Photon.MonoBehaviour, IPhotonPeerListener
{
	public static PhotonHandler SP;

	public int updateInterval;

	public int updateIntervalOnSerialize;

	private int nextSendTickCount;

	private int nextSendTickCountOnSerialize;

	private static bool sendThreadShouldRun;

	[CompilerGenerated]
	private static Func<bool> _003C_003Ef__mg_0024cache0;

	protected void Awake()
	{
		if (SP != null && SP != this && SP.gameObject != null)
		{
			UnityEngine.Object.DestroyImmediate(SP.gameObject);
		}
		SP = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		updateInterval = 1000 / PhotonNetwork.sendRate;
		updateIntervalOnSerialize = 1000 / PhotonNetwork.sendRateOnSerialize;
		StartFallbackSendAckThread();
	}

	protected void OnApplicationQuit()
	{
		StopFallbackSendAckThread();
		PhotonNetwork.Disconnect();
	}

	protected void Update()
	{
		if (PhotonNetwork.networkingPeer == null)
		{
			UnityEngine.Debug.LogError("NetworkPeer broke!");
		}
		else
		{
			if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated || PhotonNetwork.connectionStateDetailed == PeerState.Disconnected || PhotonNetwork.offlineMode || !PhotonNetwork.isMessageQueueRunning)
			{
				return;
			}
			bool flag = true;
			while (PhotonNetwork.isMessageQueueRunning && flag)
			{
				flag = PhotonNetwork.networkingPeer.DispatchIncomingCommands();
			}
			int num = (int)(Time.realtimeSinceStartup * 1000f);
			if (PhotonNetwork.isMessageQueueRunning && num > nextSendTickCountOnSerialize)
			{
				PhotonNetwork.networkingPeer.RunViewUpdate();
				nextSendTickCountOnSerialize = num + updateIntervalOnSerialize;
				nextSendTickCount = 0;
			}
			num = (int)(Time.realtimeSinceStartup * 1000f);
			if (num > nextSendTickCount)
			{
				bool flag2 = true;
				while (PhotonNetwork.isMessageQueueRunning && flag2)
				{
					flag2 = PhotonNetwork.networkingPeer.SendOutgoingCommands();
				}
				nextSendTickCount = num + updateInterval;
			}
		}
	}

	protected void OnLevelWasLoaded(int level)
	{
		PhotonNetwork.networkingPeer.NewSceneLoaded();
		PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
	}

	protected void OnJoinedRoom()
	{
		PhotonNetwork.networkingPeer.LoadLevelIfSynced();
	}

	protected void OnCreatedRoom()
	{
		PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
	}

	public static void StartFallbackSendAckThread()
	{
		if (!sendThreadShouldRun)
		{
			sendThreadShouldRun = true;
			SupportClass.CallInBackground(FallbackSendAckThread);
		}
	}

	public static void StopFallbackSendAckThread()
	{
		sendThreadShouldRun = false;
	}

	public static bool FallbackSendAckThread()
	{
		if (sendThreadShouldRun && PhotonNetwork.networkingPeer != null)
		{
			PhotonNetwork.networkingPeer.SendAcksOnly();
		}
		return sendThreadShouldRun;
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		switch (level)
		{
		case DebugLevel.ERROR:
			UnityEngine.Debug.LogError(message);
			return;
		case DebugLevel.WARNING:
			UnityEngine.Debug.LogWarning(message);
			return;
		case DebugLevel.INFO:
			if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				UnityEngine.Debug.Log(message);
				return;
			}
			break;
		}
		if (level == DebugLevel.ALL && PhotonNetwork.logLevel == PhotonLogLevel.Full)
		{
			UnityEngine.Debug.Log(message);
		}
	}

	public void OnOperationResponse(OperationResponse operationResponse)
	{
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
	}

	public void OnEvent(EventData photonEvent)
	{
	}
}
