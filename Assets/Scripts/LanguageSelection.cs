using UnityEngine;

[RequireComponent(typeof(UIPopupList))]
[AddComponentMenu("NGUI/Interaction/Language Selection")]
public class LanguageSelection : MonoBehaviour
{
	private UIPopupList mList;

	private void Start()
	{
		mList = GetComponent<UIPopupList>();
		if (Localization.instance != null && Localization.instance.languages != null && Localization.instance.languages.Length > 0)
		{
			mList.items.Clear();
			int i = 0;
			for (int num = Localization.instance.languages.Length; i < num; i++)
			{
				TextAsset textAsset = Localization.instance.languages[i];
				if (textAsset != null)
				{
					mList.items.Add(textAsset.name);
				}
			}
			mList.value = Localization.instance.currentLanguage;
		}
		EventDelegate.Add(mList.onChange, OnChange);
	}

	private void OnChange()
	{
		if (Localization.instance != null)
		{
			Localization.instance.currentLanguage = UIPopupList.current.value;
		}
	}
}
