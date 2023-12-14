using GameBase;
using UnityEngine;

namespace SpaceRTS.FeedbackAndEffects
{
	/// <summary>
	/// Changes the selection feedback of the GameEntity when its required by the Selectable component.
	/// </summary>
	[RequireComponent(typeof(Selectable))]
	public class SelectionFeedback : GameEntityComponent
	{
		/// <summary>
		/// Proxy reference to the MeshRenderer that will be used for the selection feedback
		/// </summary>
		public ProxyRef selectionMarker = new ProxyRef(typeof(MeshRenderer), "select_marker");
		/// <summary>
		/// Color to use in the selection marker when the unit is selected.
		/// </summary>
		public Color selectedColor = new Color(0.2f, 1.0f, 0.0f, 1.0f);
		/// <summary>
		/// Color to use in the selection marker when the unit is highlighted.
		/// </summary>
		public Color highlightedColor = new Color(1.0f, 1.0f, 0.0f, 0.2f);


		Selectable selectable = null;
		Selectable Selection { get { if(selectable==null) selectable = GetComponent<Selectable>(); return selectable; } }

		// Use this for initialization
		void OnEnable ()
		{
			Selection.Selected += OnSelectableChanged;
			Selection.Unselected += OnSelectableChanged;
			Selection.Highligted += OnSelectableChanged;
			Selection.Unhighligted += OnSelectableChanged;
			Selection.Hovered += OnSelectableChanged;
			Selection.Unhovered += OnSelectableChanged;
			//Messages.RegisterObserver<SelectableChanged>(OnSelectableChanged);
		}

		void OnDisable()
		{
			Selection.Selected -= OnSelectableChanged;
			Selection.Unselected -= OnSelectableChanged;
			Selection.Highligted -= OnSelectableChanged;
			Selection.Unhighligted -= OnSelectableChanged;
			Selection.Hovered -= OnSelectableChanged;
			Selection.Unhovered -= OnSelectableChanged;
			//Messages.UnregisterObserver<SelectableChanged>(OnSelectableChanged);
		}

		/// <summary>
		/// Called when the Visual Module is setted. Here we need to initialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleSetted()
		{
			selectionMarker.SafeAssign(ThisEntity);
			MeshRenderer mr = selectionMarker.Get<MeshRenderer>();
			if(mr)
				mr.enabled = false;
		}

		/// <summary>
		/// Called when the Visual Module is removed. Here we need to uninitialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleRemoved()
		{
			selectionMarker.SafeClear();
		}

		//void OnSelectableChanged(SelectableChanged msg)
		//{
		//	MeshRenderer mr = selectionMarker.Get<MeshRenderer>();
		//	if (msg == null || mr == null)
		//		return;

		//	if (msg.IsSelected)
		//	{
		//		mr.enabled = true;
		//		mr.material.SetColor("_Color", selectedColor);
		//	}
		//	else if(msg.IsHighlighted)
		//	{
		//		mr.enabled = true;
		//		mr.material.SetColor("_Color", highlightedColor);
		//	}
		//	else
		//		mr.enabled = false;
		//}

		void OnSelectableChanged()
		{
			MeshRenderer mr = selectionMarker.Get<MeshRenderer>();
			if (Selection == null || mr == null)
				return;

			if (Selection.IsSelected)
			{
				mr.enabled = true;
				mr.material.SetColor("_Color", selectedColor);
			}
			else if(Selection.IsHighlighted)
			{
				mr.enabled = true;
				mr.material.SetColor("_Color", highlightedColor);
			}
			else
				mr.enabled = false;
		}

	}
}