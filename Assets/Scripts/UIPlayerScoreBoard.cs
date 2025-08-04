using UnityEngine;

public class UIPlayerScoreBoard : MonoBehaviour
{
	public UISprite flag;

	public new UILabel name;

	public UILabel sets;

	public UILabel pts;

	public UISprite ball;

	public UILabel games;

	public void SetNameAndFlag(string nameTxt, int flagIndex)
	{
		name.text = nameTxt;
		flag.spriteName = GameConstants.Instance.CountryForFlag(flagIndex).spriteName;
	}

	public void SetServing(bool serving)
	{
	}

	public void SetScore(int game, string points)
	{
		sets.text = game.ToString();
		games.text = points;
	}
}
