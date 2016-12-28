using UnityEngine;

namespace Lean.Touch
{
	// This script selection of LeanSelectable components
	public abstract class LeanSelect : MonoBehaviour
	{
		public enum SearchType
		{
			GetComponent,
			GetComponentInParent,
			GetComponentInChildren
		}
		
		[Tooltip("How should the selected GameObject be searched for the LeanSelectable component?")]
		public SearchType Search;

		[Tooltip("The currently selected LeanSelectable")]
		public LeanSelectable CurrentSelectable;
		
		[Tooltip("Automatically deselect the CurrentSelectable if Select gets called with null?")]
		public bool AutoDeselect;
		
		public void Select(LeanFinger finger, Component component)
		{
			// Stores the selectable we will search for
			var selectable = default(LeanSelectable);

			// Was a collider found?
			if (component != null)
			{
				switch (Search)
				{
					case SearchType.GetComponent:           selectable = component.GetComponent          <LeanSelectable>(); break;
					case SearchType.GetComponentInParent:   selectable = component.GetComponentInParent  <LeanSelectable>(); break;
					case SearchType.GetComponentInChildren: selectable = component.GetComponentInChildren<LeanSelectable>(); break;
				}
			}

			// Select the selectable
			Select(finger, selectable);
		}

		public void Select(LeanFinger finger, LeanSelectable selectable)
		{
			// Something was selected?
			if (selectable != null)
			{
				// Did we select a new LeanSelectable?
				if (selectable != CurrentSelectable)
				{
					// Deselect the current
					Deselect();

					// Change current
					CurrentSelectable = selectable;

					// Call select event on current
					CurrentSelectable.Select(finger);
				}
			}
			// Nothing was selected?
			else
			{
				// Deselect?
				if (AutoDeselect == true)
				{
					Deselect();
				}
			}
		}
		
		[ContextMenu("Deselect")]
		public void Deselect()
		{
			// Is there a selected object?
			if (CurrentSelectable != null)
			{
				// Deselect it
				CurrentSelectable.Deselect();

				// Mark it null
				CurrentSelectable = null;
			}
		}

		public void Deselect(LeanFinger finger)
		{
			// Is there a selected object?
			if (CurrentSelectable != null)
			{
				// Only deselect if the selected finger is null, or it matches
				if (CurrentSelectable.Finger == null || CurrentSelectable.Finger == finger)
				{
					Deselect();
				}
			}
		}
	}
}