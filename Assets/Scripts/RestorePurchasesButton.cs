using UnityEngine;

public class RestorePurchasesButton : MonoBehaviour
{
	public void OnClick()
	{
		InAppPurchase.instance.restorePurchases();
	}
}
