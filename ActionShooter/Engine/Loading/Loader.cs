// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE

// Stijn 17/01/2013
// Loader has been introduced to load from either resources or bundles
// feel free to add functions to load different types of objects
// maybe we can use a template somehow

using UnityEngine;
using System.Collections;

public class Loader
{
	// Note in LoadObject you must specify the path in the aPathObjectName parameter (e.g. "Misc/Crate_Prefab") and cast the result to what it is
	// I'd prefer not to use it, but I think it can be handy sometimes
	public static Object LoadObject(string aPathObjectName)
	{
		return Resources.Load(aPathObjectName);
	}

	// Default behaviour of LoadGameObject is that it always instantiates a new game object, it shouldn't modify the prefab!
	public static GameObject LoadGameObject(string aPathGameObjectName)
	{
		Object tObject;
		tObject = Resources.Load("Prefabs/" + aPathGameObjectName, typeof(GameObject));

		if (tObject != null){
			GameObject tGameObject = Object.Instantiate(tObject) as GameObject;
			tGameObject.name = aPathGameObjectName.Split('/')[1]; 
			return tGameObject;
		} else return null;
	}

	// Default behaviour of LoadMaterial is that it DOESN'T instantiate the material, so if you modify this material you'll also be modifying it in the project/bundle (bad)
	public static Material LoadMaterial(string aPathMaterialName)
	{
		return Resources.Load("Materials/" + aPathMaterialName, typeof(Material)) as Material;
	}

	// Default behaviour of LoadTexture is that it DOESN'T instantiate the texture, so if you modify this texture you'll also be modifying it in the project/bundle (bad)
	public static Texture LoadTexture(string aPathTextureName)
	{
		return Resources.Load("Textures/" + aPathTextureName, typeof(Texture)) as Texture;
	}

	public static TextAsset LoadTextFile(string aPathTextFileName)
	{
		return Resources.Load(aPathTextFileName, typeof(TextAsset)) as TextAsset;
	}

	public static AudioClip LoadAudio(string anAudioSourceName)
	{
		return Resources.Load("Audio/" + anAudioSourceName, typeof(AudioClip)) as AudioClip;
	}

	public static PhysicMaterial LoadPhysicMaterial(string aPathPhysicMaterialName)
	{
		return Resources.Load("PhysicMaterials/" + aPathPhysicMaterialName, typeof(PhysicMaterial)) as PhysicMaterial;
	}
}
