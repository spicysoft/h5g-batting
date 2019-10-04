using Unity.Entities;

namespace Batting
{
	public struct TargetGenInfo : IComponentData
	{
		public bool Initialized;
		public bool IsRequested;
		public int GeneratedCnt;
	}
}
