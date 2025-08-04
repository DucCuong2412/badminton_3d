using ProtoModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentController
{
	public enum Tournaments
	{
		Friendly,
		WorldChampionship,
		Olympics
	}

	private static string ScoresFilename = "tournamentScores.bytes";

	private static TournamentController instance_;

	public TournamentScoreDAO scores;

	public Dictionary<Tournaments, Tournament> tournaments = new Dictionary<Tournaments, Tournament>();

	public static TournamentController instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new TournamentController();
				instance_.Init();
			}
			return instance_;
		}
	}

	public static string NameForPosition(int position)
	{
		switch (position)
		{
		case 0:
			return "Gold Medal";
		case 1:
			return "Silver Medal";
		case 2:
			return "Bronze Medal";
		default:
			return position + 1 + " Position";
		}
	}

	public static string tournamentName(Tournaments tournament)
	{
		switch (tournament)
		{
		case Tournaments.WorldChampionship:
			return "World Championship";
		case Tournaments.Olympics:
			return "All Star Tournament";
		default:
			return "Friendly Tournament";
		}
	}

	public static string TokenFilenameForTournament(Tournaments tournament)
	{
		switch (tournament)
		{
		case Tournaments.WorldChampionship:
			return "world-button.png";
		case Tournaments.Olympics:
			return "olympic-button.png";
		default:
			return "friendly-button.png";
		}
	}

	private void Init()
	{
		if (!ProtoIO.LoadFromFileCloudSync(ScoresFilename, out scores))
		{
			scores = new TournamentScoreDAO();
			scores.participants = new List<ScoreDAO>();
		}
		ScoreDAO scoreDAO = null;
		IEnumerator enumerator = Enum.GetValues(typeof(Tournaments)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Tournaments tournaments = (Tournaments)enumerator.Current;
				if (scoreForTournament((int)tournaments) == null)
				{
					scoreDAO = new ScoreDAO();
					scoreDAO.tournamentType = (int)tournaments;
					scores.participants.Add(scoreDAO);
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		Save();
		this.tournaments.Add(Tournaments.Friendly, new Tournament(Tournaments.Friendly));
		this.tournaments.Add(Tournaments.WorldChampionship, new Tournament(Tournaments.WorldChampionship));
		this.tournaments.Add(Tournaments.Olympics, new Tournament(Tournaments.Olympics));
	}

	public Tournament TournamentForType(Tournaments type)
	{
		return tournaments[type];
	}

	public void ResolvePotentialConflictsWithCloudData()
	{
		GGFileIOCloudSync instance = GGFileIOCloudSync.instance;
		if (instance.isInConflict(ScoresFilename))
		{
			GGFileIO cloudFileIO = instance.GetCloudFileIO();
			TournamentScoreDAO model = null;
			if (ProtoIO.LoadFromFile<ProtoSerializer, TournamentScoreDAO>(ScoresFilename, cloudFileIO, out model) && model != null && model.participants != null)
			{
				foreach (ScoreDAO participant in model.participants)
				{
					ScoreDAO scoreDAO = scoreForTournament(participant.tournamentType);
					scoreDAO.bronze = Mathf.Max(participant.bronze, scoreDAO.bronze);
					scoreDAO.silver = Mathf.Max(participant.silver, scoreDAO.silver);
					scoreDAO.gold = Mathf.Max(participant.gold, scoreDAO.gold);
				}
				ProtoIO.SaveToFile<ProtoSerializer, TournamentScoreDAO>(ScoresFilename, cloudFileIO, scores);
			}
		}
	}

	public ScoreDAO scoreForTournament(int type)
	{
		List<ScoreDAO> participants = scores.participants;
		Predicate<ScoreDAO> match = (ScoreDAO score) => score.tournamentType == type;
		return participants.Find(match);
	}

	public void Save()
	{
		ProtoIO.SaveToFileCloudSync(ScoresFilename, scores);
	}
}
