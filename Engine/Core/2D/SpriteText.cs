using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Engine.Core._2D
{
	public class SpriteText : Component2D
	{
		private string value;

		public string Value
		{
			get => value;
			set { this.value = value; }
		}

		public override void Draw(SpriteBatch sb)
		{
		}
	}
}
