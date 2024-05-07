using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Core.Editor.Tools
{
    public class CustomBuildPipeline //: IPreprocessBuildWithReport
    {
        public const char versionSeparationSymbol = '.';
        public const string versionSeparationSymbolString = ".";

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            UpBundleVersion();
        }

        private void UpBundleVersion()
        {
            string lastBuildVersion = PlayerSettings.bundleVersion;
            string[] buildVersionSplit = lastBuildVersion.Split(versionSeparationSymbol);
            buildVersionSplit[buildVersionSplit.Length - 1] = (int.Parse(buildVersionSplit.Last()) + 1).ToString();
            PlayerSettings.bundleVersion = string.Join(versionSeparationSymbolString, buildVersionSplit);

            Debug.Log($"Build version was updated!\n" +
                      $"Current build version: \"{PlayerSettings.bundleVersion}:\";\n" +
                      $"Previous build version:  \"{lastBuildVersion}\".");
        }
    }
}