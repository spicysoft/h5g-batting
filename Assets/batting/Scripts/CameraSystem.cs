using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using Unity.Tiny.Debugging;

namespace Batting
{
	public class CameraSystem : ComponentSystem
	{
		EntityQuery TitleCameraQuery;
		EntityQuery GameCameraQuery;

		protected override void OnCreate()
		{
			EntityQueryDesc titleCameraDesc = new EntityQueryDesc()
			{
				All = new ComponentType[] { typeof( Camera2D ), typeof( CameraTitleTag ), typeof( CameraInfo ) }
			};
			TitleCameraQuery = GetEntityQuery( titleCameraDesc );


			EntityQueryDesc gameCameraDesc = new EntityQueryDesc()
			{
				All = new ComponentType[] { typeof( Camera2D ), typeof( CameraInfo ) },
				None = new ComponentType[] { typeof( CameraTitleTag ) }
			};
			GameCameraQuery = GetEntityQuery( gameCameraDesc );
		}

		protected override void OnUpdate()
		{
			Entities.With( GameCameraQuery ).ForEach( ( ref CameraInfo info, ref Camera2D camera ) => {
				if( !info.Initialized ) {

					// ディスプレイ情報.
					var displayInfo = World.TinyEnvironment().GetConfigData<DisplayInfo>();
					float frameW = displayInfo.frameWidth;
					float frameH = (float)displayInfo.frameHeight;
					float frameAsp = frameH / frameW;

					// カメラ情報.
					float rectW = camera.rect.width;
					float rectH = camera.rect.height;
					float rectAsp = rectH / rectW;

					camera.halfVerticalSize = 1050f * frameAsp / rectAsp;

					Debug.LogFormat( "----	game halfvert {0}", camera.halfVerticalSize );

					info.Initialized = true;
					return;
				}
			} );

			Entities.With( TitleCameraQuery ).ForEach( ( ref CameraInfo info, ref Camera2D camera ) => {
				if( !info.Initialized ) {

					// ディスプレイ情報.
					var displayInfo = World.TinyEnvironment().GetConfigData<DisplayInfo>();
					float frameW = displayInfo.frameWidth;
					float frameH = (float)displayInfo.frameHeight;
					float frameAsp = frameH / frameW;

					// カメラ情報.
					float rectW = camera.rect.width;
					float rectH = camera.rect.height;
					float rectAsp = rectH / rectW;

					camera.halfVerticalSize = (0.5f * rectH) * frameAsp / rectAsp;

					Debug.LogFormat( "----	title halfvert {0}", camera.halfVerticalSize );

					info.Initialized = true;
					return;
				}
			} );

		}
	}
}
