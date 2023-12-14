// Descrition : Use to create list of default inputs values
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Inventory/ListInputsValues", order = 1)]
public class DefaultInputsValues : ScriptableObject {
	[System.Serializable]
	public class _ListOfInputs{
		public List<string> Desktop = new List<string>();												// Inputs Desktop
		public List<string> Gamepad = new List<string>();												// Inputs Gamepad

		public _ListOfInputs(){}
	}

	public List<_ListOfInputs> 					ListOfInputs = new List<_ListOfInputs>();			// list of Inputs on Hierarchy

}
