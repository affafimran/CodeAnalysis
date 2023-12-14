using GameBase;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Manager system that defines the list of the resources that the scene will have.
	/// </summary>
	public class ResourceSystem : GameSceneSystem
	{
		/// <summary>
		/// The actual list of defined resources that the game scene has.
		/// </summary>
		public List<string> definedResources = new List<string>();

		/// <summary>
		/// The list of the defined resources in the game system.
		/// </summary>
		public List<string> DefinedResources { get { return definedResources; } }
		/// <summary>
		/// Returns the quantity of resources defined in the system.
		/// </summary>
		public int ResourcesCount { get { return definedResources.Count; } }

		protected override void OnValidate()
		{
			base.OnValidate();
			if(definedResources.Count==0)
				definedResources.Add("default");
		}

		/// <summary>
		/// Returns the list of defined resource names.
		/// </summary>
		/// <returns>The list of defined resource names.</returns>
		public static List<string> FindResourcesNames ()
		{
			ResourceSystem resourceSystem = GameObject.FindObjectOfType<ResourceSystem>();
			if (resourceSystem != null)
				return resourceSystem.DefinedResources;
			else
				return new List<string>();
		}
		/// <summary>
		/// Returns the resource name that correspond to the given index
		/// </summary>
		/// <param name="index">The index of the requested resource.</param>
		/// <returns>The name of the requested resource or "Undefined" if some problem was encounter.</returns>
		public static string GetResourceName(int index)
		{
			List<string> resourceList = FindResourcesNames ();
			if(index < 0 || index >= resourceList.Count)
				return "Undefined";
			return resourceList[index];
		}
		/// <summary>
		/// Given a resource name returns the resource index that correspond to it.
		/// </summary>
		/// <param name="resourceName">The name of the resource whose index wants to find.</param>
		/// <returns>The resource index that correspond to the given resource name.</returns>
		public static int GetResourceIndex(string resourceName)
		{
			ResourceSystem resourceSystem = GameObject.FindObjectOfType<ResourceSystem>();
			if (resourceSystem != null)
				return resourceSystem.DefinedResources.IndexOf(resourceName);
			return -1;
		}
	}
}
