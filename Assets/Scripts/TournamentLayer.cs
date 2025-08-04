using ProtoModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TournamentLayer : MonoBehaviour
{
	[Serializable]
	public class TreeLink
	{
		public UISprite horizontal;

		public UISprite vertical;
	}

	[Serializable]
	public class Flag
	{
		public UISprite flag;

		public UISprite bck;

		public UILabel name;
	}

	public List<TreeLink> tree = new List<TreeLink>();

	public List<Flag> flags = new List<Flag>();

	public List<Flag> finalFlags = new List<Flag>();

	public UILabel finalName;

	public UILabel title;

	public Tournament tournament;

	public float fadeOutAlpha = 0.25f;

	public float halphFadeAlpha = 0.5f;

	public UILabel playButtonLabel;

	public GameObject priceGameObject;

	public UILabel price;

	public void Load(Tournament t)
	{
		tournament = t;
		UpdateLayout();
		if (t != null)
		{
			NavigationManager.instance.Push(base.gameObject);
		}
	}

	private string NameForPosition(int position)
	{
		return TournamentController.NameForPosition(position);
	}

	private void UpdateLayout()
	{
		List<LeagueMemberDAO> list = tournament.Participants();
		foreach (LeagueMemberDAO item in list)
		{
			int num = 0;
			string empty = string.Empty;
			if (item.isHuman)
			{
				empty = CareerBackend.instance.Name();
				num = CareerBackend.instance.Flag();
			}
			else
			{
				PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(item.difficulty);
				empty = playerDef.name;
				num = (int)playerDef.flag;
			}
			Flag flag = flags[item.points];
			GameConstants.SetFlag(flag.flag, num);
			UITools.ChangeText(flag.name, empty);
			flag.bck.alpha = fadeOutAlpha;
		}
		foreach (TreeLink item2 in tree)
		{
			item2.vertical.color = Color.white;
			item2.horizontal.color = Color.white;
			item2.vertical.alpha = fadeOutAlpha;
			item2.horizontal.alpha = fadeOutAlpha;
		}
		LeagueMemberDAO leagueMemberDAO = tournament.NextOpponent();
		if (tournament.isTournamentComplete())
		{
			int num2 = tournament.CurrentRound();
			tournament.humanRanking();
			title.text = "Tournament Done: " + NameForPosition(tournament.humanRanking());
			UITools.ChangeText(playButtonLabel, "New Tournament");
			UnityEngine.Debug.Log("Human Rank " + tournament.humanRanking());
			if (tournament.humanRanking() == 0 || tournament.humanRanking() == 2)
			{
				LightTrail(tournament.humanPlayer(), num2 - 1, Color.green);
			}
			else
			{
				LightTrail(tournament.humanPlayer(), num2 - 1, Color.red);
			}
		}
		else if (leagueMemberDAO != null)
		{
			int num3 = tournament.CurrentRound();
			List<LeagueMemberDAO> list2 = tournament.ParticipantsForRound(num3, exact: true);
			PlayerDeffinition.PlayerDef playerDef2 = PlayerDeffinition.instance.definitionForIndex(leagueMemberDAO.difficulty);
			foreach (LeagueMemberDAO item3 in list2)
			{
				LightTrail(item3, num3, Color.white, useWins: true);
			}
			if (tournament.CurrentRound() >= 2)
			{
				foreach (Flag finalFlag in finalFlags)
				{
					finalFlag.flag.cachedGameObject.SetActive(value: true);
					finalFlag.bck.alpha = 1f;
				}
				GameConstants.SetFlag(finalFlags[0].flag, CareerBackend.instance.Flag());
				GameConstants.SetFlag(finalFlags[1].flag, (int)playerDef2.flag);
				UITools.ChangeText(finalName, tournament.NameForCurrentRound());
			}
			else
			{
				foreach (Flag finalFlag2 in finalFlags)
				{
					finalFlag2.flag.cachedGameObject.SetActive(value: false);
					finalFlag2.bck.alpha = fadeOutAlpha;
				}
				UITools.ChangeText(finalName, "Finals");
			}
			Flag flag2 = flags[0];
			Flag flag3 = flags[leagueMemberDAO.points];
			flag2.bck.alpha = 1f;
			flag3.bck.alpha = 1f;
			title.text = tournament.NameForCurrentRound() + " VS " + playerDef2.name;
			UITools.ChangeText(playButtonLabel, "Play");
		}
		if (tournament.isTournamentComplete() || tournament.price() == 0)
		{
			priceGameObject.SetActive(value: false);
			return;
		}
		priceGameObject.SetActive(value: true);
		price.text = tournament.price().ToString();
	}

	private void LightTrail(LeagueMemberDAO player, int initialRound, Color col, bool useWins = false)
	{
		initialRound = Mathf.Clamp(initialRound, 0, 3);
		int num = 8;
		int num2 = 0;
		int num3 = 1;
		int num4 = 0;
		while (num4 <= initialRound && num > 1)
		{
			int num5 = player.points / num3 + num2;
			if (useWins)
			{
				col = ((player.wins == num4) ? Color.white : ((player.wins >= num4) ? Color.green : Color.red));
			}
			if (num5 < tree.Count)
			{
				TreeLink treeLink = tree[num5];
				col.a = 1f;
				treeLink.vertical.color = col;
				treeLink.horizontal.color = col;
			}
			num2 += num;
			num /= 2;
			num4++;
			num3 <<= 1;
		}
	}

	public void OnPlay()
	{
		if (tournament.isTournamentComplete())
		{
			tournament.CreateTournament();
			UpdateLayout();
			return;
		}
		PlayerSettings instance = PlayerSettings.instance;
		if (instance.CanBuyItemWithPrice(tournament.price()))
		{
			instance.BuyItem(tournament.price());
			ScreenNavigation.instance.LoadTouramentMatch(tournament);
		}
		else
		{
			PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(tournament.NextOpponent().difficulty);
			UIDialog.instance.ShowYesNo("Not Enough Balls!", tournament.price() + " (Ball) needed to play against " + playerDef.name + "! You can get more balls in the Shop, or by playing League.", "Shop", "Close", OnNoBalls);
		}
	}

	private void OnNoBalls(bool success)
	{
		if (success)
		{
			ShopLayer shop = ShopNavigation.instance.shop;
			shop.ShowShopTab();
			NavigationManager.instance.Pop();
			NavigationManager.instance.Push(shop.gameObject);
		}
		else
		{
			NavigationManager.instance.Pop();
		}
	}
}
