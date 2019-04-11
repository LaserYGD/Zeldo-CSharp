using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
