using System;
using UnityEngine;

public class MultiplayerMatchController : MatchController, GGNetworkDeserializer
{
	public enum RematchStatus
	{
		Unknown,
		Accepted,
		Declined
	}

	private RematchStatus myRematchStatus;

	private RematchStatus opponentRematchStatus;

	private GGNetworkDeserializer standardDeserializer;

	private RemotePlayer remotePlayer;

	private HumanPlayer human;

	private int activeServerTag;

	private GGNetwork net;

	private float timeTillSendBallUpdate;

	public MultiplayerDialog multiplayerDialog;

	protected float lastUpdateTime;

	private bool gameStarted;

	private bool shownNotConnected;

	private float updateDivisor = 5f;

	protected bool isActiveClient => base.activePlayer == human;

	private new void Awake()
	{
		base.Awake();
		StandardDeserializer standardDeserializer = new StandardDeserializer();
		standardDeserializer.onHitBall = OnRemoteHitBall;
		standardDeserializer.onBallTiming = OnRemoteBallTiming;
		standardDeserializer.onAwardPoint = OnRemoteAwardPoint;
		standardDeserializer.onFaultServe = OnRemoteFaultServe;
		standardDeserializer.onText = OnRemoteText;
		this.standardDeserializer = standardDeserializer;
		net = GGNetwork.instance;
		int num = Mathf.Max(PlayerSettings.instance.Model.usedShoe, MatchController.InitParameters.opponentShoeIndex);
		int num2 = Mathf.Max(PlayerSettings.instance.Model.usedRacket, MatchController.InitParameters.opponentRacketIndex);
		GameObject gameObject = UnityEngine.Object.Instantiate(characterPrefab);
		human = gameObject.AddComponent<HumanPlayer>();
		GameObject gameObject2 = UnityEngine.Object.Instantiate(characterPrefab);
		remotePlayer = gameObject2.AddComponent<RemotePlayer>();
		human.Init(this, players.Count, -1, cameraController, null);
		human.SetShoe(num, isMultiplayer: true);
		human.SetRacket(num2, isMultiplayer: true);
		players.Add(human);
		remotePlayer.Init(this, players.Count, 1, num, num2, MatchController.InitParameters.multiplayerParams.lookIndex);
		players.Add(remotePlayer);
		Time.timeScale = timeScale;
		PlayerSettings.instance.Model.multiplayerMatchPlayed = true;
		PlayerSettings.instance.Save();
		Analytics.instance.ReportMultiplayerMatchStart();
	}

	private void OnEnable()
	{
		net.onMessageReceived += OnRemoteMessageReceived;
	}

	private void OnDisable()
	{
		net.onMessageReceived -= OnRemoteMessageReceived;
	}

	private void Start()
	{
		if (MatchController.InitParameters.player1Name.ToLower() == MatchController.InitParameters.player2Name.ToLower())
		{
			MatchController.InitParameters.player1Name += " (You)";
		}
		ui.Init(this, MatchController.InitParameters.player1Name, MatchController.InitParameters.player1Flag, MatchController.InitParameters.player2Name, MatchController.InitParameters.player2Flag);
		ui.onMessageButton += OnMessageButton;
		ui.onVs += OnVs;
		ui.UpdateScoreBoard(score, MatchController.InitParameters.player1Score, MatchController.InitParameters.player2Score);
		if (MatchController.InitParameters.player1Score == 0 && MatchController.InitParameters.player2Score == 0)
		{
			PlayerSettings instance = PlayerSettings.instance;
			MatchParameters.MultiplayerParams multiplayerParams = MatchController.InitParameters.multiplayerParams;
			ui.ShowVS(instance.Model.multiplayerWins, instance.Model.score, instance.winPercent, multiplayerParams.multiplayerWins, multiplayerParams.score, GGFormat.WinPercent(multiplayerParams.multiplayerWins, multiplayerParams.multiplayerLoses), multiplayerParams.multiplayerLosesKnown);
			cameraController.RotateAround(new Vector3(0f, table.netHeight * 10f, 0f), new Vector3(0f, table.netHeight, 0f), table.halphLength * 3f, (float)Math.PI * -2f / 3f, -(float)Math.PI / 3f, 16f);
		}
		else
		{
			StartGame();
		}
	}

	private void OnVs()
	{
		if (MatchController.InitParameters.serverTag == 0)
		{
			StartGame();
			net.Send(12);
		}
	}

	private void StartGame()
	{
		if (!gameStarted)
		{
			gameStarted = true;
			cameraController.MoveTo(cameraController.idlePosition, new Vector3(0f, 0f, 0f), 2f);
			cameraController.rotationSpeed = 50f;
			ui.HideVS();
			foreach (PlayerBase player in players)
			{
				player.OnGameStart();
			}
			ui.showText(MatchController.InitParameters.numGamesInSet + " Games in Set", delegate
			{
				ui.showText(GGFormat.RandomFrom("Game Start", "Good Luck!", "Start!"), delegate
				{
					if (MatchController.InitParameters.serverTag == 0)
					{
						StartServe(players[0]);
					}
				});
			});
		}
	}

	private void OnRemoteText(MText t)
	{
		ui.SetTextToPlayer(remotePlayer.playerTag, t.text, 3f);
	}

	private void OnMessageButton(string msg)
	{
		ui.SetTextToPlayer(human.playerTag, msg, 2f);
		MText mText = default(MText);
		mText.text = msg;
		MText mText2 = mText;
		mText2.Send(net);
		ui.hideMessageButtons();
	}

	void GGNetworkDeserializer.Deserialize(int type, GGNetwork net)
	{
		switch (type)
		{
		case 0:
			UnityEngine.Debug.Log("Remote start serve");
			StartServe(remotePlayer);
			break;
		case 3:
			UnityEngine.Debug.Log("Remote throw in air");
			remotePlayer.ThrowServeBall();
			break;
		case 4:
			UnityEngine.Debug.Log("Remote repeat serve");
			remotePlayer.StartServe(base.ball);
			break;
		case 5:
			UnityEngine.Debug.Log("Remote can complete swing");
			break;
		case 9:
			OnOpponentAcceptedRematch();
			break;
		case 12:
			StartGame();
			break;
		case 10:
			OnOpponentDeclineRematch();
			break;
		default:
			standardDeserializer.Deserialize(type, net);
			break;
		}
	}

	private void OnOpponentDeclineRematch()
	{
		opponentRematchStatus = RematchStatus.Declined;
		multiplayerDialog.Show("No Rematch!", MatchController.InitParameters.player2Name + " refused a Rematch!", string.Empty, "Leave", null, GoToMain);
	}

	private void OnOpponentAcceptedRematch()
	{
		opponentRematchStatus = RematchStatus.Accepted;
		if (myRematchStatus == RematchStatus.Accepted)
		{
			ScreenNavigation.instance.LoadMatch(MatchController.InitParameters);
		}
		else if (myRematchStatus == RematchStatus.Unknown)
		{
			multiplayerDialog.Show("Rematch!", MatchController.InitParameters.player2Name + " asked you for a Rematch!", "Accept", "Refuse", SendRematch, GoToMain);
		}
	}

	private void OnRemoteBallTiming(MBallTiming timing)
	{
		if (!(base.activePlayer != remotePlayer) && remotePlayer.shotParams != null && !isActiveClient)
		{
			PlayerBase.ShotParameters shotParams = remotePlayer.shotParams;
			float ballPossesionTime = shotParams.ballPossesionTime;
			float contactTime = shotParams.contactTime;
			float num = base.activePlayer.shotParams.contactTime / updateDivisor;
			float num2 = timing.possesionTime - ballPossesionTime;
			float value = num / Mathf.Max(1E-06f, num - num2);
			value = Mathf.Clamp(value, 0.001f, 10f);
			Time.timeScale = value * timeScale;
			lastUpdateTime = ballPossesionTime;
			UnityEngine.Debug.Log("REMOTE BALL " + ballPossesionTime + "  " + timing.possesionTime + " ts  " + Time.timeScale);
		}
	}

	private void OnRemoteHitBall(MHitBall hit)
	{
		UnityEngine.Debug.Log("Remote Hit");
		Vector3 position = hit.myPosition.Mirror();
		Vector3 position2 = hit.opponentPosition.Mirror();
		remotePlayer.myTransform.position = position;
		human.transform.position = position2;
		remotePlayer.Hit(hit);
		Time.timeScale = timeScale;
	}

	private void OnRemoteAwardPoint(MAwardPoint award)
	{
		UnityEngine.Debug.Log("Award me " + award.playerTag);
		DoAwardPointTo(players[(award.playerTag + 1) % 2], (FaultReason)award.fault);
	}

	private void OnRemoteMessageReceived(GGNetwork net)
	{
		while (net.HasData())
		{
			net.PollMessage(this);
		}
	}

	private void OnRemoteFaultServe(MServeFault fault)
	{
		DoRepeatServe(players[(fault.playerTag + 1) % 2]);
	}

	private void DoRepeatServe(PlayerBase player)
	{
		base.ballInGame = false;
		PlaySound(ui.foulAudio, 0.5f);
		ui.showText("Fault", delegate
		{
			base.StartServe(player, newServe: false);
		});
	}

	protected override void RepeatServe(PlayerBase player)
	{
		if (isActiveClient)
		{
			MServeFault mServeFault = default(MServeFault);
			mServeFault.playerTag = player.playerTag;
			MServeFault mServeFault2 = mServeFault;
			mServeFault2.Send(net);
			DoRepeatServe(player);
			Time.timeScale = timeScale;
		}
	}

	protected override void StartServe(PlayerBase player)
	{
		ui.hideMessageButtons();
		UnityEngine.Debug.Log("Start serve");
		lastUpdateTime = 0f;
		base.StartServe(player);
		if (isActiveClient)
		{
			net.Send(0);
		}
		Time.timeScale = timeScale;
	}

	public override void OnRepeatServe(PlayerBase player)
	{
		base.OnRepeatServe(player);
		if (isActiveClient)
		{
			net.Send(4);
			Time.timeScale = timeScale;
		}
	}

	public override void OnCanCompleteSwing(PlayerBase player)
	{
		if (isActiveClient)
		{
			net.Send(5);
		}
	}

	public override void OnPlayerHitBall(PlayerBase player, HitParams p)
	{
		UnityEngine.Debug.Log("Player hit ball active " + isActiveClient + " human " + (player == human));
		lastUpdateTime = 0f;
		if (isActiveClient)
		{
			MHitBall mHitBall = default(MHitBall);
			mHitBall.ballPosition = base.ball.myTransform.position;
			mHitBall.myPosition = human.myTransform.position;
			mHitBall.opponentPosition = remotePlayer.myTransform.position;
			mHitBall.pointOnTable = p.landingPosition;
			mHitBall.height = p.heightOverTheNet;
			mHitBall.timeToLand = p.time;
			mHitBall.penalty = p.penalty;
			mHitBall.spinX = p.spinX;
			mHitBall.pressure = p.pressure;
			mHitBall.jumpInterpolatorTime = p.jumpTimeWhenHit;
			MHitBall mHitBall2 = mHitBall;
			mHitBall2.Send(net);
		}
		base.OnPlayerHitBall(player, p);
		Time.timeScale = timeScale;
	}

	public override void OnThrowServeBallInAir(PlayerBase player)
	{
		base.OnThrowServeBallInAir(player);
		if (isActiveClient)
		{
			net.Send(3);
		}
	}

	private void DoAwardPointTo(PlayerBase player, FaultReason fault)
	{
		base.ballInGame = false;
		base.ball.isBallInGame = false;
		if (!showGame)
		{
			foreach (PlayerBase player2 in players)
			{
				player2.OnPointWon(player);
			}
			score.AwardPointTo(player.playerTag);
			bool isGameWon = score.isGameWon();
			if (isGameWon)
			{
				score.AwardGameTo(score.LeaderTag());
			}
			ui.UpdateScoreBoard(score, MatchController.InitParameters.player1Score, MatchController.InitParameters.player2Score);
			ChangeCrowdIntensity(normalCrowdLevel, 6f);
			PlaySound((player.playerTag != 0) ? cheerBad : cheerGood, 0.15f);
			PlayerBase servingPlayer = base.servingPlayer;
			ui.ScoreStringForPoints(servingPlayer.playerTag, score, out string p, out string p2);
			string text = p + ":" + p2;
			if (isGameWon)
			{
				text = "Games " + score.GamesForPlayer(0) + ":" + score.GamesForPlayer(1);
			}
			ShowMessageButtons(player, fault);
			if (fault == FaultReason.Out)
			{
				PlaySound(ui.outAudio, 0.17f);
				ui.showText("Out", delegate
				{
					FinishAwardPoint(text, isGameWon, player);
				});
			}
			else
			{
				FinishAwardPoint(text, isGameWon, player);
			}
		}
	}

	protected void ShowMessageButtons(PlayerBase player, FaultReason fault)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (player == human)
		{
			if (fault == FaultReason.Out)
			{
				empty = GGFormat.RandomFrom("Better luck next time!", "Unlucky for you!", "Don't give up :)", "I'm Lucky");
				empty2 = GGFormat.RandomFrom(":p", ":D", "What whas that :)", "Ha Ha... :)", "Hilarious!", "That was easy");
			}
			else
			{
				empty = GGFormat.RandomFrom(":)", "Better luck next time!", "No Messing with the Champ!", "Did you see that :)", "You are looking at a winner", "This is how I play", "And the Crowd goes wild!");
				empty2 = GGFormat.RandomFrom(":p", ":D", "Ha Ha... :)", "Hilarious!", "That was easy", "Don't give up :)");
			}
		}
		else if (fault == FaultReason.Out)
		{
			empty = GGFormat.RandomFrom("Not again :)", "You are Lucky :)", "Well Done!", "Oh, No...", "I was so close!");
			empty2 = GGFormat.RandomFrom(":(", "What Ever :)", "AaaaAAaaA...", "Not Fair!");
		}
		else
		{
			empty = GGFormat.RandomFrom("Wow...", "That's a great shot!", "Well Done!", "Sweet", "Nice", "Good Tactic", "Grand!");
			empty2 = GGFormat.RandomFrom("You can play!", ":(", "What Ever :)", "AaaaAAaaA...", "Not again :)", "Not Fair!", "You are Lucky :)");
		}
		ui.showMessageButtons(empty, empty2);
	}

	protected override void AwardPointTo(PlayerBase player, FaultReason fault)
	{
		if (isActiveClient)
		{
			MAwardPoint mAwardPoint = default(MAwardPoint);
			mAwardPoint.playerTag = player.playerTag;
			mAwardPoint.fault = (int)fault;
			MAwardPoint mAwardPoint2 = mAwardPoint;
			mAwardPoint2.Send(net);
			DoAwardPointTo(player, fault);
		}
	}

	private void FinishAwardPoint(string text, bool isGameWon, PlayerBase playerThatWonPoint)
	{
		PlayerBase servingPlayer = base.servingPlayer;
		AudioClip clip = null;
		if (score.isMatchWon())
		{
			clip = ui.matchAudio;
		}
		else if (isGameWon)
		{
			clip = ui.gameAudio;
		}
		string eventText = null;
		if (score.isMatchPoint(0))
		{
			eventText = "Match Point " + MatchController.InitParameters.player1Name;
		}
		else if (score.isGamePoint(servingPlayer.playerTag))
		{
			eventText = "Gamepoint " + ((servingPlayer.playerTag != 0) ? MatchController.InitParameters.player2Name : MatchController.InitParameters.player1Name);
		}
		PlaySound(clip, 0.9f);
		ui.showText(text, delegate
		{
			if (score.isMatchWon())
			{
				OnGameDone();
			}
			else if (!string.IsNullOrEmpty(eventText))
			{
				ui.showText(eventText, delegate
				{
					StartServe(playerThatWonPoint);
				});
			}
			else
			{
				StartServe(playerThatWonPoint);
			}
		});
	}

	public override void PlayerWantsToExit()
	{
		PlayerSettings instance = PlayerSettings.instance;
		instance.Model.multiplayerLoses++;
		instance.Save();
		LoadNextScene();
	}

	public override void LoadNextScene()
	{
		GGNetwork.instance.StopServer();
		ScreenNavigation.instance.LoadMain();
	}

	private void OnGameDone()
	{
		bool flag = score.WinnerTag() == human.playerTag;
		if (opponentRematchStatus == RematchStatus.Unknown)
		{
			int num = (!flag) ? 50 : 200;
			string text = (!flag) ? "Settle the score with a Rematch?" : "You did great in this round, continue with a Rematch?";
			multiplayerDialog.Show((!flag) ? "You Lose" : "You Win", MatchController.InitParameters.player1Score + " - " + MatchController.InitParameters.player2Score + "\n" + text, "Rematch", "Leave", SendRematch, GoToMain, num);
			PlayerSettings instance = PlayerSettings.instance;
			instance.Model.score += num;
			if (flag)
			{
				instance.Model.multiplayerWins++;
			}
			else
			{
				instance.Model.multiplayerLoses++;
			}
			instance.Save();
		}
	}

	private void SendRematch()
	{
		myRematchStatus = RematchStatus.Accepted;
		net.Send(9);
		if (opponentRematchStatus == RematchStatus.Accepted)
		{
			ScreenNavigation.instance.LoadMatch(MatchController.InitParameters);
			Analytics.instance.ReportMultiplayerMatchEnd(rematch: true);
		}
		else if (opponentRematchStatus == RematchStatus.Unknown)
		{
			multiplayerDialog.Show("Rematch Request Sent!", "Waiting for " + MatchController.InitParameters.player2Name + " to Accept Rematch!", string.Empty, "Leave", null, GoToMain);
		}
	}

	private void GoToMain()
	{
		myRematchStatus = RematchStatus.Declined;
		net.Send(10);
		multiplayerDialog.gameObject.SetActive(value: false);
		ScreenNavigation.instance.LoadMain();
		Analytics.instance.ReportMultiplayerMatchEnd(rematch: false);
	}

	private new void Update()
	{
		OnRemoteMessageReceived(net);
		if (isActiveClient && base.activePlayer.shotParams != null)
		{
			timeTillSendBallUpdate -= Time.deltaTime;
			if (timeTillSendBallUpdate <= 0f)
			{
				timeTillSendBallUpdate = base.activePlayer.shotParams.contactTime / updateDivisor;
				MBallTiming mBallTiming = default(MBallTiming);
				mBallTiming.possesionTime = base.activePlayer.shotParams.ballPossesionTime;
				MBallTiming mBallTiming2 = mBallTiming;
				UnityEngine.Debug.Log("SEND TIMING " + mBallTiming2.possesionTime);
				mBallTiming2.Send(net);
			}
		}
		else if (base.activePlayer == remotePlayer && base.activePlayer.shotParams != null && base.activePlayer.serveFlight == null)
		{
			PlayerBase.ShotParameters shotParams = base.activePlayer.shotParams;
			float num = shotParams.contactTime / updateDivisor;
			float num2 = (shotParams.contactTime - lastUpdateTime) * 0.5f;
			if (shotParams.contactTime > lastUpdateTime && shotParams.ballPossesionTime > num && shotParams.contactTime > num)
			{
				float t = (shotParams.ballPossesionTime - lastUpdateTime - num2) / num2;
				Time.timeScale = Mathf.Lerp(Time.timeScale, 0.1f, t);
				if (lastUpdateTime < shotParams.contactTime && shotParams.ballPossesionTime >= shotParams.contactTime)
				{
					Time.timeScale = Mathf.Lerp(Time.timeScale, 0.025f, t);
				}
			}
		}
		if (net.ConnectedPlayers() < 1 && !score.isMatchWon() && !shownNotConnected)
		{
			shownNotConnected = true;
			PlayerSettings instance = PlayerSettings.instance;
			instance.Model.multiplayerWins++;
			instance.Save();
			UIDialog.instance.ShowOk("Player Forfeited", MatchController.InitParameters.player2Name + " has forfeited! You win!", "Ok", OnDisconnected);
		}
	}

	private void OnDisconnected(bool success)
	{
		LoadNextScene();
	}

	private void OnApplicationPause(bool paused)
	{
		net.StopServer();
	}
}
