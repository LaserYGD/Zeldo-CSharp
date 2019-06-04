using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;
using Engine.UI;

namespace Zeldo.Physics._2D
{
	public class GroundVisualizer : CanvasElement, IRenderTargetUser2D
	{
		private World2D world;
		private RenderTarget renderTarget;
		private Sprite sprite;

		public GroundVisualizer(World2D world)
		{
			this.world = world;

			renderTarget = new RenderTarget(300, 225, RenderTargetFlags.Color);
			sprite = new Sprite(renderTarget, null, Alignments.Left | Alignments.Top);
			sprite.Mods = SpriteModifiers.FlipVertical;
			UsesRenderTarget = true;
		}

		public override void Dispose()
		{
			renderTarget.Dispose();
			sprite.Dispose();
		}

		public void DrawTargets(SpriteBatch sb)
		{
			sb.ApplyTarget(renderTarget);

			foreach (var body in world.StaticBodies)
			{
				DrawBody(sb, body, Color.White);
			}

			foreach (var body in world.DynamicBodies)
			{
				DrawBody(sb, body, Color.Cyan);
			}
			
			sb.Flush();
		}

		private void DrawBody(SpriteBatch sb, RigidBody2D body, Color color)
		{
			var shape = body.Shape;

			switch (shape.ShapeType)
			{
				case ShapeTypes2D.Circle:
					sb.Draw((Circle)shape, 20, color, ZeldoConstants.GroundConversion);

					break;

				case ShapeTypes2D.Rectangle:
					sb.Draw((Rectangle)shape, color, ZeldoConstants.GroundConversion);

					break;
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sprite.Draw(sb);
		}
	}
}
