using System;
using Characters.Strategies;

namespace Characters.States.Goose
{
    // Token: 0x020003A6 RID: 934
    public class WindupState : ChaseState
    {
        // Token: 0x17000336 RID: 822
        // (get) Token: 0x06001ECF RID: 7887 RVA: 0x000082D9 File Offset: 0x000064D9
        public override string StateName
        {
            get
            {
                return "Windup";
            }
        }

        // Token: 0x17000337 RID: 823
        // (get) Token: 0x06001ED0 RID: 7888 RVA: 0x000082DC File Offset: 0x000064DC
        public override byte StateSymbol
        {
            get
            {
                return 87;
            }
        }

        // Token: 0x06001ED1 RID: 7889 RVA: 0x000082DF File Offset: 0x000064DF
        public WindupState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
