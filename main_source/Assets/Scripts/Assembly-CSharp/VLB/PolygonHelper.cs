using UnityEngine;

namespace VLB
{
	public class PolygonHelper : MonoBehaviour
	{
		public struct Plane2D
		{
			public Vector2 normal;

			public float distance;

			public float Distance(Vector2 point)
			{
				return 0f;
			}

			public Vector2 ClosestPoint(Vector2 pt)
			{
				return default;
			}

			public Vector2 Intersect(Vector2 p1, Vector2 p2)
			{
				return default;
			}

			public bool GetSide(Vector2 point)
			{
				return false;
			}

			public static Plane2D FromPoints(Vector3 p1, Vector3 p2)
			{
				return default;
			}

			public static Plane2D FromNormalAndPoint(Vector3 normalizedNormal, Vector3 p1)
			{
				return default;
			}

			public void Flip()
			{
			}

			public Vector2[] CutConvex(Vector2[] poly)
			{
				return null;
			}

			public override string ToString()
			{
				return null;
			}
		}
	}
}
