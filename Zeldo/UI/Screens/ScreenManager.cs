using System.Collections.Generic;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.UI;

namespace Zeldo.UI.Screens
{
	public class ScreenManager : IReceiver
	{
		private ScreenControls controls;
		private InventoryScreen inventoryScreen;

		public ScreenManager()
		{
			controls = new ScreenControls();
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		public void Load(Canvas canvas)
		{
			inventoryScreen = new InventoryScreen();
			inventoryScreen.IsVisible = false;
			canvas.Add(inventoryScreen);

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data);
			});
		}

		public void Unload(Canvas canvas)
		{
			canvas.Remove(inventoryScreen);

			MessageSystem.Unsubscribe(this);
		}

		private void ProcessInput(FullInputData data)
		{
			if (data.Query(controls.Inventory, InputStates.PressedThisFrame))
			{
				inventoryScreen.IsVisible = !inventoryScreen.IsVisible;
			}
		}
	}
}
