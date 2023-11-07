using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace PermUnlockables.Patches
{
	[HarmonyPatch]
	class GameOverPatches
	{
		[HarmonyPatch(typeof(AutoParentToShip), "StartSuckingOutOfShip")]
		[HarmonyPrefix]
		public static bool PlayersFiredItsJoever()
		{
			return (false);
		}
		[HarmonyPatch(typeof(StartOfRound), "ResetShip")]
		[HarmonyPrefix]
		public static bool ResetShip(StartOfRound __instance)
        {
			PlaceableShipObject[] array = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].parentObject.StartSuckingOutOfShip();
				Collider[] componentsInChildren = array[i].parentObject.GetComponentsInChildren<Collider>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].enabled = true;
				}
			}
			TimeOfDay.Instance.globalTime = 100f;
			TimeOfDay.Instance.profitQuota = TimeOfDay.Instance.quotaVariables.startingQuota;
			TimeOfDay.Instance.quotaFulfilled = 0;
			TimeOfDay.Instance.timesFulfilledQuota = 0;
			TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * (float)TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
			TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
			__instance.randomMapSeed++;
			Debug.Log("Reset ship A");
			__instance.companyBuyingRate = 0.3f;
			__instance.ChangeLevel(__instance.defaultPlanet);
			__instance.ChangePlanet();
			__instance.SetPlanetsWeather();
			__instance.SetMapScreenInfoToCurrentLevel();
			Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
			if (terminal != null)
			{
				terminal.groupCredits = TimeOfDay.Instance.quotaVariables.startingCredits;
			}
			if (__instance.IsServer)
			{
				RoundManager.Instance.DespawnPropsAtEndOfRound(despawnAllItems: true);
				__instance.closetLeftDoor.SetBoolOnClientOnly(setTo: false);
				__instance.closetRightDoor.SetBoolOnClientOnly(setTo: false);
			}
			Debug.Log("Reset ship A");
			Debug.Log("Reset ship B: Going to reset unlockables list!");
			Debug.Log("Reset ship C: Reset unlockables list!");
			for (int l = 0; l < __instance.allPlayerScripts.Length; l++)
			{
				SoundManager.Instance.playerVoicePitchTargets[l] = 1f;
				__instance.allPlayerScripts[l].ResetPlayerBloodObjects();
			}
			Debug.Log("Reset ship D");
			TimeOfDay.Instance.OnDayChanged();
			return (false);
		}
		[HarmonyPatch(typeof(GameNetworkManager), "ResetSavedGameValues")]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> ResetSavedGameValues(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand == "UnlockedShipObjects")
				{
					codes[i].opcode = OpCodes.Nop;
					codes[i + 1].opcode = OpCodes.Nop;
					codes[i + 2].opcode = OpCodes.Nop;
					codes[i + 3].opcode = OpCodes.Nop;
				}
				if (codes[i].opcode == OpCodes.Blt && codes[i-1].opcode == OpCodes.Callvirt)
				{
					codes[i].opcode = OpCodes.Bgt;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
