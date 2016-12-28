using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to select a GameObject using any finger, as long it has a collider
	// NOTE: This requires another component (e.g. LeanTap) to call the Select method
	public class LeanSelect3D : LeanSelect
	{
		[Tooltip("This stores the layers we want the raycast to hit (make sure this GameObject's layer is included!)")]
		public LayerMask LayerMask = Physics.DefaultRaycastLayers;
		
		// NOTE: This must be called from somewhere
		public void Select(LeanFinger finger)
		{
			// Get ray for finger
			var ray = finger.GetRay();

			// Stores the raycast hit info
			var hit = default(RaycastHit);

			// Stores the component we hit (Collider)
			var component = default(Component);
			
			// Was this finger pressed down on a collider?
			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
			{
				component = hit.collider;
			}

			// Select the component
			Select(finger, component);
		}
	}
}