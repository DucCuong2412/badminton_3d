using ProtoModels;
using System.Collections.Generic;
using UnityEngine;

public class TournamentSelectButton : MonoBehaviour
{
	public TournamentController.Tournaments tournamentType;

	public TournamentController.Tournaments prerequisit;

	public bool usePrerequisit;

	public GameObject messageGameObject;

	public UILabel message;

	public UILabel gold;

	public UILabel silver;

	public UILabel bronze;

	public UILabel title;

	public List<UISprite> tags = new List<UISprite>();

	public TournamentLayer tournamentLayer;

	private void Awake()
	{
		TournamentController instance = TournamentController.instance;
		ScoreDAO scoreDAO = instance.scoreForTournament((int)tournamentType);
		gold.text = scoreDAO.gold.ToString();
		silver.text = scoreDAO.silver.ToString();
		bronze.text = scoreDAO.bronze.ToString();
		title.text = TournamentController.tournamentName(tournamentType);
		for (int i = 0; i < tags.Count; i++)
		{
			UISprite uISprite = tags[i];
			uISprite.cachedGameObject.SetActive(i == (int)tournamentType);
		}
		UISprite component = GetComponent<UISprite>();
		messageGameObject.SetActive(value: false);
		if (!isTournamentOpen())
		{
			component.spriteName = "btn-red";
		}
		else
		{
			component.spriteName = "btn-green";
		}
	}

	private string textForUnlock()
	{
		return "Win Gold in " + TournamentController.tournamentName(prerequisit) + " to unlock!";
	}

	private bool isTournamentOpen()
	{
		if (!usePrerequisit)
		{
			return true;
		}
		ScoreDAO scoreDAO = TournamentController.instance.scoreForTournament((int)prerequisit);
		return scoreDAO.gold > 0;
	}

	public void OnClick()
	{
		if (!isTournamentOpen())
		{
			UIDialog.instance.ShowOk("Play " + TournamentController.tournamentName(prerequisit) + " to unlock!", "Win Gold in " + TournamentController.tournamentName(prerequisit) + " to unlock " + TournamentController.tournamentName(tournamentType) + "!", "Ok", null);
			return;
		}
		Tournament tournament = TournamentController.instance.TournamentForType(tournamentType);
		if (!tournament.IsTournamentCreated())
		{
			tournament.CreateTournament();
		}
		tournamentLayer.Load(tournament);
	}
}
