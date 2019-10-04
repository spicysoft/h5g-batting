using Unity.Entities;

namespace Batting
{
	public struct TargetInfo : IComponentData
	{
		public bool IsActive;
		public bool Initialized;
		public int Status;
		public float Timer;
		public float Radius;
		public float Level;
	}
}
