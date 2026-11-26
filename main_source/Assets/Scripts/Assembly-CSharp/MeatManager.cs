using Debug = UnityEngine.Debug;
using I2.Loc;
using EyesAnalytics;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MeatManager : MonoBehaviour
{

	private int meatCounter;

	[SerializeField]
	private UnityEngine.Object meat;

	private Transform playerT;

	private bool firstTime = true;

	private GameObject meatThrown;

	[SerializeField]
	private Transform throwOrigin;

	[SerializeField]
	private float throwImpulse = 6f;

	[SerializeField]
	private float upwardImpulse = 1.5f;

	private void Start()
	{
		if ((int)UserGameSettings.currentEnemy == 2)
		{
			MessageKit.addObserver(28, CollectMeat);
			MessageKit.addObserver(65, LeaveMeat);
			MessageKit<string>.addObserver(114, AvatarLeavesMeat);
			playerT = CustomSingleton<PlayerManager>.Instance.Player.transform;
			return;
		}
		foreach (GameObject item in GameObject.FindGameObjectsWithTag("Meat"))
		{
			item.SetActive(false);
		}
	}

	private void CollectMeat()
	{
		meatCounter++;
		if (firstTime)
		{
			MessageKit<string>.post(3, ScriptLocalization.PressTheMeatButton);
			firstTime = false;
		}
	}

	public int GetAvailableMeat()
	{
		return meatCounter;
	}

	public void LeaveMeat()
	{
		if (meatCounter < 1)
		{
			LevelUI.SkillUsed((LevelUI.Skill)3, false);
			return;
		}
		LevelUI.SkillUsed((LevelUI.Skill)3, true);
		Vector3 position = throwOrigin != null ? throwOrigin.position : playerT.position + playerT.forward * 0.8f + Vector3.up * 1.2f;
		Quaternion rotation = throwOrigin != null ? throwOrigin.rotation : Quaternion.LookRotation(playerT.forward, Vector3.up);
		meatThrown = (GameObject)Instantiate(meat, position, rotation);
		Rigidbody body = meatThrown.GetComponent<Rigidbody>();
		body.linearVelocity = Vector3.zero;
		body.angularVelocity = Vector3.zero;
		body.AddForce(playerT.forward.normalized * throwImpulse + Vector3.up * upwardImpulse, ForceMode.Impulse);
		meatCounter--;
		MessageKit.post(29);
		StartCoroutine(MeatFallingDown());
	}

	private void AvatarLeavesMeat(string positionString)
	{
		GuestThrowsMeat(Utils.ParseStringToVector3(positionString));
	}

	public void GuestThrowsMeat(Vector3 endPosition)
	{
		Vector3 start = CustomSingleton<GuestManager>.Instance.Heart.transform.position;
		GameObject thrown = (GameObject)Instantiate(meat, start, Quaternion.identity);
		Meat meatComponent = thrown.GetComponentInChildren<Meat>();
		meatComponent.meatId = Utils.Vector3ToString(endPosition);
		Debug.Log("ustawiam meatId w guest throws meat na: " + Utils.Vector3ToString(endPosition));
		Rigidbody body = thrown.GetComponent<Rigidbody>();
		if (body == null)
		{
			Debug.LogError("The object to throw doesn't have a Rigidbody component.");
			return;
		}
		body.AddForce((endPosition - start).normalized * 150f);
	}

	private IEnumerator MeatFallingDown()
	{
		yield return Utils.GetDelay(0.4f);
		MasterAudio.PlaySound3DAtTransform("DogFx", CustomSingleton<PlayerManager>.Instance.Player.transform, 1f, null, 0f, "meatThrow");
		if (UserGameSettings.IsMultiplayer())
		{
			string id = Utils.Vector3ToString(meatThrown.transform.position);
			meatThrown.GetComponentInChildren<Meat>().meatId = id;
			Debug.Log("ustawiam meatId w falling down  na: " + id);
			MessageKit<string>.post(29, id);
		}
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(28, CollectMeat);
		MessageKit.removeObserver(65, LeaveMeat);
		MessageKit<string>.removeObserver(114, AvatarLeavesMeat);
	}
}
