using System;
using UnityEngine;

namespace Characters.Mover
{
    // Token: 0x020003B4 RID: 948
    public class GooseTint : MonoBehaviour
    {
        // Token: 0x06001F4D RID: 8013 RVA: 0x00008489 File Offset: 0x00006689
        private void Awake()
        {
        	tintId = Shader.PropertyToID(tintProperty);
        	block = new MaterialPropertyBlock();
        	levels = new float[renderers == null ? 0 : renderers.Length];
        	Apply();
        }

        // Token: 0x06001F4E RID: 8014 RVA: 0x0000848B File Offset: 0x0000668B
        private void OnDisable()
        {
        	ResetTint();
        }

        // Token: 0x06001F4F RID: 8015 RVA: 0x0000848D File Offset: 0x0000668D
        public void StartWindup(float windupSeconds)
        {
        	phase = Phase.Windup;
        	phaseStartedAt = Time.time;
        	phaseSeconds = windupSeconds;
        }

        // Token: 0x06001F50 RID: 8016 RVA: 0x0000848F File Offset: 0x0000668F
        public void HoldCharge()
        {
        	phase = Phase.Charge;
        	for (int i = 0; i < levels.Length; i++)
        	{
        		levels[i] = 1f;
        	}
        	Apply();
        }

        // Token: 0x06001F51 RID: 8017 RVA: 0x00008491 File Offset: 0x00006691
        public void StartCooldown(float cooldownSeconds)
        {
        	phase = Phase.Cooldown;
        	phaseStartedAt = Time.time;
        	phaseSeconds = cooldownSeconds;
        }

        // Token: 0x06001F52 RID: 8018 RVA: 0x00008493 File Offset: 0x00006693
        public void ResetTint()
        {
        	phase = Phase.Idle;
        	if (levels == null)
        	{
        		return;
        	}
        	for (int i = 0; i < levels.Length; i++)
        	{
        		levels[i] = 0f;
        	}
        	Apply();
        }

        // Token: 0x06001F53 RID: 8019 RVA: 0x00008495 File Offset: 0x00006695
        private void Update()
        {
        	if (levels == null || levels.Length == 0)
        	{
        		return;
        	}
        	float elapsed = Time.time - phaseStartedAt;
        	if (phase == Phase.Cooldown)
        	{
        		float level = GooseTintCurve.Cooldown(elapsed, phaseSeconds);
        		for (int i = 0; i < levels.Length; i++)
        		{
        			levels[i] = level;
        		}
        		Apply();
        		if (level <= 0f)
        		{
        			phase = Phase.Idle;
        		}
        	}
        	else if (phase == Phase.Windup)
        	{
        		for (int i = 0; i < levels.Length; i++)
        		{
        			levels[i] = GooseTintCurve.WindupFill(i, levels.Length, elapsed, phaseSeconds);
        		}
        		Apply();
        	}
        }

        // Token: 0x06001F54 RID: 8020 RVA: 0x00008497 File Offset: 0x00006697
        private void Apply()
        {
        	if (renderers == null || block == null)
        	{
        		return;
        	}
        	for (int i = 0; i < renderers.Length; i++)
        	{
        		Renderer item = renderers[i];
        		if (item != null)
        		{
        			item.GetPropertyBlock(block);
        			block.SetColor(tintId, new Color(1f, 1f, 1f, 1f));
        			item.SetPropertyBlock(block);
        		}
        	}
        }

        // Token: 0x04001AC5 RID: 6853
        [Tooltip("Fill order: index 0 fills first. Builder wires rear companion, middle companion, leader.")]
        [SerializeField]
        private Renderer[] renderers;

        // Token: 0x04001AC6 RID: 6854
        [SerializeField]
        private Color tint = new Color(1f, 0.35f, 0.35f, 1f);

        // Token: 0x04001AC7 RID: 6855
        [SerializeField]
        private string tintProperty = "_Tint";

        // Token: 0x04001AC8 RID: 6856
        private GooseTint.Phase phase;

        // Token: 0x04001AC9 RID: 6857
        private float phaseStartedAt;

        // Token: 0x04001ACA RID: 6858
        private float phaseSeconds;

        // Token: 0x04001ACB RID: 6859
        private float[] levels;

        // Token: 0x04001ACC RID: 6860
        private MaterialPropertyBlock block;

        // Token: 0x04001ACD RID: 6861
        private int tintId;

        // Token: 0x020008D6 RID: 2262
        private enum Phase
        {
            // Token: 0x0400352E RID: 13614
            Idle,
            // Token: 0x0400352F RID: 13615
            Windup,
            // Token: 0x04003530 RID: 13616
            Charge,
            // Token: 0x04003531 RID: 13617
            Cooldown
        }
    }
}
