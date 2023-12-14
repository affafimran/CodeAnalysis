using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public float ScreenEdgeBorderThickness = 5f;

	[Header("Movement Speeds")]
	[Space]
	public float minPanSpeed;

	public float maxPanSpeed;

	public float secToMaxSpeed;

	public float zoomSpeed;

	[Header("Movement Limits")]
	[Space]
	public bool enableMovementLimits;

	public Vector2 heightLimit;

	public Vector2 lenghtLimit;

	public Vector2 widthLimit;

	public Vector2 zoomLimit;

	private float panSpeed;

	private Vector3 initialPos;

	private Vector3 panMovement;

	private Vector3 pos;

	private Vector3 touchStart;

	private float panIncrease;

	public Vector3 offset;

	public Transform player;

	private void Start()
	{
		initialPos = base.transform.position;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			touchStart = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
		}
		if (UnityEngine.Input.touchCount == 2)
		{
			Touch touch = UnityEngine.Input.GetTouch(0);
			Touch touch2 = UnityEngine.Input.GetTouch(1);
			Vector2 a = touch.position - touch.deltaPosition;
			Vector2 b = touch2.position - touch2.deltaPosition;
			float magnitude = (a - b).magnitude;
			float num = (touch.position - touch2.position).magnitude - magnitude;
			Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - num * 0.01f, zoomLimit.x, zoomLimit.y);
		}
		else if (Input.GetMouseButton(0))
		{
			Vector3 vector = touchStart - Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
			Camera.main.transform.position += vector;
		}
		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - UnityEngine.Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime, zoomLimit.x, zoomLimit.y);
		if (enableMovementLimits)
		{
			pos = base.transform.position;
			pos.y = Mathf.Clamp(pos.y, heightLimit.x, heightLimit.y);
			pos.z = Mathf.Clamp(pos.z, lenghtLimit.x, lenghtLimit.y);
			pos.x = Mathf.Clamp(pos.x, widthLimit.x, widthLimit.y);
			base.transform.position = pos;
		}
		base.transform.position = player.position + offset;
	}
}
