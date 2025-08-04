using System.Collections.Generic;
using UnityEngine;

public class CrossFade : MonoBehaviour
{
	public UIWidget fadeIn;

	public UIWidget fadeOut;

	public UILabel changeTextLabel;

	public List<string> texts = new List<string>();

	public float crossFadeDuration = 0.25f;

	public float initialDelay = 0.5f;

	public float delay = 1f;

	private float time;

	private void Update()
	{
		if (!(fadeIn == null) && !(fadeOut == null))
		{
			if (fadeIn == changeTextLabel && texts.Count > 0 && time == 0f)
			{
				changeTextLabel.text = texts[Random.Range(0, texts.Count) % texts.Count];
			}
			time += Time.deltaTime;
			float num = (time - initialDelay) / crossFadeDuration;
			float num2 = Mathf.Lerp(0f, 1f, num);
			fadeIn.alpha = num2;
			fadeOut.alpha = 1f - num2;
			if (num > delay + initialDelay + crossFadeDuration)
			{
				time = 0f;
				UIWidget uIWidget = fadeIn;
				fadeIn = fadeOut;
				fadeOut = uIWidget;
			}
		}
	}
}
