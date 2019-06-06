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
			const int RenderScale = 10;

			var shape = body.Shape;
			var offset = renderTarget.Dimensions / 2;

			switch (shape.ShapeType)
			{
				case ShapeTypes2D.Circle:
					Circle circle = ((Circle)shape).Clone(RenderScale);
					circle.Position += offset;

					sb.Draw(circle, 20, color);

					break;

				case ShapeTypes2D.Line:
					Line2D line = ((Line2D)shape).Clone(RenderScale);
					line.P1 += offset;
					line.P2 += offset;

					sb.Draw(line, color);

					break;

				case ShapeTypes2D.Rectangle:
					Rectangle rect = ((Rectangle)shape).Clone(RenderScale);
					rect.Position += offset;

					sb.Draw(rect, color);

					break;
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sprite.Draw(sb);
		}
	}
}
