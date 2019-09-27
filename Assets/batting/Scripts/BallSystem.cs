using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Batting
{
	public class BallSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach( ( Entity entity, ref BallInfo ball, ref Translation trans, ref NonUniformScale scl ) => {
				//if( !ball.Initialized )
				//	return;

				var pos = trans.Value;
				float spd = -100f * World.TinyEnvironment().frameDeltaTime;
				pos.y += spd;
				trans.Value = pos;

			} );
		}
	}
}
