using ProtoModels;
using System.Collections.Generic;
using UnityEngine;

public class AchivementsController
{
	private const string Filename = "achivements.bytes";

	private AchivementsDAO model;

	protected List<AchivementBase> achivements = new List<AchivementBase>();

	public static AchivementsController instance_;

	public static AchivementsController instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new AchivementsController();
				instance_.Init();
			}
			return instance_;
		}
	}

	protected void Init()
	{
		if (!ProtoIO.LoadFromFileCloudSync("achivements.bytes", out model))
		{
			model = new AchivementsDAO();
			model.achivements = new List<AchivementDAO>();
			Save();
		}
		if (model.achivements == null)
		{
			model.achivements = new List<AchivementDAO>();
		}
		CreateAchivements();
		for (int i = 0; i < achivements.Count; i++)
		{
			AchivementBase achivementBase = achivements[i];
			AchivementDAO achivementDAO = null;
			if (i >= model.achivements.Count)
			{
				achivementDAO = new AchivementDAO();
				model.achivements.Add(achivementDAO);
			}
			else
			{
				achivementDAO = model.achivements[i];
			}
			achivementBase.Init(this, achivementDAO);
		}
	}

	public void ResolvePotentialConflictsWithCloudData()
	{
		GGFileIOCloudSync instance = GGFileIOCloudSync.instance;
		if (!instance.isInConflict("achivements.bytes"))
		{
			return;
		}
		GGFileIO cloudFileIO = instance.GetCloudFileIO();
		AchivementsDAO achivementsDAO = null;
		if (!ProtoIO.LoadFromFile<ProtoSerializer, AchivementsDAO>("achivements.bytes", cloudFileIO, out achivementsDAO) || achivementsDAO == null || achivementsDAO.achivements == null || achivementsDAO.achivements.Count == 0)
		{
			return;
		}
		for (int i = 0; i < Mathf.Min(achivementsDAO.achivements.Count, model.achivements.Count); i++)
		{
			AchivementDAO achivementDAO = achivementsDAO.achivements[i];
			AchivementDAO achivementDAO2 = model.achivements[i];
			if (!(achivementDAO2.name != achivementDAO.name))
			{
				achivementDAO2.numStepsCompleted = Mathf.Max(achivementDAO2.numStepsCompleted, achivementDAO.numStepsCompleted);
				achivementDAO2.numStepsReported = Mathf.Max(achivementDAO2.numStepsReported, achivementDAO.numStepsReported);
				achivementDAO2.numStepsSync = Mathf.Max(achivementDAO2.numStepsSync, achivementDAO.numStepsSync);
			}
		}
		ProtoIO.SaveToFile<ProtoSerializer, AchivementsDAO>("achivements.bytes", cloudFileIO, model);
	}

	protected void CreateAchivements()
	{
		achivements.Add(new GroupAchivement
		{
			groupId = 0,
			androidId = "CgkIhPXz_58QEAIQAg",
			iosId = "badminton_training_certificate",
			name = "Training Certificate",
			description = "Passed Training!",
			balls = 10
		});
		achivements.Add(new GroupAchivement
		{
			groupId = 1,
			androidId = "CgkIhPXz_58QEAIQAw",
			iosId = "badminton_level1_domination",
			name = "Level 1 Domination",
			description = "Dominated All players on Level 1",
			balls = 15
		});
		achivements.Add(new GroupAchivement
		{
			groupId = 2,
			androidId = "CgkIhPXz_58QEAIQBA",
			iosId = "badminton_level2_domination",
			name = "Level 2 Domination",
			description = "Dominated All players on Level 2",
			balls = 15
		});
		achivements.Add(new GroupAchivement
		{
			groupId = 3,
			androidId = "CgkIhPXz_58QEAIQBQ",
			iosId = "badminton_level3_domination",
			name = "Level 3 Domination",
			description = "Dominated All players on Level 3",
			balls = 15
		});
		achivements.Add(new LeaguePlayAchivement
		{
			leagueNum = 1,
			androidId = "CgkIhPXz_58QEAIQBg",
			iosId = "badminton_bleague",
			name = "B League",
			description = "Finished in top " + LeagueController.instance.PlacesThatAdvance() + ", and advanced to B League",
			balls = 15
		});
		achivements.Add(new MultiplayerAchivement
		{
			androidId = "CgkIhPXz_58QEAIQBw",
			iosId = "badminton_multiplayer_trainee",
			name = "Multiplayer Rookie",
			description = "Won 10 games in multiplayer!",
			multiplayerWins = 10,
			balls = 15
		});
		achivements.Add(new MultiplayerAchivement
		{
			androidId = "CgkIhPXz_58QEAIQCA",
			iosId = "badminton_multiplayer_pro",
			name = "Multiplayer Pro",
			description = "Won 50 games in multiplayer!",
			multiplayerWins = 50,
			balls = 15
		});
		achivements.Add(new TournamentAchivement
		{
			androidId = "CgkIhPXz_58QEAIQCQ",
			iosId = "badminton_friendly_win",
			name = "Friendly Tournament Winner",
			description = "Friendly Tournament Gold Medalist",
			tournament = TournamentController.Tournaments.Friendly,
			wins = 1,
			balls = 10
		});
		achivements.Add(new TournamentAchivement
		{
			androidId = "CgkIhPXz_58QEAIQCg",
			iosId = "badminton_world_win",
			name = "World Champion",
			description = "World Champion!",
			tournament = TournamentController.Tournaments.WorldChampionship,
			wins = 1,
			balls = 20
		});
		achivements.Add(new TournamentAchivement
		{
			androidId = "CgkIhPXz_58QEAIQCw",
			iosId = "badminton_star_win",
			name = "All Star Champion",
			description = "All Star Champion!",
			tournament = TournamentController.Tournaments.Olympics,
			wins = 1,
			balls = 20
		});
	}

	public void Save()
	{
		if (model == null)
		{
			model = new AchivementsDAO();
			model.achivements = new List<AchivementDAO>();
		}
		ProtoIO.SaveToFileCloudSync("achivements.bytes", model);
	}

	public List<AchivementBase> ReportGameFinished(MatchController.MatchParameters initParameters)
	{
		AchivementBase.GameFinishedParams gameFinishedParams = new AchivementBase.GameFinishedParams();
		gameFinishedParams.initParameters = initParameters;
		List<AchivementBase> list = new List<AchivementBase>();
		foreach (AchivementBase achivement in achivements)
		{
			if (!achivement.isFinished())
			{
				achivement.ReportGameFinished(gameFinishedParams);
				if (achivement.isFinished())
				{
					list.Add(achivement);
				}
			}
		}
		return list;
	}

	public void CheckFinished()
	{
		foreach (AchivementBase achivement in achivements)
		{
			if (!achivement.isFinished())
			{
				achivement.CheckFinished();
			}
		}
	}

	public List<AchivementBase> ReportAchivements()
	{
		List<AchivementBase> list = new List<AchivementBase>();
		PlayerSettings instance = PlayerSettings.instance;
		foreach (AchivementBase achivement in achivements)
		{
			if (achivement.isFinished() && !achivement.isReported())
			{
				achivement.reportSteps(achivement.stepsToReport());
				instance.Model.coins += achivement.balls;
				list.Add(achivement);
			}
		}
		Save();
		instance.Save();
		return list;
	}

	public void SyncAchivementsWithServer()
	{
		UnityEngine.Debug.Log("SyncAchivementsWithServer");
		if (GGSocialGaming.instance.isSignedIn())
		{
			UnityEngine.Debug.Log("SyncAchivementsWithServer - SignedIn");
			foreach (AchivementBase achivement in achivements)
			{
				int num = achivement.stepsToSync();
				if (num > 0 && achivement.isFinished())
				{
					UnityEngine.Debug.Log("Unlock achivement " + achivement.achivementId);
					GGSocialGaming.instance.unlockAchievement(achivement.achivementId);
					achivement.syncSteps(num);
				}
			}
			Save();
		}
	}
}
