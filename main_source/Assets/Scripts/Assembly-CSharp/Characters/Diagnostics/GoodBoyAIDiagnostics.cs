using Characters.Mover;
using Characters.States.Dog;
using System.Globalization;
using System;
using System.Diagnostics;
using Characters.Agents;
using UnityEngine;

namespace Characters.Diagnostics
{
    // Token: 0x020003B9 RID: 953
    internal static class GoodBoyAIDiagnostics
    {
        // Token: 0x06001F69 RID: 8041 RVA: 0x00008507 File Offset: 0x00006707
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(DogAgent dogAgent, string category, string message, UnityEngine.Object context = null)
        {
        	if (dogAgent == null)
        	{
        		return;
        	}
        	DogStateController controller = dogAgent.GetComponent<DogStateController>();
        	string state = controller != null ? controller.DiagnosticCurrentStateName : "<no-controller>";
        	EnemyMover mover = dogAgent.EnemyMover;
        	string moverText = mover != null ? string.Format(CultureInfo.InvariantCulture, "speed={0:0.00} patrol={1:0.00} chase={2:0.00} hasPath={3} pathPending={4} pathStatus={5} remaining={6:0.00} destination={7} doorFlag={8}", mover.DiagnosticCurrentSpeed, mover.DiagnosticPatrolSpeed, mover.DiagnosticChaseSpeed, mover.DiagnosticHasPath, mover.DiagnosticPathPending, mover.DiagnosticPathStatus, mover.DiagnosticRemainingDistance, FormatVector(mover.DiagnosticDestination), mover.IsDoorDetected) : "mover=<none>";
        	string line = string.Format(CultureInfo.InvariantCulture, "{0}[{1}] t={2:0.00} frame={3} dog={4} state={5} position={6} {7} meatFound={8} eating={9} | {10}", Prefix, category, Time.time, Time.frameCount, dogAgent.name, state, FormatVector(dogAgent.transform.position), moverText, dogAgent.IsMeatFound, dogAgent.IsDogEating, message);
        	UnityEngine.Debug.Log(line, context != null ? context : dogAgent);
        }

        // Token: 0x06001F6A RID: 8042 RVA: 0x00008509 File Offset: 0x00006709
        public static string FormatVector(Vector3 value)
        {
        	return string.Format(CultureInfo.InvariantCulture, "({0:0.00},{1:0.00},{2:0.00})", value.x, value.y, value.z);
        }

        // Token: 0x04001AD4 RID: 6868
        private const string Prefix = "[GoodBoyAI]";
    }
}
