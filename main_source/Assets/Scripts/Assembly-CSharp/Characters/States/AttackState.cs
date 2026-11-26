using System;
using Characters.Strategies;

namespace Characters.States
{
    // Token: 0x0200039B RID: 923
    public class AttackState : AbstractState
    {
        // Token: 0x17000318 RID: 792
        // (get) Token: 0x06001E97 RID: 7831 RVA: 0x00008208 File Offset: 0x00006408
        public override string StateName
        {
            get
            {
                return "Attack";
            }
        }

        // Token: 0x17000319 RID: 793
        // (get) Token: 0x06001E98 RID: 7832 RVA: 0x0000820B File Offset: 0x0000640B
        public override byte StateSymbol
        {
            get
            {
                return 65;
            }
        }

        // Token: 0x06001E99 RID: 7833 RVA: 0x0000820E File Offset: 0x0000640E
        public AttackState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
