using Prime31.MessageKit;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		// Only the host decides about enemies opening doors.
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost && !UserGameSettings.hostLeftGame)
		{
			return;
		}
		Characters.Agents.EnemyAgent enemy = other != null ? other.gameObject.GetComponentInParent<Characters.Agents.EnemyAgent>() : null;
		if (enemy == null)
		{
			return;
		}
		Door door = GetComponentInParent<Door>();
		if (door == null || !door.isOpen)
		{
			MessageKit<string>.post(30, enemy.name);
		}
	}
}
