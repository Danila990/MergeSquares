using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public class SpriteHelper
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Slice unit sprites")]
#endif
        public static void SliceSprites()
        {
#if UNITY_EDITOR
            var folderPath = "Assets/Game/Units/Sprites/Units";
            var paths = Directory.GetFiles($"{folderPath}/", "*.png").ToList();

            for (int a = 0; a < paths.Count; a++)
            {
                var path = paths[a];
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                var spriteSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                ti.isReadable = true;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spriteImportMode = SpriteImportMode.Multiple;
                List<SpriteMetaData> newData = new List<SpriteMetaData>();
                // Texture2D spriteSheet = paths[a] as Texture2D;
                GetImageSize(spriteSheet, out var width, out var height);

                int rowsCount = 1;
                int columnCount = width / height;
                int frameHeight = height / rowsCount;
                var frameWidth = frameHeight;
                int[] posX = new int[columnCount];
                int[] posY = new int[rowsCount];

                int shiftX = 0;
                for (int b = 0; b < columnCount; b++)
                {
                    posX[b] = shiftX;
                    shiftX = shiftX + frameWidth;
                }

                int shiftY = height - frameHeight;
                for (int b = 0; b < rowsCount; b++)
                {
                    posY[b] = shiftY;
                    shiftY = shiftY - frameHeight;
                }

                int spriteNumber = 0;
                for (int rowNum = 0; rowNum < rowsCount; rowNum++)
                {
                    for (int columnNum = 0; columnNum < columnCount; columnNum++)
                    {
                        SpriteMetaData smd = new SpriteMetaData();

                        smd.pivot = new Vector2(0f, 0f);
                        // smd.alignment = 9;
                        smd.name = spriteSheet.name + "_" + spriteNumber.ToString();
                        smd.rect = new Rect(posX[columnNum], posY[rowNum], frameWidth, frameHeight);
                        newData.Add(smd);

                        spriteNumber++;
                    }
                }

                ti.spritesheet = newData.ToArray();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            Debug.Log("Done Slicing!");
#endif
        }

        public static bool GetImageSize(Texture2D asset, out int width, out int height)
        {
#if UNITY_EDITOR
            if (asset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null)
                {
                    object[] args = new object[2] {0, 0};
                    MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    mi.Invoke(importer, args);

                    width = (int) args[0];
                    height = (int) args[1];

                    return true;
                }
            }
#endif
            height = width = 0;
            return false;
        }
    }
}