using System.Collections.Generic;
using Core.SaveLoad;
using Tutorial.Models;
using UnityEngine;

namespace Tutorial
{
    public abstract class TutorialProviderBase : MonoBehaviour
    {
        protected TutorialService _tutorialService;
        public TutorialService TutorialService => _tutorialService;
        public abstract List<ATutorial> Init(TutorialServiceData tutorialServiceData, LoadContext context, TutorialService tutorialService);
    }

    public class TutorialProviderBase<TDesc, TStep> : TutorialProviderBase
        where TDesc : TutorialDescBase<TStep>
        where TStep : TutorialStepBase
    {
        public virtual IReadOnlyList<TDesc> TutorialDescriptions => new List<TDesc>();

        public override List<ATutorial> Init(TutorialServiceData tutorialServiceData, LoadContext context, TutorialService tutorialService)
        {
            _tutorialService = tutorialService;
            var res = new List<ATutorial>();
            foreach (var desc in TutorialDescriptions)
            {
                var passed = false;
                var tutorialData = tutorialServiceData.tutorials.Find(data => desc.name == data.id);
                if (tutorialData != null)
                {
                    passed = tutorialData.passed;
                }
                if (!passed)
                {
                    var tutorial = Create();
                    tutorial.SetDesc(desc);
                    res.Add(tutorial);
                }
            }
            return res;
        }

        protected virtual TutorialBase<TDesc, TStep> Create() => new();
    }
}