using System;
using UnityEngine;

public class GameMatchController : MatchController
{
	private bool canStartShowServe = true;

	private new void Awake()
	{
		base.Awake();
		if (MatchController.InitParameters.gameMode == GameMode.SplitScreen)
		{
			StartSplitScreenGame();
		}
		else
		{
			StartSinglePlayerGame();
		}
		if (MatchController.InitParameters.gameMode == GameMode.GameModeCareer && MatchController.InitParameters.opponent != null)
		{
			Analytics.instance.ReportCareerMatchStart(MatchController.InitParameters.opponent);
		}
		else if (MatchController.InitParameters.gameMode == GameMode.SplitScreen)
		{
			Analytics.instance.ReportSplitScreen();
		}
	}

	private void StartSplitScreenGame()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(characterPrefab);
		HumanPlayer humanPlayer = gameObject.AddComponent<HumanPlayer>();
		GameObject gameObject2 = UnityEngine.Object.Instantiate(characterPrefab);
		HumanPlayer humanPlayer2 = gameObject2.AddComponent<HumanPlayer>();
		humanPlayer.Init(this, players.Count, -1, cameraController, null);
		players.Add(humanPlayer);
		GameObject gameObject3 = UnityEngine.Object.Instantiate(cameraController.gameObject);
		CameraController component = gameObject3.GetComponent<CameraController>();
		base.cameras.Add(component);
		Camera component2 = cameraController.GetComponent<Camera>();
		Rect rect = component2.rect;
		rect.width = 0.5f;
		component2.rect = rect;
		Camera component3 = component.GetComponent<Camera>();
		rect = component3.rect;
		rect.width = 0.5f;
		rect.x = 0.5f;
		component3.rect = rect;
		component.up = Vector3.right;
		component3.cullingMask = ((component3.cullingMask & ~(1 << LayerMask.NameToLayer("Camera1Layer"))) | (1 << LayerMask.NameToLayer("Camera2Layer")));
		cameraController.up = Vector3.left;
		Transform transform = component.transform;
		Vector3 position = transform.position;
		position.z *= -1f;
		transform.position = position;
		transform.rotation = Quaternion.LookRotation(Vector3.zero - position, Vector3.down);
		humanPlayer2.Init(this, players.Count, 1, component, null);
		players.Add(humanPlayer2);
		component2.fieldOfView *= 1.4f;
		component3.fieldOfView *= 1.4f;
		cameraController.SetIdle();
		component.SetIdle();
	}

	private void StartSinglePlayerGame()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(characterPrefab);
		HumanPlayer humanPlayer = gameObject.AddComponent<HumanPlayer>();
		GameObject gameObject2 = UnityEngine.Object.Instantiate(characterPrefab);
		AiPlayer aiPlayer = gameObject2.AddComponent<AiPlayer>();
		humanPlayer.Init(this, players.Count, -1, cameraController, aiPlayer);
		players.Add(humanPlayer);
		PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(MatchController.InitParameters.playerDefIndex);
		UnityEngine.Debug.Log("Name " + playerDef.name);
		aiPlayer.Init(this, players.Count, 1, playerDef, humanPlayer, MatchController.InitParameters.player2Flag);
		players.Add(aiPlayer);
	}

	protected void Start()
	{
		Time.timeScale = timeScale;
		ui.Init(this, MatchController.InitParameters.player1Name, MatchController.InitParameters.player1Flag, MatchController.InitParameters.player2Name, MatchController.InitParameters.player2Flag);
		ui.onPause += delegate
		{
			base.isPaused = true;
			Time.timeScale = 0f;
			GC.Collect(0);
		};
		ui.onResume += delegate
		{
			base.isPaused = false;
			Time.timeScale = timeScale;
		};
		ui.onRateComplete += delegate
		{
			base.isPaused = false;
			Time.timeScale = 1f;
			if (score.isMatchWon())
			{
				LoadNextScene();
			}
		};
		ui.onVs += StartGameFromVs;
		string startText = GetStartText();
		if (MatchController.InitParameters.gameMode == GameMode.SplitScreen)
		{
			StartGame();
			ui.SetCameraSetup(0);
		}
		else if (!string.IsNullOrEmpty(startText))
		{
			base.activePlayerTag = 1;
			StartShowGame();
			ui.ShowVS(startText);
			cameraController.RotateAround(new Vector3(0f, table.netHeight * 10f, 0f), new Vector3(0f, table.netHeight, 0f), table.halphLength * 3f, (float)Math.PI * -2f / 3f, -(float)Math.PI / 3f, 16f);
		}
		else
		{
			StartGame();
		}
	}

	private string GetStartText()
	{
		if (MatchController.InitParameters.gameMode == GameMode.GameModeCareer && MatchController.InitParameters.opponent != null)
		{
			return MatchController.InitParameters.opponent.description;
		}
		if (MatchController.InitParameters.gameMode == GameMode.GameModeLeague)
		{
			LeagueController instance = LeagueController.instance;
			if (instance.MatchesPlayed() == 1)
			{
				return "Welcome to the League. First match is very important, winner takes 15 (Ball)!";
			}
			if (instance.MatchesPlayed() == 2)
			{
				return "This is you second match, anything is possible, just stay focused!";
			}
			if (instance.MatchesPlayed() == 3)
			{
				return "Win this match to get more balls!";
			}
			if (instance.MatchesPlayed() == 4)
			{
				return "This is your fourth match! Keep up the good play!";
			}
			if (instance.MatchesPlayed() == 5)
			{
				return "You are doing great! Don't lose focus!";
			}
			if (instance.MatchesPlayed() == 6)
			{
				return "You are close to the final match! Well done!";
			}
			if (instance.MatchesPlayed() == 7)
			{
				return "Congratulations, this is you last match! You are a star!";
			}
			return string.Empty;
		}
		if (MatchController.InitParameters.gameMode == GameMode.GameModeTournament && MatchController.InitParameters.tournament != null)
		{
			Tournament tournament = MatchController.InitParameters.tournament;
			string text = string.Empty;
			if (tournament.CurrentRoundTag() == Tournament.RoundTag.QuarterFinals)
			{
				if (tournament.TournamentType() == 0)
				{
					text = "Wellcome to Friendly tournament!";
				}
				else if (tournament.TournamentType() == 1)
				{
					text = "Wellcome to the World Championship!";
				}
				else if (tournament.TournamentType() == 2)
				{
					text = "Congratulations, World Champion! Wellcome to All Stars tournament!";
				}
				text += " Win to advance to semi-finals!";
			}
			else if (tournament.CurrentRoundTag() == Tournament.RoundTag.SemiFinals)
			{
				text += "Victory brings you to the finals! If you lose, you'll play in the third place playoffs";
			}
			else if (tournament.CurrentRoundTag() == Tournament.RoundTag.ThirdPlacePlayoff)
			{
				text += "Try to win the 3rd place! Good luck!";
			}
			else if (tournament.CurrentRoundTag() == Tournament.RoundTag.Finals)
			{
				text += "You have reached the Finals! Win to become the champion!";
			}
			return text;
		}
		return string.Empty;
	}

	private void StartGameFromVs()
	{
		cameraController.MoveTo(cameraController.idlePosition, new Vector3(0f, 0f, 0f), 2f);
		cameraController.rotationSpeed = 50f;
		StartGame();
	}

	private void StopShowGame()
	{
		showGame = false;
		foreach (PlayerBase player in players)
		{
			player.showGame = false;
		}
	}

	private void StartGame()
	{
		int playerThatStarts = 0;
		base.activePlayerTag = playerThatStarts;
		canStartShowServe = false;
		foreach (PlayerBase player in players)
		{
			player.showGame = false;
			player.OnGameStart();
		}
		ui.showText(MatchController.InitParameters.numGamesInSet + " Game, " + MatchController.InitParameters.numPointsInGame + " Points Match", delegate
		{
			StopShowGame();
			foreach (PlayerBase player2 in players)
			{
				player2.OnPointWon(null);
			}
			base.activePlayerTag = playerThatStarts;
			StartServe(base.activePlayer);
		});
	}

	public override void LoadNextScene()
	{
		if (!base.isGameDone && MatchController.InitParameters.gameMode == GameMode.GameModeTournament && MatchController.InitParameters.tournament != null)
		{
			Tournament tournament = MatchController.InitParameters.tournament;
			tournament.AdvanceToNextRound();
			tournament.ReportHumanScore(isHumanWin: false);
		}
		MainNavigationManager.MainParameters mainParameters = new MainNavigationManager.MainParameters();
		if (MatchController.InitParameters.gameMode == GameMode.GameModeTournament)
		{
			mainParameters.loadTournament = MatchController.InitParameters.tournament;
		}
		MainNavigationManager.InitParameters = mainParameters;
		ScreenNavigation.instance.LoadMain();
	}

	private int StarsForTournamentRank(int rank)
	{
		return Mathf.Max(0, 3 - rank);
	}

	private void OnGameDone()
	{
		base.isGameDone = true;
		PlayerSettings instance = PlayerSettings.instance;
		int num = score.TotalScoreForPlayer(0);
		int num2 = score.TotalScoreForPlayer(1);
		bool flag = score.WinnerTag() == 0;
		int num3 = num - num2;
		int scoreDifference = score.totalScores[0] - score.totalScores[1];
		if (MatchController.InitParameters.gameMode == GameMode.GameModeCareer)
		{
			CareerGameMode instance2 = CareerGameMode.instance;
			CareerGameMode.GameComplete outcome = instance2.CereerGameComplete(flag, scoreDifference, MatchController.InitParameters.opponent);
			ui.winDialog.ShowCareerDialog(outcome, OnGameDoneComplete);
			Analytics.instance.ReportCareerPlayerPassed(outcome.opponent, outcome.playerPassed, outcome.isGroupPassed);
		}
		else if (MatchController.InitParameters.gameMode == GameMode.GameModeLeague)
		{
			LeagueController instance3 = LeagueController.instance;
			if (!instance3.isLeagueInProgress())
			{
				instance3.CreateLeagueWithFlag(MatchController.InitParameters.player1Flag, MatchController.InitParameters.player1Name);
			}
			LeagueController.LeagueGameResult result = instance3.ReportGameScore(flag, score.totalScores[0], score.totalScores[1]);
			ui.winDialog.ShowLeagueDialog(result, OnGameDoneComplete);
		}
		else if (MatchController.InitParameters.gameMode == GameMode.SplitScreen)
		{
			ui.showTextsForPlayers((num <= num2) ? "You Lose" : "You Win", (num2 <= num) ? "You Lose" : "You Win", OnSplitScreenComplete);
		}
		else if (MatchController.InitParameters.gameMode == GameMode.GameModeTournament)
		{
			Tournament tournament = MatchController.InitParameters.tournament;
			tournament.AdvanceToNextRound();
			Tournament.TournamentResult result2 = tournament.ReportHumanScore(flag);
			ui.winDialog.ShowTournamentDialog(result2, OnGameDoneComplete);
			Analytics.instance.ReportTournamentGameEnd(MatchController.InitParameters.tournament);
		}
	}

	private void OnSplitScreenComplete()
	{
		UIDialog.instance.ShowYesNo("Game Complete", "Score " + score.TotalScoreForPlayer(0) + ":" + score.TotalScoreForPlayer(1), "Play Again", "Menu", delegate(bool success)
		{
			if (success)
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
			}
			else
			{
				ScreenNavigation.instance.LoadMain();
			}
		});
	}

	private void OnGameDoneComplete()
	{
		LoadNextScene();
	}

	protected override void RepeatServe(PlayerBase player)
	{
		base.RepeatServe(player);
		PlaySound(ui.foulAudio, 0.5f);
		ui.showText("Fault", delegate
		{
			base.StartServe(player, newServe: false);
		});
	}

	protected override void StartServe(PlayerBase player)
	{
		ChangeCrowdIntensity(quietCrowdLevel, 0.4f);
		ui.UpdateServe(player.playerTag);
		base.StartServe(player);
	}

	public override void OnPlayerHitBall(PlayerBase player, HitParams p)
	{
		base.OnPlayerHitBall(player, p);
		if (!showGame)
		{
			float num = Mathf.Abs(base.ball.penalty);
			if (num < 0.1f)
			{
				PlaySound(excitement, (1f - num) * 0.05f);
				ChangeCrowdIntensity(normalCrowdLevel, 4f);
			}
			else
			{
				ChangeCrowdIntensity((1f - num) * (normalCrowdLevel - quietCrowdLevel) + quietCrowdLevel, 1f);
			}
		}
	}

	protected override void AwardPointTo(PlayerBase player, FaultReason fault)
	{
		base.ballInGame = false;
		base.ball.isBallInGame = false;
		foreach (PlayerBase player2 in players)
		{
			player2.OnPointWon(player);
		}
		if (showGame)
		{
			if (canStartShowServe)
			{
				StartServe(players[1]);
			}
			return;
		}
		score.AwardPointTo(player.playerTag);
		bool isGameWon = score.isGameWon();
		if (isGameWon)
		{
			score.AwardGameTo(score.LeaderTag());
		}
		ui.UpdateScoreBoard(score);
		ChangeCrowdIntensity(normalCrowdLevel, 6f);
		PlaySound((player.playerTag != 0) ? cheerBad : cheerGood, 0.15f);
		ui.ScoreStringForPoints(0, score, out string p, out string p2);
		string text = p + ":" + p2;
		UnityEngine.Debug.Log("Total scores " + score.totalScores[0] + " 1 " + score.totalScores[1]);
		if (isGameWon)
		{
			text = "Games " + score.GamesForPlayer(0) + ":" + score.GamesForPlayer(1);
		}
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
}
