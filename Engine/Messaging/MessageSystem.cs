using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;

namespace Engine.Messaging
{
	using ReceiverFunction = Action<object, float>;

	public static class MessageSystem
	{
		private static List<List<ReceiverFunction>> receivers;

		static MessageSystem()
		{
			receivers = new List<List<ReceiverFunction>>();
		}

		public static void Subscribe(IReceiver receiver, int messageType, ReceiverFunction function)
		{
			if (messageType >= receivers.Count)
			{
				receivers.Capacity = messageType + 1;
			}
			
			var functions = receivers[messageType];

			if (functions == null)
			{
				functions = new List<ReceiverFunction>();
				receivers[messageType] = functions;
			}

			int index = -1;

			// When a class subscribes to a message type, its callback is stored in the first open slot in the function
			// list (or appended to the end if all slots are filled)
			for (int i = 0; i < functions.Count; i++)
			{
				if (functions[i] == null)
				{
					index = i;

					break;
				}
			}

			// This means that no open slots were found, so the new function must be appended instead.
			if (index == -1)
			{
				functions.Add(function);
			}
			else
			{
				functions[index] = function;
			}
			
			// It's assumed that the same object won't subscribe to the same message type more than once. If multiple
			// callbacks are needed, a single lambda can be used to call several functions.
			receiver.MessageHandles.Add(new MessageHandle(messageType, index));
		}

		public static void Unsubscribe(IReceiver receiver, int messageType = -1)
		{
		}
	}
}
