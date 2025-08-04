using System.Collections;
using UnityEngine;

public class PointTo : MonoBehaviour
{
	public UISprite pointer;

	protected Transform pointToTransform;

	protected Vector3 displaceWorld;

	protected float time;

	protected int direction = 1;

	private Transform transform_;

	private GameObject gameObject_;

	public Transform cachedTransform
	{
		get
		{
			if (transform_ == null)
			{
				transform_ = base.transform;
			}
			return transform_;
		}
	}

	public GameObject cachedGameObject
	{
		get
		{
			if (gameObject_ == null)
			{
				gameObject_ = base.gameObject;
			}
			return gameObject_;
		}
	}

	public void StartPointing(Transform pointTo, Vector3 displace)
	{
		cachedGameObject.SetActive(value: true);
		pointer.cachedGameObject.SetActive(value: true);
		pointToTransform = pointTo;
		displaceWorld = displace;
		time = 0f;
		UpdatePoint();
	}

	protected void UpdatePoint()
	{
		Vector3 position = pointToTransform.position;
		Vector3 position2 = Vector3.Lerp(position + displaceWorld, position, MathEx.Hermite(time));
		cachedTransform.position = position2;
		pointer.cachedTransform.localScale = Vector3.one;
		pointer.alpha = 0f;
		pointer.cachedTransform.position = position;
	}

	public void Hide()
	{
		cachedGameObject.SetActive(value: false);
		pointer.cachedGameObject.SetActive(value: false);
	}

	private IEnumerator ActivatePointer()
	{
		pointer.alpha = 0.5f;
		TweenScale.Begin(pointer.cachedGameObject, 0.5f, new Vector3(4f, 4f, 1f));
		TweenAlpha.Begin(pointer.cachedGameObject, 0.5f, 0f);
		yield return new WaitForSeconds(0.5f);
		pointer.cachedTransform.localScale = Vector3.one;
		pointer.alpha = 0f;
	}

	private void Update()
	{
		time += RealTime.deltaTime * (float)direction;
		if (time > 1f)
		{
			direction = -1;
			StartCoroutine(ActivatePointer());
		}
		else if (time < 0f)
		{
			direction = 1;
		}
		UpdatePoint();
	}
}
