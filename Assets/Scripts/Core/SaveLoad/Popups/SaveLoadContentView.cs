using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Core.SaveLoad.Popups
{
    public class SaveLoadContentView : MonoBehaviour
    {
        [SerializeField] private Transform panelsParent;
        [SerializeField] private SaveLoadPanelView panelPrefab;

        private readonly List<SaveLoadPanelView> _panels = new List<SaveLoadPanelView>();
        
        public void Init(string current, IReadOnlyList<string> keys, Action<string, bool, bool> save, Action<string> load, Action<string> remove)
        {
            foreach (var key in keys)
            {
                var view = Instantiate(panelPrefab, panelsParent);
                view.Init(key, Save, Load, Remove);
                view.Highlight(key == current);
                
                _panels.Add(view);

                void Save()
                {
                    save.Invoke(key, false, true);
                    Select();
                }

                void Load()
                {
                    load.Invoke(key);
                    Select();
                }
                
                void Remove()
                {
                    remove.Invoke(key);
                    _panels.Remove(view);
                    if(view.IsHighlight)
                    {
                        if (_panels.Count > 0)
                        {
                            _panels[0].Highlight(true);
                        }
                    }
                    Destroy(view);
                }

                void Select()
                {
                    _panels.ForEach(value => value.Highlight(false));
                    view.Highlight(true);
                }
            }
        }

        public void Clear()
        {
            _panels.ForEach(value => Destroy(value.gameObject));
            _panels.Clear();
        }
    }
}