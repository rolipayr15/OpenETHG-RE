using System;
using UnityEngine;

namespace Characters.Mover
{
    // Token: 0x020003AD RID: 941
    public static class GooseAnimatorDriver
    {
        // Token: 0x06001F10 RID: 7952 RVA: 0x000083B7 File Offset: 0x000065B7
        public static void Fire(Animator anim, string trigger, string[] variants)
        {
        	if (variants == null)
        	{
        		return;
        	}
        	anim.SetInteger("variant", UnityEngine.Random.Range(0, variants.Length));
        	foreach (string other in Triggers)
        	{
        		if (other != trigger)
        		{
        			anim.ResetTrigger(other);
        		}
        	}
        	anim.SetTrigger(trigger);
        }

        // Token: 0x06001F11 RID: 7953 RVA: 0x000083B9 File Offset: 0x000065B9
        public static void Walk(Animator anim)
        {
        	Fire(anim, "walk", GooseClips.Walk);
        }

        // Token: 0x06001F12 RID: 7954 RVA: 0x000083BB File Offset: 0x000065BB
        public static void Idle(Animator anim)
        {
        	Fire(anim, "idle", GooseClips.Idle);
        }

        // Token: 0x06001F13 RID: 7955 RVA: 0x000083BD File Offset: 0x000065BD
        public static void Windup(Animator anim)
        {
        	Fire(anim, "windup", GooseClips.Windup);
        }

        // Token: 0x06001F14 RID: 7956 RVA: 0x000083BF File Offset: 0x000065BF
        public static void Charge(Animator anim)
        {
        	Fire(anim, "charge", GooseClips.Idle);
        }

        // Token: 0x06001F15 RID: 7957 RVA: 0x000083C1 File Offset: 0x000065C1
        public static void Land(Animator anim)
        {
        	Fire(anim, "land", GooseClips.Dazed);
        }

        // Token: 0x04001A6E RID: 6766
        public static readonly string[] Triggers = new string[] { "idle", "walk", "windup", "charge", "land", "dazed", "attack" };
    }
}
