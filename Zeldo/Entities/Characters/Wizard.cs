using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Shapes._2D;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;
using Zeldo.Sensors;
using Zeldo.UI.Speech;

namespace Zeldo.Entities.Characters
{
	public class Wizard : Entity, IInteractive
	{
		private Circle shape;
		private Sensor sensor;
		private SpeechBox speechBox;

		private string[] lines;

		public Wizard() : base(EntityGroups.Character)
		{
			shape = new Circle(1);
			sensor = new Sensor(SensorTypes.Entity, this, shape);

			lines = new []
			{
				"You have your weapon drawn. Who are you and why did you break into my house?"
			};
		}

		public bool InteractionEnabled => true;

		public override void Initialize(Scene scene)
		{
			sensor = CreateSensor(scene, shape);
			speechBox = scene.Canvas.GetElement<SpeechBox>();

			base.Initialize(scene);
		}

		public void OnInteract(Entity entity)
		{
			speechBox.Refresh(lines[0]);
		}
	}
}
