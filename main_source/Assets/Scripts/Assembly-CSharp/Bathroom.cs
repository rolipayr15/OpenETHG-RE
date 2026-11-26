using System.Collections;
using DarkTonic.MasterAudio;
using UnityEngine;
using VLB;

public class Bathroom : Interactable
{
	[SerializeField]
	private GameObject sparks;

	[SerializeField]
	private Collider coll;

	[SerializeField]
	private EffectFlicker effectFlicker;

	[SerializeField]
	private VolumetricLightBeamSD volumetricLightBeamSD;

	private void OtherPlayerInteracts(float itemId)
	{
		if (base.transform.position.sqrMagnitude == itemId)
		{
			Clicked(true);
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		coll.enabled = false;
		InteractionEnd();
		StartCoroutine(LightSwitch());
		StartCoroutine(BulbBreak());
	}

	private IEnumerator LightSwitch()
	{
		Flash(true);
		yield return Bibloteka.Utils.GetDelay(0.15f);
		Flash(false);
		yield return Bibloteka.Utils.GetDelay(0.2f);
		Flash(true);
		yield return Bibloteka.Utils.GetDelay(0.35f);
		Flash(false);
		yield return Bibloteka.Utils.GetDelay(0.15f);
		Flash(true);
		yield return Bibloteka.Utils.GetDelay(0.08f);
		Flash(false);
		static void Flash(bool on)
		{
			EyesLightingComposer.SetFlashMultiplier(on ? new float?(1.25f) : null);
		}
	}

	private void SetHighFlicker()
	{
		effectFlicker.smoothing = 0f;
		effectFlicker.frequency = 20f;
	}

	private IEnumerator BulbBreak()
	{
		MasterAudio.PlaySound3DAtTransform("fx", base.gameObject.transform, 1f, null, 0f, "bulb_break");
		volumetricLightBeamSD.intensityMultiplier = 0.1f;
		effectFlicker.enabled = true;
		SetHighFlicker();
		yield return Bibloteka.Utils.GetDelay(1f);
		sparks.SetActive(true);
		MasterAudio.PlaySound3DAtTransform("fx", base.gameObject.transform, 1f, null, 0f, "lightbulb-break");
		yield return Bibloteka.Utils.GetDelay(0.15f);
		effectFlicker.enabled = false;
		volumetricLightBeamSD.enabled = false;
		yield return Bibloteka.Utils.GetDelay(1f);
		sparks.SetActive(false);
	}
}
