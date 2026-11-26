using UnityEngine;

namespace I2.Loc
{
	public class LocalizeTargetDesc_Type<T, G> : LocalizeTargetDesc<G> where T : Object where G : LocalizeTarget<T>
	{
		public override bool CanLocalize(Localize cmp)
		{
			return cmp.GetComponent<T>() != null;
		}

		public override ILocalizeTarget CreateTarget(Localize cmp)
		{
			T target = cmp.GetComponent<T>();
			if (target == null)
			{
				return null;
			}
			G localizeTarget = ScriptableObject.CreateInstance<G>();
			localizeTarget.mTarget = target;
			return localizeTarget;
		}
	}
}
