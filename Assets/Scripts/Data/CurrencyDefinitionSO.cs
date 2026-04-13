using UnityEngine;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    /// <summary>Designer-editable definition of a virtual currency.</summary>
    [CreateAssetMenu(fileName = "CurrencyDefinition", menuName = "HackYourWay/Currency Definition")]
    public class CurrencyDefinitionSO : ScriptableObject
    {
        public CurrencyType CurrencyType;

        [Tooltip("Full name shown in UI, e.g. 'Bitcoin'")]
        public string DisplayName;

        [Tooltip("Short symbol shown next to amounts, e.g. 'BTC'")]
        public string Symbol;
    }
}
