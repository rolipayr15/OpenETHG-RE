using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using Characters.Mover;
using Characters.Strategies.Goose;
using UnityEngine;

namespace Characters.EnemyAvatar
{
    // Token: 0x020003BD RID: 957
    public class GooseAvatar : EnemyAvatar
    {
        // Token: 0x06001F87 RID: 8071 RVA: 0x00008555 File Offset: 0x00006755
        protected override void Start()
        {
        	base.Start();
        	profile = GooseChargeRules.ProfileFor(GooseChargeRules.EffectiveDifficulty(UserGameSettings.currentDifficulty, UserGameSettings.currentMode));
        }

        // Token: 0x06001F88 RID: 8072 RVA: 0x00008557 File Offset: 0x00006757
        protected override void EnemySpecificStateChangedBehavior(byte enemySymbol, byte enemyStateSymbol)
        {
        	if (enemySymbol != 71)
        	{
        		return;
        	}
        	switch (enemyStateSymbol)
        	{
        	case 71:
        		GooseAnimatorDriver.Charge(anim);
        		SetLift(true);
        		tint.HoldCharge();
        		break;
        	case 73:
        		GooseAnimatorDriver.Idle(anim);
        		SetLift(false);
        		if (tint != null)
        		{
        			tint.StartCooldown(0.5f);
        		}
        		break;
        	case 76:
        	case 80:
        		anim.SetFloat("agent_speed", patrolSpeed);
        		GooseAnimatorDriver.Walk(anim);
        		SetLift(false);
        		if (tint != null)
        		{
        			tint.StartCooldown(0.5f);
        		}
        		break;
        	case 86:
        		GooseAnimatorDriver.Land(anim);
        		SetLift(false);
        		Play(dazeSoundGroup, dazeSoundVariation);
        		if (tint != null)
        		{
        			tint.StartCooldown(profile.DazeSeconds);
        		}
        		break;
        	case 87:
        		GooseAnimatorDriver.Windup(anim);
        		Play(windupSoundGroup, windupSoundVariation);
        		if (tint != null)
        		{
        			tint.StartWindup(profile.WindupSeconds);
        		}
        		break;
        	}
        }

        // Token: 0x06001F89 RID: 8073 RVA: 0x00008559 File Offset: 0x00006759
        private void SetLift(bool lifted)
        {
        	if (lift != null)
        	{
        		lift.SetLifted(lifted);
        	}
        }

        // Token: 0x06001F8A RID: 8074 RVA: 0x0000855B File Offset: 0x0000675B
        protected override IEnumerator BeforeAttackSoundDelay()
        {
        	yield return null;
        }

        // Token: 0x06001F8B RID: 8075 RVA: 0x0000855E File Offset: 0x0000675E
        protected override void PlayAttackSound()
        {
        	Play(attackSoundGroup, attackSoundVariation);
        }

        // Token: 0x06001F8C RID: 8076 RVA: 0x00008560 File Offset: 0x00006760
        protected override IEnumerator FadeEffect()
        {
        	yield return Utils.GetDelay(1f);
        	CustomSingleton<ScreenFader>.Instance.fadeIn = false;
        }

        // Token: 0x06001F8D RID: 8077 RVA: 0x00008563 File Offset: 0x00006763
        private void Play(string group, string variation)
        {
        	if (!string.IsNullOrEmpty(group))
        	{
        		MasterAudio.PlaySound3DAtTransform(group, transform, 1f, null, 0f, string.IsNullOrEmpty(variation) ? null : variation);
        	}
        }

        // Token: 0x04001ADE RID: 6878
        [SerializeField]
        private GooseBodyLift lift;

        // Token: 0x04001ADF RID: 6879
        [SerializeField]
        private GooseTint tint;

        // Token: 0x04001AE0 RID: 6880
        [Header("Sounds (MasterAudio group / variation; empty = silent)")]
        [SerializeField]
        private string attackSoundGroup = "goose-attack";

        // Token: 0x04001AE1 RID: 6881
        [SerializeField]
        private string attackSoundVariation = "";

        // Token: 0x04001AE2 RID: 6882
        [SerializeField]
        private string windupSoundGroup = "";

        // Token: 0x04001AE3 RID: 6883
        [SerializeField]
        private string windupSoundVariation = "";

        // Token: 0x04001AE4 RID: 6884
        [SerializeField]
        private string dazeSoundGroup = "";

        // Token: 0x04001AE5 RID: 6885
        [SerializeField]
        private string dazeSoundVariation = "";

        // Token: 0x04001AE6 RID: 6886
        private GooseChargeRules.Profile profile;
    }
}
