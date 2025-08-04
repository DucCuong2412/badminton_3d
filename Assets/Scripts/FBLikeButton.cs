using UnityEngine;

public class FBLikeButton : MonoBehaviour
{
	public UISprite ball;

	public UILabel ballText;

	private void Awake()
	{
		ballText.text = "+" + PlayerSettings.instance.CoinsForLike();
	}

	private void OnEnable()
	{
		Update();
	}

	public void OnClick()
	{
		GGSupportMenu.instance.showFacebookLike();
	}

	public void Update()
	{
		bool flag = !PlayerSettings.instance.Model.playerLikedFacebookPage;
		if (ball.cachedGameObject.activeSelf != flag)
		{
			ball.cachedGameObject.SetActive(flag);
			ballText.cachedGameObject.SetActive(flag);
		}
	}
}
