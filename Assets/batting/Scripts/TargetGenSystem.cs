using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;

namespace Batting
{
	public class TargetGenSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool isReq = false;
			Entities.ForEach( ( ref TargetGenInfo gen ) => {
				if( !gen.Initialized ) {
					gen.Initialized = true;
					gen.GeneratedCnt = 0;
					isReq = true;
					return;
				}
//				isReq = gen.IsRequested;
			} );


			if( isReq ) {

				int recycled = 0;
				Entities.ForEach( ( Entity entity, ref TargetInfo info ) => {
					info.IsActive = true;
					info.Initialized = false;
					++recycled;
				} );

				var env = World.TinyEnvironment();
				SceneReference tarBase = env.GetConfigData<GameConfig>().TargetScn;
				for( int i = 0; i < 10-recycled; i++ )
					SceneService.LoadSceneAsync( tarBase );
			}
		}
	}
}
