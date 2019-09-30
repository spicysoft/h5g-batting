using Unity.Entities;

namespace Batting
{
	public struct BallInfo : IComponentData
	{
		public bool Initialized;
		public int Status;
		public float Timer;
	}
}
