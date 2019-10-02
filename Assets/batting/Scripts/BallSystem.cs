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
		public const int StShot = 2;
		public const int StEnd = 3;

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
					float spd = -400f * dt;
					pos.y += spd;

					if( checkColli( prePos, pos ) ) {
						// hit.
						ball.Status = StShot;
						break;
					}

					trans.Value = pos;

					if( pos.y < -500f ) {
						ball.Status = StEnd;
						scl.Value.x = 0;
					}
					break;
				case StShot:
					var prePos2 = trans.Value;
					var pos2 = prePos2;
					//float spd = -400f * dt;
					float spd2 = 400f * dt;
					pos2.y += spd2;

					trans.Value = pos2;

					if( pos2.y > 500f ) {
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
					if( bat.Zrad > math.radians( 280f ) && bat.Zrad < math.radians( 440f ) ) {
						isSwing = true;
						batPos = trans.Value;
						batRot = rot.Value;
						zrad = bat.Zrad;
						pre_zrad = bat.PreZrad;
					}
				}
			} );

			if( !isSwing )
				return false;

			// 先ず距離でチェック.
			float3 dvS = stPos - batPos;	// ボールの現在地 - バット中心.
			float3 dvE = edPos - batPos;    // ボールの次の位置 - バット中心.
			float lenSqS = math.lengthsq( dvS );
			float lenSqE = math.lengthsq( dvE );

			float length = 120f;
			float lengthSq = length * length;

			if( lenSqS > lengthSq && lenSqE > lengthSq ) {
				// 2点とも範囲外.
				return false;
			}


			float3 zv = new float3( 0, 0, 1f );

			float x1 = math.cos( zrad );
			float y1 = math.sin( zrad );
			float3 e1 = new float3( x1, y1, 0 );

			// バットの先.
			float3 point1 = batPos + e1 * length;

			float3 norm1 = crossVec( e1, zv );

			float dotS1 = math.dot( dvS, norm1 );

			float dotE1 = math.dot( dvE, norm1 );

			if( dotS1 > 0 && dotE1 > 0 ) {
				//Debug.LogAlways( "now oku" );

				float x2 = math.cos( pre_zrad );
				float y2 = math.sin( pre_zrad );
				float3 e2 = new float3( x2, y2, 0 );

				// バットの先.
				float3 point2 = batPos + e2 * length;

				float3 norm2 = crossVec( e2, zv );

				float dotS2 = math.dot( dvS, norm2 );
				float dotE2 = math.dot( dvE, norm2 );

				if( dotS2 < 0 && dotE2 < 0 ) {
					Debug.LogAlways( "hit inside" );
				}
				return true;
			}

			float3 p;
			bool res = isIntersect( stPos, edPos, batPos, point1, out p );
			if( res ) {
				Debug.LogFormatAlways( "hit {0} {1}", p.x, p.y );
				return true;
			}

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
