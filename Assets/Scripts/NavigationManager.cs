using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
	public delegate void OnLayerChanged();

	public GameObject startLayer;

	public GameObject loadingLayerPrefab;

	public LoadingLayer loading;

	protected LinkedList<GameObject> history = new LinkedList<GameObject>();

	private static NavigationManager instance_;

	public static NavigationManager instance => instance_;

	public static event OnLayerChanged onLayerChanged;

	public void Awake()
	{
		instance_ = this;
		GameObject gameObject = UnityEngine.Object.Instantiate(loadingLayerPrefab);
		Transform transform = gameObject.transform;
		transform.parent = base.transform.parent;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		loading = gameObject.GetComponent<LoadingLayer>();
		loading.FadeFromTo(1f, 0f, null);
		OnLoadingOver();
	}

	protected virtual void OnLoadingOver()
	{
		Push(startLayer);
	}

	public void OnDestroy()
	{
		instance_ = null;
	}

	public GameObject TopLayer()
	{
		if (history.Count == 0)
		{
			return null;
		}
		return history.Last.Value;
	}

	public void PushModal(GameObject newLayer)
	{
		UnityEngine.Debug.Log("PushModal " + newLayer.name);
		Push(newLayer, isModal: true, activate: true);
	}

	public void Push(GameObject layer, bool activate = true)
	{
		Push(layer, isModal: false, activate);
	}

	public void Push(GameObject newLayer, bool isModal, bool activate)
	{
		if (newLayer == null || newLayer == TopLayer())
		{
			return;
		}
		if (history.Count > 0)
		{
			GameObject value = history.Last.Value;
			OnLeave(value);
			if (!isModal)
			{
				value.SetActive(value: false);
			}
		}
		history.AddLast(newLayer);
		if (activate)
		{
			newLayer.SetActive(value: true);
			OnEnter(newLayer);
		}
		if (NavigationManager.onLayerChanged != null)
		{
			NavigationManager.onLayerChanged();
		}
	}

	public void Pop(bool force = false)
	{
		if (history.Count < 1 || (history.Count <= 1 && !force))
		{
			return;
		}
		GameObject value = history.Last.Value;
		history.RemoveLast();
		OnLeave(value);
		value.SetActive(value: false);
		if (history.Count < 1)
		{
			if (NavigationManager.onLayerChanged != null)
			{
				NavigationManager.onLayerChanged();
			}
			return;
		}
		GameObject value2 = history.Last.Value;
		value2.gameObject.SetActive(value: true);
		OnEnter(value2);
		if (NavigationManager.onLayerChanged != null)
		{
			NavigationManager.onLayerChanged();
		}
	}

	private void OnLeave(GameObject obj)
	{
		if (!(obj == null))
		{
			UILayer component = obj.GetComponent<UILayer>();
			if (component != null)
			{
				component.OnLeave();
			}
		}
	}

	private void OnEnter(GameObject obj)
	{
		if (!(obj == null))
		{
			UILayer component = obj.GetComponent<UILayer>();
			if (component != null)
			{
				component.OnEnter();
			}
		}
	}
}
