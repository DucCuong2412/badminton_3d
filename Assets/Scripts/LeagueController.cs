using ProtoModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LeagueController
{
	public class Player
	{
		public string name;

		public int flag;

		public int difficulty;

		public Player(string name, int flag, int difficulty)
		{
			this.name = name;
			this.flag = flag;
			this.difficulty = difficulty;
		}
	}

	public class LeagueGameResult
	{
		public bool isWin;

		public int ballsWon;

		public int startingScore;

		public int addedScore;

		public int addedCountryScore;
	}

	private static string Filename = "league.bytes";

	private static LeagueController instance_;

	public int numGamesInSet = 1;

	public int numPoints = 11;

	private int minPoints;

	private LeagueDAO model;

	private int maxPoints => numPoints * numGamesInSet;

	public static LeagueController instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new LeagueController();
				instance_.Init();
			}
			return instance_;
		}
	}

	private void Init()
	{
		if (!ProtoIO.LoadFromFileCloudSync(Filename, out model))
		{
			model = new LeagueDAO();
			ProtoIO.SaveToFileCloudSync(Filename, model);
		}
		if (model == null)
		{
			model = new LeagueDAO();
			Save();
		}
	}

	public void ResetLeague()
	{
		model = new LeagueDAO();
		Save();
	}

	public bool isLeagueInProgress()
	{
		if (model == null || model.participants == null)
		{
			return false;
		}
		return model.participants.Count > 0;
	}

	public void Save()
	{
		ProtoIO.SaveToFileCloudSync(Filename, model);
	}

	public void ResolvePotentialConflictsWithCloudData()
	{
		GGFileIOCloudSync instance = GGFileIOCloudSync.instance;
		if (!instance.isInConflict(Filename))
		{
			return;
		}
		GGFileIO cloudFileIO = instance.GetCloudFileIO();
		LeagueDAO leagueDAO = null;
		if (ProtoIO.LoadFromFile<ProtoSerializer, LeagueDAO>(Filename, cloudFileIO, out leagueDAO) && leagueDAO.participants != null && leagueDAO.participants.Count != 0)
		{
			if (model == null)
			{
				model = new LeagueDAO();
			}
			if (leagueDAO.leagueLevel > model.leagueLevel || (model.leagueLevel == leagueDAO.leagueLevel && leagueDAO.machesPlayed > model.machesPlayed))
			{
				model.leagueLevel = leagueDAO.leagueLevel;
				model.machesPlayed = leagueDAO.machesPlayed;
				model.participants = new List<LeagueMemberDAO>(leagueDAO.participants);
			}
			ProtoIO.SaveToFile<ProtoSerializer, LeagueDAO>(Filename, cloudFileIO, model);
		}
	}

	public int MemberCount()
	{
		return model.participants.Count;
	}

	public int RankForMember(LeagueMemberDAO member)
	{
		List<LeagueMemberDAO> list = CreateListSortedByRank();
		return list.IndexOf(member);
	}

	public int PlacesThatAdvance()
	{
		return 3;
	}

	public string LeagueName()
	{
		switch (LeagueLevel())
		{
		case 0:
			return "C League";
		case 1:
			return "B League";
		case 2:
			return "A League";
		default:
			return "League " + (LeagueLevel() - 1);
		}
	}

	public int LeagueLevel()
	{
		if (model == null)
		{
			return 0;
		}
		return model.leagueLevel;
	}

	public void AdvanceLeague()
	{
		if (model != null)
		{
			LeagueMemberDAO leagueMemberDAO = HumanPlayer();
			CreateLeagueWithFlag(leagueMemberDAO.flag, leagueMemberDAO.name, model.leagueLevel + 1);
		}
		else
		{
			CreateLeagueWithFlag(0);
		}
	}

	public bool shouldAdvance()
	{
		if (!isLeagueInProgress())
		{
			return false;
		}
		if (!isLeagueOver())
		{
			return false;
		}
		LeagueMemberDAO leagueMemberDAO = HumanPlayer();
		int num = RankForMember(HumanPlayer());
		return num < PlacesThatAdvance();
	}

	public void CreateLeagueWithFlag(int flag, string name = "You", int level = 0)
	{
		model = new LeagueDAO();
		model.leagueLevel = level;
		UnityEngine.Debug.Log("League Level " + level);
		model.participants = new List<LeagueMemberDAO>();
		LeagueMemberDAO leagueMemberDAO = new LeagueMemberDAO();
		leagueMemberDAO.isHuman = true;
		leagueMemberDAO.flag = flag;
		leagueMemberDAO.name = name;
		model.participants.Add(leagueMemberDAO);
		PlayerDeffinition instance = PlayerDeffinition.instance;
		int num = 7;
		int num2 = Mathf.Clamp(1 + num * level - PlacesThatAdvance(), 1, instance.players.Count - num);
		for (int i = 0; i < num; i++)
		{
			LeagueMemberDAO leagueMemberDAO2 = new LeagueMemberDAO();
			int num3 = Mathf.Clamp(num2, 0, instance.players.Count - 1);
			PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(num3);
			leagueMemberDAO2.name = playerDef.name;
			leagueMemberDAO2.isHuman = false;
			leagueMemberDAO2.flag = (int)playerDef.flag;
			leagueMemberDAO2.difficulty = num3;
			model.participants.Add(leagueMemberDAO2);
			num2++;
		}
		List<LeagueMemberDAO> list = CreateListSortedByRank();
		for (int j = 0; j < list.Count; j++)
		{
			list[j].prevRank = j;
		}
		model.creationTime = DateTime.Now.Ticks;
		Save();
	}

	public LeagueMemberDAO HumanPlayer()
	{
		foreach (LeagueMemberDAO participant in model.participants)
		{
			if (participant.isHuman)
			{
				return participant;
			}
		}
		LeagueMemberDAO leagueMemberDAO = new LeagueMemberDAO();
		leagueMemberDAO.name = "You";
		leagueMemberDAO.flag = 0;
		return leagueMemberDAO;
	}

	public void SetFlag(int flag)
	{
		if (!isLeagueInProgress())
		{
			CreateLeagueWithFlag(flag);
			return;
		}
		HumanPlayer().flag = flag;
		Save();
	}

	public int CurrentMatch()
	{
		return model.machesPlayed;
	}

	public int DaysSinceLeagueStart()
	{
		if (model.creationTime == 0)
		{
			return -1;
		}
		try
		{
			return (int)DateTime.Now.Subtract(new DateTime(model.creationTime)).TotalDays;
		}
		catch
		{
		}
		return -1;
	}

	public LeagueMemberDAO NextOpponent()
	{
		return model.participants[Mathf.Min(model.machesPlayed + 1, model.participants.Count - 1)];
	}

	public int MatchesPlayed()
	{
		if (model == null)
		{
			return 0;
		}
		return model.machesPlayed;
	}

	public int TotalMatchesForLeague()
	{
		if (model == null || model.participants == null)
		{
			return 0;
		}
		return model.participants.Count - 1;
	}

	public int MatchesRemaining()
	{
		return TotalMatchesForLeague() - model.machesPlayed;
	}

	public bool isLeagueOver()
	{
		if (model == null)
		{
			return false;
		}
		return model.machesPlayed >= TotalMatchesForLeague();
	}

	public int OpponentIndexForRound(int round, int playerIndex)
	{
		List<LeagueMemberDAO> participants = model.participants;
		if (playerIndex >= participants.Count - 1)
		{
			UnityEngine.Debug.Log("Can't calculate that index");
			return 0;
		}
		int num = (participants.Count - playerIndex) % participants.Count;
		if (playerIndex == 0)
		{
			num = 1;
		}
		int result = num;
		for (int i = 0; i <= round; i++)
		{
			result = ((i != playerIndex - 1) ? num : (num = 0));
			if (num == playerIndex)
			{
				result = participants.Count - 1;
			}
			num = (num + 1) % participants.Count;
		}
		return result;
	}

	public static LeagueMemberDAO WinnerOfMatch(LeagueMemberDAO member1, LeagueMemberDAO member2)
	{
		float num = WinChanceForDifficulty(member1.difficulty);
		float num2 = WinChanceForDifficulty(member2.difficulty);
		return (!(UnityEngine.Random.Range(0f, num + num2) > num)) ? member1 : member2;
	}

	public static float WinChanceForDifficulty(int difficulty)
	{
		switch (difficulty)
		{
		case 2:
			return 10f;
		case 1:
			return 5f;
		default:
			return 2.5f;
		}
	}

	public LeagueGameResult ReportGameScore(bool isPlayerWinner, int humanPoints, int opponentPoints)
	{
		if (!isLeagueInProgress())
		{
			return new LeagueGameResult();
		}
		if (isPlayerWinner)
		{
			humanPoints = Mathf.Min(humanPoints, maxPoints);
			opponentPoints = Mathf.Min(opponentPoints, humanPoints - 1);
		}
		else
		{
			opponentPoints = Mathf.Min(opponentPoints, maxPoints);
			humanPoints = Mathf.Min(humanPoints, opponentPoints - 1);
		}
		int score = PlayerSettings.instance.Model.score;
		int num = Mathf.Max(model.machesPlayed - 1, 0);
		LeagueMemberDAO leagueMemberDAO = model.participants[Mathf.Min(num + 1, model.participants.Count - 1)];
		leagueMemberDAO.points += opponentPoints;
		leagueMemberDAO.wins--;
		leagueMemberDAO.points -= maxPoints;
		LeagueMemberDAO leagueMemberDAO2 = HumanPlayer();
		leagueMemberDAO2.points += humanPoints;
		LeagueMemberDAO leagueMemberDAO3 = (!isPlayerWinner) ? leagueMemberDAO : leagueMemberDAO2;
		leagueMemberDAO3.wins++;
		Save();
		int num2 = (!isPlayerWinner) ? 10 : 15;
		PlayerSettings.instance.Model.coins += num2;
		PlayerSettings.instance.Save();
		int num3 = ((!isPlayerWinner) ? 100 : 200) + 5 * (TotalMatchesForLeague() - RankForMember(leagueMemberDAO2));
		if (isLeagueOver())
		{
			num3 += 100 * (TotalMatchesForLeague() - RankForMember(leagueMemberDAO2));
		}
		BehaviourSingleton<Social>.instance.submitScore(score + num3);
		LeagueGameResult leagueGameResult = new LeagueGameResult();
		leagueGameResult.ballsWon = num2;
		leagueGameResult.isWin = isPlayerWinner;
		leagueGameResult.startingScore = score;
		leagueGameResult.addedScore = num3;
		leagueGameResult.addedCountryScore = BehaviourSingleton<OnlineRankings>.instance.ReportGameDone(num3);
		return leagueGameResult;
	}

	public bool isNextMatchActive()
	{
		return DateTime.Now > new DateTime(model.nextMatchActiveTime);
	}

	public TimeSpan TimeTillNextMatchActive()
	{
		if (isNextMatchActive())
		{
			return new TimeSpan(0L);
		}
		return new DateTime(model.nextMatchActiveTime).Subtract(DateTime.Now);
	}

	public void SetActiveTime(int hours, int minutes)
	{
		if (model != null)
		{
			DateTime now = DateTime.Now;
			TimeSpan value = new TimeSpan(hours, minutes, 0);
			DateTime dateTime = now.Add(value);
			model.nextMatchActiveTime = dateTime.Ticks;
		}
	}

	public void AdvanceToNextRound()
	{
		UnityEngine.Debug.Log("Advance To Next Round");
		if (model.machesPlayed >= model.participants.Count)
		{
			return;
		}
		List<LeagueMemberDAO> list = CreateListSortedByRank();
		for (int i = 0; i < list.Count; i++)
		{
			list[i].prevRank = i;
		}
		int hours = 0;
		int minutes = 30;
		if (ConfigBase.instance.isProVersionEnabled)
		{
			hours = (minutes = 0);
		}
		UnityEngine.Debug.Log("Time decided");
		SetActiveTime(hours, minutes);
		UnityEngine.Debug.Log("Time Added");
		List<LeagueMemberDAO> list2 = new List<LeagueMemberDAO>();
		for (int j = 0; j < model.participants.Count - 1; j++)
		{
			int index = OpponentIndexForRound(model.machesPlayed, j);
			LeagueMemberDAO leagueMemberDAO = model.participants[j];
			LeagueMemberDAO leagueMemberDAO2 = model.participants[index];
			if (!list2.Contains(leagueMemberDAO) && !list2.Contains(leagueMemberDAO2))
			{
				list2.Add(leagueMemberDAO);
				list2.Add(leagueMemberDAO2);
				if (leagueMemberDAO.isHuman || leagueMemberDAO2.isHuman)
				{
					LeagueMemberDAO leagueMemberDAO3 = (!leagueMemberDAO.isHuman) ? leagueMemberDAO : leagueMemberDAO2;
					leagueMemberDAO3.wins++;
					leagueMemberDAO3.points += maxPoints;
				}
				else
				{
					LeagueMemberDAO leagueMemberDAO4 = WinnerOfMatch(leagueMemberDAO, leagueMemberDAO2);
					LeagueMemberDAO leagueMemberDAO5 = (leagueMemberDAO4 != leagueMemberDAO) ? leagueMemberDAO : leagueMemberDAO2;
					leagueMemberDAO4.wins++;
					leagueMemberDAO5.points += UnityEngine.Random.Range(minPoints, maxPoints - 1);
					leagueMemberDAO4.points += maxPoints;
				}
			}
		}
		model.machesPlayed++;
		UnityEngine.Debug.Log(model.machesPlayed);
		Save();
	}

	public List<LeagueMemberDAO> CreateListSortedByRank()
	{
		List<LeagueMemberDAO> list = new List<LeagueMemberDAO>();
		list.AddRange(model.participants);
		list.Sort(delegate(LeagueMemberDAO a, LeagueMemberDAO b)
		{
			if (a == b)
			{
				return 0;
			}
			if (a.wins > b.wins)
			{
				return -1;
			}
			if (a.wins < b.wins)
			{
				return 1;
			}
			if (a.points > b.points)
			{
				return -1;
			}
			if (a.points < b.points)
			{
				return 1;
			}
			if (a.isHuman)
			{
				UnityEngine.Debug.Log(a.name + " vs " + b.name);
				return -1;
			}
			if (b.isHuman)
			{
				UnityEngine.Debug.Log(a.name + " vs " + b.name);
				return 1;
			}
			return 1;
		});
		return list;
	}
}
