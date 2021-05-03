using Assets.Scripts;
using Newtonsoft.Json;
using System;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string PlayerDataKey = "PlayerData";

    public static void SavePlayer(PlayerData data)
    {
        data.Timestamp = DateTime.UtcNow;
        var playerData = JsonConvert.SerializeObject(data);
        DataSerializer.SaveString(PlayerDataKey, playerData);
    }

    public static PlayerData LoadPlayerData()
    {
        var playerData = DataSerializer.LoadString(PlayerDataKey);
        
        if (string.IsNullOrEmpty(playerData))
        {
            return new PlayerData();
        }

        return JsonConvert.DeserializeObject<PlayerData>(playerData);
    }

    internal static void ClearSlot()
    {
        DataSerializer.ClearKey(PlayerDataKey);
    }
}
