using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Messaging;

namespace Engine.Graphics
{
	public class SpriteBatch : IReceiver
	{
		public SpriteBatch()
		{
			MessageSystem.Subscribe(this, CoreMessageTypes.Resize, (messageType, data) => { OnResize(); });
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void OnResize()
		{
		}

		public void Buffer()
		{
		}

		public void Flush()
		{
		}
	}
}
