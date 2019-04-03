using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Entities;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Zeldo.UI;

namespace Zeldo.Entities
{
	public class Player : Entity3D, IReceiver
	{
		private PlayerControls controls;

		public Player() : base(EntityTypes.Player)
		{
			controls = new PlayerControls();
			MessageHandles = new List<MessageHandle>();

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data);
			});
		}

		public PlayerHealthDisplay HealthDisplay { get; set; }
		public List<MessageHandle> MessageHandles { get; set; }

		private void ProcessInput(FullInputData data)
		{
		}
	}
}
