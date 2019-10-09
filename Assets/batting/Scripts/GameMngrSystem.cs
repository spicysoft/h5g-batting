using Unity.Entities;
using Unity.Tiny.Debugging;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;

namespace Batting
{
	public class GameMngrSystem : ComponentSystem
	{

		protected override void OnUpdate()
		{
			bool isUpdatedScore = false;
			int score = 0;

			Entities.ForEach( ( ref GameMngr mngr ) => {
				if( !mngr.Initialized ) {
					mngr.Initialized = true;
					mngr.IsUpdatedScore = false;
					mngr.Score = 0;
					isUpdatedScore = true;
					return;
				}
				if( mngr.IsUpdatedScore ) {
					isUpdatedScore = true;
					mngr.IsUpdatedScore = false;
					// ハイスコア更新.
					if( mngr.Score > mngr.HiScore ) {
						mngr.HiScore = mngr.Score;
					}
					score = mngr.Score;
				}
			} );


			// スコア.
			if( isUpdatedScore ) {
				// スコア表示.
				Entities.WithAll<TextScoreTag>().ForEach( ( Entity entity ) => {
					EntityManager.SetBufferFromString<TextString>( entity, score.ToString() );
				} );
			}

		}

	}
}
