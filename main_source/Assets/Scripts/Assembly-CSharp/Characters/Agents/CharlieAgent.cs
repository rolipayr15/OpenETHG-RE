using DarkTonic.MasterAudio;
using System;
using Characters.Mover;
using Characters.States.Charlie;
using Characters.Vision;
using UnityEngine;

namespace Characters.Agents
{
    // Token: 0x020003C0 RID: 960
    [RequireComponent(typeof(CharlieStateController), typeof(PlayerOnCharlieVision))]
    [RequireComponent(typeof(CharlieMover), typeof(EnemyVision))]
    public class CharlieAgent : EnemyAgent
    {
        // Token: 0x06001F99 RID: 8089 RVA: 0x00008591 File Offset: 0x00006791
        protected override void Start()
        {
        	base.Start();
        	SetCharlieTransparency(1f);
        }

        // Token: 0x06001F9A RID: 8090 RVA: 0x00008593 File Offset: 0x00006793
        public override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("Enemy", 1f, null, 0f, "charlieShout");
        }

        // Token: 0x06001F9B RID: 8091 RVA: 0x00008595 File Offset: 0x00006795
        public void SetCharlieTransparency(float a)
        {
        	Shader.SetGlobalFloat("_CharlieTransparency", baseTransparency * a);
        	Shader.SetGlobalFloat("_CharliePortalFade", a);
        }

        // Token: 0x04001AE7 RID: 6887
        [SerializeField]
        private float baseTransparency = 0.8f;
    }
}
