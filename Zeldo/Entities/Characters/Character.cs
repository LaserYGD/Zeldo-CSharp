﻿using Engine;
using Newtonsoft.Json.Linq;
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
			// TODO: Pull radius (probably overridable by character).
			//InteractionRadius = Properties.GetFloat("character.interaction.radius");
		}

		protected Character() : base(EntityGroups.Character)
		{
		}

		public virtual bool IsInteractionEnabled => true;
		public bool RequiresFacing => false;

		public override void Initialize(Scene scene, JToken data)
		{
			dialogueBox = dialogueBox ?? scene.Canvas.GetElement<DialogueBox>();
			//CreateSensor(scene, new Circle(InteractionRadius));

			base.Initialize(scene, data);
		}

		public void OnInteract(Actor actor)
		{
		}
	}
}
