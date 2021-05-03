using HackingYourWay.Assets.Scripts.Networks.Generation;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Networks
{
    internal class SmallNetworkGeneration : NetworkGeneration
    {

        public override NetworkType Type => NetworkType.Small;

        public override int NoOfDevices => 8;

        public override ProtectionType Protection => ProtectionType.WEP;

        protected override string GetSSID()
        {
            return smallSsids[new Random().Next(0, smallSsids.Count - 1)];
        }

        #region SSIDList
        private readonly List<string> smallSsids = new List<string> { "Ghost", "IBD", "Sockass",
            "MySpectrumWiFi98-5G", "MySpectrumWiFic8-5G", "MySpectrumWiFi18-5G", "flashzone-seamless", "Connectify-me", 
            "MySpectrumWiFi10-5G", "net", "MySpectrumWiFic0-5G", "MySpectrumWiFi00-5G", "MySpectrumWiFi20-5G", "LDSAccess", 
            "MySpectrumWiFid0-5G", "MySpectrumWiFia8-5G", "MySpectrumWiFib8-5G", "MySpectrumWiFie8-5G", "MySpectrumWiFia0-5G", 
            "MySpectrumWiFib0-5G", "MySpectrumWiFid8-5G", "MySpectrumWiFif0-5G", "MySpectrumWiFie0-5G", "Guests", "HOL_ALU_WLAN", 
            "MySpectrumWiFif8-5G", "myhome", "MelitaWiFi", "ConnectionPoint", "HomeSweetHome", "XFBSECA7HE6H", "Auto-BTWiFi", 
            ".1.Free_Wi-Fi", "DSL_2640NRU", "CtLsIiClK", "WirelessLocal", "router", "GABLE", "Linbury", "WWM_P2P", "Max", "SMITH", 
            "cWiFi", "ASUS_Guest1", "NETGEAR-2.4-G", "FRITZ!Powerline_540E", "mycadillac", "anna", "freebox", "CMCC-AUTO", "DANIEL", 
            "0001_Secured_Wi-Fi", "WLAN-TWDC", "NTT-SPOT", "HOL_ZTE_4", "Matrix", "Johnson", "IWIFI", "NetFasteR_IAD_2_(PSTN)", 
            "NETGEAR-5G", "FRITZ!Box_3490", "detnsw", "Wayport_Access", "Green", "IHG_Connect", "Dmpm49Bjfk10", "belkin", "DIR-320NRU", 
            "thomas", "samsung", "mike", "BLUE", "CSL", "FRITZ!Box_6320_Cable", "Telekom_Fon_WiFi_HU", "iot", "ChinaNet", 
            "03_Never_gonna_run_around", "NETGEAR_Guest1", "HUAWEI_P10", "My_Network", "tKeRl", "maison", "DIR-300NRUB7", "TELE2", 
            "BATCAVE", "Michael", "Pod", "Macysfreewifi", "SKODA_WLAN", "Knet", "Pretty_Fly_for_a_Wi-Fi", "NETGEAR1", "Sunshine", 
            "CG-Guest", "TG1672G62", "p2pkoer", "VW_WLAN", "mint", "doma", "john", "HUAWEI_P10_lite", "#o4D@n!3LS3b@$+1@N<B", "TG1672G22", 
            "MT_free", "FBI", "TG1672GD2", "TG1672G12", "TG1672G92", "TG1672G52", "Bill_Wi_the_Science_Fi", "TG1672GF2", "TG1672G32", 
            "UqtbBHX4Tt", "m3connect", "TG1672G72", "Flow_Wi-Fi", "TG1672GA2", "USR8054", "TG1672G02", "TG1672GB2", "TG1672G82", 
            "TG1672G42", "TG1672GC2", "_BTWi-fi", "FRITZ!Box_Fon_WLAN_7340", "TG1672GE2", "HUAWEI_P20_lite", "it_hurts_when_IP", 
            "george", "Williams", "vpnator", "Employee", "brown", "huawei", "Boingo_Hotspot", "chris", "VIVACOM_NET", "Airlive", 
            "VW_WLAN", "Gateway", "vendcust", "laQuinta", "TG1672GD2-5G", "TG1672G22-5G", "TG1672G12-5G", "Robert", "PLDTHOMEDSL", 
            "HomeDepot_Public_Wi-Fi", "TG1672G52-5G", "Wi2eap", "TG1672G92-5G", "Renaissance_GUEST", "TG1672G42-5G", "ask4_Wireless", 
            "TG1672G72-5G", "TG1672GA2-5G", "TG1672G82-5G", "HOGWARTS", "Apple_Store", "TG1672GF2-5G", "TG1672G62-5G", "NETGEAR-5G-GUEST", 
            "WIFI-Guest", "TG1672G32-5G", "Tele2-modem", "TG1672GC2-5G", "TG1672GB2-5G", "DSL-2640U", "TG1672G02-5G", "BETA", "TG1672GE2-5G", 
            "Angel", "plusnetwireless", ".wifisfera_telecable", "planexuser", ".@_AIS_SUPER_WiFi", "Charlie", "Guest_Wifi", 
            "ASK4_Wireless_(802.1x)", "LjLr11", "MobileConnect", "Ambassador_iPad", "FRITZ!Box_3390", "AIS_SMART_Login", "ABC", "Gamma", 
            "TP-LINK_Extender_5GHz", "ASDAFreeWifi", "9qZNkCYGzf", "MikroTik", "IBIS", "MINE", "Wireless@SG", "CSL_Auto_Connect", 
            "DG-Tablet", "james", "hol_-_NetFasteR_WLAN_3", "CVWifi", "comcast", "0A344ABB", "backup", "thuis", "DIR-300NRUB6", 
            "wifi_extra", "mprotek", "Sam", "InfostradaWiFi", "WebSTAR", "DigSig", "Boingo_Wireless", "ANY", "home2", "Marina", "DATA", 
            "Apple_Network", "University_of_Washington", "VODAFONE_WIFI_108", "Wireless@SGx", "Motel_6", "Living_Room", "wmdemo", 
            "KaiserGuest", "Vendor", "Free_WiFi", "DOM.RU_Wi-Fi", "DG-Customer", "Silverado", ".@_truemoveH", ".@_TRUEWIFI", "demo", 
            "Speedy_Instan@wifi.id", "sweethome", "Living_Room_TV", "Horizon_WifiSpots", "DSLWLANModem200", "onlime", "elena", 
            "Moscow_WiFi_FREE", "FBI_Surveillance", "wifi.tattele.com", "MO1875", "family"
        };
        #endregion
    }
}