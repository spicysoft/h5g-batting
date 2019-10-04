using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Batting
{
	public class TargetSystem : ComponentSystem
	{
		public const int StNorm = 0;
		public const int StHit = 1;
		public const int StEnd = 2;

		protected override void OnUpdate()
		{
			float dt = World.TinyEnvironment().frameDeltaTime;
			
			Entities.ForEach( ( Entity entity, ref TargetInfo tar, ref Translation trans, ref Rotation rot, ref NonUniformScale scl, ref Sprite2DRendererOptions opt ) => {
				if( !tar.IsActive || !tar.Initialized )
					return;
				/*
				if( !tar.Initialized ) {
					tar.Initialized = true;
					opt.size.x = tar.Radius * 2f;
					opt.size.y = tar.Radius * 2f;
					scl.Value.x = 1f;
					tar.Status = StNorm;
					tar.Timer = 0;
					tar.Level = 1;  //
					return;
				}*/

				if( tar.Status == StHit ) {
					tar.Timer += dt;
					if( tar.Timer > 0.1f ) {
						tar.Status = StEnd;
						scl.Value.x = 0;
						tar.IsActive = false;
					}
				}

			} );

		}
	}
}
