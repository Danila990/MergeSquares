using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameScripts.MergeSquares.Shop;
using LargeNumbers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace Core.Repositories
{
    [Serializable]
    public class Image
    {
        public string id;
        public Sprite sprite;
    }

    [Serializable]
    public class Image<TId>
    {
        public TId id;
        public Sprite sprite;
    }

    [Serializable]
    public class PrefabResource<T> where T : MonoBehaviour
    {
        public T data;
        public string id;
    }
    
    [Serializable]
    public class SquareImage
    {
        public int value;
        public Sprite numberSprite;

        public Sprite baseSprite;
        public Sprite bubbleSprite;
        public Sprite candySprite;
        public Sprite glassSprite;

        public Sprite colorFrame;
        public Sprite goldFrame;
        public Sprite silverFrame;
        public Sprite leavesFrame;
        public Sprite woodFrame;
        public Sprite external;
        public Sprite externalFrame;
        public Sprite skySprite;
        public Sprite softSprite;
        public Sprite woodSprite;
    }
    
    [Serializable]
    public class Square2248Image
    {
        public LargeNumber value;
        public int pow;
        public Color color;
        public Sprite baseSprite;
        public Sprite bubbleSprite;
        public Sprite candySprite;
        public Sprite skySprite;
        public Sprite woodSprite;
        public Sprite glassSprite;
        public Sprite normalSprite;
        public Sprite shineSprite;
        public Sprite lightFrame;
        public Sprite external;
        public Sprite externalFrame;
    }
    
    [CreateAssetMenu(fileName = "ResourceRepository", menuName = "Repositories/ResourceRepository")]
    public class ResourceRepository : ScriptableObject
    {
        [SerializeField] private List<Image> images;
        [SerializeField] private List<SquareImage> squareImages;
        [SerializeField] private List<Square2248Image> square2248Images;
        [SerializeField] private string imageUpdateName;

        public Sprite GetImageById(string id)
        {
            return images.GetBy(value => value.id == id).sprite;
        }
        
        public SquareImage GetSquareImageByValue(int value)
        {
            return squareImages.GetBy(image => image.value == value);
        }
        
        public Square2248Image GetSquare2248ImageByValue(LargeNumber value)
        {
            var image = square2248Images.GetBy(image => image.value == value);
            if (image == null)
            {
                image = square2248Images.GetBy(image => image.value == LargeNumber.zero);
                image.value = value;
            }

            return image;
        }
        
        public Square2248Image GetSquare2248ImageByPow(int pow)
        {
            if (pow > square2248Images.Count)
            {
                pow = pow % square2248Images.Count;
            }
            var image = square2248Images.GetBy(image => image.pow == pow);
            return image;
        }

        public void AddSquareImage(int value, Sprite sprite, bool frame)
        {
            var img = GetSquareImageByValue(value);
            if (img != null)
            {
                if (frame)
                {
                    img.externalFrame = sprite;
                }
                else
                {
                    img.external = sprite;
                }
            }
        }
        
        public void AddSquareImage2248(int value, Sprite sprite, bool frame)
        {
            var img = GetSquare2248ImageByValue(new LargeNumber(value));
            if (img != null)
            {
                if (frame)
                {
                    img.externalFrame = sprite;
                }
                else
                {
                    img.external = sprite;
                }
            }
        }
        
        public void UpdateImages()
        {
#if UNITY_EDITOR
            UpdateImages("Assets/Game/MergeSquares/Sprites/Units");
            Debug.Log($"[{nameof(ResourceRepository)}] Update Success");
#endif
        }
        
        public void DeleteImages()
        {
#if UNITY_EDITOR
            DeleteImages("Assets/Game/MergeSquares/Sprites/Units");
            Debug.Log($"[{nameof(ResourceRepository)}] Delete Success");
#endif
        }

        public void DownloadImages()
        {
#if UNITY_EDITOR
            // DownloadToList(new[] {"Assets/Game/Sprites"}, images);
            DownloadToList(new[] {"Assets/Game/MergeSquares/Sprites/Units"}, squareImages);
            DownloadToList(new[] {"Assets/Game/MergeSquares/Sprites/Units"}, square2248Images);
            Debug.Log($"[{nameof(ResourceRepository)}] Update Success");
#endif
        }

        private void DownloadToList<T>(string[] folders, List<PrefabResource<T>> destination, bool clear = true)
            where T : MonoBehaviour
        {
#if UNITY_EDITOR
            if (clear)
                destination.Clear();

            var objects = new List<Object>();
            var guids = AssetDatabase.FindAssets("t:GameObject", folders);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                objects.Add(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
            }

            foreach (var obj in objects)
            {
                destination.Add(new PrefabResource<T> {id = obj.name, data = ((GameObject) obj).GetComponent<T>()});
            }
#endif
        }

        private void DownloadToList(string[] folders, List<Image> destination)
        {
#if UNITY_EDITOR
            destination.Clear();
            var sprites = new List<Sprite>();
            foreach (var folder in folders)
            {
                var spriteSheetPaths = Directory.GetFiles($"{folder}/", "*");
                foreach (var path in spriteSheetPaths)
                    sprites.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList());
            }

            foreach (var sprite in sprites)
            {
                destination.Add(new Image {id = sprite.name, sprite = sprite});
            }
#endif
        }
        
        private void DownloadToList(string[] folders, List<SquareImage> destination)
        {
#if UNITY_EDITOR
            destination.Clear();
            var sprites = new List<Sprite>();
            
            foreach (var folder in folders)
            {
                var spritePaths = Directory.GetFiles($"{folder}/", "*");
                sprites.Clear();
                foreach (var path in spritePaths)
                    sprites.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList());
                var colorFrameSprite = sprites.Find(s => s.name == "ColorFrame");
                var leavesFrameSprite = sprites.Find(s => s.name == "LeavesFrame");
                var woodFrameSprite = sprites.Find(s => s.name == "WoodFrame");
                var silverFrameSprite = sprites.Find(s => s.name == "SilverFrame");
                var goldFrameSprite = sprites.Find(s => s.name == "GoldFrame");
                foreach (var valueFolder in Directory.GetDirectories($"{folder}/", "*"))
                {
                    var spriteSheetPaths = Directory.GetFiles($"{valueFolder}/", "*");
                    sprites.Clear();
                    foreach (var path in spriteSheetPaths)
                        sprites.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList());

                    var image = new SquareImage {value = Convert.ToInt32(valueFolder.Substring(valueFolder.LastIndexOf('/') + 1))};
                    if (colorFrameSprite != null)
                    {
                        image.colorFrame = colorFrameSprite;
                    }
                    if (leavesFrameSprite != null)
                    {
                        image.leavesFrame = leavesFrameSprite;
                    }
                    if (woodFrameSprite != null)
                    {
                        image.woodFrame = woodFrameSprite;
                    }
                    if (goldFrameSprite != null)
                    {
                        image.goldFrame = goldFrameSprite;
                    }
                    if (silverFrameSprite != null)
                    {
                        image.silverFrame = silverFrameSprite;
                    }
                    foreach (var sprite in sprites)
                    {
                        switch (sprite.name)
                        {
                            case "Number":
                                image.numberSprite = sprite;
                                break;
                            case "Base":
                                image.baseSprite = sprite;
                                break;
                            case "Bubble":
                                image.bubbleSprite = sprite;
                                break;
                            case "Candy":
                                image.candySprite = sprite;
                                break;
                            case "Glass":
                                image.glassSprite = sprite;
                                break;
                            case "Sky":
                                image.skySprite = sprite;
                                break;
                            case "Soft":
                                image.softSprite = sprite;
                                break;
                            case "Wood":
                                image.woodSprite = sprite;
                                break;
                        }
                    }
                    destination.Add(image);
                }
            }
#endif
        }
        
        private void DownloadToList(string[] folders, List<Square2248Image> destination)
        {
#if UNITY_EDITOR
            destination.Clear();
            var sprites = new List<Sprite>();
            
            foreach (var folder in folders)
            {
                var spritePaths = Directory.GetFiles($"{folder}/", "*");
                sprites.Clear();
                foreach (var path in spritePaths)
                    sprites.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList());
                var lightSprite = sprites.Find(s => s.name == "Light");
                
                foreach (var valueFolder in Directory.GetDirectories($"{folder}/", "*"))
                {
                    var nameInt = valueFolder.Substring(valueFolder.LastIndexOf('/') + 1);
                    if (nameInt is "0" or "1")
                    {
                        continue;
                    }
                    var spriteSheetPaths = Directory.GetFiles($"{valueFolder}/", "*");
                    sprites.Clear();
                    foreach (var path in spriteSheetPaths)
                        sprites.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList());
                    
                    var value = Convert.ToInt32(nameInt);
                    var pow = (int) Mathf.Log(value, 2);
                    var image = new Square2248Image {value = new LargeNumber(value), pow = pow};
                    if (lightSprite != null)
                    {
                        image.lightFrame = lightSprite;
                    }
                    foreach (var sprite in sprites)
                    {
                        switch (sprite.name)
                        {
                            case "Base":
                                image.baseSprite = sprite;
                                break;
                            case "Bubble":
                                image.bubbleSprite = sprite;
                                break;
                            case "Candy":
                                image.candySprite = sprite;
                                break;
                            case "Sky":
                                image.skySprite = sprite;
                                break;
                            case "Wood":
                                image.woodSprite = sprite;
                                break;
                            case "Glass":
                                image.glassSprite = sprite;
                                break;
                            case "Normal":
                                image.normalSprite = sprite;
                                break;
                            case "Shine":
                                image.shineSprite = sprite;
                                break;
                            case "Line":
                                image.color = sprite.texture.GetPixel(sprite.texture.width / 2, sprite.texture.height / 2);
                                image.color.a = 1f;
                                break;
                        }
                    }
                    destination.Add(image);
                }
            }
#endif
        }
        
        private void UpdateImages(string folder)
        {
#if UNITY_EDITOR
            var values = new List<int> {0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048};
            var filePath = Application.dataPath + "/" + "StreamingAssets" + "/";
            void TryCopyFile(string pathFrom, string pathTo)
            {
                Debug.Log($"[ResourceRepository][TryCopyFile] path: {pathFrom}");
                if (File.Exists(pathFrom))
                {
                    Debug.Log($"[ResourceRepository][TryCopyFile] found file");
                    File.Copy(pathFrom, pathTo, true);
                }
            }
            Debug.Log($"[ResourceRepository][UpdateImages2248] **************** START LOAD sprites");
            foreach (var value in values)
            {
                var pathFrom = $"{filePath}{value}.png";
                var pathTo = $"{folder}/{value}/{imageUpdateName}.png";
                TryCopyFile(pathFrom, pathTo);
            }
#endif
        }
        
        private void DeleteImages(string folder)
        {
#if UNITY_EDITOR
            var values = new List<int> {0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048};
            void DeleteFile(string path)
            {
                Debug.Log($"[ResourceRepository][DeleteFile] path: {path}");
                if (File.Exists(path))
                {
                    Debug.Log($"[ResourceRepository][DeleteFile] found file");
                    File.Delete(path);
                }
            }
            Debug.Log($"[ResourceRepository][DeleteImages2248] **************** START DELETE sprites");
            foreach (var value in values)
            {
                var path = $"{folder}/{value}/{imageUpdateName}.png";
                DeleteFile(path);
            }
#endif
        }
    }
}