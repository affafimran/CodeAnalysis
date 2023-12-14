using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float speed;

	private Rigidbody2D rb;

	private PlayerHp pHp;

	[SerializeField]
	private Joystick joystick;

	private void Start()
	{
		pHp = GetComponent<PlayerHp>();
		rb = GetComponent<Rigidbody2D>();
		if (!GameManager._instance.isMobile)
		{
			joystick.gameObject.SetActive(value: false);
		}
		else
		{
			joystick.player = this;
		}
	}

	private void Update()
	{
		if (!GameManager._instance.isMobile)
		{
			HandleMovement();
		}
		RotateToCursor();
		pHp.SetHpBarPosition();
	}

	private void HandleMovement()
	{
		Vector2 vector = Vector2.zero;
		if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
		{
			vector = new Vector2(0f, 1f);
		}
		else if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
		{
			vector = new Vector2(0f, -1f);
		}
		if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
		{
			vector = new Vector2(-1f, vector.y);
		}
		else if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
		{
			vector = new Vector2(1f, vector.y);
		}
		if (vector != Vector2.zero)
		{
			vector.Normalize();
		}
		rb.velocity = vector * speed * Time.deltaTime;
		rb.angularVelocity = 0f;
	}

	private void RotateToCursor()
	{
		if (!GameManager._instance.isMobile || !(joystick.input != Vector2.zero))
		{
			Vector2 v = base.transform.position;
			float z = AngleBetweenTwoPoints(b: (Vector2)Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition), a: v) + 180f;
			base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, z));
		}
	}

	public static float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
	{
		return Mathf.Atan2(a.y - b.y, a.x - b.x) * 57.29578f;
	}

	public void JoystickMovement(Vector2 vec)
	{
		base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, AngleBetweenTwoPoints(vec, Vector2.zero)));
		rb.velocity = vec * speed * Time.deltaTime;
		rb.angularVelocity = 0f;
	}

	public void StopJoystick()
	{
		rb.velocity = Vector2.zero;
		rb.angularVelocity = 0f;
	}
}
