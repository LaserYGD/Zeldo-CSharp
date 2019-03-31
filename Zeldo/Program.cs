using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeldo.Items;

namespace Zeldo
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			MainGame game = new MainGame();
			game.Run();
		}
	}
}
