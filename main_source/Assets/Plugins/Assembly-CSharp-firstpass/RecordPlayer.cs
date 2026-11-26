using UnityEngine;

public class RecordPlayer : MonoBehaviour
{
	public bool recordPlayerActive;

	private GameObject disc;

	private GameObject arm;

	private int mode;

	private float armAngle;

	private float discAngle;

	private float discSpeed;

	private void Start()
	{
		disc = base.gameObject.transform.Find("teller").gameObject;
		arm = base.gameObject.transform.Find("arm").gameObject;
		mode = 0;
		armAngle = 0f;
		discAngle = 0f;
		discSpeed = 0f;
	}

	private void Update()
	{
		if (mode == 0)
		{
			if (recordPlayerActive)
			{
				mode = 1;
			}
		}
		else if (mode == 1)
		{
			if (recordPlayerActive)
			{
				armAngle += Time.deltaTime * 30f;
				if (armAngle >= 30f)
				{
					armAngle = 30f;
					mode = 2;
				}
				discAngle += Time.deltaTime * discSpeed;
				discSpeed += Time.deltaTime * 80f;
			}
			else
			{
				mode = 3;
			}
		}
		else if (mode == 2)
		{
			if (recordPlayerActive)
			{
				discAngle += Time.deltaTime * discSpeed;
			}
			else
			{
				mode = 3;
			}
		}
		else if (!recordPlayerActive)
		{
			armAngle -= Time.deltaTime * 30f;
			if (armAngle <= 0f)
			{
				armAngle = 0f;
			}
			discAngle += Time.deltaTime * discSpeed;
			discSpeed -= Time.deltaTime * 80f;
			if (discSpeed <= 0f)
			{
				discSpeed = 0f;
			}
			if (discSpeed == 0f && armAngle == 0f)
			{
				mode = 0;
			}
		}
		else
		{
			mode = 1;
		}
		arm.transform.localEulerAngles = new Vector3(0f, armAngle, 0f);
		disc.transform.localEulerAngles = new Vector3(0f, discAngle, 0f);
	}
}
