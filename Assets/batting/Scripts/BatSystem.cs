using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Input;

#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace Batting
{
	public class BatSystem : ComponentSystem
	{
		public const int StWait = 0;
		public const int StSwing = 1;
		public const int StEnd = 2;

		protected override void OnUpdate()
		{
			var inputSystem = World.GetExistingSystem<InputSystem>();


			Entities.ForEach( ( Entity entity, ref BatInfo bat, ref Rotation rotation ) => {
				if( !bat.Initialized ) {
					bat.Initialized = true;
					bat.Status = StWait;
					bat.Zrad = math.radians( 210f );
					bat.Timer = 0;
					rotation.Value = quaternion.RotateZ( bat.Zrad );
					return;
				}


				float dt = World.TinyEnvironment().frameDeltaTime;

				switch( bat.Status ) {
				case StWait:
					if( inputSystem.GetMouseButtonDown(0) ) {
						bat.Status = BatSystem.StSwing;
					}
					break;
				case StSwing:
					quaternion rot = rotation.Value;
					float za = bat.Zrad;
					bat.PreZrad = za;
					float aspd = math.radians( 1200f ) * dt;
					za += aspd;
					bat.Zrad = za;
					rotation.Value = quaternion.RotateZ( za );
					if( za > math.radians(520f) ) {
						bat.Status = StEnd;
					}
					break;
				case StEnd:
					bat.Timer += dt;
					if( bat.Timer > 1f ) {
						bat.Initialized = false;
					}
					break;
				}
			} );
		}
	}
}
