using Assets.Scripts.Networks;
using System;
using System.Collections.Generic;

namespace HackingYourWay.Assets.Scripts.Networks.Generation
{
    internal class MediumNetworkGeneration : NetworkGeneration
    {
        
        public override NetworkType Type => NetworkType.Medium;

        public override int NoOfDevices => 20;

        public override ProtectionType Protection => ProtectionType.WPA;

        protected override string GetSSID()
        {
            return mediumSsids[new Random().Next(mediumSsids.Count)];
        }

        #region SSIDList
        private readonly List<string> mediumSsids = new List<string> { "TheHome", "Hacker", "DND", "DontBotherMe", "IDK",
            "TPLINK", "XLNBusinessServices", "My_WiFi", "jones", "Mywlan", "08_and_hurt_you", "Open", "sca_voice", "wilson", "SSID", 
            "04_and_desert_you", "WiFi-Repeater1", "HUAWEI_P30_Pro", "eduSTAR", "bob", "p27x1c2", "Tesco_WiFi", "MyNet", "Red5", 
            "homebase", "Miller", "Taco_Bell_WiFi", "HUAWEI_P9_lite", "RED", "NETGEAR88-5G", "NETGEAR50-5G", "NETGEAR69-5G", 
            "#NET-WIFI", "NETGEAR25-5G", "Nordstrom_Wi-Fi", "NETGEAR60-5G", "Wireless_Network", "NETGEAR12-5G", "NETGEAR80-5G", 
            "King", "NETGEAR93-5G", "NETGEAR23-5G", "NETGEAR75-5G", "FRITZ!WLAN_Repeater_310", "NETGEAR97-5G", "NETGEAR51-5G",
            "NETGEAR49-5G", "HCSC", "NETGEAR86-5G", "NETGEAR30-5G", "NETGEAR74-5G", "FON_FREE_ssid", "NETGEAR63-5G", "NETGEAR53-5G", "NETGEAR58-5G", 
            "NETGEAR11-5G", "NETGEAR47-5G", "NETGEAR22-5G", "t-mobile", "NETGEAR24-5G", "NETGEAR52-5G", "NETGEAR77-5G", "NETGEAR45-5G", 
            "NETGEAR65-5G", "NETGEAR81-5G", "secure", "HOMERUN", "NETGEAR72-5G", "NETGEAR13-5G", "NETGEAR62-5G", "NETGEAR55-5G", 
            "NETGEAR48-5G", "NETGEAR18-5G", "NETGEAR83-5G", "NETGEAR41-5G", "NETGEAR07-5G", "NETGEAR90-5G", "NETGEAR91-5G", "NETGEAR37-5G", 
            "devolo", "NETGEAR05-5G", "NETGEAR84-5G", "NETGEAR73-5G", "NETGEAR57-5G", "NETGEAR43-5G", "NETGEAR27-5G", "Android", 
            "NETGEAR28-5G", "NETGEAR10-5G", "NETGEAR33-5G", "NETGEAR92-5G", "NETGEAR94-5G", "NETGEAR16-5G", "NETGEAR70-5G", "NETGEAR29-5G", 
            "NETGEAR56-5G", "NETGEAR85-5G", "NETGEAR31-5G", "NETGEAR20-5G", "NETGEAR03-5G", "NETGEAR46-5G", "MO1975", "NETGEAR71-5G", 
            "NETGEAR89-5G", "NETGEAR78-5G", "NETGEAR96-5G", "NETGEAR06-5G", "NETGEAR44-5G", "NETGEAR09-5G", "NETGEAR87-5G", "NETGEAR32-5G", 
            "NETGEAR39-5G", "NETGEAR42-5G", "homerun1x", "NETGEAR59-5G", "NETGEAR35-5G", "NETGEAR21-5G", "NETGEAR64-5G", "NETGEAR17-5G", 
            "Bella", "NETGEAR40-5G", "NETGEAR95-5G", "NETGEAR82-5G", "NETGEAR19-5G", "NETGEAR04-5G", "Orcon-Wireless", "NETGEAR79-5G", 
            "NETGEAR08-5G", "NETGEAR54-5G", "NETGEAR61-5G", "NETGEAR38-5G", "MyNetwork", "NETGEAR98-5G", "NETGEAR68-5G", "NETGEAR15-5G", 
            "NETGEAR26-5G", "NETGEAR34-5G", "guest-wifi", "APTG_Wi-Fi", "NETGEAR76-5G", "NETGEAR14-5G", "NETGEAR67-5G", "NETGEAR99-5G", 
            "Martin_Router_King", "EMINENT", "AP", "NETGEAR02-5G", "NETGEAR66-5G", "DLINK_WIRELESS", "make_infosec_savage_again", 
            "wireless1", "delta", "scout", "NON-RIS-PWL", "NETGEAR36-5G", "olga", "Nachowifi", "SHC-RF-DS-3", "oneteam", "tardis", 
            "CFA_Private_Wi-Fi", "NETGEAR00-5G", "meijer-corp", "Telekom_SIM", "DG-Employee", "Service", "draadloos", "mybuick",
            "Pentagram", "NETGEAR01-5G", "FRITZ!Box_6320_v2_Cable", "Sheraton_Guest", "meijer-vendor", "Paul", "Peter", "AlwaysOn", 
            "PJ-WIRELESS5", "DG1670A02", "BATMAN", "mark", "Link", "NETGEAR-24-G", "TELUS_Passpoint", "Springhill_Guest", "vehicle_hotspot", 
            "Passpoint_Secure", "taylor", "FRITZ!Box_SL_WLAN", "jackson", "nacho_wifi", "HUAWEI_P20", "R", "KEVIN", "internal", 
            "H&M_Free_WiFi", "GovWiFi", "Chromecast", "Starbucks_WiFi", "google", "Buffalo", "swaaa", "conference", "GO1927", 
            "FRITZ!Box", "Radisson_Guest", "ncpsp", "TkLrCp7", "anderson", "FDS020", "kiosk", "G", "unknown", "Carlos", "fritz", 
            "Sweex_LW050v2", "BLIZZARD", "MCORP01", "vizio", "Guest1", "TitEaP21VfS", "mayoguest", "FRITZ!WLAN_Repeater_1750E", "POS",
            "Google_Starbucks", "0000_Secured_Wi-Fi", "Enterprise", "Kohls_Guest_WiFi", "lee", "mcwpa", "Inet", "Hotel", "DG1670A02-5G", 
            "jack", "BBYDemoFast", "DM-HHT", "TRENDnet652", "MERCURY", "Customer_ID", "laura", "20dealer08", "PHOENIX"
        };
        #endregion
    }
}
