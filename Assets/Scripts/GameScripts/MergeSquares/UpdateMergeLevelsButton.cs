using Core.Repositories;
using UnityEngine;

namespace GameScripts.MergeSquares
{
    public class UpdateMergeLevelsButton : UpdateRepositoryButton
    {
        [SerializeField] private MergeSquaresLevelRepository repository;

        private void Start()
        {
            _repository = repository;
        }
    }
}