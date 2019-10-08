using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace Batting
{
	public class HitEffSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var deltaTime = World.TinyEnvironment().frameDeltaTime;

			Entities.ForEach( ( Entity entity, ref HitEffInfo eff, ref Translation trans, ref NonUniformScale scl ) => {
				if( !eff.IsActive || !eff.Initialized )
					return;

				eff.Timer += deltaTime;
				if( eff.Timer > 0.5f ) {
					Debug.LogAlways("eff end");			
					eff.IsActive = false;
					eff.Initialized = false;
					//scl.Value.x = 0;
				}
			} );
		}
	}
}
