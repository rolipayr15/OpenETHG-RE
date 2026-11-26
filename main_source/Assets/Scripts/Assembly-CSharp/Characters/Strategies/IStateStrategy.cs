using System;
using System.Collections;
using Characters.States;

namespace Characters.Strategies
{
    // Token: 0x0200038D RID: 909
    public interface IStateStrategy
    {
        // Token: 0x06001E50 RID: 7760
        IEnumerator Start(EnemyStateController stateController);

        // Token: 0x06001E51 RID: 7761
        IEnumerator Update(EnemyStateController stateController);

        // Token: 0x06001E52 RID: 7762
        IEnumerator Stop(EnemyStateController stateController);
    }
}
