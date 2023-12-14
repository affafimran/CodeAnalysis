using UnityEngine;

namespace NullPointerGame.DamageSystem
{
	public class AutoDestroy : MonoBehaviour 
	{
		public float timeToLive = 5.0f;
    
	
		void Update () 
		{
			timeToLive -= Time.deltaTime;

			if (timeToLive <= 0.0f)
				Destroy(this.gameObject);
		}
	}
}