using UnityEngine;
using System.Collections;

public class NoneExplosive : Explosive
{
	// Here you can enter/override
	public override void InitializeSpecific()
	{}
	
	// emit effects
	public override void UpdateEffects()
	{}
	
	// use explosive
	public override void UseExplosive(HitData aHitData)
	{}
}
