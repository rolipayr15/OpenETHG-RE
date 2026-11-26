using System;
using Characters.Strategies;

namespace Characters.States
{
    // Token: 0x020003A0 RID: 928
    public class PatrolState : AbstractState
    {
        // Token: 0x1700032B RID: 811
        // (get) Token: 0x06001EB8 RID: 7864 RVA: 0x00008279 File Offset: 0x00006479
        public override string StateName
        {
            get
            {
                return "Patrol";
            }
        }

        // Token: 0x1700032C RID: 812
        // (get) Token: 0x06001EB9 RID: 7865 RVA: 0x0000827C File Offset: 0x0000647C
        public override byte StateSymbol
        {
            get
            {
                return 80;
            }
        }

        // Token: 0x06001EBA RID: 7866 RVA: 0x0000827F File Offset: 0x0000647F
        public PatrolState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
