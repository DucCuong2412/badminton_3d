using System;
using UnityEngine;

public class UIHand : MonoBehaviour
{
	public delegate void OnHandComplete();

	public UISprite trail;

	public UISprite endSprite;

	public UISprite startSprite;

	public TrailRenderer trailRend;

	protected UISprite mSprite;

	protected Transform mTransform;

	protected Vector3 origin;

	protected Vector3 direction;

	protected Camera camWorld;

	protected Camera camUI;

	protected float time;

	public OnHandComplete onComplete;

	protected Transform trail_;

	protected Vector3 ortoDirection;

	protected float ortoRadius;

	private float cachedTrailTime;

	private int setTimeAfter = -1;

	protected Transform cachedTrailTransform
	{
		get
		{
			if (trail_ == null && trailRend != null)
			{
				trail_ = trailRend.transform;
			}
			return trail_;
		}
	}

	private void Awake()
	{
		mTransform = base.transform;
		mSprite = mTransform.GetComponent<UISprite>();
		camWorld = Camera.main;
		camUI = UICamera.mainCamera;
	}

	public void Hide(bool shouldHide = true)
	{
		if (mTransform == null)
		{
			Awake();
		}
		mTransform.gameObject.SetActive(!shouldHide);
		trail.cachedGameObject.SetActive(!shouldHide);
		if (endSprite != null)
		{
			endSprite.cachedGameObject.SetActive(!shouldHide);
		}
		if (startSprite != null)
		{
			startSprite.cachedGameObject.SetActive(!shouldHide);
		}
		trailRend.enabled = !shouldHide;
	}

	public void PointFromTo(Vector3 from, Vector3 to, float ortoRadius = 0f)
	{
		Hide(shouldHide: false);
		this.ortoRadius = ortoRadius;
		Vector3 position = camWorld.WorldToScreenPoint(from);
		Vector3 position2 = camWorld.WorldToScreenPoint(to);
		Vector3 vector = camUI.ScreenToWorldPoint(position);
		Vector3 a = camUI.ScreenToWorldPoint(position2);
		direction = a - vector;
		origin = from;
		ortoDirection = new Vector3(direction.y, 0f - direction.x, 0f);
		Transform parent = mTransform.parent;
		Vector3 vector2 = parent.InverseTransformPoint(vector) - parent.InverseTransformPoint(vector + direction);
		trail.cachedTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector2.y, vector2.x) * 57.29578f + 90f);
		UpdatePosition();
	}

	private void Update()
	{
		time += RealTime.deltaTime * 0.75f;
		if (time > 1f)
		{
			time = 0f;
			if (onComplete != null)
			{
				onComplete();
			}
			cachedTrailTime = trailRend.time;
			trailRend.time = -1f;
			setTimeAfter = 2;
		}
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		float t = MathEx.Hermite(time);
		Transform parent = mTransform.parent;
		Vector3 vector = camUI.ScreenToWorldPoint(camWorld.WorldToScreenPoint(origin));
		Vector3 vector2 = Vector3.Lerp(vector, vector + direction, t);
		if (ortoRadius != 0f)
		{
			vector2 += ortoDirection * ortoRadius * Mathf.Sin(time * (float)Math.PI);
		}
		Vector3 localPosition = parent.InverseTransformPoint(vector2);
		Vector3 localPosition2 = mTransform.localPosition;
		localPosition.z = localPosition2.z;
		mTransform.localPosition = localPosition;
		Vector3 position = mTransform.position;
		vector2.z = position.z;
		cachedTrailTransform.position = vector2;
		Transform cachedTransform = trail.cachedTransform;
		Vector3 vector3 = cachedTransform.parent.InverseTransformPoint(vector);
		Vector3 localPosition3 = cachedTransform.localPosition;
		vector3.z = localPosition3.z;
		cachedTransform.localPosition = vector3;
		Vector3 vector4 = vector3 - parent.InverseTransformPoint(vector + direction);
		vector4.z = 0f;
		trail.height = (int)Mathf.Lerp(0f, vector4.magnitude, t);
		setTimeAfter = Mathf.Max(-1, setTimeAfter - 1);
		if (setTimeAfter == 0)
		{
			trailRend.time = cachedTrailTime;
		}
		if (endSprite != null)
		{
			Vector3 position2 = vector + direction;
			Vector3 position3 = endSprite.cachedTransform.position;
			position2.z = position3.z;
			endSprite.cachedTransform.position = position2;
		}
		if (startSprite != null)
		{
			Vector3 position4 = vector;
			Vector3 position5 = startSprite.cachedTransform.position;
			position4.z = position5.z;
			startSprite.cachedTransform.position = position4;
		}
	}
}
