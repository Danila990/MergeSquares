using Core.Pool;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Utils.ClickableList
{
    public abstract class AClickableList<TData, TItem, TParams> : MonoBehaviour where TItem : AClickableItem<TParams, TItem>
    {
        [Header("ClickableList")]
        [SerializeField] private TItem prefabItem;
        [SerializeField] private Transform itemRoot;
        [Space]

        protected Dictionary<TItem, TParams> _itemParams =
            new Dictionary<TItem, TParams>();
        
        private Pool<TItem> _itemPool;

        [Inject]
        public void ConstructClickableList()
        {
            _itemPool = new Pool<TItem>(
                () =>
                {
                    var item = Instantiate(prefabItem, itemRoot);
                    ConstructItem(item);
                    
                    return item;
                },
                0
            );
        }

        protected virtual void ConstructItem(TItem item) 
        {
            item.gameObject.SetActive(false);
        }

        protected virtual void InitItem(TItem item, int index)
        {
        }

        protected virtual void RecycleItem(TItem item)
        {
        }

        protected abstract void OnItemClicked(TItem item);

        protected abstract void OnItemInfoClicked(TItem item);

        protected abstract TParams GetInitItemParams(TData data, int index);

        protected TParams GetItemParams(TItem item)
        {
            return TryGetItemParams(item, out var itemParams) ? itemParams : default(TParams);
        }

        protected bool TryGetItemParams(TItem item, out TParams itemParams)
        {
            if (!_itemParams.ContainsKey(item))
            {
                itemParams = default(TParams);
                return false;
            }

            itemParams = _itemParams[item];
            return true;
        }

        protected void InitClickableList(IList<TData> dataList)
        {
            _itemParams.Clear();

            foreach (var languageItem in _itemPool.GetActive())
            {
                Recycle(languageItem);
            }

            for (int i = 0; i < dataList.Count; i++)
            {
                var dataItem = dataList[i];

                var item = _itemPool.Take();
                var itemParams = GetInitItemParams(dataItem, i);
                _itemParams.Add(item, itemParams);

                item.Init(itemParams);
                InitItem(item, i);

                item.ButtonClicked += OnItemClicked;
                item.InfoButtonClicked += OnItemInfoClicked;

                item.transform.SetSiblingIndex(i);
                item.gameObject.SetActive(true);
            }
        }

        private void Recycle(TItem item)
        {
            item.ButtonClicked -= OnItemClicked;
            RecycleItem(item);

            item.gameObject.SetActive(false);
            _itemPool.Recycle(item);
        }
    }
}
