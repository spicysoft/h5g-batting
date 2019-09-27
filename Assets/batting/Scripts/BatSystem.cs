using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Batting
{
	public class BatSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach( ( Entity entity, ref BatInfo bat, ref Rotation rotation ) => {
				//if( !ball.Initialized )
				//	return;

				quaternion rot = rotation.Value;
				float za = bat.ZAng;
				float aspd = math.radians(90f) * World.TinyEnvironment().frameDeltaTime;
				za += aspd;
				bat.ZAng = za;
				rotation.Value = quaternion.RotateZ( za );

			} );
		}
	}
}
