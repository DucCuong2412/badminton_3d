using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MoveByKeys : Photon.MonoBehaviour
{
	public float speed = 10f;

	private void Update()
	{
		if (UnityEngine.Input.GetKey(KeyCode.A))
		{
			base.transform.position += Vector3.left * (speed * Time.deltaTime);
		}
		if (UnityEngine.Input.GetKey(KeyCode.D))
		{
			base.transform.position += Vector3.right * (speed * Time.deltaTime);
		}
		if (UnityEngine.Input.GetKey(KeyCode.W))
		{
			base.transform.position += Vector3.forward * (speed * Time.deltaTime);
		}
		if (UnityEngine.Input.GetKey(KeyCode.S))
		{
			base.transform.position += Vector3.back * (speed * Time.deltaTime);
		}
	}

	private void Start()
	{
		base.enabled = base.photonView.isMine;
	}
}
