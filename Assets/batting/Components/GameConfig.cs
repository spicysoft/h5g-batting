using Unity.Entities;
using Unity.Tiny.Scenes;

namespace Batting
{
	public struct GameConfig : IComponentData
	{
		public SceneReference TitleScn;
		public SceneReference MainScn;
		public SceneReference GameOverScn;
		public SceneReference ResultScn;
	}
}
