using GoogleSheetsToUnity;
using UnityEngine;

namespace Core.Repositories
{
    public abstract class AbstractRepository<T> : ScriptableObject, IAbstractRepository
    {
        [SerializeField] protected T data;

        public virtual string AssociatedSheet => "1MmOf2IjTS37lvpiYUfBYllHGd4j3yBzOPwNbfIC6Vnc";
        public abstract string AssociatedWorksheet { get; }

        public T GetData() => data;

        public abstract void UpdateRepository(GstuSpreadSheet spreadSheet);

        private void Reset()
        {
            Debug.LogError($"[AbstractRepository][Reset] Data was reset to default value for data with type {typeof(T).FullName}");
        }
    }
}