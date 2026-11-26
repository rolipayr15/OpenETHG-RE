using UnityEngine;
using System;

namespace Characters.Mover
{
    // Token: 0x020003B5 RID: 949
    public static class GooseTintCurve
    {
        // Token: 0x06001F56 RID: 8022 RVA: 0x000084A1 File Offset: 0x000066A1
        public static float WindupFill(int index, int count, float elapsed, float windupSeconds)
        {
        	if (count < 1)
        	{
        		return 0f;
        	}
        	if (windupSeconds <= 0f)
        	{
        		return 1f;
        	}
        	float segment = windupSeconds / count;
        	return Mathf.Clamp01((elapsed - segment * index) / segment);
        }

        // Token: 0x06001F57 RID: 8023 RVA: 0x000084A8 File Offset: 0x000066A8
        public static float Cooldown(float elapsed, float cooldownSeconds)
        {
        	if (cooldownSeconds <= 0f)
        	{
        		return 0f;
        	}
        	return 1f - Mathf.Clamp01(elapsed / cooldownSeconds);
        }
    }
}
