using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace Batting
{
	[UpdateAfter( typeof( TargetGenSystem ) )]
	public class InitTargetSystem : ComponentSystem
	{
		private const int RowMax = 6;
		private const float Width = 200f;
		private const float Height = 240f;

		protected override void OnUpdate()
		{
			int genCnt = 0;
			Entities.ForEach( ( ref TargetGenInfo gen ) => {
				if( !gen.Initialized ) {
					return;
				}
				genCnt = gen.GeneratedCnt;
			} );

			float3 org = new float3(0, 450f, 0);

			Entities.ForEach( ( Entity entity, ref TargetInfo tar, ref Translation trans, ref NonUniformScale scl, ref Sprite2DRendererOptions opt ) => {
				if( !tar.IsActive )
					return;
				if( !tar.Initialized ) {
					tar.Initialized = true;
					scl.Value.x = 1f;
					tar.Status = TargetSystem.StNorm;
					tar.Timer = 0;
					tar.Level = 2;
					tar.Radius = TargetSystem.Radius1;
					opt.size.x = tar.Radius * 2f;
					opt.size.y = tar.Radius * 2f;

					int row = RowMax;
					int ix = genCnt % row;
					int iy = genCnt / row;
					float x = -Width * ( row - 1 ) * 0.5f + Width * ix;
					float y = Height * ( genCnt / row );
					// Y方向にゆがませる.
					float ofsy = (float)ix - (float)( row - 1 ) * 0.5f;
					float bias = -30f / (float)(iy+1);
					ofsy = ofsy * ofsy * bias;

					float3 pos = org;
					pos.x = x;
					pos.y += y + ofsy;
					trans.Value = pos;

					//Debug.LogFormatAlways("x {0} {1} {2}", genCnt, pos.x, pos.y);

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
