namespace Assets.Scripts.Commands
{
    public enum CommandOptions
    {
        Invalid,
        None,

        #region CrackOptions
        wep,
        wpa,
        wpa2,
        #endregion CrackOptions

        #region FirewallOptions
        disable,
        enable,
        #endregion FirewallOptions

        #region InjectOptions
        miner,
        bot,
        spammer,
        ransomware,
        #endregion InjectOptions

        #region SaveOptions
        clear,
        #endregion

        #region Scan&ExtractOptions
        network,
        ip,
        mac,
        #endregion Scan&ExtractOptions

        #region ShowOptions
        networks,
        ips,
        #endregion ShowOptions

        #region StatusOptions
        computer,
        money,
        #endregion StatusOptions

        #region Store&BuyOptions
        software,
        components,
        component,
        #endregion Store&BuyOptions

        #region Autocomplete
        Last
        #endregion
    }
}