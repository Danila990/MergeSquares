using UnityEditor;
using UnityEditor.SceneManagement;

namespace Core.Editor {
	[InitializeOnLoad]
	public class AutoPlayModeSceneSetup {
		static AutoPlayModeSceneSetup() {
			// Ensure at least one build scene exist.
			if ( EditorBuildSettings.scenes.Length == 0 ) {
				return;
			}

			// Set Play Mode scene to first scene defined in build settings.
			EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>( EditorBuildSettings.scenes[ 0 ].path );
		}
	}
}