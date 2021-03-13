using HackingYourWay.Assets.Scripts.Networks.Generation;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class HomeNetworkGeneration : NetworkGeneration
    {
        public override NetworkType Type => NetworkType.Home;

        public override int NoOfDevices => 5;

        public override ProtectionType Protection => ProtectionType.None;

        protected override string GetSSID()
        {
            return homeSsids[new Random().Next(0, homeSsids.Count - 1)];
        }

        #region SSIDList
        private readonly List<string> homeSsids = new List<string> { "MyHome", "PersonalSpace", "SmoothCriminal", "Alphabet", "Home",
                 "xfinitywifi", "XFINITY", "BTWiFi-with-FON", "linksys", "BTWifi-X", "AndroidAP", "UPC_Wi-Free", "Ziggo",
                 "Telekom_FON", "NETGEAR", "eduroam", "FreeWifi", "FreeWifi_secure", "optimumwifi", "dlink", "cablewifi",
                 "FRITZ!Box_7490", "iPhone", "Virgin_media", "KPN_Fon", "hpsetup", "default", "Vodafone_Hotspot", "TelenetWiFree", "asus",
                 "orange", "Vodafone_Homespot", "SFR_WiFi_Mobile", "Telstra_AIR","Fon_WiFi", "TELENETHOMESPOT", "BTWi-fi", "BTWIFI",
                 "Guest", "Unitymedia_WifiSpot", "Home", "SFR_WiFi_FON","TWCWiFi", "setup", "coxwifi", "FRITZ!Box_6490_Cable",
                 "vodafone-WiFi", "Fritz!Box_Fon_WLAN_7390", "belkin54g", "wireless", "WLAN", "SpectrumWiFi", "TWCWiFi-Passpoint", "FRITZ!Box_7362_SL",
                 "attwifi", "optimumwifi_Passpoint", "SpectrumWiFi_PluS", "portthru", "#NET-CLARO-WIFI", "MYChevrolet", "OutOfService", "BTOpenzone",
                 "PROXIMUS_FON", "internet", "WOW_FI_-_FASTWEB", "FRITZ!Box_Fon_WLAN_7360", "RedMi", "0001softbank", "UPC_WifiSpots", "MEO-WIFI",
                 "SWS1day", "BTFON", "Darmowe_Orange_WiFi", "FRITZ!Box_7312", "PROXIMUS_AUTO_FON", "no_ssid", "FRITZ!Box_Gastzugang", "Tp-link",
                 "hhonors", "FRITZ!Box_Fon_WLAN_7270", "FRITZ!Box_Fon_WLAN_7170", "FRITZ!Box_6360_Cable", "KD_WLAN_Hotspot+", "SFR_WiFi_Public", 
                 "FON_ZON_FREE_INTERNET", "alticewifi", "EAGLE", "FRITZ!Box_7412", "NOS_WIFI_Fon", "_AUTO_ONOWiFi", "wifi", "ZyXEL", "walmartwifi", "Gastzugang", 
                 "telecentro_WIFI", "Bouygues_Telecom_Wi-Fi", "netgear-guest", "ShawMobileHotspot", "ShawOpen", "au_Wi-Fi", "orange12", "FON_BELGACOM",
                 "Wi2premium", "Concrete", "Hilton_Honors", "BTOpenzone-H", "skynet", "ASUS_5G", "Free_Public_WiFi", "VOIP",
                 "FON_FREE_INTERNET", "Unifi", "Wi2premium_club", "D-LINK", "0000docomo", "PYUR_Community", "byfly_WIFI", "BTOpenzone-B",
                 "office", "bandsaw", "FON_NETIA_FREE_INTERNET", "ACTIONTEC", "Horizon_Wi-Free", "Proximus_Smart_Wi-Fi", "iptime",
                 "Telekom", "ShawPasspoint", "myGMC", "au_Wi-Fi2", "SMC", "FRITZ!Box_7330_SL", "WiFi-Repeater", "FRITZ!Box_Fon_WLAN_7113",
                 "_ONOWiFi", "airportthru", "NETGEAR_EXT", "0002softbank", "freephonie", "(null)", "dd-wrt", "BELTELECOM_WIFI", "U+zone",
                 "test", "demopool1", "steel", "Marriott_Guest", "UPC_Wi-Free_#SprawdzUPCMo", "DIR-615", "7483C2932C56", "MyPlace", "Motorola", "pretty_fly_for_a_wifi",
                 "xerox", "staff", "O2_WIFI", "_The_Cloud", ".FREE_Wi-Fi_PASSPORT", "HomeNet", "OTE_WiFi_Fon","FRITZ!Box_Fon_WLAN_7112",
                 "Target_Guest_Wi-Fi", "Sitecom", "guestnet", "0001docomo", "wlan-ap", "DIRECT-", "Casa", "ollehWiFi", "Netcomm_Wireless", "My_ASUS",
                 "Bright_House_Networks", "ATT-WiFi", "Orange_FunSpot", "mobile", "eBOS", "Lowes-Guest-WiFi", "DIR-300NRU", "FRITZ!Box_WLAN_3170",
                 "rebar", "FRITZ!Box_Fon_WLAN_7320", "dge", "WirelessNet", "VOO_HOMESPOT", "demopool2", "virus", "Telekom_Fon", "Voice",
                 "Courtyard_Guest", "Dom", "FRITZ!Box_7272", "public", "HTC_Portable_Hotspot", "KPN", "admin", "logitecuser", "FRITZ!Box_Fon_WLAN_7240",
                 "Private", "BHN_Secure", "M4m786iA", "logitecgameuser", "Sprint_Drive", "WiFi_OBDii", "McDonalds_Free_WiFi", "@wifi.id", "IPAD", "NETGEAR24",
                 "HEATKTE", "netgear11", "NETGEAR22", "netgear12", "Proximus_Public_Wi-Fi", "NETGEAR13", "Netgear88", "NETGEAR23", "NETGEAR77", "FRITZ!Box_7330",
                 "NETGEAR10", "netgear69", "NETGEAR25", "NETGEAR45", "FRITZ!Box_WLAN_3270", "NETGEAR50", "NETGEAR28", "swisscom", "NETGEAR75", "NETGEAR80", "NETGEAR01",
                 "NETGEAR43", "NETGEAR47", "NETGEAR70", "NETGEAR05", "NETGEAR55", "NETGEAR41", "NETGEAR19", "NETGEAR21", "Netgear18", "NETGEAR51", "NETGEAR40", "NETGEAR49",
                 "netgear44", "NETGEAR17", "NETGEAR32", "NETGEAR85", "NETGEAR33", "Netgear09", "NETGEAR20", "NETGEAR35", "NETGEAR99", "NETGEAR07", "NETGEAR16", "NETGEAR74",
                 "NETGEAR81", "getyourownwifi",
    };
        #endregion
    }
}