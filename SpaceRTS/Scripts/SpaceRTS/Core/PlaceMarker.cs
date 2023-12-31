﻿using GameBase;
using UnityEngine;

namespace SpaceRTS.Core
{
	/// <summary>
	/// Simply runs the "move_marker_anim" in the Animator component attached
	/// next to this one when is required.
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class PlaceMarker : GameSceneSystem
	{
		private Animator moveMarkerAnimator;

		public void Start()
		{
			moveMarkerAnimator = GetComponent<Animator>();
		}

		/// <summary>
		/// Places this GameObject at the position coordinates and runs the "move_marker_anim" animation.
		/// </summary>
		/// <param name="position"></param>
		public void ShowAt(Vector3 position)
		{
			transform.position = position;
			moveMarkerAnimator.Play("move_marker_anim");
		}
	}
}