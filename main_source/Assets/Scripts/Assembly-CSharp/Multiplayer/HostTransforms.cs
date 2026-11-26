using UnityEngine;

namespace Multiplayer
{
	public struct HostTransforms
	{
		public ObjectTransform host;

		public Vector3 enemy;

		public HostTransforms(ObjectTransform _host, Vector3 _enemy)
		{
			host = _host;
			enemy = _enemy;
		}
	}
}
