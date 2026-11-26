using System;
using Characters.Strategies;

namespace Characters.States.Goose
{
    // Token: 0x020003A5 RID: 933
    public class RecoveryState : AbstractState
    {
        // Token: 0x17000334 RID: 820
        // (get) Token: 0x06001ECC RID: 7884 RVA: 0x000082CA File Offset: 0x000064CA
        public override string StateName
        {
            get
            {
                return "Recovery";
            }
        }

        // Token: 0x17000335 RID: 821
        // (get) Token: 0x06001ECD RID: 7885 RVA: 0x000082CD File Offset: 0x000064CD
        public override byte StateSymbol
        {
            get
            {
                return 86;
            }
        }

        // Token: 0x06001ECE RID: 7886 RVA: 0x000082D0 File Offset: 0x000064D0
        public RecoveryState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
