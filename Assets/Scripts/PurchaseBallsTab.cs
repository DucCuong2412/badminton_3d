using UnityEngine;

public class PurchaseBallsTab : MonoBehaviour
{
	public GameObject inAppButonPrefab;

	public UITable table;

	private void Awake()
	{
		InAppPurchase instance = InAppPurchase.instance;
		int num = 1;
		foreach (InAppPurchase.InAppObject inAppProduct in instance.inAppProducts)
		{
			GameObject gameObject = NGUITools.AddChild(table.gameObject, inAppButonPrefab);
			gameObject.SetActive(value: true);
			gameObject.name = num++.ToString();
			InAppButton component = gameObject.GetComponent<InAppButton>();
			component.Init(inAppProduct);
		}
		table.Reposition();
	}
}
