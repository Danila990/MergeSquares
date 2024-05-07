using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Localization
{
    public class VersionLocalized : MonoBehaviour
    {
        [SerializeField] private LocalizeUi localizeUi;

        private void Start()
        {
            localizeUi.UpdateArgs(new[] { Application.version.ToString() });
        }
    }
}
