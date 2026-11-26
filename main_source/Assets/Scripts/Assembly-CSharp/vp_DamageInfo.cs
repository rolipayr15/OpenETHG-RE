using UnityEngine;

public class vp_DamageInfo
{
	public enum DamageType
	{
		Unknown,
		KillZone,
		Fall,
		Impact,
		Bullet,
		Explosion
	}

	public enum DamageMode
	{
		None,
		DamageHandler,
		UnityMessage,
		Both
	}

	public float Damage;

	public Transform Source;

	public Transform OriginalSource;

	public DamageType Type;

	public vp_DamageInfo(float damage, Transform source, DamageType type = DamageType.Unknown)
	{
		Damage = damage;
		Source = source;
		OriginalSource = source;
		Type = type;
	}

	public vp_DamageInfo(float damage, Transform source, Transform originalSource, DamageType type = DamageType.Unknown)
	{
		Damage = damage;
		Source = source;
		OriginalSource = originalSource;
		Type = type;
	}
}
