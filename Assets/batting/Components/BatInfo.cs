using Unity.Entities;

namespace Batting
{
	public struct BatInfo : IComponentData
	{
		public bool Initialized;
		public int Status;
		public float ZAng;
	}
}
