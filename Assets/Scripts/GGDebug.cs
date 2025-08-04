using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GGDebug : MonoBehaviour
{
	public UILabel label;

	public Dictionary<string, long> memMap = new Dictionary<string, long>();

	protected long startMem;

	protected string startMemName;

	protected long prevFrame;

	public int mem;

	public int memGrow;

	private Process proc = Process.GetCurrentProcess();

	public static GGDebug instance
	{
		get;
		protected set;
	}

	private void Awake()
	{
		instance = this;
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		instance = null;
	}

	private void BeginMemory(string name)
	{
	}

	private void EndMemory()
	{
	}
}
