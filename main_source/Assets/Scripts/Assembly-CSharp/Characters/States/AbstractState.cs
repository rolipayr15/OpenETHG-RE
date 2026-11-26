using System;
using System.Collections;
using Characters.Strategies;

namespace Characters.States
{
    // Token: 0x0200039A RID: 922
    public abstract class AbstractState
    {
        // Token: 0x17000316 RID: 790
        // (get) Token: 0x06001E91 RID: 7825
        public abstract string StateName { get; }

        // Token: 0x17000317 RID: 791
        // (get) Token: 0x06001E92 RID: 7826
        public abstract byte StateSymbol { get; }

        // Token: 0x06001E93 RID: 7827 RVA: 0x000081F7 File Offset: 0x000063F7
        protected AbstractState(IStateStrategy stateStrategy)
        {
        	this.stateStrategy = stateStrategy;
        }

        // Token: 0x06001E94 RID: 7828 RVA: 0x000081FF File Offset: 0x000063FF
        protected IEnumerator StateAction(EnemyStateController stateController)
        {
        	yield return stateStrategy.Start(stateController);
        	while (true)
        	{
        		yield return stateStrategy.Update(stateController);
        	}
        }

        // Token: 0x06001E95 RID: 7829 RVA: 0x00008202 File Offset: 0x00006402
        public IEnumerator DoStateAction(EnemyStateController stateController)
        {
        	return StateAction(stateController);
        }

        // Token: 0x06001E96 RID: 7830 RVA: 0x00008205 File Offset: 0x00006405
        public IEnumerator OnStateActionStop(EnemyStateController stateController)
        {
        	yield return stateStrategy.Stop(stateController);
        }

        // Token: 0x04001A50 RID: 6736
        protected IStateStrategy stateStrategy;
    }
}
