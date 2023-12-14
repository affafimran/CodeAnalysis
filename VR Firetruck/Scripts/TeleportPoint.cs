using _360Fabriek.Utility;
using UnityEngine;

namespace _360Fabriek.Scenarios {
    public class TeleportPoint : LookAtPlayer {

        protected override void Update() {
            base.Update();

            if (!target) {
                return;
            }

            float distance = Vector3.Distance(rotator.transform.position, target.position);

            TryEnable(rotator, distance > 2.0f);
        }
    }
}