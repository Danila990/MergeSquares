using GoogleSheetsToUnity;
using TMPro;
using UnityEngine;

namespace Core.Repositories
{
    public abstract class UpdateRepositoryButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI loadStatus;
        
        protected IAbstractRepository _repository;

        public void OnClick()
        {
            loadStatus.text = "In progress";
            SpreadsheetManager.ReadPublicSpreadsheet(
                new GSTU_Search(_repository.AssociatedSheet, _repository.AssociatedWorksheet),
                cc =>
                {
                    loadStatus.text = "OK!";
                    _repository.UpdateRepository(cc);
                }
            );
        }
    }
}