using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to transform the current GameObject with smoothing
	public class LeanTranslateSmooth : MonoBehaviour
	{
		[Tooltip("Does translation require an object to be selected?")]
		public LeanSelectable RequiredSelectable;
		
		public float Sharpness = 10.0f;

		// The position we still need to add
		private Vector3 remainingDelta;

		protected virtual void Update()
		{
			var screenDelta = LeanGesture.GetScreenDelta();

			if (RequiredSelectable != null)
			{
				if (RequiredSelectable.IsSelected == false)
				{
					return;
				}

				if (RequiredSelectable.Finger != null)
				{
					screenDelta = RequiredSelectable.Finger.ScreenDelta;
				}
			}

			Translate(screenDelta);
		}

		protected virtual void LateUpdate()
		{
			// The framerate independent damping factor
			var factor = Mathf.Exp(-Sharpness * Time.deltaTime);

			// Dampen remainingDelta
			var newDelta = remainingDelta * factor;

			// Shift this transform by the change in delta
			transform.position += remainingDelta - newDelta;

			// Update remainingDelta with the dampened value
			remainingDelta = newDelta;
		}

		private void Translate(Vector2 screenDelta)
		{
			// Store old position
			var oldPosition = transform.position;

			// Screen position of the transform
			var screenPosition = Camera.main.WorldToScreenPoint(oldPosition);
			
			// Add the deltaPosition
			screenPosition += (Vector3)screenDelta;
			
			// Convert back to world space
			var newPosition = Camera.main.ScreenToWorldPoint(screenPosition);
			var delPosition = newPosition - oldPosition;

			// Add to delta
			remainingDelta += delPosition;
		}
	}
}