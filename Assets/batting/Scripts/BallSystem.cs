using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

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
					var prePos = trans.Value;
					var pos = prePos;
					//float spd = -400f * dt;
					float spd = -600f * dt;
					pos.y += spd;

					checkColli( prePos, pos );


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


		bool checkColli( float3 stPos, float3 edPos )
		{
			bool isSwing = false;
			float3 batPos = float3.zero;
			float zrad = 0;
			float pre_zrad = 0;
			quaternion batRot = quaternion.identity;
			Entities.ForEach( ( Entity entity, ref BatInfo bat, ref Translation trans, ref Rotation rot ) => {
				if( bat.Status == BatSystem.StSwing ) {
					isSwing = true;
					batPos = trans.Value;
					batRot = rot.Value;
					zrad = bat.Zrad;
					pre_zrad = bat.PreZrad;
				}
			} );

			if( !isSwing )
				return false;

			float length = 120f;
			float3 zv = new float3( 0, 0, -1f );

			float x1 = math.cos( zrad );
			float y1 = math.sin( zrad );
			float3 e1 = new float3( x1, y1, 0 );

			float3 point1 = batPos + e1 * length;

			float3 norm1 = crossVec( e1, zv );

			float3 dv1 = stPos - batPos;
			float dot1 = math.dot( dv1, norm1 );

			float3 dv2 = edPos - batPos;
			float dot2 = math.dot( dv2, norm1 );

			if( dot1 < 0 && dot2 < 0 ) {
				Debug.LogAlways("temae");
			}



			float3 p;
			bool res = isIntersect( stPos, edPos, batPos, point1, out p );
			if( res ) {
				Debug.LogFormatAlways( "0 {0} {1}", p.x, p.y );
				return true;
			}
			/*
			x = math.cos( pre_zrad );
			y = math.sin( pre_zrad );

			dv = new float3( x * length, y * length, 0 );
			point = batPos + dv;


			res = isIntersect( stPos, edPos, batPos, point1, out p );
			if( res ) {
				Debug.LogFormatAlways( "1 {0} {1}", p.x, p.y );
				return true;
			}
			*/

			return false;
		}

		bool isIntersect( float3 st1, float3 ed1, float3 st2, float3 ed2, out float3 p )
		{
			p = float3.zero;

			float2 v1 = new float2( st1.x - st2.x, st1.y - st2.y );
			float2 vA = new float2( ed1.x - st1.x, ed1.y - st1.y );
			float2 vB = new float2( ed2.x - st2.x, ed2.y - st2.y );

			// 外積.
			float cross = vA.x * vB.y - vA.y * vB.x;

			if( math.abs( cross ) < 0.0001f ) {
				return false;
			}

			float t = ( v1.y * vB.x - v1.x * vB.y ) / cross;
			float s = ( v1.y * vA.x - v1.x * vA.y ) / cross;

			if( t < 0 || t > 1f || s < 0 || s > 1f ) {
				return false;
			}

			p = new float3( vA.x * t + st1.x, vA.y * t + st1.y, 0 );
			return true;
		}

		float3 crossVec( float3 a, float3 b )
		{
			float x = a.y * b.z - a.z * b.y;
			float y = a.z * b.x - a.x * b.z;
			float z = a.x * b.y - a.y * b.x;
			return new float3( x, y, z );
		}
	}
}
