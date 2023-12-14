using Autohand;
using System.Collections.Generic;
using UnityEngine;

namespace _360Fabriek {
    public class TeleportAction : AbstractAction {
        [Header("Teleport Action:")]
        [SerializeField] public List<Teleporter> teleporters = new List<Teleporter>();

        private void Start() {
            if (teleporters.Count > 0) {
                teleporters.ForEach(teleporter => teleporter.OnTeleport.AddListener(OnTeleport));
            }
        }

        private void OnTeleport() {
            if (Status == State.Active) {
                Finish();
            }
        }
    }
}