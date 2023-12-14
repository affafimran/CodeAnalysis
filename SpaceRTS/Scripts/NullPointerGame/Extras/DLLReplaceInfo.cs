using NullPointerGame.Extras;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerCore
{
	public class DLLReplaceInfo : ScriptableObject
	{

		/// <summary>
		/// Helper clas to storage all the script references info.
		/// </summary>
		[System.Serializable]
		public class UnityReference
		{
			public string className;
			public string classNamespace;
			public string dllGuid;
			public string dllFileId;
			public string monoGuid;
			public string monoFileId;

			public string FullName { get { return (!string.IsNullOrEmpty(classNamespace)?classNamespace+".":"")+className; } }

			public UnityReference() { }

			public UnityReference(string dllGuid, string dllFileId, string monoGuid, string monoFileId)
			{
				this.dllGuid = dllGuid;
				this.dllFileId = dllFileId;
				this.monoGuid = monoGuid;
				this.monoFileId = monoFileId;
			}
        
		}

		[SerializeField]
		private Object dllReference;
		[SerializeField]
		private string dllGuid;
		[SerializeField]
		private List<UnityReference> refs = new List<UnityReference>();

		private Dictionary<string, int> indexedByClass = null;

		public string DllGuid {	get	{ return dllGuid; }	set	{ dllGuid = value; } }
		public Object DllReference {	get	{ return dllReference; }	set	{ dllReference = value; } }
		public IEnumerable<UnityReference> References { get { return refs; } }

		public Dictionary<string, int> RefIndex 
		{
			get
			{
				if(indexedByClass==null)
				{
					indexedByClass = new Dictionary<string, int>();
					for(int i=0; i<refs.Count; i++)
						indexedByClass.Add(refs[i].classNamespace+"."+refs[i].className, i);
				}
				return indexedByClass;
			}
		}

		UnityReference GetValid(string classFullName)
		{
			int index = 0;
			if( RefIndex.TryGetValue(classFullName, out index) )
				return refs[index];
			UnityReference result = new UnityReference();
			RefIndex.Add(classFullName, refs.Count);
			refs.Add( result );
			return result;
		}

		public void AddMonoScriptReference(System.Type classType)
		{
			UnityReference uniref = GetValid(classType.FullName);
			uniref.dllGuid = dllGuid;
			uniref.dllFileId = FileIDUtil.Compute(classType.Namespace, classType.Name).ToString();
			uniref.classNamespace = classType.Namespace;
			uniref.className = classType.Name;
		}
	}
}
