using System;
using UnityEngine;

namespace Characters.Mover
{
    // Token: 0x020003B6 RID: 950
    public sealed class GooseTrail
    {
        // Token: 0x06001F58 RID: 8024 RVA: 0x000084AF File Offset: 0x000066AF
        public GooseTrail(int capacity = 40, float crumbSpacing = 0.2f)
        {
        	crumbs = new Vector3[Mathf.Max(capacity, 2)];
        	this.crumbSpacing = Mathf.Max(crumbSpacing, 0.001f);
        }

        // Token: 0x1700034F RID: 847
        // (get) Token: 0x06001F59 RID: 8025 RVA: 0x000084B7 File Offset: 0x000066B7
        public int Count
        {
            get
            {
                return count;
            }
        }

        // Token: 0x17000350 RID: 848
        // (get) Token: 0x06001F5A RID: 8026 RVA: 0x000084BA File Offset: 0x000066BA
        public int Capacity
        {
            get
            {
                return crumbs.Length;
            }
        }

        // Token: 0x17000351 RID: 849
        // (get) Token: 0x06001F5B RID: 8027 RVA: 0x000084BD File Offset: 0x000066BD
        public float CrumbSpacing
        {
            get
            {
                return crumbSpacing;
            }
        }

        // Token: 0x06001F5C RID: 8028 RVA: 0x000084C4 File Offset: 0x000066C4
        public void Clear()
        {
        	newest = -1;
        	count = 0;
        }

        // Token: 0x06001F5D RID: 8029 RVA: 0x000084C6 File Offset: 0x000066C6
        public bool Record(Vector3 leaderPosition)
        {
        	if (count > 0 && Vector3.Distance(crumbs[newest], leaderPosition) < crumbSpacing)
        	{
        		return false;
        	}
        	newest = (newest + 1) % crumbs.Length;
        	crumbs[newest] = leaderPosition;
        	if (count < crumbs.Length)
        	{
        		count++;
        	}
        	return true;
        }

        // Token: 0x06001F5E RID: 8030 RVA: 0x00010FD4 File Offset: 0x0000F1D4
        public Vector3 CrumbFromNewest(int stepsBack)
        {
        	if (stepsBack < 0 || count <= stepsBack)
        	{
        		throw new ArgumentOutOfRangeException("stepsBack");
        	}
        	return crumbs[(newest - stepsBack + crumbs.Length) % crumbs.Length];
        }

        // Token: 0x06001F5F RID: 8031 RVA: 0x000084C9 File Offset: 0x000066C9
        public bool PointBehind(float distance, Vector3 leaderPosition, out Vector3 point, out Vector3 direction)
        {
        	point = leaderPosition;
        	direction = Vector3.zero;
        	if (count == 0)
        	{
        		return false;
        	}
        	if (distance <= 0f)
        	{
        		direction = Direction(CrumbFromNewest(0), leaderPosition);
        		return true;
        	}
        	Vector3 previous = leaderPosition;
        	Vector3 crumb = leaderPosition;
        	for (int i = 0; i < count; i++)
        	{
        		crumb = CrumbFromNewest(i);
        		float segment = Vector3.Distance(crumb, previous);
        		if (segment > 0f && distance <= segment)
        		{
        			point = Vector3.Lerp(previous, crumb, distance / segment);
        			direction = Direction(crumb, previous);
        			return true;
        		}
        		if (segment > 0f || i == count - 1)
        		{
        			direction = Direction(crumb, previous);
        		}
        		distance -= segment;
        		previous = crumb;
        	}
        	point = crumb;
        	return false;
        }

        // Token: 0x06001F60 RID: 8032 RVA: 0x00010FEC File Offset: 0x0000F1EC
        private static Vector3 Direction(Vector3 older, Vector3 newer)
        {
        	Vector3 delta = newer - older;
        	return delta.sqrMagnitude <= 1E-08f ? Vector3.zero : delta.normalized;
        }

        // Token: 0x04001ACE RID: 6862
        public const int DefaultCapacity = 40;

        // Token: 0x04001ACF RID: 6863
        public const float DefaultCrumbSpacing = 0.2f;

        // Token: 0x04001AD0 RID: 6864
        private readonly Vector3[] crumbs;

        // Token: 0x04001AD1 RID: 6865
        private readonly float crumbSpacing;

        // Token: 0x04001AD2 RID: 6866
        private int newest = -1;

        // Token: 0x04001AD3 RID: 6867
        private int count;
    }
}
