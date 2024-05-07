using System;
using System.Collections.Generic;
using Core.SaveLoad;
using UnityEngine;
using Utils;
using Zenject;

namespace Notify
{
    [Serializable]
    public class NotifyRef
    {
        public string id;
    }

    [Serializable]
    public class NotifyServiceData
    {
        public List<NotifyRef> activeRefs = new();
    }

    public class NotifyService : MonoBehaviour
    {
        [Space]
        [SerializeField] private Saver saver;
        
        public Action<NotifyRef> Activated = notifyRef => {};
        public Action<NotifyRef> Deactivated = notifyRef => {};
        
        private NotifyServiceData _data = new();
        
        [Inject]
        public void Construct()
        {
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
        }
        
        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }
            

        public bool IsNotifyActive(NotifyRef notifyRef) => GetFrom(_data.activeRefs, notifyRef) != null;

        public void SetNotify(NotifyRef notifyRef, bool value)
        {
            var old = GetFrom(_data.activeRefs, notifyRef);
            if (value)
            {
                if (old == null)
                {
                    _data.activeRefs.Add(notifyRef);
                    Activated.Invoke(notifyRef);
                    saver.SaveNeeded.Invoke(true);
                }
            }
            else
            {
                if (old != null)
                {
                    _data.activeRefs.Remove(old);
                    Deactivated.Invoke(notifyRef);
                    saver.SaveNeeded.Invoke(true);
                }
            }
        }

        private NotifyRef GetFrom(List<NotifyRef> refs, NotifyRef notifyRef)
        {
            return refs.GetBy(n => n.id == notifyRef.id);
        }
        
        // SaveLoad

        private void Init(NotifyServiceData data, LoadContext context)
        {
            _data = data;
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new NotifyServiceData()), context);
        }
        
        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}