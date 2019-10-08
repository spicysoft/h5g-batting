using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace Batting
{
	public class InitHitEffSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{

			Entities.ForEach( ( Entity entity, ref HitEffInfo eff, ref Translation trans, ref NonUniformScale scl, ref Sprite2DSequencePlayer seq ) => {
				if( !eff.IsActive )
					return;

				if( !eff.Initialized ) {
					bool bFound = false;
					float3 hitPos = float3.zero;
					// ヒットした隕石探す.
					
					Entities.ForEach( ( ref BallInfo ball ) => {
						//if( !ball.Initialized )
						//	return;
						hitPos = ball.HitPos;
						bFound = true;
					} );

					if( bFound ) {
						trans.Value = hitPos;
					}
					scl.Value.x = 1f;

					eff.Timer = 0;
					eff.Initialized = true;

					seq.paused = false;
					seq.time = 0;

					Debug.LogAlways("eff init");
				}
			} );
		}
	}
}
