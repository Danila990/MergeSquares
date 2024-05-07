using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public interface ICollectable<T> where T : ScriptableObject
    {
        void ResetData();
        void SetData(List<T> data);
        List<string> GetRootFolders();
    }
}