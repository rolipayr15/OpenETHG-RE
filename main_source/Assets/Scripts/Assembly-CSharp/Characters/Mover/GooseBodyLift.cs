using System;
using UnityEngine;

namespace Characters.Mover
{
    // Token: 0x020003AE RID: 942
    public class GooseBodyLift : MonoBehaviour
    {
        // Token: 0x17000347 RID: 839
        // (get) Token: 0x06001F17 RID: 7959 RVA: 0x000083C5 File Offset: 0x000065C5
        public bool Lifted
        {
        	get => lifted;
        }

        // Token: 0x06001F18 RID: 7960 RVA: 0x000083C8 File Offset: 0x000065C8
        private void Awake()
        {
        	CaptureRest();
        }

        // Token: 0x06001F19 RID: 7961 RVA: 0x000083CA File Offset: 0x000065CA
        private void CaptureRest()
        {
        	if (!restCaptured && body != null)
        	{
        		restLocalPosition = body.localPosition;
        		restCaptured = true;
        	}
        }

        // Token: 0x06001F1A RID: 7962 RVA: 0x000083CC File Offset: 0x000065CC
        public void SetLifted(bool value)
        {
        	if (body == null || lifted == value)
        	{
        		return;
        	}
        	lifted = value;
        	if (!isActiveAndEnabled || liftSeconds <= 0f)
        	{
        		Snap(value ? 1f : 0f);
        	}
        }

        // Token: 0x06001F1B RID: 7963 RVA: 0x000083CE File Offset: 0x000065CE
        public void Snap(float fraction)
        {
        	CaptureRest();
        	lift01 = Mathf.Clamp01(fraction);
        	lifted = lift01 > 0f;
        	Apply();
        }

        // Token: 0x06001F1C RID: 7964 RVA: 0x000083D0 File Offset: 0x000065D0
        private void Apply()
        {
        	if (body != null)
        	{
        		body.localPosition = restLocalPosition + Vector3.up * (chargeLift * Mathf.SmoothStep(0f, 1f, lift01));
        	}
        }

        // Token: 0x06001F1D RID: 7965 RVA: 0x000083D2 File Offset: 0x000065D2
        private void LateUpdate()
        {
        	if (body == null)
        	{
        		return;
        	}
        	float target = lifted ? 1f : 0f;
        	if (!Mathf.Approximately(lift01, target))
        	{
        		lift01 = Mathf.MoveTowards(lift01, target, liftSeconds > 0f ? Time.deltaTime / liftSeconds : 1f);
        		Apply();
        	}
        }

        // Token: 0x04001A6F RID: 6767
        [Tooltip("Child holding the model.")]
        [SerializeField]
        private Transform body;

        // Token: 0x04001A70 RID: 6768
        [SerializeField]
        private float chargeLift = 0.5f;

        // Token: 0x04001A71 RID: 6769
        [Tooltip("Seconds the body takes to rise at charge start and to settle at landing.")]
        [SerializeField]
        private float liftSeconds = 0.4f;

        // Token: 0x04001A72 RID: 6770
        private Vector3 restLocalPosition;

        // Token: 0x04001A73 RID: 6771
        private bool restCaptured;

        // Token: 0x04001A74 RID: 6772
        private bool lifted;

        // Token: 0x04001A75 RID: 6773
        private float lift01;
    }
}
