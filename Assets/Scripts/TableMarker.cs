using UnityEngine;

public class TableMarker : MonoBehaviour
{
	public float timeToChange = 1f;

	public float minAlpha;

	public float maxAlpha = 0.5f;

	protected int direction = 1;

	protected float time;

	protected Transform transform_;

	protected Material material_;

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

	public Material cachedMaterial
	{
		get
		{
			if (material_ == null)
			{
				material_ = GetComponent<Renderer>().material;
			}
			return material_;
		}
	}

	public void SetColor(Color col)
	{
		Color color = cachedMaterial.color;
		col.a = color.a;
		cachedMaterial.color = col;
	}

	public void SetToTable(float tablex, float tickness, float side, Table table)
	{
		tablex = Mathf.Clamp(tablex, -1f + tickness, 1f - tickness);
		Vector3 localScale = cachedTransform.localScale;
		localScale.x = tickness * table.width;
		localScale.z = table.halphLength;
		cachedTransform.localScale = localScale;
		Vector3 position = new Vector3(tablex * table.halphwidth, 0f, side * table.halphLength * 0.5f);
		cachedTransform.position = position;
	}

	public bool isIn(Vector3 pos)
	{
		Vector3 position = cachedTransform.position;
		Vector3 vector = position - pos;
		Vector3 localScale = cachedTransform.localScale;
		return Mathf.Abs(vector.x) <= localScale.x * 0.5f && Mathf.Abs(vector.z) <= localScale.z * 0.5f;
	}

	private void Update()
	{
		time += RealTime.deltaTime * (float)direction;
		if (time >= timeToChange)
		{
			direction = -1;
		}
		else if (time <= 0f)
		{
			direction = 1;
		}
		time = Mathf.Clamp(time, 0f, timeToChange);
		float a = Mathf.Lerp(minAlpha, maxAlpha, time / timeToChange);
		Color color = cachedMaterial.color;
		color.a = a;
		cachedMaterial.color = color;
	}
}
