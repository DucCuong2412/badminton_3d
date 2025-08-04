using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStartScript : MonoBehaviour
{
	public GameObject gameMatch;

	public GameObject tutorialMatch;

	public GameObject multiplayerMatch;

	public List<Material> courtMaterials;

	private void Awake()
	{
		GC.Collect();
		int usedCourt = PlayerSettings.instance.Model.usedCourt;
		CourtItem court = ShopItems.instance.GetCourt(usedCourt);
		Texture mainTexture = Resources.Load<Texture>("Court/" + court.textureName);
		foreach (Material courtMaterial in courtMaterials)
		{
			courtMaterial.mainTexture = mainTexture;
		}
		if (MatchController.InitParameters.gameMode == MatchController.GameMode.Tutorial)
		{
			tutorialMatch.SetActive(value: true);
		}
		else if (MatchController.InitParameters.gameMode == MatchController.GameMode.Multiplayer)
		{
			multiplayerMatch.SetActive(value: true);
		}
		else
		{
			gameMatch.SetActive(value: true);
		}
		Screen.sleepTimeout = -1;
		PlayerSettings.instance.Model.arcadeGamesPlayed++;
		PlayerSettings.instance.Save();
	}
}
