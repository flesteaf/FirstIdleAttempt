using System.Collections.Generic;

namespace HackYourWay.Models
{
    /// <summary>All persistent player state; serialised to save.json via SaveData.</summary>
    [System.Serializable]
    public class Player
    {
        /// <summary>
        /// Currency balances stored as a List for JSON serialization compatibility.
        /// </summary>
        public List<CurrencyBalance> Balances = new List<CurrencyBalance>();

        public List<ToolType>     UnlockedTools      = new List<ToolType>();
        public List<CurrencyType> UnlockedCurrencies = new List<CurrencyType>();
        public List<string>       PurchasedItems     = new List<string>();
        public List<Contract>     ActiveContracts    = new List<Contract>();
        public List<string>       CompletedContracts = new List<string>();

        /// <summary>File paths copied via CopyCommand; referenced by file-retrieval contracts.</summary>
        public List<string> CopiedFiles = new List<string>();

        /// <summary>UTC ticks at last save; used for offline income calculation.</summary>
        public long LastSaveUtcTicks;

        /// <summary>CPU tier (1–5). 1 = base laptop; 5 = server rack. Reduces crack time.</summary>
        public int CpuTier = 1;

        /// <summary>Internet bandwidth tier (1–5). 1 = DSL; 5 = dedicated line. Reduces network command time.</summary>
        public int BandwidthTier = 1;

        /// <summary>GPU tier (0–3). 0 = none; 3 = compute cluster. Reduces crack time only.</summary>
        public int GpuTier = 0;

        // ── Balance helpers ──────────────────────────────────────────────────

        /// <summary>Returns the current balance for the given currency (0 if not found).</summary>
        public double GetBalance(CurrencyType currency)
        {
            for (int i = 0; i < Balances.Count; i++)
            {
                if (Balances[i].Currency == currency)
                    return Balances[i].Amount;
            }
            return 0;
        }

        /// <summary>Adds <paramref name="amount"/> to the given currency balance.</summary>
        public void AddBalance(CurrencyType currency, double amount)
        {
            for (int i = 0; i < Balances.Count; i++)
            {
                if (Balances[i].Currency == currency)
                {
                    Balances[i] = new CurrencyBalance(currency, Balances[i].Amount + amount);
                    return;
                }
            }
            Balances.Add(new CurrencyBalance(currency, amount));
        }

        /// <summary>Returns true if the player owns the given tool.</summary>
        public bool HasTool(ToolType tool)
        {
            for (int i = 0; i < UnlockedTools.Count; i++)
                if (UnlockedTools[i] == tool) return true;
            return false;
        }

        /// <summary>Returns true if the given currency is unlocked for this player.</summary>
        public bool HasCurrency(CurrencyType currency)
        {
            for (int i = 0; i < UnlockedCurrencies.Count; i++)
                if (UnlockedCurrencies[i] == currency) return true;
            return false;
        }

    }

    /// <summary>Currency/amount pair used in the balances list.</summary>
    [System.Serializable]
    public struct CurrencyBalance
    {
        public CurrencyType Currency;
        public double Amount;

        public CurrencyBalance(CurrencyType currency, double amount)
        {
            Currency = currency;
            Amount   = amount;
        }
    }
}
