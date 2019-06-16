﻿using Engine;
using Engine.Shapes._2D;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;
using Zeldo.UI.Speech;

namespace Zeldo.Entities.Characters
{
	public abstract class Character : Entity, IInteractive
	{
		private static DialogueBox dialogueBox;

		private static readonly float InteractionRadius;

		static Character()
		{
			InteractionRadius = Properties.GetFloat("character.interaction.radius");
		}

		protected Character() : base(EntityGroups.Character)
		{
		}

		public virtual bool IsInteractionEnabled => true;

		public override void Initialize(Scene scene)
		{
			dialogueBox = dialogueBox ?? scene.Canvas.GetElement<DialogueBox>();
			CreateSensor(scene, new Circle(InteractionRadius));

			base.Initialize(scene);
		}

		public void OnInteract(Entity entity)
		{
		}
	}
}