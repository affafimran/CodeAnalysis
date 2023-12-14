using UnityEngine;
using System.Collections;

public class F22AIController : VehicleInputController {
	
	private F22 f22;
	private F22Data f22Data;
	
	public TargetData targetData;
	
	// states
	public enum State {INACTIVE, FIRE, PATROL, LIFTOFF}
	public State state = State.LIFTOFF;
	
	// Firing vars
	public int fired = 0;
	
	// Movement vars
	public float returnToPatrolDistance = 250;
	public float rotateTimer = 3;
	public float rotateDirection = -1f;
	public float thrustSpeed = 50;
	public Vector3 attackPosition;
	public Vector3 targetPosition;
	public bool targetInRange;
	public Vector3 groundPosition;

	// Collision with building vars
	public Ray ray;
	public RaycastHit rayCastHit;
	public LayerMask layerMask;
	
	private float deltaTime;
	
	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		f22 = GetComponent<F22> ();
		f22Data = f22.f22Data;
		// turn off gravity and convex
		f22Data.collider.convex = false;
		f22Data.rigidBody.isKinematic = true;
		f22Data.rigidBody.useGravity = false;
		f22Data.rigidBody.drag = 0;
		f22Data.rigidBody.angularDrag = 0;
		// Set the start position (used by the patrolObject to rotate around)
		// Instantiate a patrol object
		f22Data.patrolObject = new GameObject();
		f22Data.patrolObject.name = "F22PatrolObject";
		f22Data.patrolObject.transform.position = f22Data.liftOffTarget;
		// Layermasks the plane has collision with
		layerMask = (1 << 0) | (1 << 13);
		// activate thrusters
		f22.ActivateRearThrust ();
		// activate sound
		f22Data.flyingSound.enabled = true;

		// test
		if (MissionManager.missionData.mission == 25) {
			f22Data.collider.convex = true;
			f22Data.collider.isTrigger = true;
			f22Data.baseHeight = 80f + Random.Range(0, 41);
		}


	}
	
	//----------------------------------------------------------------
	// Update
	//----------------------------------------------------------------
	public override void Update()
	{
		// no runny when paused
		if (Data.pause) return;
		
		// get target, no target exit
		targetData = TargetManager.GetTargetData(gameObject);
		if (!targetData.target) return; // return when no target. However, we could comment this line. If we use the bool to check target we can still run other code without aiming.
		
		// store deltaTime for easy access
		deltaTime = Time.deltaTime;
		
		// always move forward
		transform.position = transform.position + (transform.forward * (thrustSpeed * deltaTime));
		// Set the ground position (used to check the X and Y distance to the target without including the height)
		groundPosition = new Vector3(transform.position.x, targetData.position.y, transform.position.z);
		
		// Rotate the patrolObject
		float speed = -70 * deltaTime;
		f22Data.patrolObject.transform.position += f22Data.patrolObject.transform.forward * speed;
		f22Data.patrolObject.transform.eulerAngles += new Vector3 (0, (speed / 1.5f), 0);
		
		//Check for collision
		if (MissionManager.missionData.mission != 25){
			ray = new Ray(transform.position + transform.forward * 7.75f + transform.up * 0.85f, transform.forward);
			if(Physics.Raycast(ray, out rayCastHit, 1, layerMask)) 
			{
				f22Data.collider.convex = true;
				f22Data.rigidBody.useGravity = true;
				f22.DeactivateRearThrust();
				Destroy(f22Data.patrolObject);
				f22.Kill(new ProjectileData(), new HitData());
			}
		}


		switch(state)
		{
			
		case State.INACTIVE:
			break;
			// LIFTOFF
		case State.LIFTOFF: // flying towards the liftOffTarget
			// we are at the liftOffTarget
			if (Mathf.Abs(f22.vehicle.transform.position.y - f22Data.baseHeight) <= 1.0f) 
			{
				f22Data.liftOffTarget = gameObject.transform.position;
				transform.LookAt(new Vector3(transform.position.x + transform.forward.x, transform.position.y, transform.position.z + transform.forward.z));
				state = State.PATROL;
				return;
			}
			// aim towards the liftoff point
			transform.LookAt(f22Data.liftOffTarget);
			break;				
			
		case State.PATROL:
			if (!targetInRange) // target hasn't been in range yet
			{
				// Is patrolObject in sight
				if (Vector3.Angle(f22Data.patrolObject.transform.position - transform.position, transform.forward) < 15)
				{
					ResetAngle();
					// Lookat the patrolObject
					transform.LookAt(f22Data.patrolObject.transform.position);
					// If the distance to the patrolObject is smaller then 30 Rotate the plane
					if (Vector3.Distance(transform.position, f22Data.patrolObject.transform.position) < 30)
					{
						transform.Rotate(new Vector3(0, 0, 30));
					}
				}
				else // Plane is on its way back from an attack.
				{
					// Is the target in sight?
					if (TargetInSight())
					{
						// Target close enough to attack again, Attack.
						if (Vector3.Distance(groundPosition, targetData.position) < f22Data.idleToAimRange) 
						{
							
							Attack();
							return;
						}
					}
					ResetAngle();
					RotatePlaneInCircle();
				}
				// Target is close, Attack.
				if (Vector3.Distance(groundPosition, targetData.position) < f22Data.idleToAimRange) 
				{
					targetInRange = true;
					// Check smallest angle to turn
					CalculateTargetSideOfForward(transform.position, targetData.position);
					if (TargetInSight())
					{
						Attack();
						return;
					}
				}
			}
			if (targetInRange) // target has been in Range
			{
				rotateTimer -= deltaTime;
				if (TargetInSight()) // if target in sight
				{
					Attack();
					return;
				}
				else
				{
					// If the plane is going circles and the target is in the middle of the circle the plane wont ever find it. 
					// To fix this problem
					// If the plane is close to the target and 3 seconds have passed since the previous Change of direction. 
					// Change the direction of the circle the plane is using to find the target
					if (Vector3.Distance(targetData.position, groundPosition) < 40 && rotateTimer <= 0)
					{
						rotateTimer = 3;
						rotateDirection = -rotateDirection;
					}
					ResetAngle();
					RotatePlaneInCircle();
				}
			}
			break;
			
		case State.FIRE:
			if (fired < 4)// less then 4 bombs dropped
			{
				f22Data.currentRof -= deltaTime;
				if (f22Data.currentRof <= 0)
				{
					// Is the target close to the plane? Start dropping bombs
					if (Vector3.Distance(groundPosition, targetPosition) < 40)
					{
						// Reset RoF
						f22Data.currentRof = f22Data.rof;
						fired++;
						// Fire weapon
						Fire(targetData.gameObject);
					}
				}
			}
			else if (fired >= 4) // 4 bombs have been dropped
			{
				// Go back to patrol state.
				if (Vector3.Distance(transform.position, attackPosition) > returnToPatrolDistance)
				{
					fired = 0;
					targetInRange = false;
					state = State.PATROL;
				}
			}
			break;			
		}
	}
	
	public void Fire(GameObject target)
	{
		// Play weapon sound
		Scripts.audioManager.PlaySFX3D(f22Data.sound, f22Data.barrel, "FireGun");
		// Bombs away!
		ProjectileManager.AddProjectile(f22Data.projectile2, f22Data.turret.transform.position-new Vector3(0, 1, 0), f22Data.turret.transform.forward, new HitData(), target, this.gameObject);
	}
	
	void Attack()
	{
		// Distance from the begin of the attack to a proper distance behind the target after the attack
		returnToPatrolDistance = 150 + Vector3.Distance(groundPosition, targetData.position);
		// Set plane straight
		ResetAngle();
		// Look at target
		transform.LookAt(new Vector3(targetData.position.x, transform.position.y, targetData.position.z));
		// Save the position from the start of the attack
		attackPosition = transform.position;
		targetPosition = targetData.position;
		// Start firing
		state = State.FIRE;
	}
	
	// Is the target in front of the plane with an angle less then 15 degrees from the forward of the plane
	bool TargetInSight()
	{
		if (Vector3.Angle(targetData.position - groundPosition, transform.forward) < 15)
		{
			return true;
		}
		return false;
	}
	// Rotate the plane during turns
	void RotatePlaneInCircle()
	{
		// If there are many planes the fps will drop low and the planes start going different paths.
		// To prevent this I am calculating the difference between the desired fps and the current fps so that the planes atleast follow the same path.
		// 0.0167f is the deltatime for if the game is running on 60 fps so therefor this weird number.
		float FPSdrop = deltaTime / 0.0167f;
		// Don't want the plane to start turning what looks like 180 degrees.
		if (FPSdrop > 10) FPSdrop = 10; 
		// Don't want unity to bug to 5000 fps and ruin the planes path.
		if (FPSdrop < 0.2f) FPSdrop = 0.2f;
		transform.Rotate(new Vector3(0, rotateDirection * FPSdrop, -rotateDirection * 30));
	}
	// Reset the plane rotation
	void ResetAngle()
	{
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}
	// Check on which side of the plane the target is to determine what the smallest turn angle is
	void CalculateTargetSideOfForward(Vector3 aPosition, Vector3 aTargetPosition)
	{
		// the vector that we want to measure an angle from
		Vector3 referenceForward = aPosition;
		
		// the vector perpendicular to referenceForward (90 degrees clockwise)
		// (used to determine if angle is positive or negative)
		Vector3 referenceRight= Vector3.Cross(Vector3.up, referenceForward);
		
		// the vector of interest
		Vector3 newDirection = aTargetPosition;
		
		// sign negative is left. sign positive is right
		float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
		
		rotateDirection = -sign;
	}
	
	public override void Reset()
	{}
}