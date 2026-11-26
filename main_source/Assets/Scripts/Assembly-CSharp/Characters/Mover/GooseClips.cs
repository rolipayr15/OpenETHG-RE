using System;
using System.Runtime.CompilerServices;

namespace Characters.Mover
{
    // Token: 0x020003AF RID: 943
    public static class GooseClips
    {
        // Token: 0x06001F1F RID: 7967 RVA: 0x000083DC File Offset: 0x000065DC
        public static string StateName(string phase, int index)
        {
        	return phase + "_" + index;
        }

        // Token: 0x04001A76 RID: 6774
        public const string VariantParameter = "variant";

        // Token: 0x04001A77 RID: 6775
        public static readonly string[] Idle = new string[] { "Idle_1", "Idle_2" };

        // Token: 0x04001A78 RID: 6776
        public static readonly string[] Walk = new string[] { "Walk_F_IP" };

        // Token: 0x04001A79 RID: 6777
        public static readonly string[] Windup = new string[] { "Idle_5", "Drink" };

        // Token: 0x04001A7A RID: 6778
        public static readonly string[] ChargeStart = new string[] { "Fly_Start_IP" };

        // Token: 0x04001A7B RID: 6779
        public static readonly string[] Charge = new string[] { "Fly_F_IP" };

        // Token: 0x04001A7C RID: 6780
        public static readonly string[] Land = new string[] { "Fly_End_Landing_IP" };

        // Token: 0x04001A7D RID: 6781
        public static readonly string[] Dazed = new string[] { "Idle_3" };

        // Token: 0x04001A7E RID: 6782
        public static readonly string[] Attack = new string[] { "Attack" };

        // Token: 0x04001A7F RID: 6783
        public static readonly string[] IdleExtra = new string[] { "Idle_4", "Eat_1" };

        // Token: 0x04001A80 RID: 6784
        public static readonly string[] WindupExtra = new string[] { "Hissing", "Eat_2" };

        // Token: 0x04001A81 RID: 6785
        public static readonly string[] ChargeExtra = new string[] { "Fly_FL_IP", "Fly_FR_IP" };

        // Token: 0x04001A82 RID: 6786
        public static readonly string[] DazedExtra = new string[] { "Eat_3", "Idle_4" };

        // Token: 0x04001A83 RID: 6787
        public static readonly string[] LandExtra = new string[] { "Hit_F", "Hit_B" };

        // Token: 0x04001A84 RID: 6788
        private static readonly string[] None = new string[0];

        // Token: 0x04001A85 RID: 6789
        public static readonly (string phase, string[] variants, string[] extra, bool loops)[] Phases = new (string phase, string[] variants, string[] extra, bool loops)[] { ("Idle", Idle, IdleExtra, true), ("Walk", Walk, None, true), ("Windup", Windup, WindupExtra, true), ("ChargeStart", ChargeStart, None, false), ("Charge", Charge, ChargeExtra, true), ("Land", Land, LandExtra, false), ("Dazed", Dazed, DazedExtra, true), ("Attack", Attack, None, false) };
    }
}
