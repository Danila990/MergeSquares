using System;
using System.Collections;
using System.Collections.Generic;
using Core.Anchors;
using GameScripts.MergeSquares;
using Tutorial;
using UnityEngine;
using Zenject;

namespace Core.Windows
{
    public class Droplist : MonoBehaviour
    {
        [SerializeField] protected GameObject root;
        [SerializeField] protected float autocloseTime = 3f;
        [SerializeField] protected List<Anchor> anchors;

        protected bool active;
        
        protected TutorialService _tutorialService;
        protected GridManager _gridManager;

        [Inject]
        public void Construct(TutorialService tutorialService, GridManager gridManager)
        {
            _tutorialService = tutorialService;
            _gridManager = gridManager;
        }
        
        protected void Start()
        {
            _gridManager.OnFreeCellClicked += Hide;
            Hide();
        }

        public void OnClick()
        {
            if (active)
            {
                Hide();
            }
            else
            {
                root.SetActive(true);
                active = true;
                StartCoroutine(AutoClose());
            }
        }

        public void Hide()
        {
            root.SetActive(false);
            active = false;
        }

        protected IEnumerator AutoClose()
        {
            var closeTime = Time.time + autocloseTime;
            while ((active && closeTime > Time.time) || (AutoCloseDisabled()))
            {
                yield return null;
            }

            Hide();
        }

        protected virtual bool AutoCloseDisabled() => _tutorialService.HasActiveTutorial;
    }
}
