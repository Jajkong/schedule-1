using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Il2CppFishNet.Connection;
using Il2CppFishNet.Managing.Logging;
using Il2CppFishNet.Managing.Server;
using Il2CppFishNet.Object;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppScheduleOne;
using Il2CppScheduleOne.Combat;
using Il2CppScheduleOne.Core.Items.Framework;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Levelling;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppScheduleOne.NPCs.Relation;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Police;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Product.Packaging;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.Quests;
using Il2CppScheduleOne.Trash;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Phone;
using Il2CppScheduleOne.UI.Phone.Messages;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CDXX;

public class CDXXManager
{
	private static CDXXManager _instance;

	public CDXXMessageSystem _messageSystem;

	public NPC _npcInstance;

	public Supplier _supplierInstance;

	public GameObject _npcObject;

	public NPC _npcTemplate;

	public RectTransform _panelRt;

	public bool _isUIVisible = false;

	public Button _sendButton;

	public TextMeshProUGUI _inputDisplay;

	public TextMeshProUGUI _inputDescription;

	public string _currentInput = "";

	public Action<string> _currentInputCallback;

	public string _currentPrompt;

	public bool _awaitingInput = false;

	private bool neverwanted = false;

	private GameObject EnvironmentFX = GameObject.Find("Managers/@EnvironmentFX");

	private bool isChoosing = false;

	private ProductDefinition chosenProduct = new ProductDefinition();

	public IEnumerator WaitForInput(string prompt, Action<string> callback)
	{
		_currentPrompt = prompt;
		_currentInputCallback = callback;
		_awaitingInput = true;
		_currentInput = "";
		Scene scene = SceneManager.GetActiveScene();
		foreach (GameObject obj in (Il2CppArrayBase<GameObject>)(object)((Scene)(ref scene)).GetRootGameObjects())
		{
			if (((Object)obj).name == "CDXX_CustomInput")
			{
				obj.SetActive(true);
			}
		}
		((TMP_Text)_inputDescription).text = prompt + ":";
		((TMP_Text)_inputDisplay).text = _currentInput + "|";
		((Component)Singleton<GameInput>.Instance.PlayerInput).gameObject.SetActive(false);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(false);
		while (_awaitingInput)
		{
			yield return null;
		}
		foreach (GameObject obj2 in (Il2CppArrayBase<GameObject>)(object)((Scene)(ref scene)).GetRootGameObjects())
		{
			if (((Object)obj2).name == "CDXX_CustomInput")
			{
				obj2.SetActive(false);
			}
		}
		((Component)Singleton<GameInput>.Instance.PlayerInput).gameObject.SetActive(true);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(true);
	}

	public IEnumerator WaitForBool(Action callback)
	{
		while (isChoosing)
		{
			yield return null;
		}
		callback();
	}

	private CDXXManager()
	{
	}//IL_0038: Unknown result type (might be due to invalid IL or missing references)
	//IL_0042: Expected O, but got Unknown


	public static CDXXManager CreateNew(NPC Template)
	{
		if (_instance != null)
		{
			_instance.FullCleanup();
			_instance = null;
		}
		_instance = new CDXXManager();
		_instance.Initialize(Template);
		return _instance;
	}

	public void FullCleanup()
	{
		if ((Object)(object)_npcObject != (Object)null)
		{
			Object.Destroy((Object)(object)_npcObject);
			_npcObject = null;
		}
		if (!((Object)(object)_npcInstance == (Object)null) && NPCManager.NPCRegistry.Contains(_npcInstance))
		{
			NPCManager.NPCRegistry.Remove(_npcInstance);
			_npcInstance = null;
		}
	}

	private void Initialize(NPC Template)
	{
		if (!((Object)(object)Template == (Object)null))
		{
			_npcObject = Object.Instantiate<GameObject>(((Component)Template).gameObject);
			_npcObject.SetActive(false);
			_npcObject.SetActive(true);
			_npcInstance = _npcObject.GetComponent<NPC>();
			((Object)_npcObject).name = "CDXX";
			_npcInstance.FirstName = "CDXX";
			_npcInstance.LastName = Setup.Version ?? "";
			_npcInstance.ID = "cdxx_" + Setup.Version;
			_npcInstance.BakedGUID = "cdxx_" + Setup.Version;
			_npcInstance.ConversationCanBeHidden = false;
			_npcInstance.ShowRelationshipInfo = false;
			_npcInstance.MugshotSprite = ((App<DealerManagementApp>)(object)PlayerSingleton<DealerManagementApp>.Instance).AppIcon;
			_npcInstance.MSGConversation = null;
			_npcInstance.ConversationCategories = new List<EConversationCategory>();
			_npcInstance.Avatar = Player.Local.Avatar;
			_messageSystem = new CDXXMessageSystem(_npcInstance);
			_messageSystem.SendNPCMessage("CDXX " + Setup.Version + " has been initialized. \ndiscord.gg/8SKnBSjBdW");
			CreateCustomUI();
		}
	}

	public void StartConversation()
	{
		try
		{
			MENU_MAIN(openMessage: false);
		}
		catch (Exception value)
		{
			MelonLogger.Error($"Failed to start conversation: {value}");
		}
	}

	public static IEnumerator WaitForRequiredSystems()
	{
		float timeout = 30f;
		float elapsed = 0f;
		while (((Object)(object)Singleton<NotificationsManager>.Instance == (Object)null || (Object)(object)PlayerSingleton<MessagesApp>.Instance == (Object)null || (Object)(object)PlayerSingleton<DealerManagementApp>.Instance == (Object)null || (Object)(object)NetworkSingleton<NPCManager>.Instance == (Object)null || (Object)(object)NetworkSingleton<MoneyManager>.Instance == (Object)null || (Object)(object)NetworkSingleton<LevelManager>.Instance == (Object)null || (Object)(object)PlayerSingleton<PlayerInventory>.Instance == (Object)null) && elapsed < timeout)
		{
			elapsed += 0.1f;
			yield return (object)new WaitForSeconds(0.1f);
		}
		if ((double)elapsed >= (double)timeout)
		{
			MelonLogger.Error("Timed out waiting for required systems");
		}
	}

	public void CreateCustomUI()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Expected O, but got Unknown
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("CDXX_CustomInput");
		Canvas val2 = val.AddComponent<Canvas>();
		val2.renderMode = (RenderMode)0;
		val2.sortingOrder = 9999;
		GameObject val3 = new GameObject("Panel");
		val3.transform.SetParent(val.transform);
		Image val4 = val3.AddComponent<Image>();
		((Graphic)val4).color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
		val4.type = (Type)1;
		_panelRt = val3.GetComponent<RectTransform>();
		_panelRt.anchorMin = new Vector2(0f, 0.5f);
		_panelRt.anchorMax = new Vector2(0f, 0.5f);
		_panelRt.pivot = new Vector2(0f, 0.5f);
		_panelRt.anchoredPosition = new Vector2(300f, 0f);
		_panelRt.sizeDelta = new Vector2(200f, 25f);
		GameObject val5 = new GameObject("Description");
		val5.transform.SetParent(val3.transform);
		_inputDescription = val5.AddComponent<TextMeshProUGUI>();
		((TMP_Text)_inputDescription).fontSize = 14f;
		((Graphic)_inputDescription).color = Color.white;
		((TMP_Text)_inputDescription).alignment = (TextAlignmentOptions)257;
		((TMP_Text)_inputDescription).text = "";
		((TMP_Text)_inputDescription).enableWordWrapping = false;
		RectTransform component = val5.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 0.5f);
		component.anchorMax = new Vector2(0f, 0.5f);
		component.pivot = new Vector2(0f, 0.5f);
		component.anchoredPosition = new Vector2(0f, 25f);
		component.sizeDelta = new Vector2(300f, 25f);
		GameObject val6 = new GameObject("InputDisplay");
		val6.transform.SetParent(val3.transform);
		_inputDisplay = val6.AddComponent<TextMeshProUGUI>();
		((TMP_Text)_inputDisplay).fontSize = 14f;
		((Graphic)_inputDisplay).color = Color.white;
		((TMP_Text)_inputDisplay).alignment = (TextAlignmentOptions)257;
		((TMP_Text)_inputDisplay).enableWordWrapping = true;
		RectTransform component2 = val6.GetComponent<RectTransform>();
		component2.anchorMin = new Vector2(0f, 0.5f);
		component2.anchorMax = new Vector2(0f, 0.5f);
		component2.pivot = new Vector2(0f, 0.5f);
		component2.anchoredPosition = new Vector2(0f, 0f);
		component2.sizeDelta = new Vector2(200f, 25f);
		GameObject val7 = new GameObject("Instructions");
		val7.transform.SetParent(val3.transform);
		TextMeshProUGUI val8 = val7.AddComponent<TextMeshProUGUI>();
		((TMP_Text)val8).fontSize = 14f;
		((Graphic)val8).color = Color.white;
		((TMP_Text)val8).alignment = (TextAlignmentOptions)257;
		((TMP_Text)val8).text = "ESC - Cancel\nENTER - Change";
		RectTransform component3 = val7.GetComponent<RectTransform>();
		component3.anchorMin = new Vector2(0f, 0.5f);
		component3.anchorMax = new Vector2(0f, 0.5f);
		component3.pivot = new Vector2(0f, 0.5f);
		component3.anchoredPosition = new Vector2(0f, -30f);
		component3.sizeDelta = new Vector2(300f, 25f);
		val.SetActive(false);
	}

	public void MENU_MAIN(bool openMessage)
	{
		if (openMessage)
		{
			_messageSystem.SendPlayerMessage("Return");
		}
		_messageSystem?.ShowResponses(new MessageResponse("Players", MENU_PLAYERS), new MessageResponse("NPCs", MENU_NPCS), new MessageResponse("Products", MENU_PRODUCTS), new MessageResponse("Dealers", MENU_DEALERS), new MessageResponse("Shops", MENU_SHOPS), new MessageResponse("Suppliers", MENU_SUPPLIERS), new MessageResponse("Quests", MENU_QUESTS), new MessageResponse("World Management", MENU_WORLD), new MessageResponse("Bank Management", MENU_BANK), new MessageResponse("Building Management", MENU_BUILDINGS), new MessageResponse("Misc", MENU_MISC));
		if (openMessage)
		{
			_messageSystem.SendNPCMessage("Done.");
		}
	}

	public void MENU_WORLD()
	{
		_messageSystem.SendPlayerMessage("Open World Management");
		_messageSystem.ShowResponses(new MessageResponse("Never Wanted", delegate
		{
			_messageSystem.SendPlayerMessage("Toggle Never Wanted");
			neverwanted = !neverwanted;
			Enumerator<PoliceOfficer> enumerator = PoliceOfficer.Officers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PoliceOfficer current = enumerator.Current;
				if (neverwanted)
				{
					current.PursuitBehaviour.arrestingEnabled = false;
					((Component)((Behaviour)current.PursuitBehaviour).Npc).gameObject.SetActive(false);
					current.Deactivate();
				}
				else
				{
					current.PursuitBehaviour.arrestingEnabled = true;
					((Component)((Behaviour)current.PursuitBehaviour).Npc).gameObject.SetActive(true);
					current.Activate();
				}
			}
			_messageSystem.SendPlayerMessage($"Toggled to {neverwanted}");
		}), new MessageResponse("Set Time", delegate
		{
			MelonCoroutines.Start(WaitForInput("Enter time (eg: 1800)", delegate(string input)
			{
				if (float.TryParse(input, out var result))
				{
					if ((double)result > 0.0)
					{
						_messageSystem.SendPlayerMessage($"Set to time to {result}");
						NetworkSingleton<TimeManager>.Instance.CurrentTime = (int)result;
						_messageSystem.SendNPCMessage("Done.");
					}
					else
					{
						_messageSystem.SendNPCMessage("Amount must be positive");
					}
				}
				else
				{
					_messageSystem.SendNPCMessage("Invalid amount entered");
				}
			}));
		}), new MessageResponse("Sleep Here", delegate
		{
			_messageSystem.SendPlayerMessage("Sleep here for the night.");
			if (Object.FindObjectOfType<TimeManager>().CurrentTime > 1800)
			{
				foreach (Player item in Object.FindObjectsOfType<Player>())
				{
					item.SetReadyToSleep(true);
					item.SleepStart();
				}
				_messageSystem.SendNPCMessage("Alright, i'll provide cover from my spot...");
			}
			else
			{
				_messageSystem.SendNPCMessage("You can't sleep yet...");
			}
		}), new MessageResponse("Skip Day", delegate
		{
			_messageSystem.SendPlayerMessage("Skip The Day");
			TimeManager val = Object.FindObjectOfType<TimeManager>();
			foreach (Player item2 in Object.FindObjectsOfType<Player>())
			{
				item2.SetReadyToSleep(true);
				item2.SleepStart();
			}
			try
			{
				val.CurrentTime = 700;
				Type type = ((object)val).GetType();
				PropertyInfo property = type.GetProperty("ElapsedDays");
				if (property != null && property.CanRead && property.CanWrite && property.GetValue(val) is int num)
				{
					property.SetValue(val, num + 1);
				}
			}
			catch
			{
			}
			_messageSystem.SendNPCMessage("It's done.");
		}), new MessageResponse("Drop Money", delegate
		{
			_messageSystem.SendPlayerMessage("Drop Cash");
			if (((NetworkBehaviour)Player.Local).IsHost)
			{
				foreach (ATM item3 in Object.FindObjectsOfType<ATM>())
				{
					GameObject gameObject = ((Component)Player.Local).gameObject;
					if ((Object)(object)gameObject != (Object)null)
					{
						item3.CashSpawnPoint = GameObject.Find(((Object)gameObject).name).transform;
					}
					item3.DropCash();
				}
				_messageSystem.SendNPCMessage("It's done.");
			}
			else
			{
				_messageSystem.SendNPCMessage("Only the host can do this.");
			}
		}), new MessageResponse("Vehicle Management", delegate
		{
			MENU_VEHICLES();
		}), new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		_messageSystem.SendNPCMessage("Done.");
	}

	public void MENU_VEHICLES()
	{
		_messageSystem.SendPlayerMessage("Open Vehicle Management");
		_messageSystem.ShowResponses(new MessageResponse("Repair All Vehicles", delegate
		{
			RepairAllVehicles();
		}), new MessageResponse("Refuel All Vehicles", delegate
		{
			RefuelAllVehicles();
		}), new MessageResponse("Teleport to Closest Vehicle", delegate
		{
			TeleportToClosestVehicle();
		}), new MessageResponse("Show Vehicle Status", delegate
		{
			ShowVehicleStatus();
		}), new MessageResponse("Back", delegate
		{
			MENU_WORLD();
		}));
		_messageSystem.SendNPCMessage("Done.");
	}

	private void RepairAllVehicles()
	{
		int num = 0;
		foreach (object item in FindVehicleObjects())
		{
			if (item != null && TrySetVehicleFloatProperty(item, new string[5] { "Health", "CurrentHealth", "VehicleHealth", "Condition", "Durability" }, 100f))
			{
				num++;
			}
		}
		_messageSystem.SendNPCMessage((num > 0) ? $"Repaired {num} vehicle(s)." : "No vehicles found to repair.");
	}

	private void RefuelAllVehicles()
	{
		int num = 0;
		foreach (object item in FindVehicleObjects())
		{
			if (item != null && TrySetVehicleFloatProperty(item, new string[5] { "Fuel", "CurrentFuel", "FuelAmount", "Gas", "GasAmount" }, 100f))
			{
				num++;
			}
		}
		_messageSystem.SendNPCMessage((num > 0) ? $"Refueled {num} vehicle(s)." : "No vehicles found to refuel.");
	}

	private void TeleportToClosestVehicle()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		object obj = null;
		float num = float.MaxValue;
		foreach (object item in FindVehicleObjects())
		{
			if (item == null)
			{
				continue;
			}
			Component val = (Component)((item is Component) ? item : null);
			if (!((Object)(object)val == (Object)null))
			{
				float num2 = Vector3.Distance(((Component)Player.Local).transform.position, val.transform.position);
				if (num2 < num)
				{
					num = num2;
					obj = item;
				}
			}
		}
		if (obj != null)
		{
			Component val2 = (Component)((obj is Component) ? obj : null);
			if ((Object)(object)val2 != (Object)null)
			{
				PlayerSingleton<PlayerMovement>.Instance.Teleport(val2.transform.position + new Vector3(0f, 3f, 0f), false);
				_messageSystem.SendNPCMessage("Teleported to the nearest vehicle.");
			}
			else
			{
				_messageSystem.SendNPCMessage("No vehicles found.");
			}
		}
		else
		{
			_messageSystem.SendNPCMessage("No vehicles found.");
		}
	}

	private void ShowVehicleStatus()
	{
		List<string> list = new List<string>();
		int num = 0;
		foreach (object item in FindVehicleObjects())
		{
			if (item != null)
			{
				num++;
				string name = item.GetType().Name;
				float vehicleFloatProperty = GetVehicleFloatProperty(item, new string[5] { "Health", "CurrentHealth", "VehicleHealth", "Condition", "Durability" });
				float vehicleFloatProperty2 = GetVehicleFloatProperty(item, new string[5] { "Fuel", "CurrentFuel", "FuelAmount", "Gas", "GasAmount" });
				list.Add($"{name}: Health {vehicleFloatProperty}, Fuel {vehicleFloatProperty2}");
			}
		}
		if (num == 0)
		{
			_messageSystem.SendNPCMessage("No vehicles found.");
		}
		else
		{
			_messageSystem.SendNPCMessage("Vehicle Status:\n" + string.Join("\n", list.ToArray()));
		}
	}

	private IEnumerable FindVehicleObjects()
	{
		Type type = FindVehicleType();
		if (type == null)
		{
			return Array.Empty<object>();
		}
		MethodInfo method = typeof(Object).GetMethod("FindObjectsOfType", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(Type) }, null);
		if (method == null)
		{
			return Array.Empty<object>();
		}
		object obj = method.Invoke(null, new object[1] { type });
		if (obj is IEnumerable result)
		{
			return result;
		}
		return Array.Empty<object>();
	}

	private Type FindVehicleType()
	{
		Assembly assembly = typeof(NPC).Assembly;
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!(type == null) && !type.IsAbstract && !type.IsInterface)
			{
				string name = type.Name;
				if ((name.IndexOf("Vehicle", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Car", StringComparison.OrdinalIgnoreCase) >= 0) && (typeof(Component).IsAssignableFrom(type) || typeof(Object).IsAssignableFrom(type)))
				{
					return type;
				}
			}
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly2 in assemblies)
		{
			if (assembly2 == null)
			{
				continue;
			}
			try
			{
				Type[] types2 = assembly2.GetTypes();
				foreach (Type type2 in types2)
				{
					if (!(type2 == null) && !type2.IsAbstract && !type2.IsInterface)
					{
						string name2 = type2.Name;
						if ((name2.IndexOf("Vehicle", StringComparison.OrdinalIgnoreCase) >= 0 || name2.IndexOf("Car", StringComparison.OrdinalIgnoreCase) >= 0) && (typeof(Component).IsAssignableFrom(type2) || typeof(Object).IsAssignableFrom(type2)))
						{
							return type2;
						}
					}
				}
			}
			catch
			{
			}
		}
		return null;
	}

	private bool TrySetVehicleFloatProperty(object instance, string[] propertyNames, float value)
	{
		Type type = instance.GetType();
		foreach (string name in propertyNames)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanWrite)
			{
				try
				{
					property.SetValue(instance, value);
					return true;
				}
				catch
				{
				}
			}
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				try
				{
					field.SetValue(instance, value);
					return true;
				}
				catch
				{
				}
			}
		}
		return false;
	}

	private float GetVehicleFloatProperty(object instance, string[] propertyNames)
	{
		Type type = instance.GetType();
		foreach (string name in propertyNames)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanRead)
			{
				try
				{
					return Convert.ToSingle(property.GetValue(instance));
				}
				catch
				{
				}
			}
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				try
				{
					return Convert.ToSingle(field.GetValue(instance));
				}
				catch
				{
				}
			}
		}
		return 0f;
	}

	private void MENU_QUESTS()
	{
		_messageSystem.SendPlayerMessage("Open Quest Management");
		List<MessageResponse> list = new List<MessageResponse>();
		for (int i = 0; i < Quest.Quests.Count; i++)
		{
			Quest quest = Quest.Quests[i];
			list.Add(new MessageResponse(quest.GetQuestTitle(), delegate
			{
				_messageSystem.ShowResponses(new MessageResponse("Toggle Tracking", delegate
				{
					_messageSystem.SendPlayerMessage("Track " + quest.GetQuestTitle());
					quest.SetIsTracked(!quest.IsTracked);
					_messageSystem.SendNPCMessage(quest.IsTracked ? (quest.GetQuestTitle() + " is now being tracked.") : (quest.GetQuestTitle() + " is no longer being tracked."));
				}), new MessageResponse("Teleport", delegate
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0077: Unknown result type (might be due to invalid IL or missing references)
					//IL_007c: Unknown result type (might be due to invalid IL or missing references)
					_messageSystem.SendPlayerMessage("Take me to the location for " + quest.GetQuestTitle() + ".");
					Enumerator<QuestEntry> enumerator = quest.Entries.GetEnumerator();
					while (enumerator.MoveNext())
					{
						QuestEntry current = enumerator.Current;
						if ((Object)(object)current.PoILocation != (Object)null)
						{
							PlayerSingleton<PlayerMovement>.Instance.Teleport(current.PoILocation.position + new Vector3(0f, 5f, 0f), false);
						}
					}
					_messageSystem.SendNPCMessage("Teleported to " + quest.GetQuestTitle());
				}), new MessageResponse("Force Complete", delegate
				{
					_messageSystem.SendPlayerMessage("Force completion for " + quest.GetQuestTitle() + ".");
					Enumerator<QuestEntry> enumerator = quest.Entries.GetEnumerator();
					while (enumerator.MoveNext())
					{
						QuestEntry current = enumerator.Current;
						current.Complete();
					}
					quest.Complete(true);
					_messageSystem.SendNPCMessage("Done.");
				}), new MessageResponse("Back", MENU_QUESTS));
			}));
		}
		list.Add(new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		if (list.Count == 0)
		{
			_messageSystem.SendNPCMessage("No Businesses available!");
			MENU_MAIN(openMessage: false);
		}
		else
		{
			_messageSystem.ShowResponses(list.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
	}

	public void MENU_SHOPS()
	{
		_messageSystem.SendPlayerMessage("Open Shop Management");
		_messageSystem.ShowResponses(new MessageResponse("Dan", delegate
		{
			foreach (NPC item in Object.FindObjectsOfType<NPC>())
			{
				Dan component = ((Component)item).GetComponent<Dan>();
				if (item.FirstName == "Dan" && (Object)(object)component != (Object)null)
				{
					component.ShopInterface.SetIsOpen(true);
				}
			}
		}), new MessageResponse("Clothing", delegate
		{
			foreach (NPC item2 in Object.FindObjectsOfType<NPC>())
			{
				Fiona component = ((Component)item2).GetComponent<Fiona>();
				if (item2.FirstName == "Fiona" && (Object)(object)component != (Object)null)
				{
					component.ShopInterface.SetIsOpen(true);
				}
			}
		}), new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		_messageSystem.SendNPCMessage("Done.");
	}

	private void MENU_EMPLOYEES(Property property)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		List<MessageResponse> list = new List<MessageResponse>();
		Enumerator<Employee> enumerator = property.Employees.GetEnumerator();
		DialogueContainer reason = default(DialogueContainer);
		while (enumerator.MoveNext())
		{
			Employee current = enumerator.Current;
			Employee npc = current;
			list.Add(new MessageResponse($"{(npc.GetWorkIssue(ref reason) ? "[!]" : "")} {((NPC)npc).FirstName} ({npc.EmployeeType})", delegate
			{
				List<MessageResponse> list2 = new List<MessageResponse>
				{
					new MessageResponse("Teleport", delegate
					{
						//IL_0037: Unknown result type (might be due to invalid IL or missing references)
						_messageSystem.SendPlayerMessage("Take me to " + ((NPC)npc).FirstName);
						PlayerSingleton<PlayerMovement>.Instance.Teleport(((Component)npc).transform.position, false);
						_messageSystem.SendNPCMessage("Teleported to " + ((NPC)npc).FirstName);
					}),
					new MessageResponse("Open Bed Case", delegate
					{
						_messageSystem.SendPlayerMessage("Open " + ((NPC)npc).FirstName + "'s bed case.");
						_messageSystem.SendNPCMessage("Bed case access is not available in this build.");
					})
				};
				if (npc.GetWorkIssue(ref reason))
				{
					list2.Add(new MessageResponse("Get Work Issues", delegate
					{
						_messageSystem.SendPlayerMessage("Get " + ((NPC)npc).FirstName + "'s problems.");
						for (int i = 0; npc.WorkIssues.Count > i; i++)
						{
							_messageSystem.SendNPCMessage($"Problem #{i + 1}\n{npc.WorkIssues[i].Reason}\n{npc.WorkIssues[i].Fix}");
						}
					}));
				}
				list2.Add(new MessageResponse("Increase Daily Wage", delegate
				{
					_messageSystem.SendPlayerMessage("Increase " + ((NPC)npc).FirstName + "'s Wage.");
					Employee obj = npc;
					obj.DailyWage += 50f;
					_messageSystem.SendNPCMessage($"Increased {((NPC)npc).FirstName}'s Wage to {npc.DailyWage}.");
				}));
				list2.Add(new MessageResponse("Decrease Daily Wage", delegate
				{
					_messageSystem.SendPlayerMessage("Decrease " + ((NPC)npc).FirstName + "'s Wage.");
					Employee obj = npc;
					obj.DailyWage -= 50f;
					_messageSystem.SendNPCMessage($"Decreased {((NPC)npc).FirstName}'s Wage to {npc.DailyWage}.");
				}));
				list2.Add(new MessageResponse("Fire Employee", delegate
				{
					_messageSystem.SendPlayerMessage("Fire " + ((NPC)npc).FirstName + ".");
					npc.Fire();
					_messageSystem.SendNPCMessage("Fired " + ((NPC)npc).FirstName + ".");
				}));
				list2.Add(new MessageResponse("Back", MENU_BUILDINGS_PROPERTIES));
				_messageSystem.ShowResponses(list2.ToArray());
			}));
		}
		list.Add(new MessageResponse("Back", delegate
		{
			MENU_BUILDINGS_PROPERTIES();
		}));
		if (list.Count == 0)
		{
			_messageSystem.SendNPCMessage("No Dealers available!");
			MENU_BUILDINGS_PROPERTIES();
		}
		else
		{
			_messageSystem.ShowResponses(list.ToArray());
		}
	}

	private void MENU_SUPPLIERS()
	{
		_messageSystem.SendPlayerMessage("Open Supplier Management");
		List<MessageResponse> list = new List<MessageResponse>();
		int num = 0;
		foreach (NPC item in Object.FindObjectsOfType<NPC>())
		{
			Supplier component = ((Component)item).GetComponent<Supplier>();
			if ((Object)(object)component != (Object)null)
			{
				num++;
				Supplier _npc = component;
				list.Add(new MessageResponse(item.FirstName, delegate
				{
					MENU_SUPPLIER(_npc);
				}));
			}
		}
		list.Add(new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		if (num > 0)
		{
			_messageSystem.ShowResponses(list.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
		else
		{
			_messageSystem.SendNPCMessage("I can't find any suppliers.");
			MENU_MAIN(openMessage: false);
		}
	}

	public void MENU_SUPPLIER(Supplier supplier)
	{
		_messageSystem.SendPlayerMessage("Open " + ((NPC)supplier).FirstName + ".");
		List<MessageResponse> list = new List<MessageResponse>();
		if (!((NPC)supplier).RelationData.Unlocked)
		{
			list.Add(new MessageResponse("Unlock", delegate
			{
				_messageSystem.SendPlayerMessage("Force unlock " + ((NPC)supplier).FirstName);
				((NPC)supplier).RelationData.Unlock((EUnlockType)1, true);
				supplier.SetUnlocked();
				_messageSystem.SendNPCMessage("Forced " + ((NPC)supplier).FirstName + "'s unlock status.");
			}));
		}
		list.Add(new MessageResponse("Teleport", delegate
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			_messageSystem.SendPlayerMessage("Take me to " + ((NPC)supplier).FirstName);
			PlayerSingleton<PlayerMovement>.Instance.Teleport(((Component)supplier).transform.position, false);
			_messageSystem.SendNPCMessage("Teleported to " + ((NPC)supplier).FirstName);
		}));
		list.Add(new MessageResponse("Create Order", delegate
		{
			_messageSystem.SendPlayerMessage("Create order with " + ((NPC)supplier).FirstName);
			List<Listing> val = new List<Listing>();
			foreach (Listing item in (Il2CppArrayBase<Listing>)(object)supplier.OnlineShopItems)
			{
				val.Add(item);
			}
			Action<List<CartEntry>, float> action = delegate(List<CartEntry> cartEntries, float price)
			{
				supplier.DeaddropConfirmed(cartEntries, price);
			};
			Action<List<CartEntry>, float> val2 = DelegateSupport.ConvertDelegate<Action<List<CartEntry>, float>>((Delegate)action);
			PlayerSingleton<MessagesApp>.Instance.PhoneShopInterface.Open("Request Dead Drop", "Purchase items from " + ((NPC)supplier).FirstName + " using CDXX!", ((NPC)supplier).MSGConversation, val, 100000f, supplier.debt, val2);
			_messageSystem.SendNPCMessage("Created order with " + ((NPC)supplier).FirstName);
		}));
		list.Add(new MessageResponse("Back", MENU_SUPPLIERS));
		_messageSystem.ShowResponses(list.ToArray());
		_messageSystem.SendNPCMessage("Done.");
	}

	private void MENU_DEALERS()
	{
		_messageSystem.SendPlayerMessage("Open Dealers Management");
		List<MessageResponse> list = new List<MessageResponse>();
		int num = 0;
		foreach (NPC item in Object.FindObjectsOfType<NPC>())
		{
			Dealer component = ((Component)item).GetComponent<Dealer>();
			if ((Object)(object)component != (Object)null)
			{
				num++;
				Dealer _npc = component;
				list.Add(new MessageResponse(item.FirstName, delegate
				{
					MENU_DEALER(_npc);
				}));
			}
		}
		list.Add(new MessageResponse("Collect All Cash", delegate
		{
			foreach (NPC item2 in Object.FindObjectsOfType<NPC>())
			{
				Dealer component2 = ((Component)item2).GetComponent<Dealer>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.CollectCash();
				}
			}
		}));
		list.Add(new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		if (num > 0)
		{
			_messageSystem.ShowResponses(list.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
		else
		{
			_messageSystem.SendNPCMessage("I can't find any dealers.");
			MENU_MAIN(openMessage: false);
		}
	}

	public void MENU_DEALER(Dealer dealer)
	{
		_messageSystem.SendPlayerMessage("Open " + ((NPC)dealer).FirstName + ".");
		List<MessageResponse> list = new List<MessageResponse>();
		if (!dealer.IsRecruited)
		{
			list.Add(new MessageResponse("Unlock", delegate
			{
				_messageSystem.SendPlayerMessage("Force unlock " + ((NPC)dealer).FirstName);
				((NPC)dealer).RelationData.Unlock((EUnlockType)1, true);
				dealer.SetIsRecruited((NetworkConnection)null);
				_messageSystem.SendNPCMessage("Forced " + ((NPC)dealer).FirstName + "'s unlock status.");
			}));
		}
		list.Add(new MessageResponse("Customers", delegate
		{
			MENU_CUSTOMERS(dealer);
		}));
		list.Add(new MessageResponse("Teleport", delegate
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			_messageSystem.SendPlayerMessage("Take me to " + ((NPC)dealer).FirstName);
			PlayerSingleton<PlayerMovement>.Instance.Teleport(((Component)dealer).transform.position, false);
			_messageSystem.SendNPCMessage("Teleported to " + ((NPC)dealer).FirstName);
		}));
		list.Add(new MessageResponse("Teleport to Home", delegate
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			_messageSystem.SendPlayerMessage("Take me to " + ((NPC)dealer).FirstName + "'s home.");
			PlayerSingleton<PlayerMovement>.Instance.Teleport(((Component)dealer.Home).transform.position, false);
			_messageSystem.SendNPCMessage("Teleported to " + ((NPC)dealer).FirstName + "'s home.");
		}));
		list.Add(new MessageResponse("Open Inventory", delegate
		{
			_messageSystem.SendPlayerMessage("Trade with " + ((NPC)dealer).FirstName);
			((Component)Singleton<GameplayMenu>.Instance).gameObject.SetActive(false);
			dealer.TradeItems();
			_messageSystem.SendNPCMessage("Trading with " + ((NPC)dealer).FirstName);
		}));
		list.Add(new MessageResponse("Set Cut", delegate
		{
			MelonCoroutines.Start(WaitForInput("Set Cut Amount (eg: 0)", delegate(string input)
			{
				if (float.TryParse(input, out var result))
				{
					if ((double)result > 0.0)
					{
						_messageSystem.SendPlayerMessage($"Set {((NPC)dealer).FirstName}'s cut to {result}%.");
						dealer.Cut = result / 100f;
						_messageSystem.SendNPCMessage("Done.");
					}
					else
					{
						_messageSystem.SendNPCMessage("Amount must be positive");
					}
				}
				else
				{
					_messageSystem.SendNPCMessage("Invalid amount entered");
				}
			}));
		}));
		list.Add(new MessageResponse("Collect Cash", delegate
		{
			_messageSystem.SendPlayerMessage("Force collect " + ((NPC)dealer).FirstName + "'s cash.");
			dealer.CollectCash();
			_messageSystem.SendNPCMessage("Done.");
		}));
		list.Add(new MessageResponse("Back", MENU_DEALERS));
		_messageSystem.ShowResponses(list.ToArray());
		_messageSystem.SendNPCMessage("Done.");
	}

	public void MENU_CUSTOMERS(Dealer dealer)
	{
		_messageSystem.SendPlayerMessage("Open " + ((NPC)dealer).FirstName + "'s customers.");
		List<MessageResponse> responses = new List<MessageResponse>();
		List<Customer> customer = dealer.AssignedCustomers;
		int pageSize = 6;
		int totalPages = (int)Math.Ceiling((double)customer.Count / (double)pageSize);
		int currentPage = 0;
		GenerateCustomerPage(currentPage);
		if (dealer.AssignedCustomers.Count == 0)
		{
			_messageSystem.SendNPCMessage("No Customers available!");
			MENU_DEALER(dealer);
		}
		else
		{
			_messageSystem.ShowResponses(responses.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
		void GenerateCustomerPage(int page)
		{
			responses = new List<MessageResponse>();
			int num = page * pageSize;
			int num2 = Math.Min(num + pageSize, customer.Count);
			for (int i = num; i < num2; i++)
			{
				Customer _customer = customer[i];
				responses.Add(new MessageResponse(_customer.NPC.FirstName, delegate
				{
					MENU_CUSTOMER(_customer, dealer);
				}));
			}
			if (page < totalPages - 1)
			{
				responses.Add(new MessageResponse(">>", delegate
				{
					int num3 = currentPage + 1;
					currentPage = num3;
					GenerateCustomerPage(currentPage);
				}));
			}
			if (page > 0)
			{
				responses.Add(new MessageResponse("<<", delegate
				{
					int num3 = currentPage - 1;
					currentPage = num3;
					GenerateCustomerPage(currentPage);
				}));
			}
			else
			{
				responses.Add(new MessageResponse("Back", delegate
				{
					MENU_MAIN(openMessage: true);
				}));
			}
			_messageSystem.ShowResponses(responses.ToArray());
		}
	}

	public void MENU_CUSTOMER(Customer customer, Dealer dealer)
	{
		_messageSystem.SendPlayerMessage("Open " + customer.NPC.FirstName);
		_messageSystem.ShowResponses(new List<MessageResponse>
		{
			new MessageResponse("Teleport", delegate
			{
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				_messageSystem.SendPlayerMessage("Take me to " + customer.NPC.FirstName);
				PlayerSingleton<PlayerMovement>.Instance.Teleport(((Component)customer).transform.position, false);
				_messageSystem.SendNPCMessage("Teleported to " + customer.NPC.FirstName);
			}),
			new MessageResponse("Create Deal", delegate
			{
				_messageSystem.SendPlayerMessage("Create deal with " + customer.NPC.FirstName);
				if (customer.IsAwaitingDelivery)
				{
					_messageSystem.SendNPCMessage("They're already waiting for something.");
				}
				else
				{
					try
					{
						Type type = ((object)customer).GetType();
						MethodInfo method = type.GetMethod("CheckContractGeneration");
						MethodInfo[] array = Array.FindAll(type.GetMethods(), (MethodInfo m) => m.Name == "OfferContract");
						object obj = null;
						if (method != null)
						{
							obj = method.Invoke(customer, new object[1] { true });
						}
						bool flag = false;
						MethodInfo[] array2 = array;
						foreach (MethodInfo methodInfo in array2)
						{
							ParameterInfo[] parameters = methodInfo.GetParameters();
							if (parameters.Length == 1)
							{
								Type parameterType = parameters[0].ParameterType;
								try
								{
									if (obj != null && parameterType.IsInstanceOfType(obj))
									{
										methodInfo.Invoke(customer, new object[1] { obj });
										flag = true;
									}
									else if (!parameterType.IsValueType)
									{
										methodInfo.Invoke(customer, new object[1]);
										flag = true;
									}
									else
									{
										object obj2 = Activator.CreateInstance(parameterType);
										methodInfo.Invoke(customer, new object[1] { obj2 });
										flag = true;
									}
								}
								catch
								{
									continue;
								}
								break;
							}
						}
						if (!flag)
						{
							MethodInfo method2 = type.GetMethod("OfferContract", Type.EmptyTypes);
							if (method2 != null)
							{
								method2.Invoke(customer, null);
							}
						}
					}
					catch
					{
					}
					_messageSystem.SendNPCMessage("Created deal with " + customer.NPC.FirstName);
				}
			}),
			new MessageResponse("Back", delegate
			{
				MENU_CUSTOMERS(dealer);
			})
		}.ToArray());
		_messageSystem.SendNPCMessage("Done");
	}

	public void MENU_PLAYERS()
	{
		_messageSystem.SendPlayerMessage("Open Players Management");
		List<MessageResponse> list = new List<MessageResponse>();
		Player local = Player.Local;
		foreach (Player item in Object.FindObjectsOfType<Player>())
		{
			Player player = item;
			list.Add(new MessageResponse(player.PlayerName, delegate
			{
				MENU_PLAYER(player);
			}));
		}
		list.Add(new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		if (list.Count == 0)
		{
			_messageSystem.SendNPCMessage("No players available!");
			MENU_MAIN(openMessage: false);
		}
		else
		{
			_messageSystem.ShowResponses(list.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
	}

	public void MENU_PLAYER(Player player)
	{
		_messageSystem.SendPlayerMessage("Open " + player.PlayerName + "'s info");
		List<MessageResponse> list = new List<MessageResponse>();
		if ((Object)(object)player == (Object)(object)Player.Local)
		{
			list.Add(new MessageResponse("Give Cash", delegate
			{
				MelonCoroutines.Start(WaitForInput("Set cash to give (eg: 100)", delegate(string input)
				{
					if (float.TryParse(input, out var result))
					{
						if ((double)result > 0.0)
						{
							_messageSystem.SendPlayerMessage($"Add {result} to my balance");
							NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result, true, false);
						}
						else
						{
							_messageSystem.SendPlayerMessage($"Subtract {0f - result} to my balance");
							NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result, true, false);
						}
						_messageSystem.SendNPCMessage($"You now have ${NetworkSingleton<MoneyManager>.Instance.cashBalance}");
					}
					else
					{
						_messageSystem.SendNPCMessage("Invalid amount entered");
					}
				}));
			}));
			list.Add(new MessageResponse("Give XP", delegate
			{
				MelonCoroutines.Start(WaitForInput("Set XP to give (eg: 100)", delegate(string input)
				{
					//IL_010f: Unknown result type (might be due to invalid IL or missing references)
					if (float.TryParse(input, out var result))
					{
						if ((double)result > 0.0)
						{
							_messageSystem.SendPlayerMessage($"Add {result} XP");
						}
						else
						{
							_messageSystem.SendPlayerMessage($"Subtract {0f - result} XP");
							NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result, true, false);
						}
						NetworkSingleton<LevelManager>.Instance.AddXP((int)result);
					}
					else
					{
						_messageSystem.SendNPCMessage("Invalid amount entered");
					}
					_messageSystem.SendNPCMessage($"XP has been given. You're currently {NetworkSingleton<LevelManager>.Instance.Rank} {NetworkSingleton<LevelManager>.Instance.Tier} ({NetworkSingleton<LevelManager>.Instance.XP}/{NetworkSingleton<LevelManager>.Instance.XPToNextTier})");
				}));
			}));
			list.Add(new MessageResponse("Set Jump Power", delegate
			{
				MelonCoroutines.Start(WaitForInput("Set Jump Power (eg: 5)", delegate(string input)
				{
					if (float.TryParse(input, out var result))
					{
						if ((double)result > 0.0)
						{
							_messageSystem.SendPlayerMessage($"Set Jump Speed to {result}");
							PlayerMovement.JumpMultiplier = result;
							_messageSystem.SendNPCMessage("Done.");
						}
						else
						{
							_messageSystem.SendNPCMessage("Amount must be positive");
						}
					}
					else
					{
						_messageSystem.SendNPCMessage("Invalid amount entered");
					}
				}));
			}));
			list.Add(new MessageResponse("Set Walk Speed", delegate
			{
				MelonCoroutines.Start(WaitForInput("Set Walk Speed (eg: 5)", delegate(string input)
				{
					if (float.TryParse(input, out var result))
					{
						if ((double)result > 0.0)
						{
							_messageSystem.SendPlayerMessage($"Set Walk Speed to {result}");
							PlayerMovement.StaticMoveSpeedMultiplier = result;
							_messageSystem.SendNPCMessage("Done.");
						}
						else
						{
							_messageSystem.SendNPCMessage("Amount must be positive");
						}
					}
					else
					{
						_messageSystem.SendNPCMessage("Invalid amount entered");
					}
				}));
			}));
		}
		if (((NetworkBehaviour)player).IsNetworked && ((NetworkBehaviour)Player.Local).IsHost && (Object)(object)Player.Local != (Object)(object)player)
		{
			list.Add(new MessageResponse("Kick", delegate
			{
				string playerName = player.PlayerName;
				_messageSystem.SendPlayerMessage("Kick " + playerName);
				((NetworkBehaviour)player).ServerManager.Kick(player.Connection, (KickReason)2, (LoggingType)3, "Kicked by " + Player.Local.PlayerName + " using CDXX");
				_messageSystem.SendNPCMessage(playerName + " has been kicked!");
			}));
		}
		if (((NetworkBehaviour)player).IsNetworked && ((NetworkBehaviour)Player.Local).IsHost)
		{
			list.Add(new MessageResponse("Airstrike", delegate
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				ExplosionData val = default(ExplosionData);
				((ExplosionData)(ref val))._002Ector(1f, 1000f, 10f, true, (EExplosionType)0);
				NetworkSingleton<CombatManager>.Instance.CreateExplosion(((Component)player).transform.position, val);
			}));
		}
		list.Add(new MessageResponse("Toggle Map Marker", delegate
		{
			_messageSystem.SendPlayerMessage("Toggle " + player.PlayerName + "'s map marker");
			((Component)player.PoI).gameObject.SetActive(!((Component)player.PoI).gameObject.active);
			_messageSystem.SendNPCMessage($"Toggled {player.PlayerName}'s marker to {((Component)player.PoI).gameObject.active}!");
		}));
		list.Add(new MessageResponse("Inventory Management", delegate
		{
			MENU_INVENTORY(player);
		}));
		list.Add(new MessageResponse("Teleport", delegate
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			_messageSystem.SendPlayerMessage("Take me to " + player.PlayerName);
			PlayerSingleton<PlayerMovement>.Instance.Teleport(((Component)player).transform.position, false);
			_messageSystem.SendNPCMessage("Teleported to " + player.PlayerName);
		}));
		list.Add(new MessageResponse("Back", delegate
		{
			MENU_PLAYERS();
		}));
		_messageSystem.ShowResponses(list.ToArray());
		_messageSystem.SendNPCMessage("Done.");
	}

	public void MENU_BUILDINGS()
	{
		_messageSystem.SendPlayerMessage("Open Building Management");
		_messageSystem.ShowResponses(new MessageResponse("Properties", MENU_BUILDINGS_PROPERTIES), new MessageResponse("Business", MENU_BUILDINGS_BUSINESSES), new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		_messageSystem.SendNPCMessage("Done.");
	}

	private void MENU_BUILDINGS_BUSINESSES()
	{
		_messageSystem.SendPlayerMessage("Open Businesses");
		List<MessageResponse> list = new List<MessageResponse>();
		for (int i = 0; i < Business.Businesses.Count; i++)
		{
			Business currentProperty = Business.Businesses[i];
			list.Add(new MessageResponse(((Property)currentProperty).PropertyName, delegate
			{
				MENU_BUSINESS(currentProperty);
			}));
		}
		list.Add(new MessageResponse("Back", MENU_BUILDINGS));
		if (list.Count == 0)
		{
			_messageSystem.SendNPCMessage("No Businesses available!");
			MENU_MAIN(openMessage: false);
		}
		else
		{
			_messageSystem.ShowResponses(list.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
	}

	public void MENU_BUSINESS(Business business)
	{
		List<MessageResponse> list = new List<MessageResponse>();
		if (!((Property)business).IsOwned)
		{
			list.Add(new MessageResponse("Unlock", delegate
			{
				((Property)business).SetOwned();
			}));
		}
		list.Add(new MessageResponse("Teleport", delegate
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			_messageSystem.SendPlayerMessage("Take me to the " + ((Property)business).PropertyName);
			PlayerSingleton<PlayerMovement>.Instance.Teleport(((Property)business).SpawnPoint.position, false);
			_messageSystem.SendNPCMessage("Teleported to " + ((Property)business).PropertyName);
		}));
		int num = 0;
		Enumerator<Employee> enumerator = ((Property)business).Employees.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Employee current = enumerator.Current;
			num++;
		}
		if (num > 0)
		{
			list.Add(new MessageResponse("Employees", delegate
			{
				MENU_EMPLOYEES((Property)(object)business);
			}));
		}
		list.Add(new MessageResponse("Back", MENU_BUILDINGS_PROPERTIES));
		_messageSystem.ShowResponses(list.ToArray());
	}

	private void MENU_BUILDINGS_PROPERTIES()
	{
		_messageSystem.SendPlayerMessage("Open Properties");
		List<MessageResponse> list = new List<MessageResponse>();
		for (int i = 0; i < Property.Properties.Count; i++)
		{
			Property currentProperty = Property.Properties[i];
			list.Add(new MessageResponse(currentProperty.PropertyName, delegate
			{
				MENU_PROPERTY(currentProperty);
			}));
		}
		list.Add(new MessageResponse("Back", MENU_BUILDINGS));
		if (list.Count == 0)
		{
			_messageSystem.SendNPCMessage("No Businesses available!");
			MENU_MAIN(openMessage: false);
		}
		else
		{
			_messageSystem.ShowResponses(list.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
	}

	public void MENU_PROPERTY(Property property)
	{
		List<MessageResponse> list = new List<MessageResponse>();
		if (!property.IsOwned)
		{
			list.Add(new MessageResponse("Unlock", delegate
			{
				property.SetOwned();
			}));
		}
		int num = 0;
		foreach (TrashItem item in Object.FindObjectsOfType<TrashItem>())
		{
			if ((Object)(object)item.CurrentProperty == (Object)(object)property)
			{
				num++;
			}
		}
		if (num > 0)
		{
			list.Add(new MessageResponse("Clean Trash", delegate
			{
				foreach (TrashItem item2 in Object.FindObjectsOfType<TrashItem>())
				{
					if ((Object)(object)item2.CurrentProperty == (Object)(object)property)
					{
						item2.DestroyTrash();
					}
				}
			}));
		}
		list.Add(new MessageResponse("Teleport", delegate
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			_messageSystem.SendPlayerMessage("Take me to the " + property.PropertyName);
			PlayerSingleton<PlayerMovement>.Instance.Teleport(property.SpawnPoint.position, false);
			_messageSystem.SendNPCMessage("Teleported to " + property.PropertyName);
		}));
		int num2 = 0;
		Enumerator<Employee> enumerator2 = property.Employees.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			Employee current2 = enumerator2.Current;
			num2++;
		}
		if (num2 > 0)
		{
			list.Add(new MessageResponse("Employees", delegate
			{
				MENU_EMPLOYEES(property);
			}));
		}
		list.Add(new MessageResponse("Back", MENU_BUILDINGS_PROPERTIES));
		_messageSystem.ShowResponses(list.ToArray());
	}

	public void MENU_BANK()
	{
		_messageSystem.SendPlayerMessage("Open Bank Management");
		_messageSystem.ShowResponses(new MessageResponse("Deposit", delegate
		{
			ShowBankAmountInput(Enums.BankType.Deposit);
		}), new MessageResponse("Withdraw", delegate
		{
			ShowBankAmountInput(Enums.BankType.Withdraw);
		}), new MessageResponse("Check Balance", delegate
		{
			_messageSystem.SendPlayerMessage("Check Balance");
			_messageSystem.SendNPCMessage($"Current Balance:\nCash: ${NetworkSingleton<MoneyManager>.Instance.cashBalance}\nBank: ${NetworkSingleton<MoneyManager>.Instance.onlineBalance}");
			MENU_BANK();
		}), new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		_messageSystem.SendNPCMessage("Done.");
	}

	public void MENU_NPCS()
	{
		_messageSystem.SendPlayerMessage("Open NPC Management");
		List<MessageResponse> responses = new List<MessageResponse>();
		List<NPC> npcs = new List<NPC>();
		foreach (NPC item in Object.FindObjectsOfType<NPC>())
		{
			if ((Object)(object)((Component)item).GetComponent<Customer>() != (Object)null)
			{
				npcs.Add(item);
			}
		}
		int pageSize = 6;
		int totalPages = (int)Math.Ceiling((double)npcs.Count / (double)pageSize);
		int currentPage = 0;
		GenerateNPCPage(currentPage);
		if (responses.Count == 0)
		{
			_messageSystem.SendNPCMessage("No NPCs available!");
			MENU_MAIN(openMessage: false);
		}
		else
		{
			_messageSystem.ShowResponses(responses.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
		void GenerateNPCPage(int page)
		{
			responses = new List<MessageResponse>();
			int num = page * pageSize;
			int num2 = Math.Min(num + pageSize, npcs.Count);
			for (int i = num; i < num2; i++)
			{
				NPC npc = npcs[i];
				responses.Add(new MessageResponse(npc.FirstName, delegate
				{
					ShowNPCOptions(npc);
				}));
			}
			if (page < totalPages - 1)
			{
				responses.Add(new MessageResponse(">>", delegate
				{
					int num3 = currentPage + 1;
					currentPage = num3;
					GenerateNPCPage(currentPage);
				}));
			}
			if (page > 0)
			{
				responses.Add(new MessageResponse("<<", delegate
				{
					int num3 = currentPage - 1;
					currentPage = num3;
					GenerateNPCPage(currentPage);
				}));
			}
			else
			{
				responses.Add(new MessageResponse("Back", delegate
				{
					MENU_MAIN(openMessage: true);
				}));
			}
			_messageSystem.ShowResponses(responses.ToArray());
		}
	}

	public void MENU_PRODUCTS()
	{
		if (!isChoosing)
		{
			_messageSystem.SendPlayerMessage("Open Products Management");
		}
		List<MessageResponse> responses = new List<MessageResponse>();
		List<ProductDefinition> products = NetworkSingleton<ProductManager>.Instance.AllProducts;
		int pageSize = 6;
		int totalPages = (int)Math.Ceiling((double)products.Count / (double)pageSize);
		int currentPage = 0;
		GenerateProductPage(currentPage);
		if (responses.Count == 0)
		{
			_messageSystem.SendNPCMessage("No Products available!");
			MENU_MAIN(openMessage: false);
			return;
		}
		_messageSystem.ShowResponses(responses.ToArray());
		if (!isChoosing)
		{
			_messageSystem.SendNPCMessage("Done.");
		}
		void GenerateProductPage(int page)
		{
			responses = new List<MessageResponse>();
			int num = page * pageSize;
			int num2 = Math.Min(num + pageSize, products.Count);
			for (int i = num; i < num2; i++)
			{
				ProductDefinition product = products[i];
				responses.Add(new MessageResponse(((BaseItemDefinition)product).Name, delegate
				{
					if (!isChoosing)
					{
						ShowProductOptions(product);
					}
					else
					{
						_messageSystem.SendPlayerMessage(((BaseItemDefinition)product).Name);
						ShowProductOptions(chosenProduct);
					}
				}));
			}
			if (page < totalPages - 1)
			{
				responses.Add(new MessageResponse(">>", delegate
				{
					int num3 = currentPage + 1;
					currentPage = num3;
					GenerateProductPage(currentPage);
				}));
			}
			if (page > 0)
			{
				responses.Add(new MessageResponse("<<", delegate
				{
					int num3 = currentPage - 1;
					currentPage = num3;
					GenerateProductPage(currentPage);
				}));
			}
			else
			{
				responses.Add(new MessageResponse("Create New Product", delegate
				{
					_messageSystem.SendNPCMessage("Product creation is temporarily disabled in this build.");
				}));
				responses.Add(new MessageResponse("Back", delegate
				{
					MENU_MAIN(openMessage: true);
				}));
			}
			_messageSystem.ShowResponses(responses.ToArray());
		}
	}

	private void MENU_INVENTORY(Player player)
	{
		_messageSystem.SendPlayerMessage("Open Inventory");
		List<MessageResponse> list = new List<MessageResponse>();
		foreach (ItemSlot item2 in (Il2CppArrayBase<ItemSlot>)(object)player.Inventory)
		{
			ItemSlot slots = item2;
			if (slots.ItemInstance == null)
			{
				continue;
			}
			list.Add(new MessageResponse(((BaseItemInstance)slots.ItemInstance).Name, delegate
			{
				_messageSystem.SendPlayerMessage("Edit " + ((BaseItemInstance)slots.ItemInstance).Name);
				Task.Delay(1000);
				_messageSystem.SendNPCMessage("Editing " + ((BaseItemInstance)slots.ItemInstance).Name);
				_messageSystem.ShowResponses(new MessageResponse("Duplicate", delegate
				{
					_messageSystem.SendPlayerMessage("Duplicate " + ((BaseItemInstance)slots.ItemInstance).Name);
					if (((BaseItemInstance)slots.ItemInstance).Quantity * 2 > ((BaseItemInstance)slots.ItemInstance).StackLimit)
					{
						((BaseItemInstance)slots.ItemInstance).SetQuantity(((BaseItemInstance)slots.ItemInstance).StackLimit);
						_messageSystem.SendNPCMessage("stack limit reached");
					}
					else
					{
						((BaseItemInstance)slots.ItemInstance).SetQuantity(((BaseItemInstance)slots.ItemInstance).Quantity * 2);
						_messageSystem.SendNPCMessage("call it done.");
					}
				}), new MessageResponse("Package (Baggie)", delegate
				{
					_messageSystem.SendPlayerMessage("Package " + ((BaseItemInstance)slots.ItemInstance).Name + " into a Baggie");
					ItemDefinition item = Registry.GetItem("baggie");
					PackagingDefinition packaging = (PackagingDefinition)(object)((item is PackagingDefinition) ? item : null);
					if (slots.ItemInstance is ProductItemInstance)
					{
						ItemInstance itemInstance = slots.ItemInstance;
						((ProductItemInstance)((itemInstance is ProductItemInstance) ? itemInstance : null)).SetPackaging(packaging);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(false);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(true);
						_messageSystem.SendNPCMessage("Packaged " + ((BaseItemInstance)slots.ItemInstance).Name + " into Baggie");
					}
					else
					{
						_messageSystem.SendNPCMessage("I can't package this.");
					}
				}), new MessageResponse("Package (Jar)", delegate
				{
					_messageSystem.SendPlayerMessage("Package " + ((BaseItemInstance)slots.ItemInstance).Name + " into a Jar");
					ItemDefinition item = Registry.GetItem("jar");
					PackagingDefinition packaging = (PackagingDefinition)(object)((item is PackagingDefinition) ? item : null);
					if (slots.ItemInstance is ProductItemInstance)
					{
						ItemInstance itemInstance = slots.ItemInstance;
						((ProductItemInstance)((itemInstance is ProductItemInstance) ? itemInstance : null)).SetPackaging(packaging);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(false);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(true);
						_messageSystem.SendNPCMessage("Packaged " + ((BaseItemInstance)slots.ItemInstance).Name + " into Jar");
					}
					else
					{
						_messageSystem.SendNPCMessage("I can't package this.");
					}
				}), new MessageResponse("Back", delegate
				{
					MENU_INVENTORY(player);
				}));
			}));
		}
		list.Add(new MessageResponse("Back", MENU_PLAYERS));
		if (list.Count == 0)
		{
			_messageSystem.SendNPCMessage("No items available!");
			MENU_PLAYERS();
		}
		else
		{
			_messageSystem.ShowResponses(list.ToArray());
			_messageSystem.SendNPCMessage("Done.");
		}
	}

	private void MENU_MISC()
	{
		_messageSystem.SendPlayerMessage("Open Misc Menu");
		_messageSystem.ShowResponses(new MessageResponse("Unlock Achievements", delegate
		{
			_messageSystem.SendPlayerMessage("Unlock my achievements");
			try
			{
				Type typeFromHandle = typeof(AchievementManager);
				object obj = null;
				FieldInfo field = typeFromHandle.GetField("achievements");
				if (field != null)
				{
					obj = field.GetValue(null);
				}
				else
				{
					PropertyInfo property = typeFromHandle.GetProperty("achievements");
					if (property != null)
					{
						obj = property.GetValue(null);
					}
				}
				if (obj is IEnumerable enumerable)
				{
					MethodInfo method = typeFromHandle.GetMethod("UnlockAchievement", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
					foreach (object item in enumerable)
					{
						try
						{
							method?.Invoke(null, new object[1] { item });
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
			_messageSystem.SendNPCMessage("Done, check your profile.");
		}), new MessageResponse("Back", delegate
		{
			MENU_MAIN(openMessage: true);
		}));
		_messageSystem.SendNPCMessage("Done.");
	}

	private void ShowNPCOptions(NPC npc)
	{
		List<MessageResponse> list = new List<MessageResponse>();
		_messageSystem.SendNPCMessage($"{npc.fullName}'s Stats:\n\nUnlocked: {npc.RelationData.Unlocked}\nRelationship: {npc.RelationData.RelationDelta}%");
		if (!npc.RelationData.Unlocked)
		{
			list.Add(new MessageResponse("Unlock", delegate
			{
				_messageSystem.SendPlayerMessage("Unlock " + npc.FirstName);
				npc.RelationData.Unlock((EUnlockType)1, true);
				_messageSystem.SendNPCMessage("Unlocked relationship with " + npc.FirstName);
			}));
		}
		list.Add(new MessageResponse("Teleport", delegate
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			_messageSystem.SendPlayerMessage("Teleport to " + npc.FirstName);
			PlayerSingleton<PlayerMovement>.Instance.Teleport(((Component)npc).transform.position + new Vector3(0f, 5f, 0f), false);
			_messageSystem.SendNPCMessage("Teleported to " + npc.FirstName);
		}));
		list.Add(new MessageResponse("Set as CDXX Icon", delegate
		{
			_messageSystem.SendPlayerMessage("Set " + npc.FirstName + " as CDXX Icon");
			_npcInstance.MugshotSprite = npc.MugshotSprite;
			_messageSystem.SendNPCMessage("Set " + npc.FirstName + " to CDXX Icon");
		}));
		Customer customer = ((Component)npc).GetComponent<Customer>();
		if ((Object)(object)customer != (Object)null && !customer.IsAwaitingDelivery)
		{
			list.Add(new MessageResponse("Create Deal", delegate
			{
				_messageSystem.SendPlayerMessage("Create deal with " + npc.FirstName);
				if ((Object)(object)customer == (Object)null)
				{
					_messageSystem.SendNPCMessage("I can't do that with " + npc.FirstName);
				}
				else if (customer.IsAwaitingDelivery)
				{
					_messageSystem.SendNPCMessage("They're already waiting for something.");
				}
				else
				{
					try
					{
						Type type = ((object)customer).GetType();
						MethodInfo method = type.GetMethod("CheckContractGeneration");
						MethodInfo method2 = type.GetMethod("OfferContract");
						if (method != null && method2 != null)
						{
							object obj = method.Invoke(customer, new object[1] { true });
							method2.Invoke(customer, new object[1] { obj });
						}
						else if (method2 != null)
						{
							method2.Invoke(customer, new object[1]);
						}
					}
					catch
					{
						try
						{
							customer.OfferContract((ContractInfo)null);
						}
						catch
						{
						}
					}
					_messageSystem.SendNPCMessage("Created deal with " + customer.NPC.FirstName);
				}
			}));
		}
		list.Add(new MessageResponse("Set Relationship", delegate
		{
			_messageSystem.SendPlayerMessage("Set Relationship for " + npc.FirstName);
			MelonCoroutines.Start(WaitForInput($"Enter relationship (Current: {npc.RelationData.RelationDelta}%)", delegate(string input)
			{
				if (float.TryParse(input, out var result))
				{
					if ((double)result > 0.0)
					{
						npc.RelationData.RelationDelta = result;
						npc.RelationData.SetRelationship(result, true);
						_messageSystem.SendNPCMessage($"Set relationship for {npc.FirstName} to {npc.RelationData.RelationDelta}");
					}
				}
				else
				{
					_messageSystem.SendNPCMessage("Invalid amount entered");
				}
				ShowNPCOptions(npc);
			}));
		}));
		list.Add(new MessageResponse("Back", MENU_NPCS));
		_messageSystem.ShowResponses(list.ToArray());
	}

	private IEnumerator ChangeProductStat(ProductDefinition product, string statName, Action<ProductDefinition, float> floatSetter = null, Func<ProductDefinition, float> floatGetter = null, Action<ProductDefinition, string> stringSetter = null, Func<ProductDefinition, string> stringGetter = null)
	{
		string currentValue = ((floatGetter != null) ? floatGetter(product).ToString() : stringGetter(product));
		string input = "";
		yield return WaitForInput($"Enter new {statName} (current: {currentValue})", delegate(string result)
		{
			input = result;
		});
		if (floatSetter != null && float.TryParse(input, out var newFloatValue))
		{
			floatSetter(product, newFloatValue);
			_messageSystem.SendNPCMessage($"{statName} changed to {newFloatValue}");
		}
		else if (stringSetter != null)
		{
			stringSetter(product, input);
			_messageSystem.SendNPCMessage(statName + " changed to " + input);
		}
		else
		{
			_messageSystem.SendNPCMessage("Invalid input entered");
		}
		ShowProductOptions(product);
	}

	private bool TrySetNumericProperty(object instance, string propertyName, float value)
	{
		Type type = instance.GetType();
		PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.CanWrite)
		{
			property.SetValue(instance, value);
			return true;
		}
		FieldInfo field = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null)
		{
			field.SetValue(instance, value);
			return true;
		}
		return false;
	}

	private float GetNumericProperty(object instance, string propertyName)
	{
		Type type = instance.GetType();
		PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.CanRead)
		{
			return Convert.ToSingle(property.GetValue(instance));
		}
		FieldInfo field = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null)
		{
			return Convert.ToSingle(field.GetValue(instance));
		}
		return 0f;
	}

	private bool TryInvokeMethod(object instance, string[] methodNames, object[] args)
	{
		Type type = instance.GetType();
		foreach (string text in methodNames)
		{
			MethodInfo method = type.GetMethod(text, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(method == null))
			{
				try
				{
					method.Invoke(instance, args);
					return true;
				}
				catch (Exception value)
				{
					MelonLogger.Warning($"Bank fallback method {text} failed: {value}");
				}
			}
		}
		return false;
	}

	private bool TryProcessBankTransfer(Enums.BankType transactionType, float amount)
	{
		MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		float numericProperty = GetNumericProperty(instance, "cashBalance");
		float numericProperty2 = GetNumericProperty(instance, "onlineBalance");
		if (transactionType == Enums.BankType.Deposit)
		{
			if (numericProperty < amount)
			{
				return false;
			}
			TryInvokeMethod(instance, new string[1] { "ChangeCashBalance" }, new object[3]
			{
				0f - amount,
				true,
				false
			});
			TrySetNumericProperty(instance, "cashBalance", numericProperty - amount);
			TryInvokeMethod(instance, new string[1] { "CreateOnlineTransaction" }, new object[4]
			{
				"CDXX Deposit",
				amount,
				1f,
				"Processed by " + Player.Local.PlayerName
			});
			TrySetNumericProperty(instance, "onlineBalance", numericProperty2 + amount);
			return true;
		}
		if (numericProperty2 < amount)
		{
			return false;
		}
		TryInvokeMethod(instance, new string[1] { "ChangeCashBalance" }, new object[3] { amount, true, false });
		TrySetNumericProperty(instance, "cashBalance", numericProperty + amount);
		TryInvokeMethod(instance, new string[1] { "CreateOnlineTransaction" }, new object[4]
		{
			"CDXX Withdraw",
			0f - amount,
			1f,
			"Processed by " + Player.Local.PlayerName
		});
		TrySetNumericProperty(instance, "onlineBalance", numericProperty2 - amount);
		return true;
	}

	private void ShowBankAmountInput(Enums.BankType transactionType)
	{
		string text = ((transactionType == Enums.BankType.Deposit) ? "Deposit" : "Withdraw");
		_messageSystem.SendPlayerMessage(text + " Amount");
		MelonCoroutines.Start(WaitForInput("Enter amount to " + text, delegate(string input)
		{
			if (float.TryParse(input, out var result))
			{
				if ((double)result > 0.0)
				{
					try
					{
						if (transactionType == Enums.BankType.Deposit)
						{
							if (TryProcessBankTransfer(transactionType, result))
							{
								_messageSystem.SendNPCMessage($"Successfully deposited ${result}\nNew Balance:\nCash: ${GetNumericProperty(NetworkSingleton<MoneyManager>.Instance, "cashBalance")}\nBank: ${GetNumericProperty(NetworkSingleton<MoneyManager>.Instance, "onlineBalance")}");
							}
							else
							{
								_messageSystem.SendNPCMessage("Not enough cash to deposit");
							}
						}
						else if (TryProcessBankTransfer(transactionType, result))
						{
							_messageSystem.SendNPCMessage($"Successfully withdrew ${result}\nNew Balance:\nCash: ${GetNumericProperty(NetworkSingleton<MoneyManager>.Instance, "cashBalance")}\nBank: ${GetNumericProperty(NetworkSingleton<MoneyManager>.Instance, "onlineBalance")}");
						}
						else
						{
							_messageSystem.SendNPCMessage("Not enough funds in account");
						}
					}
					catch (Exception value)
					{
						MelonLogger.Error($"Bank transaction failed: {value}");
						_messageSystem.SendNPCMessage("Transaction failed - please try again");
					}
				}
				else
				{
					_messageSystem.SendNPCMessage("Amount must be positive");
				}
			}
			else
			{
				_messageSystem.SendNPCMessage("Invalid amount entered");
			}
			MENU_BANK();
		}));
	}

	private void ShowProductOptions(ProductDefinition product)
	{
		if (!isChoosing)
		{
			string value = "N/A";
			try
			{
				Type type = ((object)product).GetType();
				PropertyInfo property = type.GetProperty("EffectsDuration");
				if (property != null)
				{
					object value2 = property.GetValue(product);
					value = ((value2 != null) ? value2.ToString() : "0");
				}
			}
			catch
			{
			}
			_messageSystem.SendNPCMessage($"{((BaseItemDefinition)product).Name}'s Stats:\n\nPrice: ${product.Price}\nAddictiveness: {Math.Round((double)product.GetAddictiveness() * 100.0)}%\nEffect Duration: {value} seconds.");
		}
		else
		{
			isChoosing = false;
		}
		_messageSystem.ShowResponses(new MessageResponse("Give", delegate
		{
			ItemDefinition item = Registry.GetItem(((BaseItemDefinition)product).ID);
			if ((Object)(object)item == (Object)null)
			{
				MelonLogger.Warning("Item not found in registry");
			}
			else
			{
				ItemInstance defaultInstance = item.GetDefaultInstance(1);
				if (PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(defaultInstance, 1))
				{
					PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(defaultInstance);
				}
			}
		}), new MessageResponse("Set as CDXX Icon", delegate
		{
			_messageSystem.SendPlayerMessage("Set as Icon");
			_npcInstance.MugshotSprite = ((BaseItemDefinition)product).Icon;
			_messageSystem.SendNPCMessage("Done.");
		}), new MessageResponse("Change Name", delegate
		{
			_messageSystem.SendPlayerMessage("Change name of " + ((BaseItemDefinition)product).Name);
			MelonCoroutines.Start(ChangeProductStat(product, "name", null, null, delegate(ProductDefinition p, string v)
			{
				((BaseItemDefinition)p).Name = v;
			}, (ProductDefinition p) => ((BaseItemDefinition)p).Name));
		}), new MessageResponse("Change Description", delegate
		{
			_messageSystem.SendPlayerMessage("Change description of " + ((BaseItemDefinition)product).Name);
			MelonCoroutines.Start(ChangeProductStat(product, "description", null, null, delegate(ProductDefinition p, string v)
			{
				((BaseItemDefinition)p).Description = v;
			}, (ProductDefinition p) => ((BaseItemDefinition)p).Description));
		}), new MessageResponse("Change Addictiveness", delegate
		{
			_messageSystem.SendPlayerMessage("Change Addictiveness of " + ((BaseItemDefinition)product).Name);
			MelonCoroutines.Start(ChangeProductStat(product, "addictiveness", delegate(ProductDefinition p, float v)
			{
				p.BaseAddictiveness = v;
			}, (ProductDefinition p) => p.GetAddictiveness()));
		}), new MessageResponse("Change Effect Duration", delegate
		{
			_messageSystem.SendPlayerMessage("Change Effect Duration of " + ((BaseItemDefinition)product).Name);
			MelonCoroutines.Start(ChangeProductStat(product, "effectsduration", delegate(ProductDefinition p, float v)
			{
				try
				{
					PropertyInfo property2 = ((object)p).GetType().GetProperty("EffectsDuration");
					if (property2 != null)
					{
						property2.SetValue(p, Convert.ChangeType((int)v, property2.PropertyType));
					}
				}
				catch
				{
				}
			}, delegate(ProductDefinition p)
			{
				try
				{
					PropertyInfo property2 = ((object)p).GetType().GetProperty("EffectsDuration");
					if (property2 != null)
					{
						return Convert.ToSingle(property2.GetValue(p));
					}
				}
				catch
				{
				}
				return 0f;
			}));
		}), new MessageResponse("Back", MENU_PRODUCTS));
	}
}
