using NullPointerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NullPointerEditor
{
	[CustomEditor(typeof(DLLReplaceInfo))]
	public class DLLReplaceEditor : Editor
	{
		public static string MonoScriptFileID = "11500000";

		
		[MenuItem("Assets/Generate DLLReplaceInfo", true)]
		static bool ValidateSelectedIsDLL()
		{
			// Return false if no transform is selected.
			
			if(Selection.activeObject==null)
				return false;

			return AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)) is PluginImporter;
		}
		[MenuItem("Assets/Generate DLLReplaceInfo")]
		static void ReplaceDLL()
		{
			string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			DLLReplaceInfo replacer = ScriptableObject.CreateInstance<DLLReplaceInfo>();
			replacer.DllReference = Selection.activeObject;
			replacer.DllGuid = AssetDatabase.AssetPathToGUID(assetPath);

			UnityEngine.Object [] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			foreach( UnityEngine.Object subAsset in subAssets )
			{
				Type dllClassType = (subAsset as MonoScript).GetClass();
				replacer.AddMonoScriptReference(dllClassType);
			}

			string dllInfoPath = Path.GetDirectoryName(assetPath)+Path.DirectorySeparatorChar;
			dllInfoPath+=Path.GetFileNameWithoutExtension(assetPath)+"Info.asset";
			AssetDatabase.CreateAsset(replacer, dllInfoPath);
			AssetDatabase.SaveAssets();
			Selection.activeObject = replacer;
			EditorUtility.DisplayDialog("DLLInfo Generation Completed", 
										"Now you can insert the library source code to the project and remove the DLL.\n"+
										"Once you do that, select this object again and procced to click on"+
										"'Rebuild GUID References'.\n\n"+
										"MAKE SURE YOU HAVE A COPY OF THE ENTIRE PROJECT BEFORE PROCEED.", "Got It"); 
			//string log = "MonoScripts Collected: "+refs.Count;
			//foreach(DLLReplacer.UnityReference uniref in refs)
			//	log += "\n  - (guid: "+uniref.dllGuid + ", fileID: "+uniref.dllFileId+") "+uniref.classNamespace+"."+uniref.className;
			//Debug.Log(log);

			//try {
			//	AssetDatabase.StartAssetEditing();
			//	//string path = Path.GetFullPath(".") + Path.DirectorySeparatorChar + "Assets";
			//	// RegenerateGuids (path);
			//}
			//finally {
			//	AssetDatabase.StopAssetEditing();
			//	EditorUtility.ClearProgressBar();
			//	AssetDatabase.Refresh();
			//}
			//ReplaceScriptReferences();
			//PluginImporter plugin = AssetImporter.GetAtPath(assetPath) as PluginImporter;
			//Debug.LogWarning);
			//foreach( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
			//{
			
			//	foreach( Type type in assembly.GetTypes() )
			//	{
			//		// Check if it's a serializable type (Monobehavior among others)
			//		// Also check if it's defined in a assembly that is defined in the same dll.
			//		if( type.IsSubclassOf(typeof(UnityEngine.Object)) && string.Compare(assembly.Location, fullPath) == 0 )
			//		{
			//			//AssetDatabase.Mov
			//			// it's a serializable object, probably a monobehavior.
			//			Debug.Log("Type: "+type.Name+" (Assembly: "+assembly.GetName().Name+")\nCodeBase: "+assembly.CodeBase);
			//		}
			//	}
			//}
		}

		public DLLReplaceInfo Target { get { return target as DLLReplaceInfo; } }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			GUILayout.Space(30);
			if( GUILayout.Button("Rebuild GUID References", GUILayout.Height(30)) )
			{
				if( CollectMonoScriptGUIDReferences(Target.References) )
				{
					bool accept = EditorUtility.DisplayDialog("GUIDs regeneration",
						"You are going to start the process of GUID regeneration. " +
						"This may have unexpected results. \n\n MAKE A PROJECT BACKUP BEFORE PROCEEDING!",
						"Regenerate GUIDs", "Cancel");

					if (accept)
					{
						try
						{
							AssetDatabase.StartAssetEditing();
							//string path = Path.GetFullPath(".") + Path.DirectorySeparatorChar + "Assets";
							ReplaceReferences(Application.dataPath, Target.References);
						}
						finally
						{
							AssetDatabase.StopAssetEditing();
							EditorUtility.ClearProgressBar();
							AssetDatabase.Refresh();
						}
					}
						
				}
				else
				{
					EditorUtility.DisplayDialog("GUIDs regeneration",
												"Not all the missing references were found.\n"+
												"Please, check that you have the correct version of the library sources." +
												"Canceling Procedure.", "Ok");
				}
			}
		}

		public bool CollectMonoScriptGUIDReferences(IEnumerable<DLLReplaceInfo.UnityReference> refs)
		{
			Dictionary<string, string> monoScriptGUIDs = CollectMonoScriptGUIDs();
			bool result = true;
			//Type monoScriptType = typeof(MonoScript);
			foreach( DLLReplaceInfo.UnityReference uniref in refs )
			{
				uniref.monoGuid = "";
				monoScriptGUIDs.TryGetValue(uniref.FullName, out uniref.monoGuid);
				uniref.monoFileId = MonoScriptFileID;	
				if(string.IsNullOrEmpty(uniref.monoGuid) )
					result = false;
			}
			return result;
		}

		private Dictionary<string, string> CollectMonoScriptGUIDs()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			string[] scriptsGuids = AssetDatabase.FindAssets("t:MonoScript");
			foreach(string guid in scriptsGuids)
			{
				MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
				if(mono!= null && mono.GetClass()!=null && !result.ContainsKey(mono.GetClass().FullName))
					result.Add(mono.GetClass().FullName, guid);
			}
			return result;
		}

		static void ReplaceReferences(string assetFolder, IEnumerable<DLLReplaceInfo.UnityReference> r)
		{
			string[] files = Directory.GetFiles(assetFolder, "*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];
 
				if (EditorUtility.DisplayCancelableProgressBar("Replace GUIDs", file, i/(float)files.Length))
				{
					EditorUtility.ClearProgressBar();
					return;
				}
 
				if (file.EndsWith(".asset") || file.EndsWith(".prefab") || file.EndsWith(".unity"))
				{
					ReplaceInFile(file, r);
					//FindNotReplacedFiles(file, "e20699a64490c4e4284b27a8aeb05666");
				}
			}
 
			EditorUtility.ClearProgressBar();
		}
 
		static void ReplaceInFile(string filePath, IEnumerable<DLLReplaceInfo.UnityReference> references)
		{
			var fileContents = File.ReadAllText(filePath);
         
			bool match = false;
         
			foreach(DLLReplaceInfo.UnityReference r in references)
			{
				Regex regex = new Regex(@"fileID: " + r.dllFileId + ", guid: " + r.dllGuid);
				if (regex.IsMatch(fileContents))
				{
					fileContents = regex.Replace(fileContents, "fileID: " + r.monoFileId + ", guid: " + r.monoGuid);
					match = true;
					//Debug.Log("Replaced: " + filePath);
				}
			}
         
			if (match)
			{
				File.WriteAllText(filePath, fileContents); 
			}
		}
 
		/// <summary>
		/// Just to make sure that all references are replaced.
		/// </summary>
		static void FindNotReplacedFiles(string filePath, string guid)
		{
			var fileContents = File.ReadAllText(filePath);
         
			// -?        number can be negative
			// [0-9]+    1-n numbers
			Regex.Replace(fileContents, @"fileID: -?[0-9]+, guid: " + guid, 
				(match) =>
					{
					if (match.Value != "fileID: 11500000, guid: " + guid)
					{
						Debug.LogWarning("NotReplaced: " + match.Value + "  " + filePath);
					}
					return match.Value;
				});
		}

	}
}