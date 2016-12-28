using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to select a LeanSelectable component that has a Collider2D
	// NOTE: This requires another component (e.g. LeanTap) to call the Select method
	public class LeanSelect2D : LeanSelect
	{
		[Tooltip("This stores the layers we want the raycast to hit (make sure this GameObject's layer is included!)")]
		public LayerMask LayerMask = Physics2D.DefaultRaycastLayers;
		
		// NOTE: This must be called from somewhere
		public void Select(LeanFinger finger)
		{
			// Find the position under the current finger
			var point = finger.GetWorldPosition(1.0f);

			// Find the collider at this position
			var component = Physics2D.OverlapPoint(point, LayerMask);
			
			// Try and select this component
			Select(finger, component);
		}
	}
}