using UnityEngine;
using Utils.ClickableList;
using Core.Localization;

namespace Settings
{
    public class ChooseLanguageButton : AClickableItem<string, ChooseLanguageButton>
    {
        [SerializeField] private LocalizeUi languageNameLocalize;

        protected override void InitItem(string initParams)
        {
            languageNameLocalize.SetLocalizationKey($"{initParams}LanguageName");
        }
    }
}
