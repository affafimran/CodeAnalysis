using Autohand;
using NaughtyAttributes;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace _360Fabriek {
    public class GrabbableAction : InteractableAction {
        [Header("Grabbable Action:")]
        [SerializeField] private Rigidbody grabbableRigidbody = null;
        [SerializeField] protected Grabbable grabbable = null;

        [Header("Constraints: ")]
        [SerializeField] private bool applyGrabLock = true;
        [SerializeField] protected bool isGrabbedOnActivation = false;

        [Tooltip(nameof(constrainRigidbodyBeforeInteraction))]
        [SerializeField] private bool constrainRigidbodyBeforeInteraction = true;

        [Tooltip(nameof(constrainRigidbodyDuringInteraction))]
        [SerializeField] private bool constrainRigidbodyDuringInteraction = true;

        [Tooltip(nameof(constrainRigidbodyAfterInteraction))]
        [SerializeField] private bool constrainRigidbodyAfterInteraction = true;

        [Header("On Release:")]
        [SerializeField] protected bool finishOnGrab = false;
        [SerializeField] protected bool finishAfterPlacement = false;

        [ShowIf(nameof(finishAfterPlacement))]
        [SerializeField] private Transform parentTarget;
        [ShowIf(nameof(finishAfterPlacement))]
        [SerializeField] private float placementRadius = 0.1f;

        [Space]
        [ShowIf(nameof(finishAfterPlacement))]
        [SerializeField] private bool overridePostion;
        [ShowIf(nameof(overridePostion))]
        [SerializeField] private Vector3 position;
        [ShowIf(nameof(finishAfterPlacement))]
        [SerializeField] private bool overrideRotation;
        [ShowIf(nameof(overrideRotation))]
        [SerializeField] private Vector3 rotation;

        private GrabLock dynamicLock;
        private RigidbodyConstraints originalRigidbodyContraints;

        
        public Grabbable Grabbable => grabbable;
        private bool _isReleased = false;
        private void Awake() {
            originalRigidbodyContraints = grabbableRigidbody.constraints;
        }

        protected override void Start() {
            if (constrainRigidbodyBeforeInteraction) {
                SetRigidbodyConstraints(RigidbodyConstraints.FreezeAll);
            }

            grabbable.OnGrabEvent += OnGrab;
            grabbable.OnReleaseEvent += OnRelease;
            grabbable.OnJointBreakEvent += OnRelease;
            
            base.Start();
        }

        // Overrides
        protected override void OnActivate(ActionArg arg) {
            base.OnActivate(arg);

            if (applyGrabLock && (!finishAfterPlacement || finishAfterPlacement && CheckParentTarget())) {
                StartCoroutine(LockCheck());
            }

            if (isGrabbedOnActivation) {
                OnGrab(null, null);
            }
        }

        private void FixedUpdate()
        {
            if (!grabbable.IsHeld() && InRange() && Status == State.Active)
            {
                grabbable.transform.SetParent(parentTarget);

                Vector3 position = new Vector3();
                Vector3 rotation = new Vector3();

                if (overridePostion)
                {
                    position = this.position;
                }

                if (overrideRotation)
                {
                    rotation = this.rotation;
                }

                grabbable.transform.localPosition = position;
                grabbable.transform.localRotation = Quaternion.Euler(rotation);

                Finish();
            }
        }

        protected override void OnDeactivate(ActionArg arg) {
            if (constrainRigidbodyAfterInteraction) {
                SetRigidbodyConstraints(RigidbodyConstraints.FreezeAll);
            }

            base.OnDeactivate(arg);
        }

        // Callbacks
        private void OnGrab(Hand hand, Grabbable grabbable) {
            if (Status == State.Active && (grabbable == this.grabbable || hand == null)) {
                OnGrabEvent?.Invoke();
                TryEnableOutline(false);
                _isReleased = false;
                if (constrainRigidbodyBeforeInteraction || constrainRigidbodyDuringInteraction || constrainRigidbodyAfterInteraction) {
                    SetRigidbodyConstraints(originalRigidbodyContraints);
                }

                if (finishOnGrab) {
                    Finish();
                }
            }
        }

        private void OnRelease(Hand hand, Grabbable _) {
            _isReleased = true;
            if (!grabbable.IsHeld() && InRange())
            {
               
            }
            if (Status == State.Active)
            {
                if (constrainRigidbodyDuringInteraction)
                {
                    SetRigidbodyConstraints(RigidbodyConstraints.FreezeAll);
                    
                }

                if (finishAfterPlacement && CheckParentTarget() && InRange())
                {
                    grabbable.transform.SetParent(parentTarget);

                    Vector3 position = new Vector3();
                    Vector3 rotation = new Vector3();

                    if (overridePostion)
                    {
                        position = this.position;
                    }

                    if (overrideRotation)
                    {
                        rotation = this.rotation;
                    }

                    grabbable.transform.localPosition = position;
                    grabbable.transform.localRotation = Quaternion.Euler(rotation);
                   
                    Finish();
                }
                
            }
        }

        private IEnumerator LockCheck() {
            dynamicLock = grabbable.gameObject.GetComponent<GrabLock>();

            while (Status == State.Active && grabbable && applyGrabLock) {
                bool allowRelease = InRange();

                if (!dynamicLock && !allowRelease) {
                    dynamicLock = grabbable.gameObject.AddComponent<GrabLock>();
                } else if (allowRelease && dynamicLock) {
                    Destroy(dynamicLock);
                }

                yield return new WaitForFixedUpdate();
            }
        }

        private void SetRigidbodyConstraints(RigidbodyConstraints constraints) {
            if (grabbableRigidbody) {
                grabbableRigidbody.constraints = constraints;
            }
        }

        private bool InRange() {
            return finishAfterPlacement ? Vector3.Distance(parentTarget.position, grabbable.transform.position) < placementRadius : false;
        }

        private bool CheckParentTarget() {
            if (parentTarget) {
                return true;
            }

            Debug.LogWarning("Parent target is missing in " + name + " (" + this.GetType().Name + ")");
            return false;
        }

#if UNITY_EDITOR
        private bool isGrabbed = false;

        private void OnDrawGizmos() {
            if (debug && finishAfterPlacement && CheckParentTarget()) {
                Gizmos.DrawWireSphere(parentTarget.position, placementRadius);
            }
        }

        private void Update() {
            if (Status != State.Active) {
                return;
            }

            if (Input.GetMouseButtonUp(1)) {
                if (!isGrabbed) {
                    Grabbable.OnGrabEvent?.Invoke(null, null);
                    print("Grabbable Action => OnGrab of " + name + " (" + GetType().Name + ")");
                } else {
                    Grabbable.OnReleaseEvent?.Invoke(null, null);
                    print("Grabbable Action => OnRelease of " + name + " (" + GetType().Name + ")");
                }

                isGrabbed = !isGrabbed;
            }
        }
#endif
    }
}