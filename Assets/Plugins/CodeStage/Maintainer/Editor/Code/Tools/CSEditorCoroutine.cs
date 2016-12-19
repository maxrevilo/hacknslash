#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public sealed class CSEditorCoroutine
{
	static CSEditorCoroutine()
	{
		EditorApplication.update += Update;
	}

	private static readonly Dictionary<IEnumerator, Coroutine> asyncList = new Dictionary<IEnumerator, Coroutine>();
	private static readonly List<WaitForSeconds> waitForSecondsList = new List<WaitForSeconds>();

	private static void Update()
	{
		CheckIEnumerator();
		CheckWaitForSeconds();
	}

	private static void CheckIEnumerator()
	{
		List<IEnumerator> removeList = new List<IEnumerator>();
		foreach (KeyValuePair<IEnumerator, Coroutine> pair in asyncList)
		{
			if (pair.Key != null)
			{
				Coroutine c = pair.Key.Current as Coroutine;
				if (c != null)
				{
					if (c.isActive) continue;
				}

				WWW www = pair.Key.Current as WWW;
				if (www != null)
				{
					if (!www.isDone) continue;
				}

				if (!pair.Key.MoveNext())
				{
					if (pair.Value != null)
					{
						pair.Value.isActive = false;
					}
					removeList.Add(pair.Key);
				}
			}
			else
			{
				removeList.Add(pair.Key);
			}
		}

		foreach (IEnumerator async in removeList)
		{
			asyncList.Remove(async);
		}
	}

	private static void CheckWaitForSeconds()
	{
		for (int i = 0; i < waitForSecondsList.Count; i++)
		{
			if (waitForSecondsList[i] != null)
			{
				if (EditorApplication.timeSinceStartup - waitForSecondsList[i].InitTime > waitForSecondsList[i].Time)
				{
					waitForSecondsList[i].isActive = false;
					waitForSecondsList.RemoveAt(i);
				}
			}
			else
			{
				waitForSecondsList.RemoveAt(i);
			}
		}
	}

	public static Coroutine Start(IEnumerator iEnumerator)
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			Coroutine c = new Coroutine();
			if (!asyncList.Keys.Contains(iEnumerator)) asyncList.Add(iEnumerator, c);
			iEnumerator.MoveNext();
			return c;
		}

		Debug.LogError("CSEditorCoroutine.Start can't be used in Play Mode!");
		return null;
	}

	public static void Stop(IEnumerator iEnumerator)
	{
		if (Application.isEditor)
		{
			if (asyncList.Keys.Contains(iEnumerator))
			{
				asyncList.Remove(iEnumerator);
			}
		}
		else
		{
			Debug.LogError("CSEditorCoroutine.Stop can't be used outside Editor");
		}
	}

	public static void AddWaitForSecondsList(WaitForSeconds coroutine)
	{
		if (waitForSecondsList.Contains(coroutine) == false)
		{
			waitForSecondsList.Add(coroutine);
		}
	}

	public class Coroutine
	{
		public bool isActive;

		public Coroutine()
		{
			isActive = true;
		}
	}

	public sealed class WaitForSeconds : Coroutine
	{
		private readonly float time;
		private readonly double initTime;

		public float Time
		{
			get { return time; }
		}
		public double InitTime
		{
			get { return initTime; }
		}

		public WaitForSeconds(float time)
		{
			this.time = time;
			initTime = EditorApplication.timeSinceStartup;
			AddWaitForSecondsList(this);
		}
	}
}

#endif