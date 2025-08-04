using UnityEngine;

public class Flag : MonoBehaviour
{
	private void Awake()
	{
		GameConstants.Country country = GameConstants.Instance.CountryForFlag(MatchController.InitParameters.player2Flag);
		Texture texture = null;
		if (country != null)
		{
			texture = (Resources.Load("Flags/" + country.spriteName) as Texture);
		}
		if (texture == null)
		{
			base.transform.position = new Vector3(0f, -100f, 0f);
			base.transform.localScale = Vector3.zero;
		}
		else
		{
			GetComponent<Renderer>().material.mainTexture = texture;
		}
	}
}
