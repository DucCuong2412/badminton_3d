using System;
using System.Collections.Generic;
using UnityEngine;

public class UITabController : MonoBehaviour
{
	[Serializable]
	public class TabItem
	{
		public UITabButton button;

		public GameObject layer;
	}

	public UITabButton activeTab;

	protected bool started;

	public List<TabItem> tabs = new List<TabItem>();

	private void Start()
	{
		if (!started)
		{
			foreach (TabItem tab in tabs)
			{
				tab.button.controller = this;
			}
			if (activeTab == null && tabs.Count > 0)
			{
				activeTab = tabs[0].button;
			}
			started = true;
		}
	}

	private void OnEnable()
	{
		if (activeTab != null)
		{
			OnTabSelected(activeTab);
		}
	}

	public void OnTabSelected(UITabButton selected)
	{
		activeTab = selected;
		foreach (TabItem tab in tabs)
		{
			bool flag = tab.button == selected;
			tab.button.isActive = flag;
			tab.layer.SetActive(flag);
		}
	}

	public void SelectTab(UITabButton selected)
	{
		if (!started)
		{
			activeTab = selected;
		}
		else
		{
			OnTabSelected(selected);
		}
	}
}
