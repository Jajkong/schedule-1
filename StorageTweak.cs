using HarmonyLib;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.UI;
using UnityEngine;

namespace CDXX;

public class StorageTweak
{
	[HarmonyPatch(typeof(Dealer), "TradeItemsDone")]
	public static class DealerUI
	{
		[HarmonyPostfix]
		public static void Postfix()
		{
			((Component)Singleton<GameplayMenu>.Instance).gameObject.SetActive(true);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		}
	}

	[HarmonyPatch(typeof(StorageEntity), "Close")]
	public static class StorageEntityUI
	{
		[HarmonyPrefix]
		public static void Prefix()
		{
			((Component)Singleton<GameplayMenu>.Instance).gameObject.SetActive(true);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		}
	}
}
