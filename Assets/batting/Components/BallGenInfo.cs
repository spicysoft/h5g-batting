using Unity.Entities;

namespace Batting
{
	public struct BallGenInfo : IComponentData
	{
		public bool Initialized;
		public bool IsRequested;
		public int GeneratedCnt;
	}
}
