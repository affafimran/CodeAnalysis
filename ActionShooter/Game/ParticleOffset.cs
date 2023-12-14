using UnityEngine;
using System.Collections;

/// <summary>
/// [BUG] Particle offset.
/// Unity 5.2 has a bug where parented particle system don't spawn their particles properly.
/// They spawn from the center of the world to the location of the particlesystem, as in a line.
/// This should be fixed in the next patch.
/// However, we're not waiting for that. The workaround now is that for some particle systems (vehicle damage smoke and fire)
/// I add this behavior. It offsets the system and destroys itself. The system should the spawn normally.
/// My advice is that you remove this script (and the other references) when the bug is fixed!
/// </summary>
public class ParticleOffset : MonoBehaviour
{
	void Update ()
	{
		gameObject.transform.localPosition += new Vector3(0, 0.0001f, 0);
		Destroy (this);
	}
}

