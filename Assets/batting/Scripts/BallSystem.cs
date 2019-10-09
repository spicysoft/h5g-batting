using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;

namespace Batting
{
	public class BallSystem : ComponentSystem
	{
		public const int StPrepare = 0;
		public const int StThrow = 1;
		public const int StShot = 2;
		public const int StEnd = 3;
		public const int StPause = 4;
		Random _random;

		protected override void OnCreate()
		{
			_random = new Random();
		}

		protected override void OnUpdate()
		{
			bool reqResult = false;
			int ballCnt = -1;
			bool reqHitEff = false;

			Entities.ForEach( ( Entity entity, ref BallInfo ball, ref Translation trans, ref NonUniformScale scl ) => {
				if( !ball.SeedInitialized ) {
					// 乱数シードセット 1回だけ.
					ball.SeedInitialized = true;
					int seed = World.TinyEnvironment().frameNum;
					Debug.LogFormatAlways( "seed {0}", seed );
					_random.InitState( (uint)seed );
				}

				if( !ball.Initialized ) {
					ball.Initialized = true;
					ball.Status = StPrepare;
					ball.Timer = 0;
					scl.Value = new float3( 1f, 1f, 1f );
					ball.Speed = _random.NextFloat( 300f, 900f );

					// ボール軌道.
					float3 stPos = new float3( _random.NextFloat( -20f, 40f ), 100f, 0 );
					float3 edPos = new float3( _random.NextFloat( -20f, 40f ), -300f, 0 );
					//float3 stPos = new float3( 0, 100f, 0 );
					//float3 edPos = new float3( 0, -300f, 0 );
					float3 dvec = edPos - stPos;
					trans.Value = stPos;
					ball.Dir = math.normalize( dvec );

					++ball.Count;
					return;
				}

				float dt = World.TinyEnvironment().frameDeltaTime;

				switch( ball.Status ) {
				case StPrepare:
					ball.Timer += dt;
					if( ball.Timer > 1.0f ) {
						ball.Status = StThrow;
						ball.Timer = 0;
					}
					break;
				case StThrow:
					var prePos = trans.Value;
					var pos = prePos;

					float3 vel = dt*ball.Speed * ball.Dir;
					pos += vel;

					float3 p;
					float3 n;
					float refRate;
					// バットとの当たりチェック.
					if( checkColliBat( prePos, pos, out p, out n, out refRate ) ) {
						// hit.
						ball.Status = StShot;
						ball.Timer = 0;
						ball.Dir = calcReflectVec( ball.Dir, n );
						trans.Value = p;
						ball.HitPos = p;
						Debug.LogFormatAlways("hitpos {0} {1}", ball.HitPos.x, ball.HitPos.y);
						ball.Speed *= refRate;
						// hit effect.
						reqHitEff = true;
						break;
					}

					trans.Value = pos;

					if( pos.y < -500f ) {
						ball.Status = StEnd;
						ball.Timer = 0;
						scl.Value.x = 0;
					}
					break;
				case StShot:
					var prePos2 = trans.Value;
					var pos2 = prePos2;

					float3 vel2 = dt * ball.Speed * ball.Dir;
					pos2 += vel2;

					float3 refv;
					// ターゲットとの当たりチェック.
					if( checkTarget( prePos2, pos2, ball.Dir, out refv ) ) {
						//ball.Speed *= 0.9f;
						ball.Dir = refv;    // 跳ね返り.
						break;
					}

					trans.Value = pos2;

					ball.Timer += dt;
					if( ball.Timer > 2.5f ) {
						ball.Status = StEnd;
						scl.Value.x = 0;
						ball.Timer = 0;
					}
					break;
				case StEnd:
					ball.Timer += dt;
					if( ball.Timer > 1f ) {
						if( ball.Count >= 10 ) {
							// todo リザルト.
							//ball.Initialized = false;
							// 仮.
							ball.Count = 0;
							ball.Status = StPause;
							reqResult = true;
						}
						else {
							ball.Initialized = false;
						}
						ballCnt = ball.Count + 1;
					}
					break;
				}

			} );

			if( ballCnt != -1 ) {
				dispBallCount( ballCnt );
			}

			if( reqHitEff ) {
				// ヒットエフェクト.
				Entities.ForEach( ( Entity entity, ref HitEffInfo info ) => {
					info.IsActive = true;
					Debug.LogAlways( "eff req" );
				} );
			}

			if( reqResult ) {
				// リザルト表示.
				SceneReference resultScn = World.TinyEnvironment().GetConfigData<GameConfig>().ResultScn;
				SceneService.LoadSceneAsync( resultScn );
			}
		}

		// ボールカウント表示.
		void dispBallCount( int cnt )
		{
			Entities.WithAll<TextBallCntTag>().ForEach( ( Entity entity ) => {
				string str = cnt.ToString() + "/10";
				EntityManager.SetBufferFromString<TextString>( entity, str );
			} );
		}

		// 反射ベクトル.
		float3 calcReflectVec( float3 inVec, float3 norm )
		{
			float dot = math.dot( inVec, norm );
			float3 vec = inVec - 2f * dot * norm;
			return math.normalize( vec );
		}

		bool checkColliBat( float3 stPos, float3 edPos, out float3 intersectPos, out float3 normVec, out float refRate )
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

			intersectPos = stPos;
			normVec = float3.zero;
			refRate = 1.5f;

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

			// クリティカルヒット判定.
			if( zrad > math.radians( 330f ) && zrad < math.radians( 390f ) ) {
				refRate = 1.2f;
			}

			float3 zv = new float3( 0, 0, 1f );

			float x1 = math.cos( zrad );
			float y1 = math.sin( zrad );
			float3 e1 = new float3( x1, y1, 0 );
			// バットの先.
			float3 point1 = batPos + e1 * length;

			float x2 = math.cos( pre_zrad );
			float y2 = math.sin( pre_zrad );
			float3 e2 = new float3( x2, y2, 0 );
			// バットの先.
			float3 point2 = batPos + e2 * length;

			// 法線.
			float3 norm1 = crossVec( e1, zv );
			// それぞれ内積とる.
			float dotS1 = math.dot( dvS, norm1 );
			float dotE1 = math.dot( dvE, norm1 );
			// 範囲比較.
			if( dotS1 >= 0 && dotE1 >= 0 ) {
				// 法線.
				float3 norm2 = crossVec( e2, zv );
				// それぞれ内積とる.
				float dotS2 = math.dot( dvS, norm2 );
				float dotE2 = math.dot( dvE, norm2 );
				// 範囲比較.
				if( dotS2 <= 0 && dotE2 <= 0 ) {
					//Debug.LogAlways( "hit inside" );
					// 法線計算し直し.
					float3 normS = crossVec( dvS, zv );
					normVec = math.normalize( normS );
					refRate *= 1.5f;
					return true;
				}
			}

			// バット手前.
			bool res = isIntersect( stPos, edPos, batPos, point1, out intersectPos );
			if( res ) {
				//Debug.LogFormatAlways( "hit1 {0} {1}", intersectPos.x, intersectPos.y );
				normVec = norm1;
				refRate *= 2f;
				return true;
			}

			// バット後方.
			bool res2 = isIntersect( stPos, edPos, batPos, point2, out intersectPos );
			if( res2 ) {
				//Debug.LogFormatAlways( "hit2 {0} {1}", intersectPos.x, intersectPos.y );
				normVec = norm1;
				refRate *= 1.5f;
				return true;
			}

			return false;
		}


		// 参考: https://qiita.com/Nunocky/items/55db409d90ebe0aac280
		bool isIntersect( float3 st1, float3 ed1, float3 st2, float3 ed2, out float3 p )
		{
			p = st1;

			float2 v1 = new float2( st1.x - st2.x, st1.y - st2.y );
			float2 vA = new float2( ed1.x - st1.x, ed1.y - st1.y );
			float2 vB = new float2( ed2.x - st2.x, ed2.y - st2.y );

			// 外積.
			float cross = vA.x * vB.y - vA.y * vB.x;

			// 外積=0(平行)なら交差しない.
			if( math.abs( cross ) < 0.00001f ) {
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
			return math.cross( a, b );
			/*
			float x = a.y * b.z - a.z * b.y;
			float y = a.z * b.x - a.x * b.z;
			float z = a.x * b.y - a.y * b.x;
			return new float3( x, y, z );*/
		}

		// 外積.
		float cross2d( float2 v1, float2 v2 )
		{
			return v1.x * v2.y - v1.y * v2.x;
		}

		// 内積.
		float inner2d( float2 v1, float2 v2 )
		{
			return v1.x * v2.x + v1.y * v2.y;
		}

		// 的との当たりチェック.
		bool checkTarget( float3 vSt, float3 vEd, float3 dir, out float3 refvec )
		{
			bool isHit = false;
			float3 norm = float3.zero;
			Entities.ForEach( ( Entity entity, ref TargetInfo tar, ref Translation trans ) => {
				//if( isHit )
				//	return;
				if( !tar.IsActive )
					return;
				if( tar.Status != TargetSystem.StNorm )
					return;

				var center = trans.Value;
				var r = tar.Radius + 10f;       // ボールの半径足す.
				float3 vnorm;
				if( checkColliTarget( vSt, vEd, center, r, out vnorm ) ) {
					//Debug.LogAlways( "Tar Hit" );
					tar.Status = TargetSystem.StHit;
					isHit = true;
					norm = vnorm;
				}
			} );

			if( isHit ) {
				refvec = calcReflectVec( dir, norm );
				//Debug.LogFormatAlways( "norm2 {0} {1}", norm.x, norm.y );
				//Debug.LogFormatAlways( "ref {0} {1}", refvec.x, refvec.y );
			}
			else {
				refvec = float3.zero;
			}

			return isHit;
		}

		// 的との当たり判定.
		bool checkColliTarget( float3 vSt, float3 vEd, float3 center, float r, out float3 norm )
		{
			// A:線分の始点、B:線分の終点、P:円の中心、X:PからABに下ろした垂線との交点.
			float2 vAB = new float2( vEd.x - vSt.x, vEd.y - vSt.y );
			float2 vAP = new float2( center.x - vSt.x, center.y - vSt.y );
			float2 vBP = new float2( center.x - vEd.x, center.y - vEd.y );

			float2 vABnorm = math.normalize( vAB );
			// AXの距離.
			float lenAX = inner2d( vAP, vABnorm );

			// 線分ABとPの最短距離
			float shortestDistance;
			int pattern = 0;
			if( lenAX < 0 ) {
				// AXが負なら APが円の中心までの最短距離
				shortestDistance = math.length( vAP );
				pattern = 1;
			}
			else if( lenAX > math.length( vAB ) ) {
				// AXがABよりも長い場合は、BPが円の中心までの最短距離
				shortestDistance = math.length( vBP );
				pattern = 2;
			}
			else {
				// XがAB上にあるので、AXが最短距離
				// 単位ベクトルABとベクトルAPの外積で求める
				shortestDistance = math.abs( cross2d( vAP, vABnorm ) );
				pattern = 3;
			}
			
			if( shortestDistance > r ) {
				// はずれ.
				norm = float3.zero;
				return false;
			}

			// Xの座標.
			float2 pX = vABnorm * lenAX + new float2(vSt.x, vSt.y);

			float2 pP = new float2( center.x, center.y );
			float2 vPX = pP - pX;
			float lenPXsp = math.lengthsq( vPX );

			// 交点求める.
			// Xから交点までの距離.
			float dist = math.sqrt( r * r - lenPXsp );
			float2 pQ = float2.zero;
			if( pattern == 1 ) {
				pQ = vABnorm * dist + pX;
			}
			else if( pattern == 2 ) {
				pQ = -vABnorm * dist + pX;
			}
			else if( pattern == 3 ) {
				pQ = -vABnorm * dist + pX;
			}

			// 法線計算.
			float2 vPQ = pQ - pP;
			float2 vPQnorm = math.normalize( vPQ );
			norm = new float3( vPQnorm.x, vPQnorm.y, 0 );

			return true;
		}

	}
}
