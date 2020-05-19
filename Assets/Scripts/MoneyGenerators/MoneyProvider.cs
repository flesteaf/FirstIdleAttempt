using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MoneyGenerators
{
    public class MoneyProvider
    {
        private float moneyGenerated = 0.01f;
        internal float GetAmmountGenerated()
        {
            return moneyGenerated;
        }
    }
}
