using System;
using Characters.Strategies;

namespace Characters.States.Goose
{
    // Token: 0x020003A3 RID: 931
    public class ChargeState : AbstractState
    {
        // Token: 0x1700032F RID: 815
        // (get) Token: 0x06001EC3 RID: 7875 RVA: 0x000082A6 File Offset: 0x000064A6
        public override string StateName
        {
            get
            {
                return "Charge";
            }
        }

        // Token: 0x17000330 RID: 816
        // (get) Token: 0x06001EC4 RID: 7876 RVA: 0x000082A9 File Offset: 0x000064A9
        public override byte StateSymbol
        {
            get
            {
                return 71;
            }
        }

        // Token: 0x06001EC5 RID: 7877 RVA: 0x000082AC File Offset: 0x000064AC
        public ChargeState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
