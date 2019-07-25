using Engine.Core;

namespace Engine.Animation
{
	public class AnimationPlayer : Component
	{
		private Skeleton skeleton;
		private Animation animation;

		private float elapsed;

		public AnimationPlayer(Skeleton skeleton)
		{
			this.skeleton = skeleton;
		}

		public void Play(Animation animation)
		{
			this.animation = animation;

			elapsed = 0;
		}

		public override void Update(float dt)
		{
			elapsed += dt;

			var bones = skeleton.Bones;
		}
	}
}
