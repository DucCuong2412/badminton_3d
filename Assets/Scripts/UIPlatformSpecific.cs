using System.Collections.Generic;
using UnityEngine;

public class UIPlatformSpecific : MonoBehaviour
{
	public RuntimePlatform platform;

	public List<RuntimePlatform> platforms = new List<RuntimePlatform>();

	public bool socialProviderSpecific;

	public ConfigBase.SocialProvider socialProvider;

	private void Awake()
	{
		bool flag = Application.platform == platform;
		foreach (RuntimePlatform platform2 in platforms)
		{
			flag = (flag || Application.platform == platform2);
		}
		if (socialProviderSpecific)
		{
			flag = (flag && socialProvider == ConfigBase.instance.socialProvider);
		}
		base.gameObject.SetActive(flag);
	}
}
