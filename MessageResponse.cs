using System;

namespace CDXX;

public class MessageResponse
{
	public string Text { get; }

	public Action Action { get; }

	public MessageResponse[] NextResponses { get; set; }

	public MessageResponse(string text, Action action)
	{
		Text = text;
		Action = action;
	}
}
