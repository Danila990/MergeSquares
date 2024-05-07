using GoogleSheetsToUnity;

namespace Core.Repositories
{
    public interface IAbstractRepository
    {
        public void UpdateRepository(GstuSpreadSheet spreadSheet);
        public string AssociatedSheet { get; }
        public string AssociatedWorksheet { get; }
    }
}