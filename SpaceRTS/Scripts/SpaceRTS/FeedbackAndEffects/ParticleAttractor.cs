using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.Extras
{
	/// <summary>
	/// Special FX component that attracts particles to a point
	/// </summary>
	public class ParticleAttractor : MonoBehaviour
	{
		// The particle system to operate on
		public ParticleSystem AffectedParticles = null;
	
		/// <summary>
		/// Normalized treshold on the particle lifetime
		/// 0: affect particles right after they are born
		/// 1: never affect particles
		/// </summary>
		[Range(0.0f, 1.0f)]
		public float ActivationTreshold = 1.0f;
		/// <summary>
		/// How strong is the attraction factor
		/// </summary>
		//[Range(0.0f, 1.0f)]
		public float attractionFactor = 1.0f;

		public Transform target=null;
		public Vector3 targetOffset;

		// Array to store particles info
		private ParticleSystem.Particle[] m_rParticlesArray = null;
 
		public Vector3 TargetPositionBase { get { return target!=null? target.position : transform.position; } }
		public Vector3 TargetPositionFinal { get { return TargetPositionBase + targetOffset; } }

		void Awake()
		{
			// Setup particle system info
			Setup();
		}

		void LateUpdate()
		{
			// Work only if we have something to work on :)
			if(AffectedParticles == null)
				return;

			if(m_rParticlesArray==null || m_rParticlesArray.Length != AffectedParticles.main.maxParticles)
				m_rParticlesArray = new ParticleSystem.Particle[AffectedParticles.main.maxParticles];

			// Is this particle system simulating in world space?
			bool worldPosition = AffectedParticles.main.simulationSpace == ParticleSystemSimulationSpace.World;
			// Let's fetch active particles info
			int numActiveParticles = AffectedParticles.GetParticles(m_rParticlesArray);
			// The attractor's target is it's world space position
			Vector3 particlesTarget = TargetPositionFinal;
			// If the system is not simulating in world space, let's project the attractor's target in the system's local space
			if (!worldPosition)
				particlesTarget -= AffectedParticles.transform.position;

			float lerpValue = attractionFactor * Time.deltaTime;
			// For each active particle...
			for(int iParticle = 0; iParticle < numActiveParticles; iParticle++) { // The movement cursor is the opposite of the normalized particle's lifetime m_fCursor = 1.0f - (m_rParticlesArray[iParticle].lifetime / m_rParticlesArray[iParticle].startLifetime); // Are we over the activation treshold? if (m_fCursor >= ActivationTreshold)
				{
					if(m_rParticlesArray[iParticle].startLifetime==0)
						continue;
					// Let's project the overall cursor in the "over treshold" normalized space
					float currLifetimeFactor = m_rParticlesArray[iParticle].remainingLifetime / m_rParticlesArray[iParticle].startLifetime;
					if(currLifetimeFactor > ActivationTreshold)
						continue;
					
 					m_rParticlesArray[iParticle].position = Vector3.Lerp(m_rParticlesArray[iParticle].position, particlesTarget, lerpValue);
				}
			}

			// Let's update the active particles
			AffectedParticles.SetParticles(m_rParticlesArray, numActiveParticles);
		}

		public void Setup()
		{
			// If we have a system to setup...
			if (AffectedParticles != null)
			{
				// Prepare enough space to store particles info
				m_rParticlesArray = new ParticleSystem.Particle[AffectedParticles.main.maxParticles];
			}
		}

		public void OnDrawGizmosSelected()
		{
			//Gizmos.DrawWireSphere(transform.position, 1.0f);
			if(AffectedParticles!=null)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(AffectedParticles.transform.position, 1.0f);
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(TargetPositionFinal, 0.5f);
			}
		
		}
	}
}