using System;
using UnityEngine;

namespace Characters.Agents
{
    // Token: 0x020003C4 RID: 964
    public class GooseHitTrigger : MonoBehaviour
    {
        // Token: 0x06001FDF RID: 8159 RVA: 0x000086AF File Offset: 0x000068AF
        private void Awake()
        {
        	if (agent == null)
        	{
        		agent = GetComponentInParent<GooseAgent>();
        	}
        }

        // Token: 0x06001FE0 RID: 8160 RVA: 0x000086B1 File Offset: 0x000068B1
        private void OnTriggerEnter(Collider other)
        {
        	if (agent != null && other != null && (other.CompareTag("Player") || other.CompareTag("Avatar")))
        	{
        		agent.HitRegistered = true;
        	}
        }

        // Token: 0x04001B08 RID: 6920
        [SerializeField]
        private GooseAgent agent;
    }
}
