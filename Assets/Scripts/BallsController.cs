using UnityEngine;

public class BallsController : MonoBehaviour
{
	public ShopLayer shop;

	public UILabel ballsCount;

	public UISprite exclamation;

	private void OnEnable()
	{
		UpdateBalls();
	}

	private void OnClick()
	{
		BuyBalls();
	}

	private void BuyBalls()
	{
		if (!(shop == null))
		{
			shop.ShowShopTab();
			NavigationManager.instance.Push(shop.gameObject);
		}
	}

	private void Update()
	{
		UpdateBalls();
		if (exclamation != null)
		{
			bool flag = !CareerGameMode.instance.HasEnoughMoneyForMatch() && CareerGameMode.instance.isLeagueWon();
			if (exclamation.cachedGameObject.activeSelf != flag)
			{
				exclamation.cachedGameObject.SetActive(flag);
			}
		}
	}

	private void UpdateBalls()
	{
		string text = PlayerSettings.instance.Model.coins.ToString();
		if (ballsCount != null && ballsCount.text != text)
		{
			ballsCount.text = text;
		}
	}
}
