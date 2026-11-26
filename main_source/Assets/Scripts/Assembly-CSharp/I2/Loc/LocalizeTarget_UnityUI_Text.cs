using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
	public class LocalizeTarget_UnityUI_Text : LocalizeTarget<Text>
	{
		private TextAnchor mAlignment_RTL;

		private TextAnchor mAlignment_LTR;

		private bool mAlignmentWasRTL;

		private bool mInitializeAlignment = true;

		static LocalizeTarget_UnityUI_Text()
		{
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void AutoRegister()
		{
			LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<Text, LocalizeTarget_UnityUI_Text>
			{
				Name = "Text",
				Priority = 100
			});
		}

		public override eTermType GetPrimaryTermType(Localize cmp)
		{
			return eTermType.Text;
		}

		public override eTermType GetSecondaryTermType(Localize cmp)
		{
			return eTermType.Font;
		}

		public override bool CanUseSecondaryTerm()
		{
			return true;
		}

		public override bool AllowMainTermToBeRTL()
		{
			return true;
		}

		public override bool AllowSecondTermToBeRTL()
		{
			return false;
		}

		public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			primaryTerm = mTarget != null ? mTarget.text : null;
			secondaryTerm = mTarget != null && mTarget.font != null ? mTarget.font.name : string.Empty;
		}

		public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
		{
			Font font = cmp.GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (font != null && font != mTarget.font)
			{
				mTarget.font = font;
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentWasRTL = LocalizationManager.IsRight2Left;
				InitAlignment(mAlignmentWasRTL, mTarget.alignment, out mAlignment_LTR, out mAlignment_RTL);
			}
			else
			{
				InitAlignment(mAlignmentWasRTL, mTarget.alignment, out TextAnchor alignLTR, out TextAnchor alignRTL);
				if ((mAlignmentWasRTL && mAlignment_RTL != alignRTL) || (!mAlignmentWasRTL && mAlignment_LTR != alignLTR))
				{
					mAlignment_LTR = alignLTR;
					mAlignment_RTL = alignRTL;
				}
				mAlignmentWasRTL = LocalizationManager.IsRight2Left;
			}
			if (mainTranslation != null && mTarget.text != mainTranslation)
			{
				if (cmp.CorrectAlignmentForRTL)
				{
					mTarget.alignment = LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR;
				}
				mTarget.text = mainTranslation;
				mTarget.SetVerticesDirty();
			}
		}

		private void InitAlignment(bool isRTL, TextAnchor alignment, out TextAnchor alignLTR, out TextAnchor alignRTL)
		{
			TextAnchor mirrored = alignment;
			switch (alignment)
			{
			case TextAnchor.UpperLeft:
				mirrored = TextAnchor.UpperRight;
				break;
			case TextAnchor.UpperRight:
				mirrored = TextAnchor.UpperLeft;
				break;
			case TextAnchor.MiddleLeft:
				mirrored = TextAnchor.MiddleRight;
				break;
			case TextAnchor.MiddleRight:
				mirrored = TextAnchor.MiddleLeft;
				break;
			case TextAnchor.LowerLeft:
				mirrored = TextAnchor.LowerRight;
				break;
			case TextAnchor.LowerRight:
				mirrored = TextAnchor.LowerLeft;
				break;
			}
			alignLTR = isRTL ? mirrored : alignment;
			alignRTL = isRTL ? alignment : mirrored;
		}
	}
}
