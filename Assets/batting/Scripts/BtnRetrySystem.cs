using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;
using Unity.Tiny.UIControls;

namespace Batting
{
	public class BtnRetrySystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool btnOn = false;
			Entities.WithAll<BtnRetryTag>().ForEach( ( Entity entity, ref PointerInteraction pointerInteraction ) => {
				if( pointerInteraction.clicked ) {
					//Debug.LogAlways("btn ret click");
					btnOn = true;
				}
			} );

			if( btnOn ) {
				var env = World.TinyEnvironment();
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().ResultScn );

				// 初期化.
				Entities.ForEach( ( ref BallInfo ball ) => {
					ball.Count = 0;
					ball.Initialized = false;
				} );

				Entities.ForEach( ( ref BatInfo bat ) => {
					bat.Initialized = false;
				} );

				Entities.ForEach( ( ref TargetGenInfo gen ) => {
					gen.Initialized = false;
				} );

				Entities.ForEach( ( ref GameMngr mngr ) => {
					mngr.Initialized = false;
				} );

			}
		}
	}
}
