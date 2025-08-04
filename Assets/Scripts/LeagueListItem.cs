using UnityEngine;

public class LeagueListItem : MonoBehaviour
{
	public UILabel place;

	public new UILabel name;

	public UILabel wins;

	public UILabel pts;

	public UISprite flag;

	protected UIWidget mWidget;

	public Color finalsColor;

	public Color firstPlaceColor;

	public Color normalColor;

	private void Awake()
	{
		mWidget = GetComponent<UIWidget>();
	}

	public void SetParams(int place, string name, int wins, int pts, int flag)
	{
		this.place.text = place.ToString();
		this.name.text = name;
		if (this.wins != null)
		{
			this.wins.text = wins.ToString();
		}
		this.pts.text = pts.ToString();
		if (mWidget == null)
		{
			mWidget = GetComponent<UIWidget>();
		}
		if (place <= 1)
		{
			mWidget.color = firstPlaceColor;
		}
		else if (place <= 3)
		{
			mWidget.color = finalsColor;
		}
		else
		{
			mWidget.color = normalColor;
		}
		if (flag < 0)
		{
			this.flag.cachedGameObject.SetActive(value: false);
			return;
		}
		this.flag.cachedGameObject.SetActive(value: true);
		this.flag.spriteName = GameConstants.Instance.CountryForFlag(flag).spriteName;
	}
}
