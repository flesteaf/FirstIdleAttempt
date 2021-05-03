using HackingYourWay.Assets.Scripts.Networks.Generation;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class SmallOfficeNetworkGeneration : NetworkGeneration
    {

        public override NetworkType Type => NetworkType.SmallOffice;

        public override int NoOfDevices => 15;
         
        public override ProtectionType Protection => ProtectionType.WPA;

        protected override string GetSSID()
        {
            return officeSsids[new Random().Next(0, officeSsids.Count - 1)];
        }

        #region SSIDList
        private readonly List<string> officeSsids = new List<string> { "OnlyPrinting", "Decisions", "The oners",
                "Netgear57", "Netgear30", "NETGEAR60", "NETGEAR71", "Netgear91", "NETGEAR42", "NETGEAR53", "NETGEAR63", "NETGEAR48", 
                "netgear27", "NETGEAR65", "NETGEAR37", "NETGEAR78", "NETGEAR72", "NETGEAR73", "NETGEAR46", "NETGEAR82", "NETGEAR84",
                "netgear31", "netgear76", "NETGEAR93", "NETGEAR14", "NETGEAR38", "NETGEAR67", "NETGEAR15", "NETGEAR79", "netgear83",
                "NETGEAR58", "NETGEAR97", "NETGEAR87", "NETGEAR39", "NETGEAR94", "netgear29", "NETGEAR86", "NETGEAR92", "NETGEAR89",
                "NETGEAR52", "NETGEAR95", "NETGEAR90", "NETGEAR64", "NETGEAR96", "NETGEAR62", "Netgear61", "netgear54", "NETGEAR56",
                "NETGEAR26", "NETGEAR34", "NETGEAR08", "NETGEAR66", "NETGEAR36", "NETGEAR04", "NETGEAR68", "Oi_WiFi_Fon", "NETGEAR03",
                "NETGEAR06", "NETGEAR59", "Alex", "DIR-300", "netgear02", "NETGEAR98", "NETGEAR00", "mywifi", "airport", "FRITZ!Box_Fon_WLAN_7141",
                "CHT_Wi-Fi(HiNet)", "MSHOME", "netis", "Swisscom_Auto_Login", "Gast", "o2DSL", "X03B-89m6274IR", "CMCC", "3Com", "A9F1BDF1DAB1NVT4F4F59",
                "optimumwifi-Emergency", "homewifi", "Tele_Columbus", "Visitor", "5ECUR3w3p5TOR3", "CHT_Wi-Fi_Auto", "FRITZ!Box_6340_Cable",
                "Vodafone-Guest", "MySpectrumWiFi78-2G", "MySpectrumWiFi80-2G", "Wi2", "MySpectrumWiFi88-2G", "MySpectrumWiFi30-2G",
                "MySpectrumWiFi40-2G", "MySpectrumWiFi48-2G", "MySpectrumWiFi58-2G", "MySpectrumWiFi28-2G", "MySpectrumWiFi98-2G",
                "MySpectrumWiFi68-2G", "MySpectrumWiFi38-2G", "MySpectrumWiFi50-2G", "PrettyFlyForAWiFi", "MySpectrumWiFi18-2G", "FBI_Surveillance_Van",
                "MySpectrumWiFic8-2G", "MySpectrumWiFi10-2G", "MySpectrumWiFi70-2G", "MySpectrumWiFi60-2G", "Draytek", "MySpectrumWiFi20-2G",
                "MySpectrumWiFi90-2G", "MySpectrumWiFi00-2G", "MySpectrumWiFia8-2G", "MySpectrumWiFi08-2G", "TENDA", "MySpectrumWiFie8-2G", 
                "MySpectrumWiFic0-2G", "MySpectrumWiFid0-2G", "MySpectrumWiFif8-2G", "MySpectrumWiFid8-2G", "MySpectrumWiFib8-2G", 
                "MySpectrumWiFif0-2G", "MySpectrumWiFia0-2G", "MySpectrumWiFie0-2G", "Student", "MySpectrumWiFib0-2G", "WirelessICC", 
                "@Hyatt_WiFi", "ZTE", "tsunami", "Wi-Fi", "Shawgo", "Hotspot", "Neuf_WiFi", "WirelessNet2", "TrendNET", "COSMOTE_WiFi_Fon", 
                "FON_FREE_EAP", "NetWork", "Jt9x11w3", "FRITZ!Box_3272", "govroam", "Home_Network", "YOTA", "ResidenceInn_GUEST", 
                "mycloud", "Speedport_W_501V", "DELTA_WifiSpots", "attwifi_-_Passpoint", "Wireless-N", "IHGConnect", "Wi2_club", 
                "freifunk", "TP-LINK_Extender", "HHT-WM2", "HHT-WM1", "WirelessSGF", "Apple", "Carbon", "?MUSIC?", "LjLr511", "cisco", 
                "wirelessmobile", "Ikea_WiFi", "SSID2-2.4", "THOMSON", "Walmartwifi_2.4", "iPhone_(2)", "UBNT", "david", "scandic_easy", 
                "Home_WiFi", "myLGNet", "House", "Guest_Network", "TP-LINK_Extender_2.4GHz", "LWL-M", "DIR-620", "RSS-351540", "EdiMAX", 
                "mesh", "dell_device", "INTERSVYAZ_OPEN", "Spectrum_Mobile", "TCWireless", "Your_New_Wi-Fi", "CORP", "T_wifi_zone", 
                "Maria", "HUAWEI_p8_lite", "eir_WiFi", "FRITZ!Box_WLAN_3131", "SYNC_00000000", "Philips_WiFi", "Home1", "GuestWiFi", 
                "alpha", "WirelessLocal_IO", "corporate", "Mi_Phone", "Homenetwork", "swsecure", "serviceswifi", "Tiscali", "tmobile", 
                "BYOD", "absauthz", "?MUSIC?", "ALICE-WLAN", "yrneh09", "T_wifi_zone_secure", "docomo", "nomad5", "str241xipv", "martin", 
                "wificlientesR", "Guest_Access", "FRITZ!Box_Fon_WLAN_7050", "Zoom", "#TELUS", "Linksys-G", "_EUSKALTELWIFI_KALEAN", 
                "07_Never_gonna_tell_a_lie", "HUAWEI_P20_Pro", "Fairfield_Guest", "MySpectrumWiFi40-5G", "MySpectrumWiFi58-5G", "ZMD_SAP", 
                "MySpectrumWiFi88-5G", "MySpectrumWiFi78-5G", "MySpectrumWiFi90-5G", "MySpectrumWiFi68-5G", "MySpectrumWiFi38-5G", "Fon", 
                "MySpectrumWiFi48-5G", "MySpectrumWiFi08-5G", "MySpectrumWiFi80-5G", "MySpectrumWiFi28-5G", "FRITZ!Box_WLAN_3370", 
                "MySpectrumWiFi60-5G", "MySpectrumWiFi30-5G", "MySpectrumWiFi70-5G", "GDS_VCI_01", "MySpectrumWiFi50-5G"
        };
        #endregion
    }
}