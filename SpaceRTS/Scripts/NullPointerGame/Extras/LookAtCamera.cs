using UnityEngine;

namespace NullPointerCore
{
	[ExecuteInEditMode]
	public class LookAtCamera : MonoBehaviour
	{
		public bool lockRelativeToParent = false;
		public Vector3 lockedDir = Vector3.zero;

		private void LateUpdate()
		{
			if(Camera.main==null)
				return;

			if( lockedDir.x != 0 || lockedDir.y != 0 || lockedDir.z != 0 )
			{
				Vector3 up = lockedDir.normalized;
				if( lockRelativeToParent )
					up = transform.parent.TransformDirection(lockedDir.normalized);

				Vector3 camDir = (transform.position - Camera.main.transform.position).normalized;
				Vector3 right = Vector3.Cross( up, camDir );
				//Debug.DrawRay(transform.position, right, Color.magenta);
				Vector3 forward = Vector3.Cross( right, up );
				transform.rotation = Quaternion.LookRotation(forward, right);
			}
			else
				transform.LookAt( transform.position+Camera.main.transform.forward );
		}
	}
}