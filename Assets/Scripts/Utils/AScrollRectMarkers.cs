using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Utils
{
    [Serializable]
    public class ScrollRectMarker
    {
        public RectTransform rect;
        public Color outlineColor = Color.white;
        public Color solidColor;
    }

    public class AScrollRectMarkers<TEnum> : MonoBehaviour where TEnum : Enum
    {
        [SerializeField] private ScrollRect _scrollRect;
        // [SerializeField] private bool onlyVertical = true;
        [SerializeField] private HashMap<TEnum, ScrollRectMarker> _markers = new HashMap<TEnum, ScrollRectMarker>();

        public HashMap<TEnum,ScrollRectMarker> GetMarkers => _markers;

        private void Reset()
        {
            ResetDependencies();
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (_scrollRect == null)
            {
                return;
            }

            foreach (var marker in _markers.Values)
            {
                var size = Vector2.Scale(marker.rect.rect.size, marker.rect.lossyScale);
                var position = marker.rect.position;
                position.y -= size.y;
                position.x -= size.x / 2;
                UnityEditor.Handles.DrawSolidRectangleWithOutline(new Rect(position, size), marker.solidColor, marker.outlineColor);
            }
#endif
        }

        public TEnum GetNameOfMarker(ScrollRectMarker marker)
        {
            if (!_markers.ContainsValue(marker))
            {
                return default(TEnum);
            }

            return _markers.GetKeyByValue(marker);
        }

        public void ScrollToMarker(TEnum markerName)
        {
            if (_scrollRect != null && _markers.ContainsKey(markerName))
            {
                var marker = _markers[markerName];
                var viewportSize = Vector2.Scale(_scrollRect.viewport.rect.size, _scrollRect.viewport.lossyScale);
                var dist = _scrollRect.content.position.y - marker.rect.position.y + viewportSize.y / 2 + _scrollRect.viewport.position.y;
                // LogMarkerPosition(markerName);
                _scrollRect.velocity = Vector2.zero;
                _scrollRect.content.position = new Vector3(_scrollRect.content.position.x, dist);
            }
        }

        public void LogMarkersPositions()
        {
            foreach (var marker in _markers.Keys)
            {
                LogMarkerPosition(marker);
            }
        }

        public void AddMarker(TEnum markerName, RectTransform rect, Color outlineColor = default, Color solidColor = default)
        {
            if (outlineColor == default)
            {
                outlineColor = Color.white;
            }

            if (solidColor == default)
            {
                solidColor = new Color(1f, 1f, 1f, 0);
            }

            var marker = new ScrollRectMarker()
            {
                rect = rect,
                solidColor = solidColor,
                outlineColor = outlineColor
            };

            _markers.Add(markerName, marker);
        }

        public void RemoveMarker(TEnum markerName)
        {
            if (_markers.ContainsKey(markerName))
            {
                _markers.Remove(markerName);
            }
        }

        public void ResetDependencies()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        private void LogMarkerPosition(TEnum markerName)
        {
            if (_scrollRect != null && _markers.ContainsKey(markerName))
            {
                var marker = _markers[markerName];
                var size = Vector2.Scale(marker.rect.rect.size, marker.rect.lossyScale);
                var viewportSize = Vector2.Scale(_scrollRect.viewport.rect.size, _scrollRect.viewport.lossyScale);
                var dist = _scrollRect.content.position.y - marker.rect.position.y + viewportSize.y / 2 + _scrollRect.viewport.position.y;
                var contentSize = Vector2.Scale(_scrollRect.content.rect.size, _scrollRect.content.lossyScale);
                var str = new StringBuilder();
                str.Append($"[AScrollRectMarkers][ScrollToMarker] scroll to: {markerName}\n");
                str.Append($"pos: {marker.rect.position.y} size: {size.y}\n");
                str.Append($"Viewport\n");
                str.Append($"pos: {_scrollRect.viewport.position.y} size: {viewportSize.y}\n");
                str.Append($"Content\n");
                str.Append($"pos: {_scrollRect.content.position.y} size: {contentSize.y}\n");
                str.Append($"dist: {dist}\n");
                Debug.Log(str.ToString());
            }
        }
    }
}
