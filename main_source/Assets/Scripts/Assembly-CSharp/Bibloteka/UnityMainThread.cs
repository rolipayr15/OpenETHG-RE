using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bibloteka
{
    // Token: 0x0200063A RID: 1594
    internal class UnityMainThread : MonoBehaviour
    {
        // Token: 0x060035DA RID: 13786 RVA: 0x0000CC65 File Offset: 0x0000AE65
        public static UnityMainThread GetWkr()
        {
            return null;
        }

        // Token: 0x060035DB RID: 13787 RVA: 0x0000CC68 File Offset: 0x0000AE68
        private void Awake()
        {
        }

        // Token: 0x060035DC RID: 13788 RVA: 0x0000CC6A File Offset: 0x0000AE6A
        private void Update()
        {
        }

        // Token: 0x060035DD RID: 13789 RVA: 0x0000CC6C File Offset: 0x0000AE6C
        internal void AddJob(Action newJob)
        {
        }

        // Token: 0x04002B3E RID: 11070
        internal static UnityMainThread wkr;

        // Token: 0x04002B3F RID: 11071
        private Queue<Action> jobs;
    }
}
