using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace Batting
{
	[UpdateAfter( typeof( TargetGenSystem ) )]
	public class InitTargetSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			int genCnt = 0;
			Entities.ForEach( ( ref TargetGenInfo gen ) => {
				if( !gen.Initialized ) {
					return;
				}
				genCnt = gen.GeneratedCnt;
			} );

			float3 org = new float3(0, 400f, 0);

			Entities.ForEach( ( Entity entity, ref TargetInfo tar, ref Translation trans, ref NonUniformScale scl, ref Sprite2DRendererOptions opt ) => {
				if( !tar.IsActive )
					return;
				if( !tar.Initialized ) {
					tar.Initialized = true;
					scl.Value.x = 1f;
					tar.Status = TargetSystem.StNorm;
					tar.Timer = 0;
					tar.Level = 2;
					tar.Radius = 90f;
					opt.size.x = tar.Radius * 2f;
					opt.size.y = tar.Radius * 2f;

					int colm = 6;
					float ix = genCnt % colm;
					float x = -200f*(colm-1)*0.5f + 200f * ix;
					float y = 240f * ( genCnt / colm );
					float ofsy = (float)ix - (float)(colm-1) * 0.5f;
					ofsy = ofsy*ofsy * ( -20f );
					float3 pos = org;
					pos.x = x;
					pos.y += y + ofsy;
					trans.Value = pos;

					Debug.LogFormatAlways("x {0} {1} {2}", genCnt, pos.x, pos.y);

					++genCnt;
					return;
				}

			} );

			Entities.ForEach( ( ref TargetGenInfo gen ) => {
				if( !gen.Initialized ) {
					return;
				}
				gen.GeneratedCnt = genCnt;
			} );

		}
	}
}
