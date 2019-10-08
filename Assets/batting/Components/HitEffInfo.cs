using Unity.Entities;
using Unity.Mathematics;

namespace Batting
{
	public struct HitEffInfo : IComponentData
	{
		public bool IsActive;
		public bool Initialized;
		public float Timer;
	}
}
