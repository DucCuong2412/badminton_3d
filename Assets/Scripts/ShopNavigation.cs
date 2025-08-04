using UnityEngine;

public class ShopNavigation : MonoBehaviour
{
	public ShopLayer shop;

	public static ShopNavigation instance
	{
		get;
		protected set;
	}

	private void Awake()
	{
		instance = this;
	}

	public void OnDestroy()
	{
		instance = null;
	}
}
