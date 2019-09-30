using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;
using Unity.Tiny.UIControls;

namespace Batting
{
	public class BtnBattingSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool btnOn = false;
			Entities.WithAll<BtnBattingTag>().ForEach( ( Entity entity, ref PointerInteraction pointerInteraction ) => {
				if( pointerInteraction.clicked ) {
					Debug.LogAlways( "btn bat click" );
					btnOn = true;
				}
			} );


			if( btnOn ) {
				Entities.ForEach( ( Entity entity, ref BatInfo bat ) => {
					if( bat.Status == BatSystem.StWait ) {
						bat.Status = BatSystem.StSwing;
					}
				} );
			}
		}
	}
}
