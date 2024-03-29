using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Batting
{
	public class TargetSystem : ComponentSystem
	{
		public const float Radius1 = 80f;
		public const float Radius2 = 60f;

		public const int StNorm = 0;
		public const int StHit = 1;
		public const int StWait = 2;
		public const int StEnd = 3;

		protected override void OnUpdate()
		{
			float dt = World.TinyEnvironment().frameDeltaTime;
			int score = 0;

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
					if( tar.Level == 2 ) {
						tar.Level = 1;
						tar.Radius = Radius2;
						opt.size.x = tar.Radius * 2f;
						opt.size.y = tar.Radius * 2f;
						score += 100;
					}
					else if( tar.Level == 1 ) {
						tar.Level = 0;
						score += 150;
					}
					tar.Status = StWait;
				}
				else if( tar.Status == StWait ) {
					tar.Timer += dt;
					if( tar.Timer > 0.1f ) {
						if( tar.Level > 0 ) {
							tar.Status = StNorm;
						}
						else {
							tar.Status = StEnd;
							scl.Value.x = 0;
							tar.IsActive = false;
						}
						tar.Timer = 0;
					}
				}

			} );

			// スコア更新.
			if( score > 0 ) {
				Entities.ForEach( ( ref GameMngr mngr ) => {
					mngr.IsUpdatedScore = true;
					mngr.Score += score;
				} );
			}
		}
	}
}
