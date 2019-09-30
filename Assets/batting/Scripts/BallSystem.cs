using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Batting
{
	public class BallSystem : ComponentSystem
	{
		public const int StPrepare = 0;
		public const int StMove = 1;
		public const int StEnd = 2;

		protected override void OnUpdate()
		{
			Entities.ForEach( ( Entity entity, ref BallInfo ball, ref Translation trans, ref NonUniformScale scl ) => {
				if( !ball.Initialized ) {
					ball.Initialized = true;
					ball.Status = StPrepare;
					ball.Timer = 0;
					trans.Value = new float3( 0, 100f, 0 );
					scl.Value = new float3( 1f, 1f, 1f );
					return;
				}

				float dt = World.TinyEnvironment().frameDeltaTime;

				switch( ball.Status ) {
				case StPrepare:
					ball.Timer += dt;
					if( ball.Timer > 1.0f ) {
						ball.Status = StMove;
						ball.Timer = 0;
					}
					break;
				case StMove:
					var pos = trans.Value;
					float spd = -400f * dt;
					pos.y += spd;
					trans.Value = pos;

					if( pos.y < -400f ) {
						ball.Status = StEnd;
						scl.Value.x = 0;
					}
					break;
				case StEnd:
					ball.Timer += dt;
					if( ball.Timer > 1f ) {
						ball.Initialized = false;
					}
					break;
				}

			} );
		}
	}
}
