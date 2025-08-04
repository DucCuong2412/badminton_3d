using UnityEngine;

public class UINotification : MonoBehaviour
{
	public UILabel text;

	protected Transform mTransform;

	protected UIWidget mWidget;

	protected Camera camWorld;

	protected Camera camUI;

	protected Vector3 worldPos;

	protected Vector3 trDisplace;

	protected Vector3 blDisplace;

	protected Transform tracked;

	protected Vector3 worldDisplace;

	protected float duration;

	protected float showTime;

	protected float width;

	private void Awake()
	{
		mTransform = base.transform;
		mWidget = mTransform.GetComponent<UIWidget>();
		camWorld = Camera.main;
		camUI = UICamera.mainCamera;
		Vector3[] worldCorners = mWidget.worldCorners;
		width = worldCorners[2].x - worldCorners[1].x;
		trDisplace = (mTransform.position - worldCorners[2]) * 1.1f;
		blDisplace = (mTransform.position - worldCorners[0]) * 1.1f;
	}

	public void Hide()
	{
		mTransform.gameObject.SetActive(value: false);
	}

	public void ShowOnWorld(Vector3 worldPosition, string text)
	{
		if (mTransform == null)
		{
			Awake();
		}
		tracked = null;
		worldPos = worldPosition;
		mTransform.gameObject.SetActive(value: true);
		this.text.text = text;
		worldPos = worldPosition;
		UpdateWorldPos();
	}

	public void ShowOnTransform(Transform tracked, Vector3 worldDisplace, string text, float duration = -1f)
	{
		if (mTransform == null)
		{
			Awake();
		}
		this.duration = duration;
		showTime = 0f;
		this.tracked = tracked;
		mTransform.gameObject.SetActive(value: true);
		this.worldDisplace = worldDisplace;
		this.text.text = text;
		float num = 1.2f;
		this.text.cachedTransform.localScale = new Vector3(num, num, 1f);
		TweenScale.Begin(this.text.cachedGameObject, 0.2f, Vector3.one);
		UpdateWorldPos();
	}

	private void UpdateWorldPos()
	{
		if (tracked != null)
		{
			worldPos = tracked.position + worldDisplace;
		}
		Vector3 position = camWorld.WorldToScreenPoint(worldPos);
		Transform parent = mTransform.parent;
		Vector3[] localCorners = mWidget.localCorners;
		Vector3 vector = camUI.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
		Vector3 b = blDisplace;
		if (position.x > (float)Screen.width * 0.5f)
		{
			b.x = trDisplace.x - width * 0.1f;
		}
		else
		{
			b.x += width * 0.1f;
		}
		Vector3 position2 = camUI.ScreenToWorldPoint(position) + b;
		if (position2.y - trDisplace.y > vector.y)
		{
			position2.y += vector.y - position2.y + trDisplace.y;
		}
		Vector3 localPosition = parent.InverseTransformPoint(position2);
		Vector3 localPosition2 = mTransform.localPosition;
		localPosition.z = localPosition2.z;
		mTransform.localPosition = localPosition;
	}

	private void Update()
	{
		UpdateWorldPos();
		showTime += Time.deltaTime;
		if (duration > 0f && showTime > duration)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
