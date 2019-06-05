using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;
using Engine.UI;
using GlmSharp;

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
					sb.Draw((Circle)Clone((Circle)shape), 20, color);

					break;

				case ShapeTypes2D.Line:
					sb.Draw((Line2D)Clone((Line2D)shape), color);

					break;

				case ShapeTypes2D.Rectangle:
					sb.Draw((Rectangle)Clone((Rectangle)shape), color);

					break;
			}
		}

		private Shape2D Clone(Shape2D shape)
		{
			const float Scale = 12;

			Shape2D result = null;
			vec2 offset = renderTarget.Dimensions / 2;

			switch (shape.ShapeType)
			{
				case ShapeTypes2D.Circle:
					Circle circle = (Circle)shape;
					Circle newCircle = new Circle(circle.Radius * Scale);
					newCircle.Position = circle.Position * Scale;

					result = newCircle;

					break;

				case ShapeTypes2D.Line:
					Line2D line = (Line2D)shape;
					Line2D newLine = new Line2D(line.P1 * Scale, line.P2 * Scale + offset);

					result = newLine;

					break;

				case ShapeTypes2D.Rectangle:
					Rectangle rect = (Rectangle)shape;
					Rectangle newRect = new Rectangle(rect.X * Scale, rect.Y * Scale, rect.Width * Scale,
						rect.Height * Scale);
					newRect.Rotation = rect.Rotation;

					result = newRect;

					break;
			}

			result.Position += offset;

			return result;
		}

		public override void Draw(SpriteBatch sb)
		{
			sprite.Draw(sb);
		}
	}
}
