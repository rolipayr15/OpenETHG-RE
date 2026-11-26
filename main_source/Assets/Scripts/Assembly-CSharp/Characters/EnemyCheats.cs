using System;
using Characters.Mover;
using Characters.States;
using Characters.Vision;
using UnityEngine;

namespace Characters
{
    // Token: 0x0200037A RID: 890
    public class EnemyCheats : MonoBehaviour
    {
        // Token: 0x06001DFB RID: 7675 RVA: 0x00007FDA File Offset: 0x000061DA
        private void Start()
        {
        }

        // Token: 0x040019E0 RID: 6624
        private EnemyVision enemyVision;

        // Token: 0x040019E1 RID: 6625
        private EnemyStateController enemyStateController;

        // Token: 0x040019E2 RID: 6626
        private EnemyMover enemyMover;
    }
}
