// Stijn 16/05/2013 - Added this to extend GameObject functionality. Stumbled upon this from KGF system (see Candy Mountain Massacre 3: KGFUtility.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GameObjectExtensions : System.Object
{
	#region Extension methods for: GameObject
	
	/// <summary>
	/// SetActive for all children
	/// </summary>
	/// <param name="theActive"></param>
	public static void SetChildrenActiveRecursively(this GameObject theGameObject, bool theActive)
	{
		foreach (Transform aChildTransform in theGameObject.transform)
		{
			//TODO: maybe do something like ignore all object_scripts
			//if (aChildTransform.GetComponent<sound_audiosource_script>() == null)
			#if UNITY_4_0
			aChildTransform.gameObject.SetActive(theActive);
			#else
			aChildTransform.gameObject.SetActive(theActive);
			#endif
		}
	}

	/// <summary>
	/// Sets the layer of all base_scripts recursively.
	/// </summary>
	/// <param name='theLayer'>
	/// The layer.
	/// </param>
	public static void SetLayerRecursively(this GameObject aGameObject, int theLayer)
	{
		aGameObject.layer = theLayer;
		foreach(Transform aTransform in aGameObject.transform)
		{
			GameObject gameObject = aTransform.gameObject;
			SetLayerRecursively(gameObject,theLayer);
		}
	}
	public static List<GameObject> UnparentChildren(this GameObject aGameObject) {return UnparentChildren(aGameObject, false);}
	public static List<GameObject> UnparentChildren(this GameObject aGameObject, bool onlyMeshes)
	{
		List<GameObject> gameObjects = new List<GameObject>();
		Transform[] transforms = aGameObject.GetComponentsInChildren<Transform>();
		foreach(Transform transform in transforms)
		{
			if (onlyMeshes)
			{
				if (transform.GetComponent<MeshFilter>() != null){gameObjects.Add(transform.gameObject);transform.parent = null;} // unparent

			} else {
				gameObjects.Add(transform.gameObject);
				transform.parent = null; // unparent
			}
		}
		return gameObjects;
	}


	/// <summary>
	/// Sets the emission of POSSIBLE particle systems in children
	/// </summary>
	/// <param name="obj">Object.</param>
	/// <param name="state">If set to <c>true</c> state.</param>
	/// // RENAME???
	public static void SetEmissionInChildren(this GameObject aGameObject, bool aState)
	{
		ParticleSystem[] systems = aGameObject.GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem ps in systems)
			ps.enableEmission = aState;
	}

	/// <summary>
	/// Emits particlesystems in the children, if any
	/// </summary>
	/// <param name="obj">Object.</param>
	/// <param name="anAmount">An amount.</param>
	public static void EmitInChildren(this GameObject obj, int anAmount)
	{
		ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem ps in systems)
			ps.Emit(anAmount);
	}

	public static void PlayInChildren(this GameObject obj)
	{
		ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem ps in systems)
			ps.Play();
	}

	public static void StopInChildren(this GameObject obj)
	{
		ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem ps in systems)
			ps.Stop();
	}

	/// <summary>
	/// Enable/Disabled AudioSources in source object and its children (if any)
	/// </summary>
	/// <param name="gameObject">Game object.</param>
	/// <param name="aState">If set to <c>true</c> a state.</param>
	public static void SetAudioSourcesInChildren(this GameObject aGameObject, bool aState)
	{
		AudioSource[] sources = aGameObject.GetComponentsInChildren<AudioSource>();
		foreach (AudioSource source in sources)
			source.enabled = aState;
	}

	/// <summary>
	/// Determines if the specified gameObject is in view of the active camera.
	/// Please NOTE THIS FUNCTION ONLY WORKS WHEN USING THE CAMERAMANAGER
	/// </summary>
	/// <returns><c>true</c> if is in view the specified gameObject; otherwise, <c>false</c>.</returns>
	/// <param name="gameObject">Game object.</param>
	public static bool IsInView(this GameObject aGameObject)
	{
		if (CameraManager.activeCamera != null)
		{
			Vector3 inView = CameraManager.activeCamera.GetComponent<Camera>().WorldToScreenPoint(aGameObject.transform.position);
			if (inView.z < 0) return false;
			else 
			{
				Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
				return screenRect.Contains(inView);
			}
		}
		else return false;
	}

	#endregion

}
