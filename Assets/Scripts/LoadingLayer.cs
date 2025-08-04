using UnityEngine;

public class LoadingLayer : MonoBehaviour
{
	public delegate void OnLoadingDone();

	protected OnLoadingDone onDone;

	public UISprite background;

	public float duration = 0.1f;

	protected float from = 1f;

	protected float to;

	protected float time;

	protected bool deactivateWhenDone = true;

	protected GameObject cachedGameObject_;

	public GameObject cachedGameObject
	{
		get
		{
			if (cachedGameObject_ == null)
			{
				cachedGameObject_ = base.gameObject;
			}
			return cachedGameObject_;
		}
	}

	public void FadeFromTo(float from, float to, OnLoadingDone onDone, bool deactivateWhenDone = true)
	{
		this.deactivateWhenDone = deactivateWhenDone;
		this.from = from;
		this.to = to;
		this.onDone = onDone;
		time = 0f;
		cachedGameObject.SetActive(value: true);
		background.alpha = from;
	}

	private void Update()
	{
		time += RealTime.deltaTime;
		background.alpha = Mathf.Lerp(from, to, time / duration);
		if (time >= duration)
		{
			OnDone();
		}
	}

	private void OnDone()
	{
		if (deactivateWhenDone)
		{
			cachedGameObject.SetActive(value: false);
		}
		OnLoadingDone onLoadingDone = onDone;
		onDone = null;
		onLoadingDone?.Invoke();
	}
}
