namespace Assets.Scripts.Commands
{
    public enum CommandOptions
    {
        Invalid,
        None,

        #region ScanOptions

        network,
        ip,
        mac,

        #endregion ScanOptions

        #region CrackOptions

        wep,
        wpa,
        wpa2,
        #endregion CrackOptions

        #region InjectOptions

        miner,
        bot,
        spammer,
        ransomware,
        #endregion InjectOptions

        #region FirewallOptions

        disable,
        enable,
        #endregion FirewallOptions

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
        component
        #endregion Store&BuyOptions

    }
}