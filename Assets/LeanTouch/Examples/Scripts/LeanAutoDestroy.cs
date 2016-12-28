using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
	// This script will automatically destroy this GameObject after 'Seconds' seconds
	public class LeanAutoDestroy : MonoBehaviour
	{
		[Tooltip("The amount of seconds remaining before this GameObject gets destroyed")]
		public float Seconds = 1.0f;
		
		protected virtual void Update()
		{
			Seconds -= Time.deltaTime;

			if (Seconds <= 0.0f)
			{
				Destroy(gameObject);
			}
		}
	}
}