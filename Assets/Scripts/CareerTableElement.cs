using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CareerTableElement : MonoBehaviour
{
	public UIWidget background;

	protected Transform mTransform;

	protected bool started;

	public float padding;

	protected List<UIWidget> widgets = new List<UIWidget>();

	public UILabel header;

	public GameObject buttonPrefab;

	private void Awake()
	{
		mTransform = base.transform;
		widgets.Clear();
		started = true;
	}

	public void SetCareerGroup(CareerGameMode.CareerGroup group, bool isEnabled)
	{
		header.text = ((!isEnabled) ? "Win All Stars To Unlock" : group.name);
		GameObject gameObject = base.gameObject;
		foreach (CareerGameMode.CareerPlayer playerDef in group.playerDefs)
		{
			GameObject gameObject2 = NGUITools.AddChild(gameObject, buttonPrefab);
			CareerPlayButton component = gameObject2.GetComponent<CareerPlayButton>();
			component.SetCareerPlayer(playerDef, isEnabled);
			widgets.Add(gameObject2.GetComponent<UIWidget>());
		}
		Reposition();
	}

	private void Update()
	{
		Reposition();
	}

	private void Reposition()
	{
		if (widgets.Count != 0)
		{
			Vector2 b = default(Vector2);
			foreach (UIWidget widget in widgets)
			{
				Vector3[] localCorners = widget.localCorners;
				b.y += localCorners[1].y - localCorners[0].y;
				b.x = Mathf.Max(localCorners[2].x - localCorners[1].x, b.x);
			}
			b.y += padding * (float)(widgets.Count - 1);
			Vector3[] localCorners2 = background.localCorners;
			Vector2 a = new Vector2(localCorners2[2].x - localCorners2[1].x, localCorners2[1].y - localCorners2[0].y);
			Vector2 vector = -(a - b) * 0.5f + new Vector2(localCorners2[1].x, localCorners2[1].y);
			UnityEngine.Debug.Log("Top Left " + localCorners2[1] + " offset " + vector);
			UIWidget uIWidget = widgets[0];
			Vector3[] localCorners3 = uIWidget.localCorners;
			vector.y = Mathf.Min(localCorners2[1].y + (localCorners3[0].y - localCorners3[1].y) * 0.5f, vector.y);
			Vector3 vector2 = new Vector3(vector.x, vector.y, 0f);
			Transform transform = mTransform;
			foreach (UIWidget widget2 in widgets)
			{
				Vector3[] localCorners4 = widget2.localCorners;
				Vector2 pivotOffset = widget2.pivotOffset;
				UnityEngine.Debug.Log("Offset " + vector2);
				Vector3 b2 = new Vector3(NGUIMath.Lerp(0f, localCorners4[2].x - localCorners4[1].x, pivotOffset.x), 0f);
				Vector3 localPosition = vector2 + b2;
				Vector3 localPosition2 = widget2.cachedTransform.localPosition;
				localPosition.z = localPosition2.z;
				widget2.cachedTransform.localPosition = localPosition;
				vector2.y -= localCorners4[1].y - localCorners4[0].y + padding;
			}
			if (Application.isPlaying)
			{
				base.enabled = false;
			}
		}
	}
}
