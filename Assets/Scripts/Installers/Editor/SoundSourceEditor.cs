using Core.Audio;
using Core.Repositories.Editor;
using UnityEditor;

namespace Installers.Editor
{
    [CustomEditor(typeof(ScriptableInstaller))]
    public class SoundSourceEditor : ScriptableObjectCollector<SoundSource>
    {
    }
}