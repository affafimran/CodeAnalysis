using GameBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	public class ProjectileBeam : Projectile
	{
		[Header ("Beam Config")]
		public MeshRenderer beamRenderer;

		public float maxDistance = 6.0f;

		public MeshRenderer beam;
		public MeshRenderer flare;
		public Transform beamScaler;
		public float beamLength=120.0f;

		//private GameObject explosionObject;
		private Vector3 beamScale = Vector3.one;

		// Use this for initialization
		public void Start () 
		{
			RefreshBeamPositionAndDirection();
			//RefreshExplosionPosition();
		}

		protected override Transform GetEmitter(GameEntity source, Damageable target)
		{
			return GetBestFireSource( source, target.transform.position );
		}

		protected override Vector3 GetTargetOffset(GameEntity source, Damageable target)
		{
			if(target==null)
				return Vector3.zero;
			return target.GetClosestDamagePoint(source.transform.position);
		}

		public override void InitSetup(GameEntity source, Damageable target)
		{
			base.InitSetup(source, target);
			if( target != null )
				ApplyHitEffect(target, lockedPointOnTarget);
		}

		// Update is called once per frame
		void Update () 
		{
			RefreshBeamPositionAndDirection();
			//flare.material.mainTextureOffset = CalcAnimatedOffset(4,4,Time.realtimeSinceStartup, 12);
			//beam.material.mainTextureOffset = CalcAnimatedOffset(1,8,Time.realtimeSinceStartup, 12);
		}

		static public Vector2 CalcAnimatedOffset( int rows, int cols, float time, float rate )
		{
			int imgIndex = Mathf.RoundToInt( (time * rate) % (rows * cols) );
			return new Vector2( (imgIndex % cols) / (float)cols, (imgIndex / cols) / (float)rows );
		}

		protected override void OnDisarmed()
		{
			base.OnDisarmed();
			AutoDestroy();
		}

		void RefreshBeamPositionAndDirection()
		{
			// We refresh the beam source position (our position) only if we have a source point assigned
			if( sourceEmitter != null )
				transform.position = sourceEmitter.position;

			// If we have a target assigned we should look at it. Also, we are taking into account the specific lockedPointOnTarget.
			if( trackedTarget != null )
			{
				Vector3 finalBeamPosition = trackedTarget.transform.TransformPoint( lockedPointOnTarget );
				transform.LookAt( finalBeamPosition );
				beamLength = Vector3.Distance( transform.position, finalBeamPosition );
			}
			else
				beamLength = maxDistance;

			beamScale.x = beamLength;
			beamScale.z = beamScale.y = 0.5f;
			beamScaler.localPosition = Vector3.forward * beamLength / 2;
			beamScaler.localScale = beamScale;
		}
	}
}