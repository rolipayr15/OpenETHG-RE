using System;
using Characters.Strategies;

namespace Characters.States
{
    // Token: 0x0200039F RID: 927
    public class InactiveState : AbstractState
    {
        // Token: 0x17000329 RID: 809
        // (get) Token: 0x06001EB5 RID: 7861 RVA: 0x0000826A File Offset: 0x0000646A
        public override string StateName
        {
            get
            {
                return "Inactive";
            }
        }

        // Token: 0x1700032A RID: 810
        // (get) Token: 0x06001EB6 RID: 7862 RVA: 0x0000826D File Offset: 0x0000646D
        public override byte StateSymbol
        {
            get
            {
                return 73;
            }
        }

        // Token: 0x06001EB7 RID: 7863 RVA: 0x00008270 File Offset: 0x00006470
        public InactiveState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
