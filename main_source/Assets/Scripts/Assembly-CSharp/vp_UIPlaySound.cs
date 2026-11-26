using UnityEngine;

public class vp_UIPlaySound : MonoBehaviour
{
	public enum vp_UIPlaySoundTrigger
	{
		OnPress,
		OnRelease
	}

	public AudioClip Sound;

	public vp_UIPlaySoundTrigger Trigger;

	public float Volume = 1f;

	protected vp_UIManager m_Manager;

	protected virtual void Awake()
	{
		m_Manager = base.transform.root.GetComponent<vp_UIManager>();
	}

	protected virtual void OnPressControl()
	{
		if (Trigger == vp_UIPlaySoundTrigger.OnPress)
		{
			PlaySound();
		}
	}

	protected virtual void OnReleaseControl()
	{
		if (Trigger == vp_UIPlaySoundTrigger.OnRelease)
		{
			PlaySound();
		}
	}

	protected virtual void PlaySound()
	{
		if (!(Sound == null) && !(m_Manager == null) && m_Manager.AudioSource != null)
		{
			m_Manager.AudioSource.PlayOneShot(Sound);
		}
	}
}
