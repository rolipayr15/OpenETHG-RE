using System;
using Characters.Strategies;

namespace Characters.States
{
    // Token: 0x0200039C RID: 924
    public class ChaseState : AbstractState
    {
        // Token: 0x1700031A RID: 794
        // (get) Token: 0x06001E9A RID: 7834 RVA: 0x00008217 File Offset: 0x00006417
        public override string StateName
        {
            get
            {
                return "Chase";
            }
        }

        // Token: 0x1700031B RID: 795
        // (get) Token: 0x06001E9B RID: 7835 RVA: 0x0000821A File Offset: 0x0000641A
        public override byte StateSymbol
        {
            get
            {
                return 67;
            }
        }

        // Token: 0x06001E9C RID: 7836 RVA: 0x0000821D File Offset: 0x0000641D
        public ChaseState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
