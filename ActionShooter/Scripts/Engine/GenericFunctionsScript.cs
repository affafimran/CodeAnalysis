// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class GenericFunctionsScript
{
	public const float COLORINTTOFLOAT = 1/255.0f;

	// Written to also find an inactive child in the scene
	public static GameObject FindChild(GameObject aGameObject, string aName)
	{
		if (aGameObject == null || aName == null || aName == "") return null;
		Transform tChild;
		GameObject tFoundGO = null;
		for (int i = aGameObject.transform.childCount - 1; i >= 0; --i)
		{
			tChild = aGameObject.transform.GetChild(i);
			tFoundGO = FindChild(tChild.gameObject, aName);
			if (tFoundGO != null) return tFoundGO;
			if (aGameObject.transform.GetChild(i).gameObject.name == aName) return aGameObject.transform.GetChild(i).gameObject;
		}
		return null;
	}

	public static GameObject FindChildSimple(GameObject aGameObject, string aName)
	{
		Transform tChild;
		for (int i = aGameObject.transform.childCount - 1; i >= 0; --i)
		{
			tChild = aGameObject.transform.GetChild(i);
			if (tChild.gameObject.name == aName) return tChild.gameObject;
		}
		return null;
	}

	public static GameObject FindChildAbsolute(GameObject aGameObject, string[] aNameList)
	{
		if (aNameList.Length == 0) return aGameObject;
		return FindChildAbsolute(aGameObject, aNameList, 0);
	}

	public static List<GameObject> FindGameObjectsWithLayer(int aLayer)
	{
		GameObject[] tGoArray = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		List<GameObject> tGoList = new List<GameObject>();
		for (int i = tGoArray.Length - 1; i >= 0; --i)
		{
			if (tGoArray[i].layer == aLayer)
			{
				tGoList.Add(tGoArray[i]);
			}
		}
		if (tGoList.Count == 0)
			return null;
		return tGoList;
	}

	private static GameObject FindChildAbsolute(GameObject aGameObject, string[] aNameList, int aIndex)
	{
		if (aIndex == aNameList.Length) return aGameObject;
		Transform tChild;
		for (int i = aGameObject.transform.childCount - 1; i >= 0; --i)
		{
			tChild = aGameObject.transform.GetChild(i);
			if (tChild.name == aNameList[aIndex])
			{
				return FindChildAbsolute(tChild.gameObject, aNameList, aIndex + 1);
			}
		}
		return null;
	}

	// Not too sure on this function, lets see how it turns out...
	public static GameObject FindTopParentInCurrentLayer(GameObject aGameObject, int aLayer)
	{
		GameObject topParent = aGameObject;
		GameObject rootParent = aGameObject.transform.root.gameObject;
		if (topParent == rootParent) return topParent;
		GameObject currentParent = null;
		for (int i = 0; i < 15; i++) 
		{
			if (topParent == rootParent) return topParent;
			currentParent = topParent.transform.parent.gameObject;
			if (currentParent.layer == aLayer) topParent = currentParent;
			else return topParent;
		}
		return topParent;
	}

	// DEPRICATED. Create a sprite that fades in or out.
	public static void Fade(string fadePreset)
	{
		 Debug.Log("GenericFunctionsScript Fade received: " + fadePreset + ", DISABLED FOR NOW!!!");
	}

	public static void Medal(int aMedalNumber)
	{
		Debug.Log("GenericFunctionsScript Medal received: " + aMedalNumber.ToString() + ", DISABLED FOR NOW!!!");
	}

	// this is used to convert a list with 3 float entries to a vector, in the future we should probably parse this immediately from the TextLoader (.v)
	public static Vector3 VectorFromList(List<DicEntry> aList)
	{
		return new Vector3(aList[0].f, aList[1].f, aList[2].f);
	}

	// this is used to convert a list with 3 float entries to a vector, in the future we should probably parse this immediately from the TextLoader (.v)
	public static Vector2 Vector2FromList(List<DicEntry> aList)
	{
		return new Vector2(aList[0].f, aList[1].f);
	}

	// this is used to convert a list with 3 float entries to a vector, in the future we should probably parse this immediately from the TextLoader (.v)
	public static Vector3 Vector3FromList(List<DicEntry> aList)
	{
		return new Vector3(aList[0].f, aList[1].f, aList[2].f);
	}



	// this is used to convert a list with 3 (or 4) float (or int) entries to a color, in the future we should probably parse this immediately from the TextLoader (.c)
	public static Color ColorFromList(List<DicEntry> aList)
	{
		if (aList[0].type == DicEntry.EntryType.FLOAT)
		{  // specified in floats, no conversion needed (faster)
			if (aList.Count == 3)  // 3 components in color
				return new Color(aList[0].f, aList[1].f, aList[2].f);
			else  // 4 components in color, or so we assume
				return new Color(aList[0].f, aList[1].f, aList[2].f, aList[3].f);
		} else
		{  // specified in ints, convert to float
			if (aList.Count == 3)  // 3 components in color
				return new Color(aList[0].i*COLORINTTOFLOAT, aList[1].i*COLORINTTOFLOAT, aList[2].i*COLORINTTOFLOAT);
			else  // 4 components in color, or so we assume
				return new Color(aList[0].i*COLORINTTOFLOAT, aList[1].i*COLORINTTOFLOAT, aList[2].i*COLORINTTOFLOAT, aList[3].i*COLORINTTOFLOAT);
		}
	}

	public static float StandardSCurve(float x)
	{  // up to the user to make sure 0 <= x < = 1
		// Standard S-curve: 3*x^2 - 2*x^3
		return (3 - 2*x) * x*x;
	}

	public static float StandardSCurveInverseY(float x)
	{  // up to the user to make sure 0 <= x < = 1
		// Standard S-curve: 1 - (3*x^2 - 2*x^3)    1 - to inverse Y value
		return 1.0f - (3 - 2*x) * x*x;
	}

	public static float StandardParabole(float x)
	{  // up to the user to make sure 0 <= x < = 1
		return x*x;
	}

	// 1|    *****
	//  |  **
	//  | *
	//  |*
	//  |*
	// 0|--------- 1
	public static float StandardParaboleInverseY(float x)
	{  // up to the user to make sure 0 <= x < = 1
		float tX = (x - 1);
		return 1-tX*tX;
	}

	public static IEnumerator WaitForSecondsSkippable(float aDelaySeconds)
	{
		float tTimer = Time.time + aDelaySeconds;
		while ((Time.time < tTimer) && (!Input.GetKeyUp(KeyCode.Space)))
		{
			yield return null;
		}
	}

	public static string ConvertTimeToStringMSS(int intTime){ return ConvertTimeToStringMSS((float)intTime * 0.001f); } 
	public static string ConvertTimeToStringMSS(float aTime)
	{
		int tSec = Mathf.FloorToInt(aTime);
		//int tCentisec = (int)((aTime - tSec)*100);  // 2 numbers for millisec (so centisec)
		int tMinutes = (int)(tSec / 60);
		int tSeconds = tSec % 60;
		string tTimerText = "";

		// minutes (pad with a 0 if necessary)
		if (tMinutes <= 9)
		{
			tTimerText = tTimerText + tMinutes;
			tTimerText = tTimerText + ":";
			// seconds (pad with a 0 if necessary)
			if (tSeconds > 9) tTimerText = tTimerText + tSeconds;
			else              tTimerText = tTimerText + "0" + tSeconds;
//			tTimerText = tTimerText + ".";
//			if (tCentisec > 9) tTimerText = tTimerText + tCentisec;
//			else               tTimerText = tTimerText + "0" + tCentisec;
		} else
		{  // tMinutes > 9
			tTimerText = "9:59";
		}
		return tTimerText;
	}

	public static string AddSeparatorInInt(int tInt, string aSeperator)
	{
		string tString = tInt.ToString();
		int tLength = tString.Length;
		for (int i = tLength-3; i > 0; i -= 3)
			tString = tString.Insert(i, aSeperator);
		return tString;
	}

	public static Transform Search(Transform target, string name)
	{
		if (target.name == name) return target;
		for (int i = 0; i < target.childCount; ++i)
		{
			var result = Search(target.GetChild(i), name);
			if (result != null) return result;
		}
		return null;
	}

	// makes a copy of the list with the DicEntries copied as well (except for dictionaries and lists)
	public static List<DicEntry> CopyList(List<DicEntry> aList)
	{
		List<DicEntry> tList = new List<DicEntry>();
		int tCount = aList.Count;
		for (int i = 0; i < tCount; ++i)
			tList.Add(new DicEntry(aList[i]));
		return tList;
	}

	//-------------------------------------------------------------------------------------------------------
	// Experimental!
	//-------------------------------------------------------------------------------------------------------
	public static object ParseDictionaryToClass(Dictionary<string, DicEntry> aDictionary, object aClass)
	{
		FieldInfo[] properties = aClass.GetType().GetFields();
		foreach(FieldInfo prop in properties)
		{
			// Commented for now since it displays a lot of logs with information... which is not necessary since this 
			// should fail/skipped silently
			if (!aDictionary.ContainsKey(prop.Name)){ continue;} // Debug.Log("Couldn't find: " + prop.Name + " in the Dictionary"); }	
			
			// set it
			if (prop.FieldType == typeof(bool))   prop.SetValue(aClass, Convert.ToBoolean(aDictionary[prop.Name].b)); // bool
			if (prop.FieldType == typeof(string)) prop.SetValue(aClass,                   aDictionary[prop.Name].s ); // string
			if (prop.FieldType == typeof(int))    prop.SetValue(aClass,   Convert.ToInt32(aDictionary[prop.Name].i)); // int
			if (prop.FieldType == typeof(float))  prop.SetValue(aClass,  Convert.ToSingle(aDictionary[prop.Name].f)); // float
			if (prop.FieldType == typeof(List<string>)) // list (AS STRING ONLY NOW!)
			{
				List<string> list = new List<string>();
				foreach(DicEntry entry in aDictionary[prop.Name].l)
					list.Add(entry.s);
				prop.SetValue(aClass, list);
			}
			if (prop.FieldType == typeof(Vector3))   prop.SetValue(aClass, GenericFunctionsScript.VectorFromList(aDictionary[prop.Name].l)); // vector3
			if (prop.FieldType == typeof(Vector2))   prop.SetValue(aClass, GenericFunctionsScript.Vector2FromList(aDictionary[prop.Name].l)); // vector2
			if (prop.FieldType.IsEnum) prop.SetValue(aClass, System.Enum.Parse(prop.FieldType, aDictionary[prop.Name].s)); // enum

		}
		return aClass;
	}

	/// <summary>
	/// Gets the parent below by raycasting. So you would need colliders.
	/// This is not an optimized function since we're not using a layermask.
	/// </summary>
	/// <returns>Transform of the collider hit. Null if nothing hit.</returns>
	/// <param name="aPosition">A position as Vector3</param>
	public static Transform GetParentBelow(Vector3 aPosition)
	{
		Transform transform = null;
		Vector3 offset = Vector3.up * 0.025f; // Small offset
		Ray ray = new Ray(aPosition + offset, Vector3.down);
		RaycastHit rayCastHit;
		if (Physics.Raycast(ray, out rayCastHit, 100)) transform = rayCastHit.collider.transform;
		return transform;
	}



	public static Type GetType(string aComponent)
	{
		#if UNITY_WEBGL
			//Debug.Log("Component through Assembly.Load()");	
			return Assembly.Load("Assembly-CSharp").GetType(aComponent);			
		#else
			//Debug.Log("Component through System.Type.GetType()");
			return System.Type.GetType(aComponent);			
		#endif
	}



}
