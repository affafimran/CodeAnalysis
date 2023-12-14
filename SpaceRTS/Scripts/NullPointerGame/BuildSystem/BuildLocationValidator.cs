using GameBase;
using GameBase.RTSKit;
using NullPointerCore.Extras;
using SpaceRTS.FeedbackAndEffects;
using System.Collections;
using UnityEngine;

namespace NullPointerGame.Spatial
{
	/// <summary>
	/// Validates the build placement selected by the user. Also handles the visual feedback.
	/// </summary>
	public class BuildLocationValidator : GameEntityComponent
	{
		/// <summary>
		/// Contains the reference for the script that will control the visual feedback of 
		/// the unit to indicate if can be built at the specific point or not.
		/// </summary>
		public ProxyRef fxBuildValidator = new ProxyRef(typeof(FXValidBuildLocation), "buildhandler_fx");
		public ProxyRef buildRadius = new ProxyRef(typeof(SphereCollider), "build_radius");
		public float radius = 10.0f;
		public bool isAtValidPosition = false;
		[SpatialAreaMask]
		public int validBuildAreas = 1;

		private IEnumerator checkRoutine = null;
		private FXValidBuildLocation fxvbl = null;

		public override void OnVisualModuleSetted()
		{
			fxBuildValidator.SafeAssign(ThisEntity);
			buildRadius.SafeAssign(ThisEntity);

			if(ProxyRef.IsInvalid(fxBuildValidator))
			{
				Debug.LogWarning("Invalid fxBuildValidator reference.", this.gameObject);
				return;
			}
			else
				fxvbl = fxBuildValidator.Get<FXValidBuildLocation>();

			if(ProxyRef.IsInvalid(buildRadius))
				Debug.LogWarning("Invalid buildRadius reference.", this.gameObject);
			else
				radius = buildRadius.Get<SphereCollider>().radius;
			
			checkRoutine = UpdatePositionTick();
			StartCoroutine(checkRoutine);
			base.OnVisualModuleSetted();
		}

		public override void OnVisualModuleRemoved()
		{
			if(checkRoutine!=null)
			{
				StopCoroutine(checkRoutine);
				checkRoutine = null;
			}
			fxBuildValidator.SafeClear();
			buildRadius.SafeClear();
			base.OnVisualModuleRemoved();
		}

		// Update is called once per frame
		IEnumerator UpdatePositionTick ()
		{
			SceneBounds bounds = GetSceneSystem<SceneBounds>();
			SpatialSystem spatial = GetSceneSystem<SpatialSystem>();
			if(spatial==null)
			{
				Debug.LogError("SpatialSystem not properly configured. ", this.gameObject);
				checkRoutine = null;
				yield break;
			}
			float distance;
			Vector3 surfacePosition;
			while(true)
			{
				spatial.SamplePosition(bounds.CursorLookPoint, out surfacePosition, spatial.DefaultAllAreasMask);
				transform.position = surfacePosition;

				if (spatial.GetClosestEdgeDistance(transform.position, out distance, validBuildAreas) )
					isAtValidPosition = distance >= radius;
				else
					isAtValidPosition = false;

				fxvbl.enabled = true;

				if( isAtValidPosition )
					fxvbl.SetAsValid();
				else
					fxvbl.SetAsInvalid();

				yield return null;
			}
		}

		/// <summary>
		/// Simple refresh position method using the current cursor position over the gameplay surface.
		/// Don't use it in a update loop, it's unessary costly, it was meant to be used in the first 
		/// visible frame to fix a ugly glitch
		/// </summary>
		public void RefreshPosition()
		{
			SceneBounds bounds = GetSceneSystem<SceneBounds>();
			SpatialSystem spatial = GetSceneSystem<SpatialSystem>();
			if(spatial==null)
			{
				Debug.LogError("SpatialSystem not properly configured. ", this.gameObject);
				checkRoutine = null;
				return;
			}
			Vector3 surfacePosition;
			spatial.SamplePosition(bounds.CursorLookPoint, out surfacePosition, spatial.DefaultAllAreasMask);
			transform.position = surfacePosition;
		}

		public void OnDrawGizmos()
		{
			if(ThisEntity!=null && ThisEntity.VisualProxy != null)
			{
				Gizmos.color = Color.green;
				GizmosExt.DrawWireCircle(transform.position, radius);
			}
		}
	}
}
