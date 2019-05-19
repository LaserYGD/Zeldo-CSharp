using System;
using System.Collections.Generic;
using Engine.Messaging;

namespace Engine.Interfaces
{
	public interface IReceiver : IDisposable
	{
		List<MessageHandle> MessageHandles { get; set; }
	}
}
