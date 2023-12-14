using UnityEngine;
using System.Collections;

// For some odd reason we cannot seem to avoid AnimationEvents being 'available' even after animations which contain them are being overriden by others
// Once in while it could happen that (maybe in the same frame) the AnimationEvent is calling a function while we just removed a component because the character is dead
// I would really like some insight on this as I don't want to have the full character component active or available for 'loose' and unnecessary Animation Events that won't add anything sice you're dead
public class CharacterAnimationEventCatcher : MonoBehaviour
{
	public virtual void SetAnimation(string anAnimation){Debug.Log("[CharacterAnimationEventCatcher] We're dead, but still let's try to SET: " + anAnimation);}	
	public virtual void ResetAnimation(string anAnimation){Debug.Log("[CharacterAnimationEventCatcher] We're dead, but still let's try to RESET: " + anAnimation);}		
	public virtual void PlaySFX(string aSoundEffect){Debug.Log("[CharacterAnimationEventCatcher] We're dead, but still let's try to play a SFX: " + aSoundEffect);}	
}

