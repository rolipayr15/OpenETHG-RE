using System;
using Characters.Strategies;

namespace Characters.States.Dog
{
    // Token: 0x020003A8 RID: 936
    public class EatState : AbstractState
    {
        // Token: 0x1700033A RID: 826
        // (get) Token: 0x06001ED7 RID: 7895 RVA: 0x000082FA File Offset: 0x000064FA
        public override string StateName
        {
            get
            {
                return "Eat";
            }
        }

        // Token: 0x1700033B RID: 827
        // (get) Token: 0x06001ED8 RID: 7896 RVA: 0x000082FD File Offset: 0x000064FD
        public override byte StateSymbol
        {
            get
            {
                return 69;
            }
        }

        // Token: 0x06001ED9 RID: 7897 RVA: 0x00008300 File Offset: 0x00006500
        public EatState(IStateStrategy stateStrategy) : base(stateStrategy)
        {
        }
    }
}
