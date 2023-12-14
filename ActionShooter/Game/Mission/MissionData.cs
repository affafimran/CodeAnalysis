using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// MissionData.
/// <para>Contains all data mission related.</para>
/// </summary>
[System.Serializable]
public class MissionData
{
	public int 		mission 	  = 1;  					// mission number
	[HideInInspector] public bool mainContentBuilt = false;
	public bool 	completed	  = false;					// completed tag?
	public string 	target 		  = "Default";				// target
	public int 		targetAmount  = 1;						// amount to destroy/remove kill
	public int 		targetsLeft   = 1;						// targets left
	public int		time		  = 0;						// This is the actual time that we're going to use
	public int 		targetTime    = 999999;					// target time
	public int		totalTime     = 0;						// total time taken per mission
	public int 		score 		  = 0;						// score gained
	public int		targetScore   = 0;						// target score
	public int      totalScore    = 0;						// total score gained per mission
	public bool		hiddenPackage = false;					// hidden package collected
	public string spawnLocation   = "Default";				// Hammer spawnLocation
	public List<string> enemySet  = new List<string>(); 	// which enemies are allowed
	public List<string> rewards   = new List<string>(); 	// rewards, if implemented
	public string	ambientStyle  = "Level1";				// Ambientstyle
}