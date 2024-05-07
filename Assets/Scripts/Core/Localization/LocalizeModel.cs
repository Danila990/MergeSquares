using System;
using System.Collections.Generic;

namespace Core.Localization
{
    [Serializable]
    public class LocalizeModel
    {
        public string fieldName;
        public Dictionary<string, string> values = new Dictionary<string, string>(); // lang, value
    }
}