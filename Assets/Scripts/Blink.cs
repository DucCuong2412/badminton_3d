using UnityEngine;

public class Blink : MonoBehaviour
{
	public UIWidget widget;

	public float startAlpha = 0.5f;

	public float endAplha = 1f;

	public float duration = 1f;

	protected float time;

	public int direction = 1;

	private void Awake()
	{
		if (widget == null)
		{
			widget = GetComponent<UIWidget>();
		}
		Update();
	}

	private void Update()
	{
		if (!(widget == null))
		{
			time += (float)direction * Time.deltaTime / duration;
			if (time >= 1f)
			{
				direction = -1;
			}
			else if (time <= 0f)
			{
				direction = 1;
			}
			time = Mathf.Clamp01(time);
			widget.alpha = Mathf.Lerp(startAlpha, endAplha, time);
		}
	}
}
