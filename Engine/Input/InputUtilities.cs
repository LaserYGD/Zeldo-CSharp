using System.Runtime.InteropServices;

namespace Engine.Input
{
	public enum LockKeys
	{
		CapsLock,
		NumLock,
		ScrollLock
	}

	public static class InputUtilities
	{
		// Key codes taken from the StackOverflow link below.
		private static int[] lockCodes =
		{
			0x14,
			0x90,
			0x91
		};

		// See https://stackoverflow.com/a/577422/7281613.
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
		public static extern short GetKeyState(int keyCode);

		public static bool IsEnabled(LockKeys key)
		{
			return ((ushort)GetKeyState(lockCodes[(int)key]) & 0xffff) != 0;
		}
	}
}
