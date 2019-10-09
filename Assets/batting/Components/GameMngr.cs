using Unity.Entities;

namespace Batting
{
	public struct GameMngr : IComponentData
	{
		public bool Initialized;
		public bool IsUpdatedScore;
		public int Score;           // スコア.
		public int HiScore;			// ハイスコア.
	}
}
