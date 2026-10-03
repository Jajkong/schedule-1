using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.NPCs;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CDXX;

public class CDXXCore : MelonMod
{
	private bool loadCDXX = true;

	private CDXXManager _cdxxManager;

	public TextMeshProUGUI TitleClone;

	public GameObject ChatObject;

	public override void OnInitializeMelon()
	{
		MelonLogger.Warning("Successfully Loaded Melon");
	}

	public override void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		if (!(sceneName == "Main"))
		{
			if (sceneName == "Menu" && !((Object)(object)TitleClone != (Object)null))
			{
				TitleClone = Object.Instantiate<TextMeshProUGUI>(GameObject.Find("MainMenu/Home/Bank/Title").GetComponentInChildren<TextMeshProUGUI>());
				((Object)TitleClone).name = "CDXXVer";
				((Graphic)TitleClone).color = Color.white;
				((TMP_Text)TitleClone).transform.SetParent(((TMP_Text)GameObject.Find("MainMenu/Home/Bank/Title").GetComponentInChildren<TextMeshProUGUI>()).transform.parent, false);
				Transform transform = ((TMP_Text)TitleClone).transform;
				Resolution currentResolution = Screen.currentResolution;
				Vector3 position = default(Vector3);
				((Vector3)(ref position))._002Ector(300f, (float)((Resolution)(ref currentResolution)).height);
				transform.position = position;
				((TMP_Text)TitleClone).text = $"CDXX v{Setup.Version}\nStatus: {(loadCDXX ? "Enabled" : "Disabled")}\nF4: Toggle CDXX";
			}
			return;
		}
		string fullName = ((MelonBase)this).MelonAssembly.Assembly.FullName;
		MelonLogger.Warning("[CDXX] OnSceneWasInitialized for 'Main' - assembly: " + fullName);
		if (!fullName.Contains("CDXX"))
		{
			MelonLogger.Warning("[CDXX] Assembly name does not contain 'CDXX'. Continuing initialization for debugging.");
		}
		if (!loadCDXX)
		{
			return;
		}
		MelonLogger.Warning("[CDXX] Initializing CDXX");
		try
		{
			MelonCoroutines.Start(InitializeCDXX());
		}
		catch (Exception ex)
		{
			MelonLogger.Error("[CDXX] Exception while starting InitializeCDXX: " + ex);
		}
	}

	public override void OnUpdate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		if (((Scene)(ref activeScene)).name == "Menu")
		{
			if (!((Object)(object)TitleClone == (Object)null))
			{
				if (Input.GetKeyDown((KeyCode)285))
				{
					loadCDXX = !loadCDXX;
					((TMP_Text)TitleClone).text = $"CDXX v{Setup.Version}\nStatus: {(loadCDXX ? "Enabled" : "Disabled")}\nF4: Toggle CDXX";
				}
				Transform transform = ((TMP_Text)TitleClone).transform;
				Resolution currentResolution = Screen.currentResolution;
				Vector3 position = default(Vector3);
				((Vector3)(ref position))._002Ector(150f, (float)(((Resolution)(ref currentResolution)).height - 50));
				transform.position = position;
				((Graphic)TitleClone).color = (loadCDXX ? Color.green : Color.red);
			}
		}
		else
		{
			if (!(((Scene)(ref activeScene)).name == "Main") || _cdxxManager == null || !ChatObject.active || !_cdxxManager._awaitingInput)
			{
				return;
			}
			if (Input.GetKeyDown((KeyCode)27) && ChatObject.active)
			{
				_cdxxManager._awaitingInput = false;
				ChatObject.SetActive(false);
				((Component)Singleton<GameInput>.Instance.PlayerInput).gameObject.SetActive(true);
			}
			string inputString = Input.inputString;
			for (int i = 0; i < inputString.Length; i++)
			{
				char c = inputString[i];
				int num;
				switch (c)
				{
				case '\b':
					if (_cdxxManager._currentInput.Length > 0)
					{
						CDXXManager cdxxManager = _cdxxManager;
						string currentInput = _cdxxManager._currentInput;
						string currentInput2 = currentInput.Substring(0, currentInput.Length - 1);
						cdxxManager._currentInput = currentInput2;
					}
					continue;
				case '\n':
					num = 1;
					break;
				default:
					num = ((c == '\r') ? 1 : 0);
					break;
				}
				if (num != 0)
				{
					_cdxxManager._awaitingInput = false;
					_cdxxManager._currentInputCallback?.Invoke(_cdxxManager._currentInput);
				}
				else if (!char.IsControl(c))
				{
					_cdxxManager._currentInput += c;
				}
			}
			((TMP_Text)_cdxxManager._inputDescription).text = _cdxxManager._currentPrompt + ":";
			((TMP_Text)_cdxxManager._inputDisplay).text = _cdxxManager._currentInput + "|";
		}
	}

	private IEnumerator InitializeCDXX()
	{
		MelonLogger.Warning("[CDXX] InitializeCDXX: waiting for required systems...");
		yield return CDXXManager.WaitForRequiredSystems();
		MelonLogger.Warning("[CDXX] InitializeCDXX: required systems ready. Searching NPCs...");
		NPC[] npcArray = Il2CppArrayBase<NPC>.op_Implicit(Object.FindObjectsOfType<NPC>(true));
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 1);
		defaultInterpolatedStringHandler.AppendLiteral("[CDXX] InitializeCDXX: found ");
		defaultInterpolatedStringHandler.AppendFormatted((npcArray != null) ? npcArray.Length : 0);
		defaultInterpolatedStringHandler.AppendLiteral(" NPCs");
		MelonLogger.Warning(defaultInterpolatedStringHandler.ToStringAndClear());
		int index = 0;
		while (index < npcArray.Length)
		{
			NPC npc = npcArray[index];
			try
			{
				if ((Object)(object)npc != (Object)null)
				{
					if (index < 5)
					{
						MelonLogger.Warning($"[CDXX] NPC[{index}] ID={npc.ID}");
					}
					if (!string.IsNullOrEmpty(npc.ID) && npc.ID.Contains("beth"))
					{
						MelonLogger.Warning($"[CDXX] Found matching NPC 'beth' at index {index} (ID={npc.ID}). Creating manager...");
						try
						{
							_cdxxManager = CDXXManager.CreateNew(npc);
						}
						catch (Exception ex)
						{
							MelonLogger.Error("[CDXX] Exception in CDXXManager.CreateNew: " + ex);
						}
						MelonLogger.Warning("[CDXX] CDXXManager.CreateNew returned: " + ((_cdxxManager == null) ? "null" : "instance"));
						break;
					}
				}
			}
			catch (Exception ex2)
			{
				MelonLogger.Error("[CDXX] Exception while scanning NPCs: " + ex2);
			}
			int num = index + 1;
			index = num;
		}
		if (_cdxxManager == null)
		{
			MelonLogger.Warning("[CDXX] _cdxxManager is null after scanning NPCs");
		}
		else
		{
			MelonLogger.Warning("[CDXX] Starting conversation on manager...");
			try
			{
				_cdxxManager.StartConversation();
			}
			catch (Exception ex3)
			{
				MelonLogger.Error("[CDXX] Exception while starting conversation: " + ex3);
			}
		}
		Scene scene = SceneManager.GetActiveScene();
		foreach (GameObject obj in (Il2CppArrayBase<GameObject>)(object)((Scene)(ref scene)).GetRootGameObjects())
		{
			try
			{
				if ((Object)(object)obj != (Object)null && ((Object)obj).name == "CDXX_CustomInput")
				{
					ChatObject = obj;
					MelonLogger.Warning("[CDXX] Found ChatObject = CDXX_CustomInput");
					break;
				}
			}
			catch (Exception ex4)
			{
				MelonLogger.Error("[CDXX] Exception while scanning root objects: " + ex4);
			}
		}
		if ((Object)(object)ChatObject == (Object)null)
		{
			MelonLogger.Error("Failed to find CDXX_CustomInput");
		}
		MelonLogger.Warning("[CDXX] Successfully Initialized");
	}

	public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
	{
		if (sceneName == "Main")
		{
			MelonLogger.Warning("[CDXX] Cleaning");
			_cdxxManager?.FullCleanup();
			_cdxxManager = null;
			MelonLogger.Warning("[CDXX] Cleanup Done");
		}
	}
}
