using System;
using Characters.Strategies;

namespace Characters.States
{
    // Token: 0x0200039D RID: 925
    public class CheckLastLocationState : AbstractState
    {
        // Token: 0x1700031C RID: 796
        // (get) Token: 0x06001E9D RID: 7837 RVA: 0x00008226 File Offset: 0x00006426
        public override string StateName
        {
            get
            {
                return "CheckLastLocation";
            }
        }

        // Token: 0x1700031D RID: 797
        // (get) Token: 0x06001E9E RID: 7838 RVA: 0x00008229 File Offset: 0x00006429
        public override byte StateSymbol
        {
            get
            {
                return 76;
            }
        }

        // Token: 0x06001E9F RID: 7839 RVA: 0x0000822C File Offset: 0x0000642C
        public CheckLastLocationState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
