using Unity.Entities;

namespace Batting
{
	public class BallGenSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool isReq = false;
			Entities.ForEach( ( ref BallGenInfo gen ) => {
				if( !gen.Initialized ) {
					gen.GeneratedCnt = 0;
					return;
				}
				isReq = gen.IsRequested;
			} );


			if( isReq ) {

			}
		}
	}
}
