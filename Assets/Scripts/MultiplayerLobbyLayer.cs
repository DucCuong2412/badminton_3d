using System;
using UnityEngine;

public class MultiplayerLobbyLayer : UILayer
{
	public enum GameType
	{
		Server,
		Client,
		FindMatch
	}

	public enum RequestType
	{
		AskMatch,
		IsWaiting,
		Remove
	}

	protected struct MyState
	{
		public bool isHandShakeSent;

		public bool isHandShakeReceived;

		public bool isAskResponseReceived;

		public bool isServer;

		public int clientRetries;

		public bool clientPicketUpAddress;

		public float serverTimeoutTime;

		public float waitingPollTime;

		//public ConnectionTesterStatus testStatus;

		public bool isTesting;

		public string clientName;

		public bool createdRoom;

		public bool triedToJoinRoom;

		public bool isNetworkAvailable;
	}

	public UILabel text;

	public UILabel wins;

	public UILabel winPercent;

	public UILabel score;

	public new UILabel name;

	public UISprite myFlag;

	public UISprite opponentFlag;

	protected string serverName = string.Empty;

	protected float timeoutBetweenWaitingPoll = 2f;

	protected float serverTimeout = 10f;

	private MyState state;

	protected int maxClientRetries = 10;

	protected GameType gameType;

	protected StandardDeserializer deserializer = new StandardDeserializer();

	private float introTimeStamp;

	protected float countryTimer;

	public float countryChangeDuration = 0.25f;

	private int opponentFlagIndex;

	public void FindMatch()
	{
		gameType = GameType.FindMatch;
		NavigationManager.instance.Push(base.gameObject);
	}

	private void Awake()
	{
		introTimeStamp = Time.timeSinceLevelLoad + UnityEngine.Random.Range(0f, 10f);
		deserializer.onIntro = OnIntro;
		serverName = "com.gg.rtt" + Guid.NewGuid().ToString();
	}

	private void OnEnable()
	{
		DoOnEnable();
		GameConstants.SetFlag(myFlag, CareerBackend.instance.Flag());
		PlayerSettings instance = PlayerSettings.instance;
		UITools.ChangeText(wins, instance.Model.multiplayerWins.ToString());
		UITools.ChangeText(score, instance.Model.score.ToString());
		UITools.ChangeText(winPercent, GGFormat.FormatPercent(instance.winPercent) + "%");
		Screen.sleepTimeout = -1;
	}

	private void DoOnEnable()
	{
		GGSupportMenu instance = GGSupportMenu.instance;
		state = default(MyState);
		if (!instance.isNetworkConnected())
		{
			state.isNetworkAvailable = false;
			text.text = "You need to be connected to a network...";
			return;
		}
		GGNetwork instance2 = GGNetwork.instance;
		instance2.onMessageReceived -= OnNetworkMessage;
		instance2.onMessageReceived += OnNetworkMessage;
		switch (gameType)
		{
		case GameType.Client:
			instance2.StartClient(serverName);
			UnityEngine.Debug.Log("START_CLIENT");
			break;
		case GameType.Server:
			instance2.StartServer(serverName);
			UnityEngine.Debug.Log("START_SERVER");
			break;
		case GameType.FindMatch:
			instance2.Start(ConfigBase.instance.matchServerApp);
			break;
		}
		text.text = "Looking for a match...";
		Analytics.instance.ReportMultiplayerEvent("OpenLobby");
	}

	private void OnDisable()
	{
		UnityEngine.Debug.Log("Disable Lobby Layer");
		GGNetwork instance = GGNetwork.instance;
		instance.onMessageReceived -= OnNetworkMessage;
	}

	private void OnNetworkMessage(GGNetwork network)
	{
		UnityEngine.Debug.Log("Message in lobby layer");
		if (!state.isHandShakeReceived)
		{
			network.PollMessage(deserializer);
		}
	}

	private void OnIntro(MIntro intro)
	{
		UnityEngine.Debug.Log("OnIntro");
		if (state.isHandShakeReceived)
		{
			return;
		}
		UnityEngine.Debug.Log("OnIntro");
		if (introTimeStamp == intro.timeStamp)
		{
			introTimeStamp = Time.timeSinceLevelLoad + UnityEngine.Random.Range(0f, 20f);
			UnityEngine.Debug.Log("Resend Intro");
			SendIntro();
			return;
		}
		if (!state.isHandShakeSent)
		{
			UnityEngine.Debug.Log("Send Hand shake");
			SendIntro();
		}
		int serverTag = (!(introTimeStamp > intro.timeStamp)) ? 1 : 0;
		MatchController.MatchParameters.MultiplayerParams multiplayerParams = default(MatchController.MatchParameters.MultiplayerParams);
		multiplayerParams.opponentName = intro.myName;
		multiplayerParams.opponentFlag = intro.myFlag;
		multiplayerParams.serverTag = serverTag;
		multiplayerParams.shoeIndex = intro.shoeIndex;
		multiplayerParams.racketIndex = intro.racketIndex;
		multiplayerParams.score = intro.score;
		multiplayerParams.multiplayerWins = intro.multiplayerWins;
		multiplayerParams.lookIndex = intro.playerLook;
		multiplayerParams.multiplayerLoses = intro.multiplayerLoses;
		multiplayerParams.multiplayerLosesKnown = intro.knowsLoses;
		MatchController.MatchParameters.MultiplayerParams mp = multiplayerParams;
		state.isHandShakeReceived = true;
		ScreenNavigation.instance.LoadMultiplayerMatch(mp);
		Analytics.instance.ReportMultiplayerEvent("Matched");
	}

	private void UpdateFlag()
	{
		countryTimer += Time.deltaTime;
		if (countryTimer > countryChangeDuration)
		{
			countryTimer = 0f;
			opponentFlagIndex = (opponentFlagIndex + 1) % GameConstants.Instance.Countries.Count;
			GameConstants.SetFlag(opponentFlag, opponentFlagIndex);
			opponentFlag.alpha = 0.6f;
			opponentFlag.cachedTransform.localScale = Vector3.one * 1.05f;
			TweenScale.Begin(opponentFlag.cachedGameObject, 0.2f, Vector3.one);
			TweenAlpha.Begin(opponentFlag.cachedGameObject, 0.2f, 1f);
		}
	}

	private void OnJoinedLobby()
	{
		UnityEngine.Debug.Log("Joined Lobby");
		GGNetwork.instance.JoinRandomRoom(ConfigBase.instance.matchServerApp);
	}

	private void OnPhotonRandomJoinFailed()
	{
		UnityEngine.Debug.Log("Creating room");
		GGNetwork.instance.CreateRoom(null, ConfigBase.instance.matchServerApp, 2);
	}

	private new void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			OnBack();
			return;
		}
		if (state.isNetworkAvailable)
		{
			if (GGSupportMenu.instance.isNetworkConnected())
			{
				DoOnEnable();
			}
			return;
		}
		UpdateFlag();
		GGNetwork instance = GGNetwork.instance;
		if (instance.isInError())
		{
			text.text = "Error with the connection, retry later...";
		}
		else if (!state.isHandShakeReceived)
		{
			if (instance.ConnectedPlayers() > 0 && !state.isHandShakeSent)
			{
				text.text = "Found an opponent!";
				SendIntro();
			}
			instance.PollMessage(deserializer);
		}
	}

	private void SendIntro()
	{
		GGNetwork instance = GGNetwork.instance;
		state.isHandShakeSent = true;
		CareerBackend instance2 = CareerBackend.instance;
		MIntro mIntro = default(MIntro);
		mIntro.version = 2;
		mIntro.timeStamp = introTimeStamp;
		mIntro.myFlag = instance2.Flag();
		mIntro.myName = instance2.Name();
		mIntro.shoeIndex = PlayerSettings.instance.Model.usedShoe;
		mIntro.racketIndex = PlayerSettings.instance.Model.usedRacket;
		mIntro.score = PlayerSettings.instance.Model.score;
		mIntro.multiplayerWins = PlayerSettings.instance.Model.multiplayerWins;
		mIntro.multiplayerLoses = PlayerSettings.instance.Model.multiplayerLoses;
		MIntro mIntro2 = mIntro;
		mIntro2.Send(instance);
	}

	public void OnBack()
	{
		GGNetwork.instance.StopServer();
		NavigationManager.instance.Pop();
	}
}
