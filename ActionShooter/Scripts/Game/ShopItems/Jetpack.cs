using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

/// <summary>
/// Jetpack.
/// <para>Last minute addition</para>
/// </summary>
public class Jetpack : MonoBehaviour {

	private Hammer hammer; // [HARDCODED] Hammer reference
	private Animator animator; // animator ref
	private GameObject jetpack; // object

	public bool jetpackOn;  // toggle
	private bool jetpackState; // state (on/off)

	public void Initialize()
	{
		hammer = Scripts.hammer; // store
		jetpack = Loader.LoadGameObject("Effects/Jetpack_Prefab"); // load object

		// parent to spine, position and rotate
		jetpack.transform.parent = hammer.spine.transform;
		jetpack.transform.localPosition = new Vector3(-0.28f, -0.08f, 0.0f);
		jetpack.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 90.0f);

		jetpack.SetEmissionInChildren(false); // turn off all particle systems

		animator = hammer.GetComponent<Animator>(); // store reference to animator
	}

	void FixedUpdate(){
		// activate or release
		// play sound and/or (de)activate particle systems.
		if (jetpackOn){
			if (jetpackState != jetpackOn) {
				Scripts.audioManager.PlaySFX("Effects/JetPackThrust", 1.0f, -1);
				Scripts.audioManager.PlaySFX("Effects/JetPackIgnition");
				jetpackState = jetpackOn;
			}
			hammer.characterMotor.grounded = false;
			// calculate and set max upwardsvelocity
			Vector3 velocity = hammer.characterMotor.movement.velocity;
			velocity += new Vector3(0, 3.0f, 0); // boost
			velocity.y = Mathf.Min(velocity.y, 15.0f); // limit y speed
			hammer.characterMotor.movement.velocity = velocity;
			jetpack.SetEmissionInChildren(true);
		} else {
			if (jetpackState != jetpackOn) {
				jetpack.SetEmissionInChildren(false);
				Scripts.audioManager.StopSFX("JetPackThrust");
				Scripts.audioManager.PlaySFX("Effects/JetPackOff");
				jetpackState = jetpackOn;
			}
		}
	}

	void Update() {
		animator.applyRootMotion = false; // always turn this off! Since this could influence the gravity of your character
		jetpackOn = CrossPlatformInputManager.GetButton("Jump") && hammer.allowInput; // check if we should flyyyyyyyy
	}
}
