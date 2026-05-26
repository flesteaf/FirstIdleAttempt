using Godot;
using HackYourWay.Models;

namespace HackYourWay.Data
{
    /// <summary>Designer-editable definition of a virtual currency.</summary>
    [GlobalClass]
    public partial class CurrencyDefinitionSO : Resource
    {
        [Export] public CurrencyType CurrencyType { get; set; }

        [Export] public string DisplayName { get; set; }

        [Export] public string Symbol { get; set; }
    }
}
