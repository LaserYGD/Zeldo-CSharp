using Zeldo.Entities.Core;
using Zeldo.Interfaces;
using Zeldo.UI.Speech;

namespace Zeldo.Entities.Characters
{
	public abstract class Character : Entity, IInteractive
	{
		private static DialogueBox dialogueBox;

		protected Character() : base(EntityGroups.Character)
		{
		}

		public virtual bool IsInteractionEnabled => true;

		public override void Initialize(Scene scene)
		{
			dialogueBox = dialogueBox ?? scene.Canvas.GetElement<DialogueBox>();

			base.Initialize(scene);
		}

		public void OnInteract(Entity entity)
		{
		}
	}
}
