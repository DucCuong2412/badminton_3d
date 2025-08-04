using UnityEngine;

public class InputToEvent : MonoBehaviour
{
	private GameObject lastGo;

	public static Vector3 inputHitPos;

	public bool DetectPointedAtGameObject;

	public static GameObject goPointedAt
	{
		get;
		private set;
	}

	private void Update()
	{
		if (DetectPointedAtGameObject)
		{
			goPointedAt = RaycastObject(UnityEngine.Input.mousePosition);
		}
		if (UnityEngine.Input.touchCount > 0)
		{
			Touch touch = UnityEngine.Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began)
			{
				Press(touch.position);
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				Release(touch.position);
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0))
			{
				Press(UnityEngine.Input.mousePosition);
			}
			if (Input.GetMouseButtonUp(0))
			{
				Release(UnityEngine.Input.mousePosition);
			}
		}
	}

	private void Press(Vector2 screenPos)
	{
		lastGo = RaycastObject(screenPos);
		if (lastGo != null)
		{
			lastGo.SendMessage("OnPress", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Release(Vector2 screenPos)
	{
		if (lastGo != null)
		{
			GameObject x = RaycastObject(screenPos);
			if (x == lastGo)
			{
				lastGo.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			}
			lastGo.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
			lastGo = null;
		}
	}

	private GameObject RaycastObject(Vector2 screenPos)
	{
		if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(screenPos), out RaycastHit hitInfo, 200f))
		{
			inputHitPos = hitInfo.point;
			return hitInfo.collider.gameObject;
		}
		return null;
	}
}
