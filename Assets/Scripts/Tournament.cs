using ProtoModels;
using System.Collections.Generic;
using UnityEngine;

public class Tournament
{
	public class TournamentResult
	{
		public int startingScore;

		public int addedScore;

		public Tournament tournament;

		public int addedCountryScore;
	}

	public enum RoundTag
	{
		QuarterFinals,
		SemiFinals,
		Finals,
		ThirdPlacePlayoff
	}

	private TournamentDAO tournament;

	private int tournamentType;

	protected string Filename => "tournament-" + tournamentType.ToString() + ".bytes";

	public int numPointsInGame => 11;

	public int numGamesInMatch
	{
		get
		{
			if (CurrentRoundTag() == RoundTag.Finals || CurrentRoundTag() == RoundTag.ThirdPlacePlayoff)
			{
				return 2;
			}
			return 1;
		}
	}

	public Tournament(TournamentController.Tournaments t)
	{
		tournamentType = (int)t;
		if (!ProtoIO.LoadFromFileCloudSync(Filename, out tournament))
		{
			tournament = new TournamentDAO();
			tournament.tournamentType = tournamentType;
			Save();
		}
	}

	public void Save()
	{
		ProtoIO.SaveToFileCloudSync(Filename, tournament);
	}

	public bool IsTournamentCreated()
	{
		return tournament != null && tournament.participants != null && tournament.participants.Count > 0;
	}

	public void CreateTournament()
	{
		tournament = new TournamentDAO();
		tournament.participants = new List<LeagueMemberDAO>();
		tournament.tournamentType = tournamentType;
		LeagueMemberDAO leagueMemberDAO = new LeagueMemberDAO();
		leagueMemberDAO.isHuman = true;
		tournament.participants.Add(leagueMemberDAO);
		int num = 0;
		num = (int)((float)(PlayerDeffinition.instance.players.Count - 8) / 3f * (float)tournamentType);
		num = Mathf.Min(num, PlayerDeffinition.instance.players.Count - 8);
		for (int i = 0; i < 7; i++)
		{
			LeagueMemberDAO leagueMemberDAO2 = new LeagueMemberDAO();
			leagueMemberDAO2.difficulty = Mathf.Min(num + i, PlayerDeffinition.instance.players.Count - 1);
			leagueMemberDAO2.points = tournament.participants.Count;
			tournament.participants.Add(leagueMemberDAO2);
		}
		Shuffle(tournament.participants, 2, 2);
		Shuffle(tournament.participants, 4, 4);
		int num2 = 0;
		foreach (LeagueMemberDAO participant in tournament.participants)
		{
			participant.points = num2;
			num2++;
		}
		Save();
	}

	public void Shuffle(List<LeagueMemberDAO> list, int startIndex, int count)
	{
		List<LeagueMemberDAO> list2 = new List<LeagueMemberDAO>();
		for (int i = 0; i < count; i++)
		{
			int num = i + startIndex;
			if (num < list.Count)
			{
				list2.Add(list[num]);
			}
		}
		for (int j = 0; j < count; j++)
		{
			int index = j + startIndex;
			if (list2.Count == 0)
			{
				break;
			}
			int index2 = Random.Range(0, list2.Count) % list2.Count;
			list[index] = list2[index2];
			int index3 = list2.Count - 1;
			list2[index2] = list2[index3];
			list2.RemoveAt(index3);
		}
	}

	public bool isTournamentComplete()
	{
		if (tournament == null)
		{
			return false;
		}
		LeagueMemberDAO leagueMemberDAO = humanPlayer();
		return tournament.machesPlayed > 2 || (tournament.machesPlayed >= 1 && leagueMemberDAO.wins == 0);
	}

	public List<LeagueMemberDAO> Participants()
	{
		return tournament.participants;
	}

	public int CurrentPlayerNumPoints()
	{
		switch (CurrentRoundTag())
		{
		case RoundTag.QuarterFinals:
			return 8;
		case RoundTag.SemiFinals:
			return 11;
		case RoundTag.ThirdPlacePlayoff:
			return 15;
		default:
			return 15;
		}
	}

	public int TournamentType()
	{
		return tournamentType;
	}

	public LeagueMemberDAO NextOpponent()
	{
		if (!IsTournamentCreated())
		{
			return null;
		}
		LeagueMemberDAO leagueMemberDAO = humanPlayer();
		List<LeagueMemberDAO> list = ParticipantsForRound(leagueMemberDAO.wins, exact: true);
		if (list.Count < 2)
		{
			return null;
		}
		return list[1];
	}

	public int CurrentRound()
	{
		if (tournament == null)
		{
			return 0;
		}
		return tournament.machesPlayed;
	}

	public int humanRanking()
	{
		if (!isTournamentComplete())
		{
			return 8;
		}
		LeagueMemberDAO leagueMemberDAO = humanPlayer();
		if (leagueMemberDAO.wins == 0)
		{
			return 7;
		}
		if (leagueMemberDAO.wins == 1)
		{
			return 3;
		}
		if (leagueMemberDAO.wins == 2 && leagueMemberDAO.prevRank == 1)
		{
			return 2;
		}
		if (leagueMemberDAO.wins == 2 && leagueMemberDAO.prevRank == 2)
		{
			return 1;
		}
		if (leagueMemberDAO.wins == 3)
		{
			return 0;
		}
		return 8;
	}

	public int price()
	{
		switch (tournamentType)
		{
		case 1:
			return 5;
		case 2:
			return 15;
		default:
			return 0;
		}
	}

	public LeagueMemberDAO humanPlayer()
	{
		if (tournament == null)
		{
			return null;
		}
		return tournament.participants[0];
	}

	public TournamentResult ReportHumanScore(bool isHumanWin)
	{
		LeagueMemberDAO leagueMemberDAO = humanPlayer();
		List<LeagueMemberDAO> list = ParticipantsForRound(leagueMemberDAO.wins, exact: true, usePrevRound: false);
		leagueMemberDAO.prevRank = leagueMemberDAO.wins;
		LeagueMemberDAO leagueMemberDAO2 = list[1];
		leagueMemberDAO2.prevRank = leagueMemberDAO2.wins;
		if (isHumanWin)
		{
			leagueMemberDAO.wins++;
		}
		else
		{
			leagueMemberDAO2.wins++;
		}
		int num = humanRanking();
		if (isTournamentComplete() && num < 3)
		{
			ScoreDAO scoreDAO = TournamentController.instance.scoreForTournament(tournament.tournamentType);
			switch (num)
			{
			case 0:
				scoreDAO.gold++;
				break;
			case 1:
				scoreDAO.silver++;
				break;
			case 2:
				scoreDAO.bronze++;
				break;
			}
			TournamentController.instance.Save();
		}
		Save();
		PlayerSettings instance = PlayerSettings.instance;
		int score = instance.Model.score;
		int num2 = 100;
		if (isTournamentComplete())
		{
			num2 = ((humanRanking() >= 3) ? 500 : ((tournamentType + 3 - humanRanking()) * 1000));
		}
		instance.Model.score += num2;
		instance.Save();
		TournamentResult tournamentResult = new TournamentResult();
		tournamentResult.tournament = this;
		tournamentResult.startingScore = score;
		tournamentResult.addedScore = num2;
		tournamentResult.addedCountryScore = BehaviourSingleton<OnlineRankings>.instance.ReportGameDone(num2);
		return tournamentResult;
	}

	public string TokenFilenameForCurrentTournament()
	{
		TournamentController.Tournaments tournaments = TournamentController.Tournaments.Friendly;
		if (tournament != null)
		{
			tournaments = (TournamentController.Tournaments)tournament.tournamentType;
		}
		return TournamentController.TokenFilenameForTournament(tournaments);
	}

	public string NameForCurrentRound()
	{
		switch (CurrentRoundTag())
		{
		case RoundTag.QuarterFinals:
			return "Quarter Finals";
		case RoundTag.SemiFinals:
			return "Semi Finals";
		case RoundTag.ThirdPlacePlayoff:
			return "Third Place Playoffs";
		default:
			return "Finals";
		}
	}

	public RoundTag CurrentRoundTag()
	{
		LeagueMemberDAO leagueMemberDAO = humanPlayer();
		int num = CurrentRound();
		switch (num)
		{
		case 0:
			return RoundTag.QuarterFinals;
		case 1:
			return RoundTag.SemiFinals;
		case 2:
			if (leagueMemberDAO.wins == 2)
			{
				return RoundTag.Finals;
			}
			break;
		}
		if (num == 2 && leagueMemberDAO.wins < 2)
		{
			return RoundTag.ThirdPlacePlayoff;
		}
		return RoundTag.QuarterFinals;
	}

	public string CurrentLevelLocation()
	{
		switch (CurrentRoundTag())
		{
		case RoundTag.QuarterFinals:
			return tournament.quarterName;
		case RoundTag.SemiFinals:
			return tournament.semiName;
		case RoundTag.Finals:
			return tournament.finalsName;
		default:
			return tournament.tirdPlaceName;
		}
	}

	public void AdvanceToNextRound()
	{
		if (tournament == null || isTournamentComplete())
		{
			return;
		}
		List<LeagueMemberDAO> list = ParticipantsForRound(tournament.machesPlayed, exact: true);
		if (list.Count % 2 != 0)
		{
			return;
		}
		for (int i = 0; i < list.Count / 2; i++)
		{
			LeagueMemberDAO leagueMemberDAO = list[2 * i];
			LeagueMemberDAO leagueMemberDAO2 = list[2 * i + 1];
			if (!leagueMemberDAO.isHuman && !leagueMemberDAO2.isHuman)
			{
				LeagueMemberDAO leagueMemberDAO3 = (leagueMemberDAO.difficulty <= leagueMemberDAO2.difficulty) ? leagueMemberDAO2 : leagueMemberDAO;
				leagueMemberDAO3.prevRank = leagueMemberDAO3.wins;
				leagueMemberDAO3.wins++;
				LeagueMemberDAO leagueMemberDAO4 = (leagueMemberDAO3 != leagueMemberDAO) ? leagueMemberDAO : leagueMemberDAO2;
				leagueMemberDAO4.prevRank = leagueMemberDAO4.wins;
			}
		}
		tournament.machesPlayed++;
		Save();
	}

	public List<LeagueMemberDAO> ParticipantsForRound(int round, bool exact)
	{
		return ParticipantsForRound(round, exact, usePrevRound: false);
	}

	public List<LeagueMemberDAO> ParticipantsForRound(int round, bool exact, bool usePrevRound)
	{
		List<LeagueMemberDAO> list = new List<LeagueMemberDAO>();
		foreach (LeagueMemberDAO participant in tournament.participants)
		{
			int num = (!usePrevRound) ? participant.wins : participant.prevRank;
			if (exact)
			{
				if (num == round)
				{
					list.Add(participant);
				}
			}
			else if (num >= round)
			{
				list.Add(participant);
			}
		}
		return list;
	}
}
