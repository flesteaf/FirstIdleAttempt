namespace Assets.Scripts.Commands
{
    public enum CommandOptions
    {
        #region ScanOptions
        network,
        ip,
        mac,
        #endregion ScanOptions

        #region CrackOptions
        wep,
        wpa,
        wpa2,
        #endregion 

        #region InjectOptions
        miner,
        bot,
        spammer,
        ransomware,
        #endregion 

        #region FirewallOptions
        disable,
        enable,
        #endregion 

        #region ShowOptions
        networks,
        ips,
        #endregion

        #region StatusOptions
        computer,
        money,
        #endregion

        #region StoreOptions
        software,
        components
        #endregion 

        #region SetRansomwareOptions
        #endregion 
    }
}
