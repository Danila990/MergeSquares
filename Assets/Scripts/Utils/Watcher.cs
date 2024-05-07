using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class Watcher
    {
        private readonly string _name;
        private readonly bool _needWatch;
        private Dictionary<string, System.Diagnostics.Stopwatch> _watchData = new();

        public Watcher(string name, bool watch)
        {
            _name = name;
            _needWatch = watch;
        }
        
        public void StartWatch(string key)
        {
            if(_needWatch && !_watchData.ContainsKey(key))
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                _watchData.Add(key, watch);
                Debug.Log($"[{_name}][StartWatch] key: {key}");
            }
        }
        
        public void StopWatch(string key)
        {
            if(_needWatch && _watchData.TryGetValue(key, out var watch))
            {
                watch.Stop();
                Debug.Log($"[{_name}][StopWatch] key: {key} time: {watch.ElapsedMilliseconds} ms");
            }
        }
    }
}