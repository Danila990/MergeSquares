using System.Collections;
using System.Collections.Generic;
using Core.Windows;
using UnityEngine;

namespace Core.Windows
{
    public class DroplistClues : Droplist
    {
        protected override bool AutoCloseDisabled()
        {
            return (_tutorialService.HasActiveTutorial || _gridManager.CurrentGridView.IsClueActive());
        }
    }
}