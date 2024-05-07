using System;
using System.Collections.Generic;
using System.Linq;
using Tutorial;
using UnityEngine;
using Utils;

namespace Core.Anchors
{
    public class AnchorService : MonoBehaviour
    {
        private Dictionary<EAnchorType, List<Anchor>> _anchors = new Dictionary<EAnchorType, List<Anchor>>();

        public bool TryGetAnchor(EAnchorType type, out Anchor anchor, string id = null)
        {
            RemoveOldAnchors();
            if (_anchors.ContainsKey(type))
            {
                var anchors = _anchors[type].FindAll(a => String.IsNullOrEmpty(id) || a.Id == id);
                if (anchors.Count > 0)
                {
                    anchor = anchors.Last();
                    return true;
                }
            }

            anchor = null;
            return false;
        }
        
        public void AddAnchor(EAnchorType type, Anchor anchor)
        {
            if (!_anchors.ContainsKey(type))
            {
                var list = new List<Anchor>();
                _anchors[type] = list;
            }
            
            var anchors = _anchors[type];
            if(!anchors.Contains(anchor))
            {
                anchors.Add(anchor);
            }
        }

        public void RemoveAnchor(EAnchorType type, Anchor anchor)
        {
            if(_anchors.ContainsKey(type))
            {
                _anchors[type].Remove(anchor);
            }
        }

        private void RemoveOldAnchors()
        {
            foreach (var pair in _anchors)
            {
                var anchors = new List<Anchor>(pair.Value);
                foreach (var anchor in anchors)
                {
                    if (anchor == null)
                    {
                        pair.Value.Remove(anchor);
                    }
                }
            }
        }
    }
}
