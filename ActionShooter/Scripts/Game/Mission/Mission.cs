using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mission behavior.
/// <para>Simple script </para>
/// </summary>
public class Mission : MonoBehaviour
{

	public MissionData missionData;
	public List<MissionData> missionProgress;

	// these are not use but can be worthwhile
	public bool nextMission = false; // should we go to the next mission
	public float nextMissionCounter = 3f; // how long should we wait for the next mission if we're allowed

	/// <summary>
	/// Initialize this mission.
	/// <para>We store both missionData and missionProgress so you can see/check them in the inspector.</para>
	/// </summary>
	/// <param name="aMissionData">A mission data.</param>
	/// <param name="aMissionProgress">A mission progress.</param>
	public void Initialize(MissionData aMissionData, List<MissionData> aMissionProgress)
	{
		missionData = aMissionData;
		missionProgress = aMissionProgress;
	}

	void Update()
	{
		if (nextMission){
			nextMissionCounter -= Time.deltaTime; // countdown to next mission
			if (nextMissionCounter <= 0f) Scripts.levelScript.NextLevel(); // Ideally this should go through the MissionManager
		}else{
			// calculate and store total mission time
			if (MissionManager.missionInProgress){
				int time = (int)(Time.deltaTime*1000); 
				missionData.time += time;
			}
		}
	}
}

