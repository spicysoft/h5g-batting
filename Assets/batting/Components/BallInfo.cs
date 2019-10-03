using Unity.Entities;
using Unity.Mathematics;

namespace Batting
{
	public struct BallInfo : IComponentData
	{
		public bool SeedInitialized;
		public bool Initialized;
		public int Status;
		public float Timer;
		public float3 Dir;
		public float Speed;
		public int Count;
	}
}
