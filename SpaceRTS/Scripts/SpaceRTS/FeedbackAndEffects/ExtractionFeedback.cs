using System;
using GameBase;
using NullPointerGame.Extras;
using NullPointerGame.ResourceSystem;
using UnityEngine;
using System.Collections;

namespace SpaceRTS.FeedbackAndEffects
{
	public class ExtractionFeedback : GameEntityComponent
	{
		/// <summary>
		/// Proxy reference to the PArticleAttractor used as extraction feedback
		/// </summary>
		public ProxyRef extractionFX = new ProxyRef(typeof(ParticleAttractor), "extraction_fx");

		private ResourceCarrier carrier;
		private IEnumerator extractRoutine = null;

		void Start ()
		{
			carrier = GetComponent<ResourceCarrier>();
			if(carrier!=null)
			{
				carrier.onCargoTransferStarted += ExtractionStarted;
				carrier.onCargoTransferEnded += ExtractionEnd;
			}
		}

		void OnDestroy()
		{
			if(carrier!=null)
			{
				carrier.onCargoTransferStarted -= ExtractionStarted;
				carrier.onCargoTransferEnded -= ExtractionEnd;
			}
		}

		/// <summary>
		/// Called when the Visual Module is setted. Here we need to initialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleSetted()
		{
			extractionFX.SafeAssign(ThisEntity);
		}

		/// <summary>
		/// Called when the Visual Module is removed. Here we need to uninitialize all the component related functionality.
		/// </summary>
		override public void OnVisualModuleRemoved()
		{
			extractionFX.SafeClear();
		}

		private void ExtractionStarted()
		{
			if(ProxyRef.IsInvalid(extractionFX))
				return;
			if( carrier.IsTransferUnloading )
				return;

			extractRoutine = ExtractFXRefresh();
			StartCoroutine(extractRoutine);
		}

		private void ExtractionEnd()
		{
			if(extractRoutine!=null)
			{
				StopCoroutine(extractRoutine);
				extractRoutine = null;
			}
			if(ProxyRef.IsInvalid(extractionFX))
				return;
			if( carrier.IsTransferUnloading )
				return;
			ParticleAttractor pa = extractionFX.Get<ParticleAttractor>();
			if(pa!=null)
				pa.gameObject.SetActive(false);
		}

		IEnumerator ExtractFXRefresh()
		{
			Vector3 extractionPoint = carrier.AssignedWarehouse.transform.position;
			ParticleAttractor pa = extractionFX.Get<ParticleAttractor>();
			while(true)
			{
				if(pa!=null)
				{
					pa.gameObject.SetActive(true);
					pa.transform.position = extractionPoint;
				}
				yield return null;
			}
		}
	}
}