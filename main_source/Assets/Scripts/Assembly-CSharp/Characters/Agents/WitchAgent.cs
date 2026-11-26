using Prime31.MessageKit;
using DarkTonic.MasterAudio;
using System;
using Characters.Mover;
using Characters.States.Witch;
using Characters.Vision;
using UnityEngine;

namespace Characters.Agents
{
    // Token: 0x020003C6 RID: 966
    [RequireComponent(typeof(WitchStateController), typeof(PlayerOnWitchVision))]
    [RequireComponent(typeof(WitchMover), typeof(EnemyVision))]
    public class WitchAgent : EnemyAgent
    {
        // Token: 0x06001FE5 RID: 8165 RVA: 0x000086C8 File Offset: 0x000068C8
        protected override void Start()
        {
        	base.Start();
        	MessageKit<int, float>.addObserver(76, GoCheckActivatedTotem);
        }

        // Token: 0x06001FE6 RID: 8166 RVA: 0x000086CA File Offset: 0x000068CA
        private void GoCheckActivatedTotem(int _totem_type_not_used, float _potion_duration_not_used)
        {
        	EyesAnalytics.Game.NoteActivationCause((EyesAnalytics.ActivationCause)5);
        	MessageKit.post(25);
        	ForceCheckEnemyTarget = CustomSingleton<WitchTotemManager>.Instance.WitchTarget;
        	if ((int)ForceCheckEnemyTarget == 0)
        	{
        		ForceCheckDestination = null;
        	}
        }

        // Token: 0x06001FE7 RID: 8167 RVA: 0x000086CC File Offset: 0x000068CC
        public override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("Enemy", 1f, null, 0f, "witchShout");
        }
    }
}
