using DarkTonic.MasterAudio;
using System;
using Characters.Mover;
using Characters.States.Krasue;
using Characters.Vision;
using UnityEngine;

namespace Characters.Agents
{
    // Token: 0x020003C5 RID: 965
    [RequireComponent(typeof(KrasueStateController), typeof(PlayerOnKrasueVision))]
    [RequireComponent(typeof(KrasueMover), typeof(EnemyVision))]
    public class KrasueAgent : EnemyAgent
    {
        // Token: 0x06001FE2 RID: 8162 RVA: 0x000086BB File Offset: 0x000068BB
        protected virtual string GetShoutSoundName()
        {
        	return "shout";
        }

        // Token: 0x06001FE3 RID: 8163 RVA: 0x000086BE File Offset: 0x000068BE
        public override void PlayAttackSound()
        {
        	MasterAudio.PlaySound("Enemy", 1f, null, 0f, GetShoutSoundName());
        }
    }
}
