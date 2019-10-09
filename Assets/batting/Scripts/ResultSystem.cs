using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Text;

namespace Batting
{
	public class ResultSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			// スコア表示.
			int score = 0;
			int hiScore = 0;
			Entities.ForEach( ( ref GameMngr mngr ) => {
				score = mngr.Score;
				hiScore = mngr.HiScore;
			} );

			Entities.ForEach( ( ref ResultComponent result ) => {
				if( !result.IsInitialized ) {
					result.IsInitialized = true;

					Entities.WithAll<TextResultScoreTag>().ForEach( ( Entity entity ) =>
					{
						EntityManager.SetBufferFromString<TextString>( entity, score.ToString() );
					} );

					Entities.WithAll<TextResultHiScoreTag>().ForEach( ( Entity entity ) => {
						EntityManager.SetBufferFromString<TextString>( entity, hiScore.ToString() );
					} );
				}
			} );
		}
	}
}
