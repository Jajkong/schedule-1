using HarmonyLib;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI;

namespace CDXX;

public class RangedWeapons
{
	[HarmonyPatch(typeof(Equippable_RangedWeapon), "Update")]
	public static class Patch
	{
		[HarmonyPostfix]
		public static void Postfix(Equippable_RangedWeapon __instance)
		{
			Singleton<HUD>.Instance.SetCrosshairVisible(true);
			PlayerSingleton<PlayerMovement>.Instance.RemoveSprintBlocker("Aiming");
		}
	}
}
