using System;
using System.Collections;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.UI.Phone.Messages;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace CDXX;

public class CDXXMessageSystem
{
	private readonly NPC _npc;

	public MSGConversation _conversation;

	public CDXXMessageSystem(NPC npc)
	{
		_npc = npc;
		Initialize();
	}

	private void Initialize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		_conversation = new MSGConversation(_npc, _npc.fullName);
		_npc.MSGConversation = _conversation;
		_conversation.messageHistory = new List<Message>();
		_conversation.messageChainHistory = new List<MessageChain>();
		_conversation.bubbles = new List<MessageBubble>();
		_conversation.EntryVisible = true;
		_conversation.Sendables = new List<SendableMessage>();
		_npc.ConversationCategories = new List<EConversationCategory>();
		_npc.ConversationCategories.Add((EConversationCategory)0);
		_conversation.SetCategories(_npc.ConversationCategories);
	}

	public void ShowResponses(params MessageResponse[] responses)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		if (_conversation == null)
		{
			return;
		}
		_conversation.ClearResponses(false);
		List<Response> val = new List<Response>();
		foreach (MessageResponse messageResponse in responses)
		{
			MessageResponse response = messageResponse;
			Action action = delegate
			{
				response.Action?.Invoke();
				if (response.NextResponses != null)
				{
					MelonCoroutines.Start(DelayedShowResponses(0.5f, response.NextResponses));
				}
			};
			Action val2 = DelegateSupport.ConvertDelegate<Action>((Delegate)action);
			val.Add(new Response(response.Text, response.Text, val2, true));
		}
		_conversation.ShowResponses(val, 0.5f, false);
	}

	private IEnumerator DelayedShowResponses(float delay, params MessageResponse[] responses)
	{
		yield return (object)new WaitForSeconds(delay);
		ShowResponses(responses);
	}

	public void SendNPCMessage(string text)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_conversation.SendMessage(new Message(text, (ESenderType)1, true, -1), true, false);
	}

	public void SendPlayerMessage(string text)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_conversation.SendMessage(new Message(text, (ESenderType)0, true, -1), true, false);
	}
}
