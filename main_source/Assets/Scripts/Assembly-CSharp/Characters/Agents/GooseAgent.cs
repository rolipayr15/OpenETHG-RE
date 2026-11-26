using DarkTonic.MasterAudio;
using System;
using Characters.Mover;
using Characters.States.Goose;
using Characters.Strategies.Goose;
using Characters.Vision;
using UnityEngine;

namespace Characters.Agents
{
    // Token: 0x020003C3 RID: 963
    [RequireComponent(typeof(GooseStateController), typeof(PlayerOnGooseVision))]
    [RequireComponent(typeof(GooseMover), typeof(EnemyVision), typeof(GooseBodyLift))]
    public class GooseAgent : EnemyAgent
    {
        // Token: 0x17000359 RID: 857
        // (get) Token: 0x06001FC0 RID: 8128 RVA: 0x0000860F File Offset: 0x0000680F
        public GooseTint Tint
        {
            get
            {
                return tintDriver != null ? tintDriver : (tintDriver = GetComponent<GooseTint>());
            }
        }

        // Token: 0x1700035A RID: 858
        // (get) Token: 0x06001FC1 RID: 8129 RVA: 0x00008612 File Offset: 0x00006812
        // (set) Token: 0x06001FC2 RID: 8130 RVA: 0x0000861A File Offset: 0x0000681A
        public bool ChaseRaisedLocal { get; private set; }

        // Token: 0x1700035B RID: 859
        // (get) Token: 0x06001FC3 RID: 8131 RVA: 0x00008623 File Offset: 0x00006823
        // (set) Token: 0x06001FC4 RID: 8132 RVA: 0x0000862B File Offset: 0x0000682B
        public bool ChaseRaisedGuest { get; private set; }

        // Token: 0x06001FC5 RID: 8133 RVA: 0x00008634 File Offset: 0x00006834
        public void MarkChaseRaised(bool local, bool guest)
        {
        	ChaseRaisedLocal |= local;
        	ChaseRaisedGuest |= guest;
        }

        // Token: 0x06001FC6 RID: 8134 RVA: 0x00008636 File Offset: 0x00006836
        public void ClearChaseRaised()
        {
        	ChaseRaisedLocal = false;
        	ChaseRaisedGuest = false;
        }

        // Token: 0x1700035C RID: 860
        // (get) Token: 0x06001FC7 RID: 8135 RVA: 0x00008638 File Offset: 0x00006838
        // (set) Token: 0x06001FC8 RID: 8136 RVA: 0x00008640 File Offset: 0x00006840
        public GooseChargeRules.Profile Profile { get; private set; }

        // Token: 0x1700035D RID: 861
        // (get) Token: 0x06001FC9 RID: 8137 RVA: 0x00008649 File Offset: 0x00006849
        // (set) Token: 0x06001FCA RID: 8138 RVA: 0x00008651 File Offset: 0x00006851
        public Vector3 ChargeDestination { get; set; }

        // Token: 0x1700035E RID: 862
        // (get) Token: 0x06001FCB RID: 8139 RVA: 0x0000865A File Offset: 0x0000685A
        // (set) Token: 0x06001FCC RID: 8140 RVA: 0x00008662 File Offset: 0x00006862
        public bool HitRegistered { get; internal set; }

        // Token: 0x1700035F RID: 863
        // (get) Token: 0x06001FCD RID: 8141 RVA: 0x0000866B File Offset: 0x0000686B
        // (set) Token: 0x06001FCE RID: 8142 RVA: 0x00008673 File Offset: 0x00006873
        public bool StalkRequested { get; private set; }

        // Token: 0x17000360 RID: 864
        // (get) Token: 0x06001FCF RID: 8143 RVA: 0x0000867C File Offset: 0x0000687C
        // (set) Token: 0x06001FD0 RID: 8144 RVA: 0x00008684 File Offset: 0x00006884
        public Vector3 StalkOrigin { get; private set; }

        // Token: 0x06001FD1 RID: 8145 RVA: 0x0000868D File Offset: 0x0000688D
        public void RequestStalk()
        {
        	StalkRequested = true;
        	StalkOrigin = transform.position;
        }

        // Token: 0x06001FD2 RID: 8146 RVA: 0x0000868F File Offset: 0x0000688F
        public void ClearStalk()
        {
        	StalkRequested = false;
        }

        // Token: 0x06001FD3 RID: 8147 RVA: 0x00008691 File Offset: 0x00006891
        protected override void Start()
        {
        	lift = GetComponent<GooseBodyLift>();
        	Profile = GooseChargeRules.ProfileFor(GooseChargeRules.EffectiveDifficulty(UserGameSettings.currentDifficulty, UserGameSettings.currentMode));
        	SetHitTriggerEnabled(false);
        	base.Start();
        }

        // Token: 0x06001FD4 RID: 8148 RVA: 0x00008693 File Offset: 0x00006893
        private void OnDisable()
        {
        	SetHitTriggerEnabled(false);
        	if (lift == null)
        	{
        		lift = GetComponent<GooseBodyLift>();
        	}
        	if (lift != null)
        	{
        		lift.Snap(0f);
        	}
        	StalkRequested = false;
        	ClearChaseRaised();
        	if (Tint != null)
        	{
        		Tint.ResetTint();
        	}
        	if (enemyVision != null)
        	{
        		enemyVision.SetRaycastDisabled(false);
        	}
        }

        // Token: 0x06001FD5 RID: 8149 RVA: 0x00008695 File Offset: 0x00006895
        protected override void AfterRevive()
        {
        	enemyVision.ResetSight();
        	StalkRequested = false;
        	ClearChaseRaised();
        	if (Tint != null)
        	{
        		Tint.ResetTint();
        	}
        	if (lift != null)
        	{
        		lift.Snap(0f);
        	}
        }

        // Token: 0x06001FD6 RID: 8150 RVA: 0x00008697 File Offset: 0x00006897
        public void ResetHit()
        {
        	HitRegistered = false;
        }

        // Token: 0x06001FD7 RID: 8151 RVA: 0x00008699 File Offset: 0x00006899
        public void RegisterHit()
        {
        	HitRegistered = true;
        }

        // Token: 0x06001FD8 RID: 8152 RVA: 0x0000869B File Offset: 0x0000689B
        public void SetHitTriggerEnabled(bool enabledState)
        {
        	if (hitTrigger != null)
        	{
        		hitTrigger.enabled = enabledState;
        	}
        }

        // Token: 0x06001FD9 RID: 8153 RVA: 0x0000869D File Offset: 0x0000689D
        public void SetBodyLifted(bool lifted)
        {
        	lift.SetLifted(lifted);
        }

        // Token: 0x06001FDA RID: 8154 RVA: 0x0000869F File Offset: 0x0000689F
        public override void PlayAttackSound()
        {
        	Play(attackSoundGroup, attackSoundVariation);
        }

        // Token: 0x06001FDB RID: 8155 RVA: 0x000086A1 File Offset: 0x000068A1
        public void PlayWindupSound()
        {
        	Play(windupSoundGroup, windupSoundVariation);
        }

        // Token: 0x06001FDC RID: 8156 RVA: 0x000086A3 File Offset: 0x000068A3
        public void PlayDazeSound()
        {
        	Play(dazeSoundGroup, dazeSoundVariation);
        }

        // Token: 0x06001FDD RID: 8157 RVA: 0x000086A5 File Offset: 0x000068A5
        private void Play(string group, string variation)
        {
        	if (!string.IsNullOrEmpty(group))
        	{
        		MasterAudio.PlaySound3DAtTransform(group, transform, 1f, null, 0f, string.IsNullOrEmpty(variation) ? null : variation);
        	}
        }

        // Token: 0x04001AF8 RID: 6904
        [Header("Hit")]
        [SerializeField]
        private Collider hitTrigger;

        // Token: 0x04001AF9 RID: 6905
        [Header("Sounds (MasterAudio group / variation; empty = silent)")]
        [SerializeField]
        private string attackSoundGroup = "goose-attack";

        // Token: 0x04001AFA RID: 6906
        [SerializeField]
        private string attackSoundVariation = "";

        // Token: 0x04001AFB RID: 6907
        [SerializeField]
        private string windupSoundGroup = "";

        // Token: 0x04001AFC RID: 6908
        [SerializeField]
        private string windupSoundVariation = "";

        // Token: 0x04001AFD RID: 6909
        [SerializeField]
        private string dazeSoundGroup = "";

        // Token: 0x04001AFE RID: 6910
        [SerializeField]
        private string dazeSoundVariation = "";

        // Token: 0x04001AFF RID: 6911
        private GooseBodyLift lift;

        // Token: 0x04001B00 RID: 6912
        private GooseTint tintDriver;
    }
}
