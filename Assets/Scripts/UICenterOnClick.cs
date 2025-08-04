using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Center Scroll View on Click")]
public class UICenterOnClick : MonoBehaviour
{
	private UIPanel mPanel;

	private UICenterOnChild mCenter;

	private void Start()
	{
		mCenter = NGUITools.FindInParents<UICenterOnChild>(base.gameObject);
		mPanel = NGUITools.FindInParents<UIPanel>(base.gameObject);
	}

	private void OnClick()
	{
		if (mCenter != null)
		{
			if (mCenter.enabled)
			{
				mCenter.CenterOn(base.transform);
			}
		}
		else if (mPanel != null && mPanel.clipping != 0)
		{
			SpringPanel.Begin(mPanel.cachedGameObject, mPanel.cachedTransform.InverseTransformPoint(base.transform.position), 6f);
		}
	}
}
