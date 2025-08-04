using UnityEngine;

public class UILayer : MonoBehaviour
{
	public bool escActivatesPop = true;

	protected bool isEntered;

	public virtual void Update()
	{
		if (isEntered && escActivatesPop && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			NavigationManager.instance.Pop();
		}
	}

	public virtual void OnEnter()
	{
		isEntered = true;
	}

	public virtual void OnLeave()
	{
		isEntered = false;
	}
}
