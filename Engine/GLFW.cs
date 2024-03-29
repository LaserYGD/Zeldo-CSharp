﻿// MIT License
// Copyright (c) 2016 - 2018 Zachary Snow
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace Engine
{
	public static class GLFW
	{
		public const int GLFW_VERSION_MAJOR = 3;
		public const int GLFW_VERSION_MINOR = 2;
		public const int GLFW_VERSION_REVISION = 1;
		public const int GLFW_TRUE = 1;
		public const int GLFW_FALSE = 0;
		public const int GLFW_RELEASE = 0;
		public const int GLFW_PRESS = 1;
		public const int GLFW_REPEAT = 2;
		public const int GLFW_KEY_UNKNOWN = -1;
		public const int GLFW_KEY_SPACE = 32;
		public const int GLFW_KEY_APOSTROPHE = 39;
		public const int GLFW_KEY_COMMA = 44;
		public const int GLFW_KEY_MINUS = 45;
		public const int GLFW_KEY_PERIOD = 46;
		public const int GLFW_KEY_SLASH = 47;
		public const int GLFW_KEY_0 = 48;
		public const int GLFW_KEY_1 = 49;
		public const int GLFW_KEY_2 = 50;
		public const int GLFW_KEY_3 = 51;
		public const int GLFW_KEY_4 = 52;
		public const int GLFW_KEY_5 = 53;
		public const int GLFW_KEY_6 = 54;
		public const int GLFW_KEY_7 = 55;
		public const int GLFW_KEY_8 = 56;
		public const int GLFW_KEY_9 = 57;
		public const int GLFW_KEY_SEMICOLON = 59;
		public const int GLFW_KEY_EQUAL = 61;
		public const int GLFW_KEY_A = 65;
		public const int GLFW_KEY_B = 66;
		public const int GLFW_KEY_C = 67;
		public const int GLFW_KEY_D = 68;
		public const int GLFW_KEY_E = 69;
		public const int GLFW_KEY_F = 70;
		public const int GLFW_KEY_G = 71;
		public const int GLFW_KEY_H = 72;
		public const int GLFW_KEY_I = 73;
		public const int GLFW_KEY_J = 74;
		public const int GLFW_KEY_K = 75;
		public const int GLFW_KEY_L = 76;
		public const int GLFW_KEY_M = 77;
		public const int GLFW_KEY_N = 78;
		public const int GLFW_KEY_O = 79;
		public const int GLFW_KEY_P = 80;
		public const int GLFW_KEY_Q = 81;
		public const int GLFW_KEY_R = 82;
		public const int GLFW_KEY_S = 83;
		public const int GLFW_KEY_T = 84;
		public const int GLFW_KEY_U = 85;
		public const int GLFW_KEY_V = 86;
		public const int GLFW_KEY_W = 87;
		public const int GLFW_KEY_X = 88;
		public const int GLFW_KEY_Y = 89;
		public const int GLFW_KEY_Z = 90;
		public const int GLFW_KEY_LEFT_BRACKET = 91;
		public const int GLFW_KEY_BACKSLASH = 92;
		public const int GLFW_KEY_RIGHT_BRACKET = 93;
		public const int GLFW_KEY_GRAVE_ACCENT = 96;
		public const int GLFW_KEY_ESCAPE = 256;
		public const int GLFW_KEY_ENTER = 257;
		public const int GLFW_KEY_TAB = 258;
		public const int GLFW_KEY_BACKSPACE = 259;
		public const int GLFW_KEY_INSERT = 260;
		public const int GLFW_KEY_DELETE = 261;
		public const int GLFW_KEY_RIGHT = 262;
		public const int GLFW_KEY_LEFT = 263;
		public const int GLFW_KEY_DOWN = 264;
		public const int GLFW_KEY_UP = 265;
		public const int GLFW_KEY_PAGE_UP = 266;
		public const int GLFW_KEY_PAGE_DOWN = 267;
		public const int GLFW_KEY_HOME = 268;
		public const int GLFW_KEY_END = 269;
		public const int GLFW_KEY_CAPS_LOCK = 280;
		public const int GLFW_KEY_SCROLL_LOCK = 281;
		public const int GLFW_KEY_NUM_LOCK = 282;
		public const int GLFW_KEY_PRINT_SCREEN = 283;
		public const int GLFW_KEY_PAUSE = 284;
		public const int GLFW_KEY_F1 = 290;
		public const int GLFW_KEY_F2 = 291;
		public const int GLFW_KEY_F3 = 292;
		public const int GLFW_KEY_F4 = 293;
		public const int GLFW_KEY_F5 = 294;
		public const int GLFW_KEY_F6 = 295;
		public const int GLFW_KEY_F7 = 296;
		public const int GLFW_KEY_F8 = 297;
		public const int GLFW_KEY_F9 = 298;
		public const int GLFW_KEY_F10 = 299;
		public const int GLFW_KEY_F11 = 300;
		public const int GLFW_KEY_F12 = 301;
		public const int GLFW_KEY_F13 = 302;
		public const int GLFW_KEY_F14 = 303;
		public const int GLFW_KEY_F15 = 304;
		public const int GLFW_KEY_F16 = 305;
		public const int GLFW_KEY_F17 = 306;
		public const int GLFW_KEY_F18 = 307;
		public const int GLFW_KEY_F19 = 308;
		public const int GLFW_KEY_F20 = 309;
		public const int GLFW_KEY_F21 = 310;
		public const int GLFW_KEY_F22 = 311;
		public const int GLFW_KEY_F23 = 312;
		public const int GLFW_KEY_F24 = 313;
		public const int GLFW_KEY_F25 = 314;
		public const int GLFW_KEY_KP_0 = 320;
		public const int GLFW_KEY_KP_1 = 321;
		public const int GLFW_KEY_KP_2 = 322;
		public const int GLFW_KEY_KP_3 = 323;
		public const int GLFW_KEY_KP_4 = 324;
		public const int GLFW_KEY_KP_5 = 325;
		public const int GLFW_KEY_KP_6 = 326;
		public const int GLFW_KEY_KP_7 = 327;
		public const int GLFW_KEY_KP_8 = 328;
		public const int GLFW_KEY_KP_9 = 329;
		public const int GLFW_KEY_KP_DECIMAL = 330;
		public const int GLFW_KEY_KP_DIVIDE = 331;
		public const int GLFW_KEY_KP_MULTIPLY = 332;
		public const int GLFW_KEY_KP_SUBTRACT = 333;
		public const int GLFW_KEY_KP_ADD = 334;
		public const int GLFW_KEY_KP_ENTER = 335;
		public const int GLFW_KEY_KP_EQUAL = 336;
		public const int GLFW_KEY_LEFT_SHIFT = 340;
		public const int GLFW_KEY_LEFT_CONTROL = 341;
		public const int GLFW_KEY_LEFT_ALT = 342;
		public const int GLFW_KEY_LEFT_SUPER = 343;
		public const int GLFW_KEY_RIGHT_SHIFT = 344;
		public const int GLFW_KEY_RIGHT_CONTROL = 345;
		public const int GLFW_KEY_RIGHT_ALT = 346;
		public const int GLFW_KEY_RIGHT_SUPER = 347;
		public const int GLFW_KEY_MENU = 348;
		public const int GLFW_KEY_LAST = GLFW_KEY_MENU;
		public const int GLFW_MOD_SHIFT = 0x0001;
		public const int GLFW_MOD_CONTROL = 0x0002;
		public const int GLFW_MOD_ALT = 0x0004;
		public const int GLFW_MOD_SUPER = 0x0008;
		public const int GLFW_MOUSE_BUTTON_1 = 0;
		public const int GLFW_MOUSE_BUTTON_2 = 1;
		public const int GLFW_MOUSE_BUTTON_3 = 2;
		public const int GLFW_MOUSE_BUTTON_4 = 3;
		public const int GLFW_MOUSE_BUTTON_5 = 4;
		public const int GLFW_MOUSE_BUTTON_6 = 5;
		public const int GLFW_MOUSE_BUTTON_7 = 6;
		public const int GLFW_MOUSE_BUTTON_8 = 7;
		public const int GLFW_MOUSE_BUTTON_LAST = GLFW_MOUSE_BUTTON_8;
		public const int GLFW_MOUSE_BUTTON_LEFT = GLFW_MOUSE_BUTTON_1;
		public const int GLFW_MOUSE_BUTTON_RIGHT = GLFW_MOUSE_BUTTON_2;
		public const int GLFW_MOUSE_BUTTON_MIDDLE = GLFW_MOUSE_BUTTON_3;
		public const int GLFW_JOYSTICK_1 = 0;
		public const int GLFW_JOYSTICK_2 = 1;
		public const int GLFW_JOYSTICK_3 = 2;
		public const int GLFW_JOYSTICK_4 = 3;
		public const int GLFW_JOYSTICK_5 = 4;
		public const int GLFW_JOYSTICK_6 = 5;
		public const int GLFW_JOYSTICK_7 = 6;
		public const int GLFW_JOYSTICK_8 = 7;
		public const int GLFW_JOYSTICK_9 = 8;
		public const int GLFW_JOYSTICK_10 = 9;
		public const int GLFW_JOYSTICK_11 = 10;
		public const int GLFW_JOYSTICK_12 = 11;
		public const int GLFW_JOYSTICK_13 = 12;
		public const int GLFW_JOYSTICK_14 = 13;
		public const int GLFW_JOYSTICK_15 = 14;
		public const int GLFW_JOYSTICK_16 = 15;
		public const int GLFW_JOYSTICK_LAST = GLFW_JOYSTICK_16;
		public const int GLFW_NOT_INITIALIZED = 0x00010001;
		public const int GLFW_NO_CURRENT_CONTEXT = 0x00010002;
		public const int GLFW_INVALID_ENUM = 0x00010003;
		public const int GLFW_INVALID_VALUE = 0x00010004;
		public const int GLFW_OUT_OF_MEMORY = 0x00010005;
		public const int GLFW_API_UNAVAILABLE = 0x00010006;
		public const int GLFW_VERSION_UNAVAILABLE = 0x00010007;
		public const int GLFW_PLATFORM_ERROR = 0x00010008;
		public const int GLFW_FORMAT_UNAVAILABLE = 0x00010009;
		public const int GLFW_NO_WINDOW_CONTEXT = 0x0001000A;
		public const int GLFW_FOCUSED = 0x00020001;
		public const int GLFW_ICONIFIED = 0x00020002;
		public const int GLFW_RESIZABLE = 0x00020003;
		public const int GLFW_VISIBLE = 0x00020004;
		public const int GLFW_DECORATED = 0x00020005;
		public const int GLFW_AUTO_ICONIFY = 0x00020006;
		public const int GLFW_FLOATING = 0x00020007;
		public const int GLFW_MAXIMIZED = 0x00020008;
		public const int GLFW_RED_BITS = 0x00021001;
		public const int GLFW_GREEN_BITS = 0x00021002;
		public const int GLFW_BLUE_BITS = 0x00021003;
		public const int GLFW_ALPHA_BITS = 0x00021004;
		public const int GLFW_DEPTH_BITS = 0x00021005;
		public const int GLFW_STENCIL_BITS = 0x00021006;
		public const int GLFW_ACCUM_RED_BITS = 0x00021007;
		public const int GLFW_ACCUM_GREEN_BITS = 0x00021008;
		public const int GLFW_ACCUM_BLUE_BITS = 0x00021009;
		public const int GLFW_ACCUM_ALPHA_BITS = 0x0002100A;
		public const int GLFW_AUX_BUFFERS = 0x0002100B;
		public const int GLFW_STEREO = 0x0002100C;
		public const int GLFW_SAMPLES = 0x0002100D;
		public const int GLFW_SRGB_CAPABLE = 0x0002100E;
		public const int GLFW_REFRESH_RATE = 0x0002100F;
		public const int GLFW_DOUBLEBUFFER = 0x00021010;
		public const int GLFW_CLIENT_API = 0x00022001;
		public const int GLFW_CONTEXT_VERSION_MAJOR = 0x00022002;
		public const int GLFW_CONTEXT_VERSION_MINOR = 0x00022003;
		public const int GLFW_CONTEXT_REVISION = 0x00022004;
		public const int GLFW_CONTEXT_ROBUSTNESS = 0x00022005;
		public const int GLFW_OPENGL_FORWARD_COMPAT = 0x00022006;
		public const int GLFW_OPENGL_DEBUG_CONTEXT = 0x00022007;
		public const int GLFW_OPENGL_PROFILE = 0x00022008;
		public const int GLFW_CONTEXT_RELEASE_BEHAVIOR = 0x00022009;
		public const int GLFW_CONTEXT_NO_ERROR = 0x0002200A;
		public const int GLFW_CONTEXT_CREATION_API = 0x0002200B;
		public const int GLFW_NO_API = 0;
		public const int GLFW_OPENGL_API = 0x00030001;
		public const int GLFW_OPENGL_ES_API = 0x00030002;
		public const int GLFW_NO_ROBUSTNESS = 0;
		public const int GLFW_NO_RESET_NOTIFICATION = 0x00031001;
		public const int GLFW_LOSE_CONTEXT_ON_RESET = 0x00031002;
		public const int GLFW_OPENGL_ANY_PROFILE = 0;
		public const int GLFW_OPENGL_CORE_PROFILE = 0x00032001;
		public const int GLFW_OPENGL_COMPAT_PROFILE = 0x00032002;
		public const int GLFW_CURSOR = 0x00033001;
		public const int GLFW_STICKY_KEYS = 0x00033002;
		public const int GLFW_STICKY_MOUSE_BUTTONS = 0x00033003;
		public const int GLFW_CURSOR_NORMAL = 0x00034001;
		public const int GLFW_CURSOR_HIDDEN = 0x00034002;
		public const int GLFW_CURSOR_DISABLED = 0x00034003;
		public const int GLFW_ANY_RELEASE_BEHAVIOR = 0;
		public const int GLFW_RELEASE_BEHAVIOR_FLUSH = 0x00035001;
		public const int GLFW_RELEASE_BEHAVIOR_NONE = 0x00035002;
		public const int GLFW_NATIVE_CONTEXT_API = 0x00036001;
		public const int GLFW_EGL_CONTEXT_API = 0x00036002;
		public const int GLFW_ARROW_CURSOR = 0x00036001;
		public const int GLFW_IBEAM_CURSOR = 0x00036002;
		public const int GLFW_CROSSHAIR_CURSOR = 0x00036003;
		public const int GLFW_HAND_CURSOR = 0x00036004;
		public const int GLFW_HRESIZE_CURSOR = 0x00036005;
		public const int GLFW_VRESIZE_CURSOR = 0x00036006;
		public const int GLFW_CONNECTED = 0x00040001;
		public const int GLFW_DISCONNECTED = 0x00040002;
		public const int GLFW_DONT_CARE = -1;

		public struct GLFWvidmode
		{
			public int width;
			public int height;
			public int redBits;
			public int greenBits;
			public int blueBits;
			public int refreshRate;
		}

		public struct GLFWgammaramp
		{
			public ushort[] red;
			public ushort[] green;
			public ushort[] blue;
			public uint size;
		}

		public struct GLFWimage
		{
			public int width;
			public int height;
			public IntPtr pixels;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWerrorfun(int error, string description);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWwindowposfun(IntPtr window, int xpos, int ypos);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWwindowsizefun(IntPtr window, int width, int height);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWwindowclosefun(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWwindowrefreshfun(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWwindowfocusfun(IntPtr window, int focused);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWwindowiconifyfun(IntPtr window, int iconified);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWframebuffersizefun(IntPtr window, int width, int height);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWmousebuttonfun(IntPtr window, int button, int action, int mods);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWcursorposfun(IntPtr window, double xpos, double ypos);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWcursorenterfun(IntPtr window, int entered);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWscrollfun(IntPtr window, double xoffset, double yoffset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWkeyfun(IntPtr window, int key, int scancode, int action, int mods);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWcharfun(IntPtr window, uint codepoint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWcharmodsfun(IntPtr window, uint codepoint, int mods);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWdropfun(IntPtr window, int count, IntPtr paths);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWmonitorfun(IntPtr monitor, int @event);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public delegate void GLFWjoystickfun(int joy, int @event);

		private static class Delegates
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwInit();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwTerminate();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetVersion(out int major, out int minor, out int rev);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetVersionString();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWerrorfun glfwSetErrorCallback(GLFWerrorfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetMonitors(out int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetPrimaryMonitor();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetMonitorPos(IntPtr monitor, out int xpos, out int ypos);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetMonitorPhysicalSize(IntPtr monitor, out int widthMM, out int heightMM);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetMonitorName(IntPtr monitor);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWmonitorfun glfwSetMonitorCallback(GLFWmonitorfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetVideoModes(IntPtr monitor, out int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetVideoMode(IntPtr monitor);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetGamma(IntPtr monitor, float gamma);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetGammaRamp(IntPtr monitor);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetGammaRamp(IntPtr monitor, IntPtr ramp);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwDefaultWindowHints();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwWindowHint(int hint, int value);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwCreateWindow(int width, int height, string title, IntPtr monitor, IntPtr share);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwDestroyWindow(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwWindowShouldClose(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowShouldClose(IntPtr window, int value);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowTitle(IntPtr window, string title);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowIcon(IntPtr window, int count, IntPtr images);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetWindowPos(IntPtr window, out int xpos, out int ypos);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowPos(IntPtr window, int xpos, int ypos);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetWindowSize(IntPtr window, out int width, out int height);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowSizeLimits(IntPtr window, int minwidth, int minheight, int maxwidth, int maxheight);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowAspectRatio(IntPtr window, int numer, int denom);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowSize(IntPtr window, int width, int height);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetFramebufferSize(IntPtr window, out int width, out int height);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetWindowFrameSize(IntPtr window, out int left, out int top, out int right, out int bottom);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwIconifyWindow(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwRestoreWindow(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwMaximizeWindow(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwShowWindow(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwHideWindow(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwFocusWindow(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetWindowMonitor(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowMonitor(IntPtr window, IntPtr monitor, int xpos, int ypos, int width, int height, int refreshRate);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwGetWindowAttrib(IntPtr window, int attrib);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetWindowUserPointer(IntPtr window, IntPtr pointer);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetWindowUserPointer(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWwindowposfun glfwSetWindowPosCallback(IntPtr window, GLFWwindowposfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWwindowsizefun glfwSetWindowSizeCallback(IntPtr window, GLFWwindowsizefun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWwindowclosefun glfwSetWindowCloseCallback(IntPtr window, GLFWwindowclosefun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWwindowrefreshfun glfwSetWindowRefreshCallback(IntPtr window, GLFWwindowrefreshfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWwindowfocusfun glfwSetWindowFocusCallback(IntPtr window, GLFWwindowfocusfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWwindowiconifyfun glfwSetWindowIconifyCallback(IntPtr window, GLFWwindowiconifyfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWframebuffersizefun glfwSetFramebufferSizeCallback(IntPtr window, GLFWframebuffersizefun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwPollEvents();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwWaitEvents();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwWaitEventsTimeout(double timeout);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwPostEmptyEvent();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwGetInputMode(IntPtr window, int mode);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetInputMode(IntPtr window, int mode, int value);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetKeyName(int key, int scancode);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwGetKey(IntPtr window, int key);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwGetMouseButton(IntPtr window, int button);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwGetCursorPos(IntPtr window, out double xpos, out double ypos);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetCursorPos(IntPtr window, double xpos, double ypos);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwCreateCursor(IntPtr image, int xhot, int yhot);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwCreateStandardCursor(int shape);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwDestroyCursor(IntPtr cursor);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetCursor(IntPtr window, IntPtr cursor);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWkeyfun glfwSetKeyCallback(IntPtr window, GLFWkeyfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWcharfun glfwSetCharCallback(IntPtr window, GLFWcharfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWcharmodsfun glfwSetCharModsCallback(IntPtr window, GLFWcharmodsfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWmousebuttonfun glfwSetMouseButtonCallback(IntPtr window, GLFWmousebuttonfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWcursorposfun glfwSetCursorPosCallback(IntPtr window, GLFWcursorposfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWcursorenterfun glfwSetCursorEnterCallback(IntPtr window, GLFWcursorenterfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWscrollfun glfwSetScrollCallback(IntPtr window, GLFWscrollfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWdropfun glfwSetDropCallback(IntPtr window, GLFWdropfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwJoystickPresent(int joy);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetJoystickAxes(int joy, out int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetJoystickButtons(int joy, out int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetJoystickName(int joy);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate GLFWjoystickfun glfwSetJoystickCallback(GLFWjoystickfun cbfun);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetClipboardString(IntPtr window, string @string);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetClipboardString(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate double glfwGetTime();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSetTime(double time);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate ulong glfwGetTimerValue();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate ulong glfwGetTimerFrequency();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwMakeContextCurrent(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetCurrentContext();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSwapBuffers(IntPtr window);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate void glfwSwapInterval(int interval);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwExtensionSupported(string extension);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetProcAddress(string procname);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwVulkanSupported();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetRequiredInstanceExtensions(out uint count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate IntPtr glfwGetInstanceProcAddress(IntPtr instance, string procname);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwGetPhysicalDevicePresentationSupport(IntPtr instance, IntPtr device, uint queuefamily);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
			public delegate int glfwCreateWindowSurface(IntPtr instance, IntPtr window, IntPtr allocator, out IntPtr surface);

		}

		private static Delegates.glfwInit _glfwInit;

		private static Delegates.glfwTerminate _glfwTerminate;

		private static Delegates.glfwGetVersion _glfwGetVersion;

		private static Delegates.glfwGetVersionString _glfwGetVersionString;

		private static Delegates.glfwSetErrorCallback _glfwSetErrorCallback;

		private static Delegates.glfwGetMonitors _glfwGetMonitors;

		private static Delegates.glfwGetPrimaryMonitor _glfwGetPrimaryMonitor;

		private static Delegates.glfwGetMonitorPos _glfwGetMonitorPos;

		private static Delegates.glfwGetMonitorPhysicalSize _glfwGetMonitorPhysicalSize;

		private static Delegates.glfwGetMonitorName _glfwGetMonitorName;

		private static Delegates.glfwSetMonitorCallback _glfwSetMonitorCallback;

		private static Delegates.glfwGetVideoModes _glfwGetVideoModes;

		private static Delegates.glfwGetVideoMode _glfwGetVideoMode;

		private static Delegates.glfwSetGamma _glfwSetGamma;

		private static Delegates.glfwGetGammaRamp _glfwGetGammaRamp;

		private static Delegates.glfwSetGammaRamp _glfwSetGammaRamp;

		private static Delegates.glfwDefaultWindowHints _glfwDefaultWindowHints;

		private static Delegates.glfwWindowHint _glfwWindowHint;

		private static Delegates.glfwCreateWindow _glfwCreateWindow;

		private static Delegates.glfwDestroyWindow _glfwDestroyWindow;

		private static Delegates.glfwWindowShouldClose _glfwWindowShouldClose;

		private static Delegates.glfwSetWindowShouldClose _glfwSetWindowShouldClose;

		private static Delegates.glfwSetWindowTitle _glfwSetWindowTitle;

		private static Delegates.glfwSetWindowIcon _glfwSetWindowIcon;

		private static Delegates.glfwGetWindowPos _glfwGetWindowPos;

		private static Delegates.glfwSetWindowPos _glfwSetWindowPos;

		private static Delegates.glfwGetWindowSize _glfwGetWindowSize;

		private static Delegates.glfwSetWindowSizeLimits _glfwSetWindowSizeLimits;

		private static Delegates.glfwSetWindowAspectRatio _glfwSetWindowAspectRatio;

		private static Delegates.glfwSetWindowSize _glfwSetWindowSize;

		private static Delegates.glfwGetFramebufferSize _glfwGetFramebufferSize;

		private static Delegates.glfwGetWindowFrameSize _glfwGetWindowFrameSize;

		private static Delegates.glfwIconifyWindow _glfwIconifyWindow;

		private static Delegates.glfwRestoreWindow _glfwRestoreWindow;

		private static Delegates.glfwMaximizeWindow _glfwMaximizeWindow;

		private static Delegates.glfwShowWindow _glfwShowWindow;

		private static Delegates.glfwHideWindow _glfwHideWindow;

		private static Delegates.glfwFocusWindow _glfwFocusWindow;

		private static Delegates.glfwGetWindowMonitor _glfwGetWindowMonitor;

		private static Delegates.glfwSetWindowMonitor _glfwSetWindowMonitor;

		private static Delegates.glfwGetWindowAttrib _glfwGetWindowAttrib;

		private static Delegates.glfwSetWindowUserPointer _glfwSetWindowUserPointer;

		private static Delegates.glfwGetWindowUserPointer _glfwGetWindowUserPointer;

		private static Delegates.glfwSetWindowPosCallback _glfwSetWindowPosCallback;

		private static Delegates.glfwSetWindowSizeCallback _glfwSetWindowSizeCallback;

		private static Delegates.glfwSetWindowCloseCallback _glfwSetWindowCloseCallback;

		private static Delegates.glfwSetWindowRefreshCallback _glfwSetWindowRefreshCallback;

		private static Delegates.glfwSetWindowFocusCallback _glfwSetWindowFocusCallback;

		private static Delegates.glfwSetWindowIconifyCallback _glfwSetWindowIconifyCallback;

		private static Delegates.glfwSetFramebufferSizeCallback _glfwSetFramebufferSizeCallback;

		private static Delegates.glfwPollEvents _glfwPollEvents;

		private static Delegates.glfwWaitEvents _glfwWaitEvents;

		private static Delegates.glfwWaitEventsTimeout _glfwWaitEventsTimeout;

		private static Delegates.glfwPostEmptyEvent _glfwPostEmptyEvent;

		private static Delegates.glfwGetInputMode _glfwGetInputMode;

		private static Delegates.glfwSetInputMode _glfwSetInputMode;

		private static Delegates.glfwGetKeyName _glfwGetKeyName;

		private static Delegates.glfwGetKey _glfwGetKey;

		private static Delegates.glfwGetMouseButton _glfwGetMouseButton;

		private static Delegates.glfwGetCursorPos _glfwGetCursorPos;

		private static Delegates.glfwSetCursorPos _glfwSetCursorPos;

		private static Delegates.glfwCreateCursor _glfwCreateCursor;

		private static Delegates.glfwCreateStandardCursor _glfwCreateStandardCursor;

		private static Delegates.glfwDestroyCursor _glfwDestroyCursor;

		private static Delegates.glfwSetCursor _glfwSetCursor;

		private static Delegates.glfwSetKeyCallback _glfwSetKeyCallback;

		private static Delegates.glfwSetCharCallback _glfwSetCharCallback;

		private static Delegates.glfwSetCharModsCallback _glfwSetCharModsCallback;

		private static Delegates.glfwSetMouseButtonCallback _glfwSetMouseButtonCallback;

		private static Delegates.glfwSetCursorPosCallback _glfwSetCursorPosCallback;

		private static Delegates.glfwSetCursorEnterCallback _glfwSetCursorEnterCallback;

		private static Delegates.glfwSetScrollCallback _glfwSetScrollCallback;

		private static Delegates.glfwSetDropCallback _glfwSetDropCallback;

		private static Delegates.glfwJoystickPresent _glfwJoystickPresent;

		private static Delegates.glfwGetJoystickAxes _glfwGetJoystickAxes;

		private static Delegates.glfwGetJoystickButtons _glfwGetJoystickButtons;

		private static Delegates.glfwGetJoystickName _glfwGetJoystickName;

		private static Delegates.glfwSetJoystickCallback _glfwSetJoystickCallback;

		private static Delegates.glfwSetClipboardString _glfwSetClipboardString;

		private static Delegates.glfwGetClipboardString _glfwGetClipboardString;

		private static Delegates.glfwGetTime _glfwGetTime;

		private static Delegates.glfwSetTime _glfwSetTime;

		private static Delegates.glfwGetTimerValue _glfwGetTimerValue;

		private static Delegates.glfwGetTimerFrequency _glfwGetTimerFrequency;

		private static Delegates.glfwMakeContextCurrent _glfwMakeContextCurrent;

		private static Delegates.glfwGetCurrentContext _glfwGetCurrentContext;

		private static Delegates.glfwSwapBuffers _glfwSwapBuffers;

		private static Delegates.glfwSwapInterval _glfwSwapInterval;

		private static Delegates.glfwExtensionSupported _glfwExtensionSupported;

		private static Delegates.glfwGetProcAddress _glfwGetProcAddress;

		private static Delegates.glfwVulkanSupported _glfwVulkanSupported;

		private static Delegates.glfwGetRequiredInstanceExtensions _glfwGetRequiredInstanceExtensions;

		private static Delegates.glfwGetInstanceProcAddress _glfwGetInstanceProcAddress;

		private static Delegates.glfwGetPhysicalDevicePresentationSupport _glfwGetPhysicalDevicePresentationSupport;

		private static Delegates.glfwCreateWindowSurface _glfwCreateWindowSurface;

		private static class Win32
		{
			[DllImport("kernel32")]
			public static extern IntPtr LoadLibrary(string fileName);

			[DllImport("kernel32")]
			public static extern IntPtr GetProcAddress(IntPtr module, string procName);
		}

		/// <summary>
		/// Initializes the GLFW library.
		/// </summary>
		/// <remarks>
		/// This function initializes the GLFW library.  Before most GLFW functions can
		/// be used, GLFW must be initialized, and before an application terminates GLFW
		/// should be terminated in order to free any resources allocated during or
		/// after initialization.
		/// </remarks>
		/// <returns>
		/// `GLFW_TRUE` if successful, or `GLFW_FALSE` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static int glfwInit()
		{
			LoadFunctions(LoadAssembly());
			return _glfwInit();
		}

		private static Func<string, IntPtr> LoadAssembly()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				string assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "glfw3.dll");
				IntPtr assembly = Win32.LoadLibrary(assemblyPath);

				if (assembly == IntPtr.Zero)
					throw new InvalidOperationException($"Failed to load GLFW dll from path '{assemblyPath}'.");

				return x => Win32.GetProcAddress(assembly, x);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				// Coming soon...
			}

			throw new NotImplementedException("Unsupported platform.");
		}

		private static void LoadFunctions(Func<string, IntPtr> getProcAddress)
		{
			_glfwInit = (Delegates.glfwInit)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwInit"), typeof(Delegates.glfwInit));
			_glfwTerminate = (Delegates.glfwTerminate)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwTerminate"), typeof(Delegates.glfwTerminate));
			_glfwGetVersion = (Delegates.glfwGetVersion)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetVersion"), typeof(Delegates.glfwGetVersion));
			_glfwGetVersionString = (Delegates.glfwGetVersionString)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetVersionString"), typeof(Delegates.glfwGetVersionString));
			_glfwSetErrorCallback = (Delegates.glfwSetErrorCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetErrorCallback"), typeof(Delegates.glfwSetErrorCallback));
			_glfwGetMonitors = (Delegates.glfwGetMonitors)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetMonitors"), typeof(Delegates.glfwGetMonitors));
			_glfwGetPrimaryMonitor = (Delegates.glfwGetPrimaryMonitor)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetPrimaryMonitor"), typeof(Delegates.glfwGetPrimaryMonitor));
			_glfwGetMonitorPos = (Delegates.glfwGetMonitorPos)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetMonitorPos"), typeof(Delegates.glfwGetMonitorPos));
			_glfwGetMonitorPhysicalSize = (Delegates.glfwGetMonitorPhysicalSize)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetMonitorPhysicalSize"), typeof(Delegates.glfwGetMonitorPhysicalSize));
			_glfwGetMonitorName = (Delegates.glfwGetMonitorName)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetMonitorName"), typeof(Delegates.glfwGetMonitorName));
			_glfwSetMonitorCallback = (Delegates.glfwSetMonitorCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetMonitorCallback"), typeof(Delegates.glfwSetMonitorCallback));
			_glfwGetVideoModes = (Delegates.glfwGetVideoModes)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetVideoModes"), typeof(Delegates.glfwGetVideoModes));
			_glfwGetVideoMode = (Delegates.glfwGetVideoMode)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetVideoMode"), typeof(Delegates.glfwGetVideoMode));
			_glfwSetGamma = (Delegates.glfwSetGamma)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetGamma"), typeof(Delegates.glfwSetGamma));
			_glfwGetGammaRamp = (Delegates.glfwGetGammaRamp)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetGammaRamp"), typeof(Delegates.glfwGetGammaRamp));
			_glfwSetGammaRamp = (Delegates.glfwSetGammaRamp)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetGammaRamp"), typeof(Delegates.glfwSetGammaRamp));
			_glfwDefaultWindowHints = (Delegates.glfwDefaultWindowHints)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwDefaultWindowHints"), typeof(Delegates.glfwDefaultWindowHints));
			_glfwWindowHint = (Delegates.glfwWindowHint)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwWindowHint"), typeof(Delegates.glfwWindowHint));
			_glfwCreateWindow = (Delegates.glfwCreateWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwCreateWindow"), typeof(Delegates.glfwCreateWindow));
			_glfwDestroyWindow = (Delegates.glfwDestroyWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwDestroyWindow"), typeof(Delegates.glfwDestroyWindow));
			_glfwWindowShouldClose = (Delegates.glfwWindowShouldClose)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwWindowShouldClose"), typeof(Delegates.glfwWindowShouldClose));
			_glfwSetWindowShouldClose = (Delegates.glfwSetWindowShouldClose)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowShouldClose"), typeof(Delegates.glfwSetWindowShouldClose));
			_glfwSetWindowTitle = (Delegates.glfwSetWindowTitle)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowTitle"), typeof(Delegates.glfwSetWindowTitle));
			_glfwSetWindowIcon = (Delegates.glfwSetWindowIcon)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowIcon"), typeof(Delegates.glfwSetWindowIcon));
			_glfwGetWindowPos = (Delegates.glfwGetWindowPos)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetWindowPos"), typeof(Delegates.glfwGetWindowPos));
			_glfwSetWindowPos = (Delegates.glfwSetWindowPos)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowPos"), typeof(Delegates.glfwSetWindowPos));
			_glfwGetWindowSize = (Delegates.glfwGetWindowSize)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetWindowSize"), typeof(Delegates.glfwGetWindowSize));
			_glfwSetWindowSizeLimits = (Delegates.glfwSetWindowSizeLimits)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowSizeLimits"), typeof(Delegates.glfwSetWindowSizeLimits));
			_glfwSetWindowAspectRatio = (Delegates.glfwSetWindowAspectRatio)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowAspectRatio"), typeof(Delegates.glfwSetWindowAspectRatio));
			_glfwSetWindowSize = (Delegates.glfwSetWindowSize)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowSize"), typeof(Delegates.glfwSetWindowSize));
			_glfwGetFramebufferSize = (Delegates.glfwGetFramebufferSize)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetFramebufferSize"), typeof(Delegates.glfwGetFramebufferSize));
			_glfwGetWindowFrameSize = (Delegates.glfwGetWindowFrameSize)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetWindowFrameSize"), typeof(Delegates.glfwGetWindowFrameSize));
			_glfwIconifyWindow = (Delegates.glfwIconifyWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwIconifyWindow"), typeof(Delegates.glfwIconifyWindow));
			_glfwRestoreWindow = (Delegates.glfwRestoreWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwRestoreWindow"), typeof(Delegates.glfwRestoreWindow));
			_glfwMaximizeWindow = (Delegates.glfwMaximizeWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwMaximizeWindow"), typeof(Delegates.glfwMaximizeWindow));
			_glfwShowWindow = (Delegates.glfwShowWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwShowWindow"), typeof(Delegates.glfwShowWindow));
			_glfwHideWindow = (Delegates.glfwHideWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwHideWindow"), typeof(Delegates.glfwHideWindow));
			_glfwFocusWindow = (Delegates.glfwFocusWindow)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwFocusWindow"), typeof(Delegates.glfwFocusWindow));
			_glfwGetWindowMonitor = (Delegates.glfwGetWindowMonitor)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetWindowMonitor"), typeof(Delegates.glfwGetWindowMonitor));
			_glfwSetWindowMonitor = (Delegates.glfwSetWindowMonitor)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowMonitor"), typeof(Delegates.glfwSetWindowMonitor));
			_glfwGetWindowAttrib = (Delegates.glfwGetWindowAttrib)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetWindowAttrib"), typeof(Delegates.glfwGetWindowAttrib));
			_glfwSetWindowUserPointer = (Delegates.glfwSetWindowUserPointer)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowUserPointer"), typeof(Delegates.glfwSetWindowUserPointer));
			_glfwGetWindowUserPointer = (Delegates.glfwGetWindowUserPointer)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetWindowUserPointer"), typeof(Delegates.glfwGetWindowUserPointer));
			_glfwSetWindowPosCallback = (Delegates.glfwSetWindowPosCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowPosCallback"), typeof(Delegates.glfwSetWindowPosCallback));
			_glfwSetWindowSizeCallback = (Delegates.glfwSetWindowSizeCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowSizeCallback"), typeof(Delegates.glfwSetWindowSizeCallback));
			_glfwSetWindowCloseCallback = (Delegates.glfwSetWindowCloseCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowCloseCallback"), typeof(Delegates.glfwSetWindowCloseCallback));
			_glfwSetWindowRefreshCallback = (Delegates.glfwSetWindowRefreshCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowRefreshCallback"), typeof(Delegates.glfwSetWindowRefreshCallback));
			_glfwSetWindowFocusCallback = (Delegates.glfwSetWindowFocusCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowFocusCallback"), typeof(Delegates.glfwSetWindowFocusCallback));
			_glfwSetWindowIconifyCallback = (Delegates.glfwSetWindowIconifyCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetWindowIconifyCallback"), typeof(Delegates.glfwSetWindowIconifyCallback));
			_glfwSetFramebufferSizeCallback = (Delegates.glfwSetFramebufferSizeCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetFramebufferSizeCallback"), typeof(Delegates.glfwSetFramebufferSizeCallback));
			_glfwPollEvents = (Delegates.glfwPollEvents)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwPollEvents"), typeof(Delegates.glfwPollEvents));
			_glfwWaitEvents = (Delegates.glfwWaitEvents)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwWaitEvents"), typeof(Delegates.glfwWaitEvents));
			_glfwWaitEventsTimeout = (Delegates.glfwWaitEventsTimeout)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwWaitEventsTimeout"), typeof(Delegates.glfwWaitEventsTimeout));
			_glfwPostEmptyEvent = (Delegates.glfwPostEmptyEvent)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwPostEmptyEvent"), typeof(Delegates.glfwPostEmptyEvent));
			_glfwGetInputMode = (Delegates.glfwGetInputMode)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetInputMode"), typeof(Delegates.glfwGetInputMode));
			_glfwSetInputMode = (Delegates.glfwSetInputMode)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetInputMode"), typeof(Delegates.glfwSetInputMode));
			_glfwGetKeyName = (Delegates.glfwGetKeyName)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetKeyName"), typeof(Delegates.glfwGetKeyName));
			_glfwGetKey = (Delegates.glfwGetKey)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetKey"), typeof(Delegates.glfwGetKey));
			_glfwGetMouseButton = (Delegates.glfwGetMouseButton)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetMouseButton"), typeof(Delegates.glfwGetMouseButton));
			_glfwGetCursorPos = (Delegates.glfwGetCursorPos)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetCursorPos"), typeof(Delegates.glfwGetCursorPos));
			_glfwSetCursorPos = (Delegates.glfwSetCursorPos)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetCursorPos"), typeof(Delegates.glfwSetCursorPos));
			_glfwCreateCursor = (Delegates.glfwCreateCursor)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwCreateCursor"), typeof(Delegates.glfwCreateCursor));
			_glfwCreateStandardCursor = (Delegates.glfwCreateStandardCursor)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwCreateStandardCursor"), typeof(Delegates.glfwCreateStandardCursor));
			_glfwDestroyCursor = (Delegates.glfwDestroyCursor)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwDestroyCursor"), typeof(Delegates.glfwDestroyCursor));
			_glfwSetCursor = (Delegates.glfwSetCursor)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetCursor"), typeof(Delegates.glfwSetCursor));
			_glfwSetKeyCallback = (Delegates.glfwSetKeyCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetKeyCallback"), typeof(Delegates.glfwSetKeyCallback));
			_glfwSetCharCallback = (Delegates.glfwSetCharCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetCharCallback"), typeof(Delegates.glfwSetCharCallback));
			_glfwSetCharModsCallback = (Delegates.glfwSetCharModsCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetCharModsCallback"), typeof(Delegates.glfwSetCharModsCallback));
			_glfwSetMouseButtonCallback = (Delegates.glfwSetMouseButtonCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetMouseButtonCallback"), typeof(Delegates.glfwSetMouseButtonCallback));
			_glfwSetCursorPosCallback = (Delegates.glfwSetCursorPosCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetCursorPosCallback"), typeof(Delegates.glfwSetCursorPosCallback));
			_glfwSetCursorEnterCallback = (Delegates.glfwSetCursorEnterCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetCursorEnterCallback"), typeof(Delegates.glfwSetCursorEnterCallback));
			_glfwSetScrollCallback = (Delegates.glfwSetScrollCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetScrollCallback"), typeof(Delegates.glfwSetScrollCallback));
			_glfwSetDropCallback = (Delegates.glfwSetDropCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetDropCallback"), typeof(Delegates.glfwSetDropCallback));
			_glfwJoystickPresent = (Delegates.glfwJoystickPresent)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwJoystickPresent"), typeof(Delegates.glfwJoystickPresent));
			_glfwGetJoystickAxes = (Delegates.glfwGetJoystickAxes)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetJoystickAxes"), typeof(Delegates.glfwGetJoystickAxes));
			_glfwGetJoystickButtons = (Delegates.glfwGetJoystickButtons)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetJoystickButtons"), typeof(Delegates.glfwGetJoystickButtons));
			_glfwGetJoystickName = (Delegates.glfwGetJoystickName)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetJoystickName"), typeof(Delegates.glfwGetJoystickName));
			_glfwSetJoystickCallback = (Delegates.glfwSetJoystickCallback)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetJoystickCallback"), typeof(Delegates.glfwSetJoystickCallback));
			_glfwSetClipboardString = (Delegates.glfwSetClipboardString)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetClipboardString"), typeof(Delegates.glfwSetClipboardString));
			_glfwGetClipboardString = (Delegates.glfwGetClipboardString)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetClipboardString"), typeof(Delegates.glfwGetClipboardString));
			_glfwGetTime = (Delegates.glfwGetTime)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetTime"), typeof(Delegates.glfwGetTime));
			_glfwSetTime = (Delegates.glfwSetTime)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSetTime"), typeof(Delegates.glfwSetTime));
			_glfwGetTimerValue = (Delegates.glfwGetTimerValue)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetTimerValue"), typeof(Delegates.glfwGetTimerValue));
			_glfwGetTimerFrequency = (Delegates.glfwGetTimerFrequency)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetTimerFrequency"), typeof(Delegates.glfwGetTimerFrequency));
			_glfwMakeContextCurrent = (Delegates.glfwMakeContextCurrent)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwMakeContextCurrent"), typeof(Delegates.glfwMakeContextCurrent));
			_glfwGetCurrentContext = (Delegates.glfwGetCurrentContext)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetCurrentContext"), typeof(Delegates.glfwGetCurrentContext));
			_glfwSwapBuffers = (Delegates.glfwSwapBuffers)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSwapBuffers"), typeof(Delegates.glfwSwapBuffers));
			_glfwSwapInterval = (Delegates.glfwSwapInterval)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwSwapInterval"), typeof(Delegates.glfwSwapInterval));
			_glfwExtensionSupported = (Delegates.glfwExtensionSupported)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwExtensionSupported"), typeof(Delegates.glfwExtensionSupported));
			_glfwGetProcAddress = (Delegates.glfwGetProcAddress)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetProcAddress"), typeof(Delegates.glfwGetProcAddress));
			_glfwVulkanSupported = (Delegates.glfwVulkanSupported)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwVulkanSupported"), typeof(Delegates.glfwVulkanSupported));
			_glfwGetRequiredInstanceExtensions = (Delegates.glfwGetRequiredInstanceExtensions)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetRequiredInstanceExtensions"), typeof(Delegates.glfwGetRequiredInstanceExtensions));
			_glfwGetInstanceProcAddress = (Delegates.glfwGetInstanceProcAddress)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetInstanceProcAddress"), typeof(Delegates.glfwGetInstanceProcAddress));
			_glfwGetPhysicalDevicePresentationSupport = (Delegates.glfwGetPhysicalDevicePresentationSupport)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwGetPhysicalDevicePresentationSupport"), typeof(Delegates.glfwGetPhysicalDevicePresentationSupport));
			_glfwCreateWindowSurface = (Delegates.glfwCreateWindowSurface)Marshal.GetDelegateForFunctionPointer(getProcAddress("glfwCreateWindowSurface"), typeof(Delegates.glfwCreateWindowSurface));
		}

		/// <summary>
		/// Terminates the GLFW library.
		/// </summary>
		/// <remarks>
		/// This function destroys all remaining windows and cursors, restores any
		/// modified gamma ramps and frees any other allocated resources.  Once this
		/// function is called, you must again call @ref glfwInit successfully before
		/// you will be able to use most GLFW functions.
		/// </remarks>
		public static void glfwTerminate()
		{
			_glfwTerminate();
		}

		/// <summary>
		/// Retrieves the version of the GLFW library.
		/// </summary>
		/// <remarks>
		/// This function retrieves the major, minor and revision numbers of the GLFW
		/// library.  It is intended for when you are using GLFW as a shared library and
		/// want to ensure that you are using the minimum required version.
		/// </remarks>
		/// <param name="major">
		/// Where to store the major version number, or `NULL`.
		/// </param>
		/// <param name="minor">
		/// Where to store the minor version number, or `NULL`.
		/// </param>
		/// <param name="rev">
		/// Where to store the revision number, or `NULL`.
		/// </param>
		public static void glfwGetVersion(out int major, out int minor, out int rev)
		{
			_glfwGetVersion(out major, out minor, out rev);
		}

		/// <summary>
		/// Returns a string describing the compile-time configuration.
		/// </summary>
		/// <remarks>
		/// This function returns the compile-time generated
		/// [version string](@ref intro_version_string) of the GLFW library binary.  It
		/// describes the version, platform, compiler and any platform-specific
		/// compile-time options.  It should not be confused with the OpenGL or OpenGL
		/// ES version string, queried with `glGetString`.
		/// </remarks>
		/// <returns>
		/// The ASCII encoded GLFW version string.
		/// </returns>
		public static string glfwGetVersionString()
		{
			var versionStringPtr = _glfwGetVersionString();
			return Marshal.PtrToStringAnsi(versionStringPtr);
		}

		/// <summary>
		/// Sets the error callback.
		/// </summary>
		/// <remarks>
		/// This function sets the error callback, which is called with an error code
		/// and a human-readable description each time a GLFW error occurs.
		/// </remarks>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set.
		/// </returns>
		public static GLFWerrorfun glfwSetErrorCallback(GLFWerrorfun cbfun)
		{
			return _glfwSetErrorCallback(cbfun);
		}

		/// <summary>
		/// Returns the currently connected monitors.
		/// </summary>
		/// <remarks>
		/// This function returns an array of handles for all currently connected
		/// monitors.  The primary monitor is always first in the returned array.  If no
		/// monitors were found, this function returns `NULL`.
		/// </remarks>
		/// <param name="count">
		/// Where to store the number of monitors in the returned
		/// array.  This is set to zero if an error occurred.
		/// </param>
		/// <returns>
		/// An array of monitor handles, or `NULL` if no monitors were found or
		/// if an [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr[] glfwGetMonitors()
		{
			var arrayPtr = _glfwGetMonitors(out int count);

			if (arrayPtr == IntPtr.Zero)
				return null;

			var result = new IntPtr[count];

			for (int i = 0; i < count; i++)
			{
				var ptr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
				result[i] = ptr;
			}

			return result;
		}

		/// <summary>
		/// Returns the primary monitor.
		/// </summary>
		/// <remarks>
		/// This function returns the primary monitor.  This is usually the monitor
		/// where elements like the task bar or global menu bar are located.
		/// </remarks>
		/// <returns>
		/// The primary monitor, or `NULL` if no monitors were found or if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetPrimaryMonitor()
		{
			return _glfwGetPrimaryMonitor();
		}

		/// <summary>
		/// Returns the position of the monitor's viewport on the virtual screen.
		/// </summary>
		/// <remarks>
		/// This function returns the position, in screen coordinates, of the upper-left
		/// corner of the specified monitor.
		/// </remarks>
		/// <param name="monitor">
		/// The monitor to query.
		/// </param>
		/// <param name="xpos">
		/// Where to store the monitor x-coordinate, or `NULL`.
		/// </param>
		/// <param name="ypos">
		/// Where to store the monitor y-coordinate, or `NULL`.
		/// </param>
		public static void glfwGetMonitorPos(IntPtr monitor, out int xpos, out int ypos)
		{
			_glfwGetMonitorPos(monitor, out xpos, out ypos);
		}

		/// <summary>
		/// Returns the physical size of the monitor.
		/// </summary>
		/// <remarks>
		/// This function returns the size, in millimetres, of the display area of the
		/// specified monitor.
		/// </remarks>
		/// <param name="monitor">
		/// The monitor to query.
		/// </param>
		/// <param name="widthMM">
		/// Where to store the width, in millimetres, of the
		/// monitor's display area, or `NULL`.
		/// </param>
		/// <param name="heightMM">
		/// Where to store the height, in millimetres, of the
		/// monitor's display area, or `NULL`.
		/// </param>
		public static void glfwGetMonitorPhysicalSize(IntPtr monitor, out int widthMM, out int heightMM)
		{
			_glfwGetMonitorPhysicalSize(monitor, out widthMM, out heightMM);
		}

		/// <summary>
		/// Returns the name of the specified monitor.
		/// </summary>
		/// <remarks>
		/// This function returns a human-readable name, encoded as UTF-8, of the
		/// specified monitor.  The name typically reflects the make and model of the
		/// monitor and is not guaranteed to be unique among the connected monitors.
		/// </remarks>
		/// <param name="monitor">
		/// The monitor to query.
		/// </param>
		/// <returns>
		/// The UTF-8 encoded name of the monitor, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetMonitorName(IntPtr monitor)
		{
			return _glfwGetMonitorName(monitor);
		}

		/// <summary>
		/// Sets the monitor configuration callback.
		/// </summary>
		/// <remarks>
		/// This function sets the monitor configuration callback, or removes the
		/// currently set callback.  This is called when a monitor is connected to or
		/// disconnected from the system.
		/// </remarks>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWmonitorfun glfwSetMonitorCallback(GLFWmonitorfun cbfun)
		{
			return _glfwSetMonitorCallback(cbfun);
		}

		/// <summary>
		/// Returns the available video modes for the specified monitor.
		/// </summary>
		/// <remarks>
		/// This function returns an array of all video modes supported by the specified
		/// monitor.  The returned array is sorted in ascending order, first by color
		/// bit depth (the sum of all channel depths) and then by resolution area (the
		/// product of width and height).
		/// </remarks>
		/// <param name="monitor">
		/// The monitor to query.
		/// </param>
		/// <param name="count">
		/// Where to store the number of video modes in the returned
		/// array.  This is set to zero if an error occurred.
		/// </param>
		/// <returns>
		/// An array of video modes, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetVideoModes(IntPtr monitor, out int count)
		{
			return _glfwGetVideoModes(monitor, out count);
		}

		/// <summary>
		/// Returns the current mode of the specified monitor.
		/// </summary>
		/// <remarks>
		/// This function returns the current video mode of the specified monitor.  If
		/// you have created a full screen window for that monitor, the return value
		/// will depend on whether that window is iconified.
		/// </remarks>
		/// <param name="monitor">
		/// The monitor to query.
		/// </param>
		/// <returns>
		/// The current mode of the monitor, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static GLFWvidmode glfwGetVideoMode(IntPtr monitor)
		{
			var ptr = _glfwGetVideoMode(monitor);
			return Marshal.PtrToStructure<GLFWvidmode>(ptr);
		}

		/// <summary>
		/// Generates a gamma ramp and sets it for the specified monitor.
		/// </summary>
		/// <remarks>
		/// This function generates a 256-element gamma ramp from the specified exponent
		/// and then calls @ref glfwSetGammaRamp with it.  The value must be a finite
		/// number greater than zero.
		/// </remarks>
		/// <param name="monitor">
		/// The monitor whose gamma ramp to set.
		/// </param>
		/// <param name="gamma">
		/// The desired exponent.
		/// </param>
		public static void glfwSetGamma(IntPtr monitor, float gamma)
		{
			_glfwSetGamma(monitor, gamma);
		}

		/// <summary>
		/// Returns the current gamma ramp for the specified monitor.
		/// </summary>
		/// <remarks>
		/// This function returns the current gamma ramp of the specified monitor.
		/// </remarks>
		/// <param name="monitor">
		/// The monitor to query.
		/// </param>
		/// <returns>
		/// The current gamma ramp, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static GLFWgammaramp glfwGetGammaRamp(IntPtr monitor)
		{
			var structPtr = _glfwGetGammaRamp(monitor);

			var redArrayPtr = Marshal.ReadIntPtr(structPtr);
			var blueArrayPtr = Marshal.ReadIntPtr(IntPtr.Add(structPtr, IntPtr.Size));
			var greenArrayPtr = Marshal.ReadIntPtr(IntPtr.Add(structPtr, IntPtr.Size * 2));
			var size = (uint)Marshal.ReadInt32(IntPtr.Add(structPtr, IntPtr.Size * 3));

			var result = new GLFWgammaramp()
			{
				size = size,
				red = new ushort[size],
				green = new ushort[size],
				blue = new ushort[size],
			};

			int uintSize = Marshal.SizeOf(typeof(uint));

			for (int i = 0; i < size; i++)
			{
				result.red[i] = (ushort)Marshal.ReadInt16(IntPtr.Add(redArrayPtr, uintSize * i));
				result.blue[i] = (ushort)Marshal.ReadInt16(IntPtr.Add(blueArrayPtr, uintSize * i));
				result.green[i] = (ushort)Marshal.ReadInt16(IntPtr.Add(greenArrayPtr, uintSize * i));
			}

			return result;
		}

		/// <summary>
		/// Sets the current gamma ramp for the specified monitor.
		/// </summary>
		/// <remarks>
		/// This function sets the current gamma ramp for the specified monitor.  The
		/// original gamma ramp for that monitor is saved by GLFW the first time this
		/// function is called and is restored by @ref glfwTerminate.
		/// </remarks>
		/// <param name="monitor">
		/// The monitor whose gamma ramp to set.
		/// </param>
		/// <param name="ramp">
		/// The gamma ramp to use.
		/// </param>
		public static void glfwSetGammaRamp(IntPtr monitor, IntPtr ramp)
		{
			_glfwSetGammaRamp(monitor, ramp);
		}

		/// <summary>
		/// Resets all window hints to their default values.
		/// </summary>
		/// <remarks>
		/// This function resets all window hints to their
		/// [default values](@ref window_hints_values).
		/// </remarks>
		public static void glfwDefaultWindowHints()
		{
			_glfwDefaultWindowHints();
		}

		/// <summary>
		/// Sets the specified window hint to the desired value.
		/// </summary>
		/// <remarks>
		/// This function sets hints for the next call to @ref glfwCreateWindow.  The
		/// hints, once set, retain their values until changed by a call to @ref
		/// glfwWindowHint or @ref glfwDefaultWindowHints, or until the library is
		/// terminated.
		/// </remarks>
		/// <param name="hint">
		/// The [window hint](@ref window_hints) to set.
		/// </param>
		/// <param name="value">
		/// The new value of the window hint.
		/// </param>
		public static void glfwWindowHint(int hint, int value)
		{
			_glfwWindowHint(hint, value);
		}

		/// <summary>
		/// Creates a window and its associated context.
		/// </summary>
		/// <remarks>
		/// This function creates a window and its associated OpenGL or OpenGL ES
		/// context.  Most of the options controlling how the window and its context
		/// should be created are specified with [window hints](@ref window_hints).
		/// </remarks>
		/// <param name="width">
		/// The desired width, in screen coordinates, of the window.
		/// This must be greater than zero.
		/// </param>
		/// <param name="height">
		/// The desired height, in screen coordinates, of the window.
		/// This must be greater than zero.
		/// </param>
		/// <param name="title">
		/// The initial, UTF-8 encoded window title.
		/// </param>
		/// <param name="monitor">
		/// The monitor to use for full screen mode, or `NULL` for
		/// windowed mode.
		/// </param>
		/// <param name="share">
		/// The window whose context to share resources with, or `NULL`
		/// to not share resources.
		/// </param>
		/// <returns>
		/// The handle of the created window, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwCreateWindow(int width, int height, string title, IntPtr monitor, IntPtr share)
		{
			return _glfwCreateWindow(width, height, title, monitor, share);
		}

		/// <summary>
		/// Destroys the specified window and its context.
		/// </summary>
		/// <remarks>
		/// This function destroys the specified window and its context.  On calling
		/// this function, no further callbacks will be called for that window.
		/// </remarks>
		/// <param name="window">
		/// The window to destroy.
		/// </param>
		public static void glfwDestroyWindow(IntPtr window)
		{
			_glfwDestroyWindow(window);
		}

		/// <summary>
		/// Checks the close flag of the specified window.
		/// </summary>
		/// <remarks>
		/// This function returns the value of the close flag of the specified window.
		/// </remarks>
		/// <param name="window">
		/// The window to query.
		/// </param>
		/// <returns>
		/// The value of the close flag.
		/// </returns>
		public static int glfwWindowShouldClose(IntPtr window)
		{
			return _glfwWindowShouldClose(window);
		}

		/// <summary>
		/// Sets the close flag of the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the value of the close flag of the specified window.
		/// This can be used to override the user's attempt to close the window, or
		/// to signal that it should be closed.
		/// </remarks>
		/// <param name="window">
		/// The window whose flag to change.
		/// </param>
		/// <param name="value">
		/// The new value.
		/// </param>
		public static void glfwSetWindowShouldClose(IntPtr window, int value)
		{
			_glfwSetWindowShouldClose(window, value);
		}

		/// <summary>
		/// Sets the title of the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the window title, encoded as UTF-8, of the specified
		/// window.
		/// </remarks>
		/// <param name="window">
		/// The window whose title to change.
		/// </param>
		/// <param name="title">
		/// The UTF-8 encoded window title.
		/// </param>
		public static void glfwSetWindowTitle(IntPtr window, string title)
		{
			_glfwSetWindowTitle(window, title);
		}

		/// <summary>
		/// Sets the icon for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the icon of the specified window.  If passed an array of
		/// candidate images, those of or closest to the sizes desired by the system are
		/// selected.  If no images are specified, the window reverts to its default
		/// icon.
		/// </remarks>
		/// <param name="window">
		/// The window whose icon to set.
		/// </param>
		/// <param name="count">
		/// The number of images in the specified array, or zero to
		/// revert to the default window icon.
		/// </param>
		/// <param name="images">
		/// The images to create the icon from.  This is ignored if
		/// count is zero.
		/// </param>
		public static void glfwSetWindowIcon(IntPtr window, int count, IntPtr images)
		{
			_glfwSetWindowIcon(window, count, images);
		}

		/// <summary>
		/// Retrieves the position of the client area of the specified window.
		/// </summary>
		/// <remarks>
		/// This function retrieves the position, in screen coordinates, of the
		/// upper-left corner of the client area of the specified window.
		/// </remarks>
		/// <param name="window">
		/// The window to query.
		/// </param>
		/// <param name="xpos">
		/// Where to store the x-coordinate of the upper-left corner of
		/// the client area, or `NULL`.
		/// </param>
		/// <param name="ypos">
		/// Where to store the y-coordinate of the upper-left corner of
		/// the client area, or `NULL`.
		/// </param>
		public static void glfwGetWindowPos(IntPtr window, out int xpos, out int ypos)
		{
			_glfwGetWindowPos(window, out xpos, out ypos);
		}

		/// <summary>
		/// Sets the position of the client area of the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the position, in screen coordinates, of the upper-left
		/// corner of the client area of the specified windowed mode window.  If the
		/// window is a full screen window, this function does nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to query.
		/// </param>
		/// <param name="xpos">
		/// The x-coordinate of the upper-left corner of the client area.
		/// </param>
		/// <param name="ypos">
		/// The y-coordinate of the upper-left corner of the client area.
		/// </param>
		public static void glfwSetWindowPos(IntPtr window, int xpos, int ypos)
		{
			_glfwSetWindowPos(window, xpos, ypos);
		}

		/// <summary>
		/// Retrieves the size of the client area of the specified window.
		/// </summary>
		/// <remarks>
		/// This function retrieves the size, in screen coordinates, of the client area
		/// of the specified window.  If you wish to retrieve the size of the
		/// framebuffer of the window in pixels, see @ref glfwGetFramebufferSize.
		/// </remarks>
		/// <param name="window">
		/// The window whose size to retrieve.
		/// </param>
		/// <param name="width">
		/// Where to store the width, in screen coordinates, of the
		/// client area, or `NULL`.
		/// </param>
		/// <param name="height">
		/// Where to store the height, in screen coordinates, of the
		/// client area, or `NULL`.
		/// </param>
		public static void glfwGetWindowSize(IntPtr window, out int width, out int height)
		{
			_glfwGetWindowSize(window, out width, out height);
		}

		/// <summary>
		/// Sets the size limits of the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the size limits of the client area of the specified
		/// window.  If the window is full screen, the size limits only take effect
		/// once it is made windowed.  If the window is not resizable, this function
		/// does nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to set limits for.
		/// </param>
		/// <param name="minwidth">
		/// The minimum width, in screen coordinates, of the client
		/// area, or `GLFW_DONT_CARE`.
		/// </param>
		/// <param name="minheight">
		/// The minimum height, in screen coordinates, of the
		/// client area, or `GLFW_DONT_CARE`.
		/// </param>
		/// <param name="maxwidth">
		/// The maximum width, in screen coordinates, of the client
		/// area, or `GLFW_DONT_CARE`.
		/// </param>
		/// <param name="maxheight">
		/// The maximum height, in screen coordinates, of the
		/// client area, or `GLFW_DONT_CARE`.
		/// </param>
		public static void glfwSetWindowSizeLimits(IntPtr window, int minwidth, int minheight, int maxwidth, int maxheight)
		{
			_glfwSetWindowSizeLimits(window, minwidth, minheight, maxwidth, maxheight);
		}

		/// <summary>
		/// Sets the aspect ratio of the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the required aspect ratio of the client area of the
		/// specified window.  If the window is full screen, the aspect ratio only takes
		/// effect once it is made windowed.  If the window is not resizable, this
		/// function does nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to set limits for.
		/// </param>
		/// <param name="numer">
		/// The numerator of the desired aspect ratio, or
		/// `GLFW_DONT_CARE`.
		/// </param>
		/// <param name="denom">
		/// The denominator of the desired aspect ratio, or
		/// `GLFW_DONT_CARE`.
		/// </param>
		public static void glfwSetWindowAspectRatio(IntPtr window, int numer, int denom)
		{
			_glfwSetWindowAspectRatio(window, numer, denom);
		}

		/// <summary>
		/// Sets the size of the client area of the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the size, in screen coordinates, of the client area of
		/// the specified window.
		/// </remarks>
		/// <param name="window">
		/// The window to resize.
		/// </param>
		/// <param name="width">
		/// The desired width, in screen coordinates, of the window
		/// client area.
		/// </param>
		/// <param name="height">
		/// The desired height, in screen coordinates, of the window
		/// client area.
		/// </param>
		public static void glfwSetWindowSize(IntPtr window, int width, int height)
		{
			_glfwSetWindowSize(window, width, height);
		}

		/// <summary>
		/// Retrieves the size of the framebuffer of the specified window.
		/// </summary>
		/// <remarks>
		/// This function retrieves the size, in pixels, of the framebuffer of the
		/// specified window.  If you wish to retrieve the size of the window in screen
		/// coordinates, see @ref glfwGetWindowSize.
		/// </remarks>
		/// <param name="window">
		/// The window whose framebuffer to query.
		/// </param>
		/// <param name="width">
		/// Where to store the width, in pixels, of the framebuffer,
		/// or `NULL`.
		/// </param>
		/// <param name="height">
		/// Where to store the height, in pixels, of the framebuffer,
		/// or `NULL`.
		/// </param>
		public static void glfwGetFramebufferSize(IntPtr window, out int width, out int height)
		{
			_glfwGetFramebufferSize(window, out width, out height);
		}

		/// <summary>
		/// Retrieves the size of the frame of the window.
		/// </summary>
		/// <remarks>
		/// This function retrieves the size, in screen coordinates, of each edge of the
		/// frame of the specified window.  This size includes the title bar, if the
		/// window has one.  The size of the frame may vary depending on the
		/// [window-related hints](@ref window_hints_wnd) used to create it.
		/// </remarks>
		/// <param name="window">
		/// The window whose frame size to query.
		/// </param>
		/// <param name="left">
		/// Where to store the size, in screen coordinates, of the left
		/// edge of the window frame, or `NULL`.
		/// </param>
		/// <param name="top">
		/// Where to store the size, in screen coordinates, of the top
		/// edge of the window frame, or `NULL`.
		/// </param>
		/// <param name="right">
		/// Where to store the size, in screen coordinates, of the
		/// right edge of the window frame, or `NULL`.
		/// </param>
		/// <param name="bottom">
		/// Where to store the size, in screen coordinates, of the
		/// bottom edge of the window frame, or `NULL`.
		/// </param>
		public static void glfwGetWindowFrameSize(IntPtr window, out int left, out int top, out int right, out int bottom)
		{
			_glfwGetWindowFrameSize(window, out left, out top, out right, out bottom);
		}

		/// <summary>
		/// Iconifies the specified window.
		/// </summary>
		/// <remarks>
		/// This function iconifies (minimizes) the specified window if it was
		/// previously restored.  If the window is already iconified, this function does
		/// nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to iconify.
		/// </param>
		public static void glfwIconifyWindow(IntPtr window)
		{
			_glfwIconifyWindow(window);
		}

		/// <summary>
		/// Restores the specified window.
		/// </summary>
		/// <remarks>
		/// This function restores the specified window if it was previously iconified
		/// (minimized) or maximized.  If the window is already restored, this function
		/// does nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to restore.
		/// </param>
		public static void glfwRestoreWindow(IntPtr window)
		{
			_glfwRestoreWindow(window);
		}

		/// <summary>
		/// Maximizes the specified window.
		/// </summary>
		/// <remarks>
		/// This function maximizes the specified window if it was previously not
		/// maximized.  If the window is already maximized, this function does nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to maximize.
		/// </param>
		public static void glfwMaximizeWindow(IntPtr window)
		{
			_glfwMaximizeWindow(window);
		}

		/// <summary>
		/// Makes the specified window visible.
		/// </summary>
		/// <remarks>
		/// This function makes the specified window visible if it was previously
		/// hidden.  If the window is already visible or is in full screen mode, this
		/// function does nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to make visible.
		/// </param>
		public static void glfwShowWindow(IntPtr window)
		{
			_glfwShowWindow(window);
		}

		/// <summary>
		/// Hides the specified window.
		/// </summary>
		/// <remarks>
		/// This function hides the specified window if it was previously visible.  If
		/// the window is already hidden or is in full screen mode, this function does
		/// nothing.
		/// </remarks>
		/// <param name="window">
		/// The window to hide.
		/// </param>
		public static void glfwHideWindow(IntPtr window)
		{
			_glfwHideWindow(window);
		}

		/// <summary>
		/// Brings the specified window to front and sets input focus.
		/// </summary>
		/// <remarks>
		/// This function brings the specified window to front and sets input focus.
		/// The window should already be visible and not iconified.
		/// </remarks>
		/// <param name="window">
		/// The window to give input focus.
		/// </param>
		public static void glfwFocusWindow(IntPtr window)
		{
			_glfwFocusWindow(window);
		}

		/// <summary>
		/// Returns the monitor that the window uses for full screen mode.
		/// </summary>
		/// <remarks>
		/// This function returns the handle of the monitor that the specified window is
		/// in full screen on.
		/// </remarks>
		/// <param name="window">
		/// The window to query.
		/// </param>
		/// <returns>
		/// The monitor, or `NULL` if the window is in windowed mode or an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetWindowMonitor(IntPtr window)
		{
			return _glfwGetWindowMonitor(window);
		}

		/// <summary>
		/// Sets the mode, monitor, video mode and placement of a window.
		/// </summary>
		/// <remarks>
		/// This function sets the monitor that the window uses for full screen mode or,
		/// if the monitor is `NULL`, makes it windowed mode.
		/// </remarks>
		/// <param name="window">
		/// The window whose monitor, size or video mode to set.
		/// </param>
		/// <param name="monitor">
		/// The desired monitor, or `NULL` to set windowed mode.
		/// </param>
		/// <param name="xpos">
		/// The desired x-coordinate of the upper-left corner of the
		/// client area.
		/// </param>
		/// <param name="ypos">
		/// The desired y-coordinate of the upper-left corner of the
		/// client area.
		/// </param>
		/// <param name="width">
		/// The desired with, in screen coordinates, of the client area
		/// or video mode.
		/// </param>
		/// <param name="height">
		/// The desired height, in screen coordinates, of the client
		/// area or video mode.
		/// </param>
		/// <param name="refreshRate">
		/// The desired refresh rate, in Hz, of the video mode,
		/// or `GLFW_DONT_CARE`.
		/// </param>
		public static void glfwSetWindowMonitor(IntPtr window, IntPtr monitor, int xpos, int ypos, int width, int height, int refreshRate)
		{
			_glfwSetWindowMonitor(window, monitor, xpos, ypos, width, height, refreshRate);
		}

		/// <summary>
		/// Returns an attribute of the specified window.
		/// </summary>
		/// <remarks>
		/// This function returns the value of an attribute of the specified window or
		/// its OpenGL or OpenGL ES context.
		/// </remarks>
		/// <param name="window">
		/// The window to query.
		/// </param>
		/// <param name="attrib">
		/// The [window attribute](@ref window_attribs) whose value to
		/// return.
		/// </param>
		/// <returns>
		/// The value of the attribute, or zero if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static int glfwGetWindowAttrib(IntPtr window, int attrib)
		{
			return _glfwGetWindowAttrib(window, attrib);
		}

		/// <summary>
		/// Sets the user pointer of the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the user-defined pointer of the specified window.  The
		/// current value is retained until the window is destroyed.  The initial value
		/// is `NULL`.
		/// </remarks>
		/// <param name="window">
		/// The window whose pointer to set.
		/// </param>
		/// <param name="pointer">
		/// The new value.
		/// </param>
		public static void glfwSetWindowUserPointer(IntPtr window, IntPtr pointer)
		{
			_glfwSetWindowUserPointer(window, pointer);
		}

		/// <summary>
		/// Returns the user pointer of the specified window.
		/// </summary>
		/// <remarks>
		/// This function returns the current value of the user-defined pointer of the
		/// specified window.  The initial value is `NULL`.
		/// </remarks>
		/// <param name="window">
		/// The window whose pointer to return.
		/// </param>
		public static IntPtr glfwGetWindowUserPointer(IntPtr window)
		{
			return _glfwGetWindowUserPointer(window);
		}

		/// <summary>
		/// Sets the position callback for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the position callback of the specified window, which is
		/// called when the window is moved.  The callback is provided with the screen
		/// position of the upper-left corner of the client area of the window.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWwindowposfun glfwSetWindowPosCallback(IntPtr window, GLFWwindowposfun cbfun)
		{
			return _glfwSetWindowPosCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the size callback for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the size callback of the specified window, which is
		/// called when the window is resized.  The callback is provided with the size,
		/// in screen coordinates, of the client area of the window.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWwindowsizefun glfwSetWindowSizeCallback(IntPtr window, GLFWwindowsizefun cbfun)
		{
			return _glfwSetWindowSizeCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the close callback for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the close callback of the specified window, which is
		/// called when the user attempts to close the window, for example by clicking
		/// the close widget in the title bar.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWwindowclosefun glfwSetWindowCloseCallback(IntPtr window, GLFWwindowclosefun cbfun)
		{
			return _glfwSetWindowCloseCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the refresh callback for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the refresh callback of the specified window, which is
		/// called when the client area of the window needs to be redrawn, for example
		/// if the window has been exposed after having been covered by another window.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWwindowrefreshfun glfwSetWindowRefreshCallback(IntPtr window, GLFWwindowrefreshfun cbfun)
		{
			return _glfwSetWindowRefreshCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the focus callback for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the focus callback of the specified window, which is
		/// called when the window gains or loses input focus.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWwindowfocusfun glfwSetWindowFocusCallback(IntPtr window, GLFWwindowfocusfun cbfun)
		{
			return _glfwSetWindowFocusCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the iconify callback for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the iconification callback of the specified window, which
		/// is called when the window is iconified or restored.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWwindowiconifyfun glfwSetWindowIconifyCallback(IntPtr window, GLFWwindowiconifyfun cbfun)
		{
			return _glfwSetWindowIconifyCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the framebuffer resize callback for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets the framebuffer resize callback of the specified window,
		/// which is called when the framebuffer of the specified window is resized.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWframebuffersizefun glfwSetFramebufferSizeCallback(IntPtr window, GLFWframebuffersizefun cbfun)
		{
			return _glfwSetFramebufferSizeCallback(window, cbfun);
		}

		/// <summary>
		/// Processes all pending events.
		/// </summary>
		/// <remarks>
		/// This function processes only those events that are already in the event
		/// queue and then returns immediately.  Processing events will cause the window
		/// and input callbacks associated with those events to be called.
		/// </remarks>
		public static void glfwPollEvents()
		{
			_glfwPollEvents();
		}

		/// <summary>
		/// Waits until events are queued and processes them.
		/// </summary>
		/// <remarks>
		/// This function puts the calling thread to sleep until at least one event is
		/// available in the event queue.  Once one or more events are available,
		/// it behaves exactly like @ref glfwPollEvents, i.e. the events in the queue
		/// are processed and the function then returns immediately.  Processing events
		/// will cause the window and input callbacks associated with those events to be
		/// called.
		/// </remarks>
		public static void glfwWaitEvents()
		{
			_glfwWaitEvents();
		}

		/// <summary>
		/// Waits with timeout until events are queued and processes them.
		/// </summary>
		/// <remarks>
		/// This function puts the calling thread to sleep until at least one event is
		/// available in the event queue, or until the specified timeout is reached.  If
		/// one or more events are available, it behaves exactly like @ref
		/// glfwPollEvents, i.e. the events in the queue are processed and the function
		/// then returns immediately.  Processing events will cause the window and input
		/// callbacks associated with those events to be called.
		/// </remarks>
		/// <param name="timeout">
		/// The maximum amount of time, in seconds, to wait.
		/// </param>
		public static void glfwWaitEventsTimeout(double timeout)
		{
			_glfwWaitEventsTimeout(timeout);
		}

		/// <summary>
		/// Posts an empty event to the event queue.
		/// </summary>
		/// <remarks>
		/// This function posts an empty event from the current thread to the event
		/// queue, causing @ref glfwWaitEvents or @ref glfwWaitEventsTimeout to return.
		/// </remarks>
		public static void glfwPostEmptyEvent()
		{
			_glfwPostEmptyEvent();
		}

		/// <summary>
		/// Returns the value of an input option for the specified window.
		/// </summary>
		/// <remarks>
		/// This function returns the value of an input option for the specified window.
		/// The mode must be one of `GLFW_CURSOR`, `GLFW_STICKY_KEYS` or
		/// `GLFW_STICKY_MOUSE_BUTTONS`.
		/// </remarks>
		/// <param name="window">
		/// The window to query.
		/// </param>
		/// <param name="mode">
		/// One of `GLFW_CURSOR`, `GLFW_STICKY_KEYS` or
		/// `GLFW_STICKY_MOUSE_BUTTONS`.
		/// </param>
		public static int glfwGetInputMode(IntPtr window, int mode)
		{
			return _glfwGetInputMode(window, mode);
		}

		/// <summary>
		/// Sets an input option for the specified window.
		/// </summary>
		/// <remarks>
		/// This function sets an input mode option for the specified window.  The mode
		/// must be one of `GLFW_CURSOR`, `GLFW_STICKY_KEYS` or
		/// `GLFW_STICKY_MOUSE_BUTTONS`.
		/// </remarks>
		/// <param name="window">
		/// The window whose input mode to set.
		/// </param>
		/// <param name="mode">
		/// One of `GLFW_CURSOR`, `GLFW_STICKY_KEYS` or
		/// `GLFW_STICKY_MOUSE_BUTTONS`.
		/// </param>
		/// <param name="value">
		/// The new value of the specified input mode.
		/// </param>
		public static void glfwSetInputMode(IntPtr window, int mode, int value)
		{
			_glfwSetInputMode(window, mode, value);
		}

		/// <summary>
		/// Returns the localized name of the specified printable key.
		/// </summary>
		/// <remarks>
		/// This function returns the localized name of the specified printable key.
		/// This is intended for displaying key bindings to the user.
		/// </remarks>
		/// <param name="key">
		/// The key to query, or `GLFW_KEY_UNKNOWN`.
		/// </param>
		/// <param name="scancode">
		/// The scancode of the key to query.
		/// </param>
		/// <returns>
		/// The localized name of the key, or `NULL`.
		/// </returns>
		public static IntPtr glfwGetKeyName(int key, int scancode)
		{
			return _glfwGetKeyName(key, scancode);
		}

		/// <summary>
		/// Returns the last reported state of a keyboard key for the specified
		/// window.
		/// </summary>
		/// <remarks>
		/// This function returns the last state reported for the specified key to the
		/// specified window.  The returned state is one of `GLFW_PRESS` or
		/// `GLFW_RELEASE`.  The higher-level action `GLFW_REPEAT` is only reported to
		/// the key callback.
		/// </remarks>
		/// <param name="window">
		/// The desired window.
		/// </param>
		/// <param name="key">
		/// The desired [keyboard key](@ref keys).  `GLFW_KEY_UNKNOWN` is
		/// not a valid key for this function.
		/// </param>
		/// <returns>
		/// One of `GLFW_PRESS` or `GLFW_RELEASE`.
		/// </returns>
		public static int glfwGetKey(IntPtr window, int key)
		{
			return _glfwGetKey(window, key);
		}

		/// <summary>
		/// Returns the last reported state of a mouse button for the specified
		/// window.
		/// </summary>
		/// <remarks>
		/// This function returns the last state reported for the specified mouse button
		/// to the specified window.  The returned state is one of `GLFW_PRESS` or
		/// `GLFW_RELEASE`.
		/// </remarks>
		/// <param name="window">
		/// The desired window.
		/// </param>
		/// <param name="button">
		/// The desired [mouse button](@ref buttons).
		/// </param>
		/// <returns>
		/// One of `GLFW_PRESS` or `GLFW_RELEASE`.
		/// </returns>
		public static int glfwGetMouseButton(IntPtr window, int button)
		{
			return _glfwGetMouseButton(window, button);
		}

		/// <summary>
		/// Retrieves the position of the cursor relative to the client area of
		/// the window.
		/// </summary>
		/// <remarks>
		/// This function returns the position of the cursor, in screen coordinates,
		/// relative to the upper-left corner of the client area of the specified
		/// window.
		/// </remarks>
		/// <param name="window">
		/// The desired window.
		/// </param>
		/// <param name="xpos">
		/// Where to store the cursor x-coordinate, relative to the
		/// left edge of the client area, or `NULL`.
		/// </param>
		/// <param name="ypos">
		/// Where to store the cursor y-coordinate, relative to the to
		/// top edge of the client area, or `NULL`.
		/// </param>
		public static void glfwGetCursorPos(IntPtr window, out double xpos, out double ypos)
		{
			_glfwGetCursorPos(window, out xpos, out ypos);
		}

		/// <summary>
		/// Sets the position of the cursor, relative to the client area of the
		/// window.
		/// </summary>
		/// <remarks>
		/// This function sets the position, in screen coordinates, of the cursor
		/// relative to the upper-left corner of the client area of the specified
		/// window.  The window must have input focus.  If the window does not have
		/// input focus when this function is called, it fails silently.
		/// </remarks>
		/// <param name="window">
		/// The desired window.
		/// </param>
		/// <param name="xpos">
		/// The desired x-coordinate, relative to the left edge of the
		/// client area.
		/// </param>
		/// <param name="ypos">
		/// The desired y-coordinate, relative to the top edge of the
		/// client area.
		/// </param>
		public static void glfwSetCursorPos(IntPtr window, double xpos, double ypos)
		{
			_glfwSetCursorPos(window, xpos, ypos);
		}

		/// <summary>
		/// Creates a custom cursor.
		/// </summary>
		/// <remarks>
		/// Creates a new custom cursor image that can be set for a window with @ref
		/// glfwSetCursor.  The cursor can be destroyed with @ref glfwDestroyCursor.
		/// Any remaining cursors are destroyed by @ref glfwTerminate.
		/// </remarks>
		/// <param name="image">
		/// The desired cursor image.
		/// </param>
		/// <param name="xhot">
		/// The desired x-coordinate, in pixels, of the cursor hotspot.
		/// </param>
		/// <param name="yhot">
		/// The desired y-coordinate, in pixels, of the cursor hotspot.
		/// </param>
		/// <returns>
		/// The handle of the created cursor, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwCreateCursor(IntPtr image, int xhot, int yhot)
		{
			return _glfwCreateCursor(image, xhot, yhot);
		}

		/// <summary>
		/// Creates a cursor with a standard shape.
		/// </summary>
		/// <remarks>
		/// Returns a cursor with a [standard shape](@ref shapes), that can be set for
		/// a window with @ref glfwSetCursor.
		/// </remarks>
		/// <param name="shape">
		/// One of the [standard shapes](@ref shapes).
		/// </param>
		/// <returns>
		/// A new cursor ready to use or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwCreateStandardCursor(int shape)
		{
			return _glfwCreateStandardCursor(shape);
		}

		/// <summary>
		/// Destroys a cursor.
		/// </summary>
		/// <remarks>
		/// This function destroys a cursor previously created with @ref
		/// glfwCreateCursor.  Any remaining cursors will be destroyed by @ref
		/// glfwTerminate.
		/// </remarks>
		/// <param name="cursor">
		/// The cursor object to destroy.
		/// </param>
		public static void glfwDestroyCursor(IntPtr cursor)
		{
			_glfwDestroyCursor(cursor);
		}

		/// <summary>
		/// Sets the cursor for the window.
		/// </summary>
		/// <remarks>
		/// This function sets the cursor image to be used when the cursor is over the
		/// client area of the specified window.  The set cursor will only be visible
		/// when the [cursor mode](@ref cursor_mode) of the window is
		/// `GLFW_CURSOR_NORMAL`.
		/// </remarks>
		/// <param name="window">
		/// The window to set the cursor for.
		/// </param>
		/// <param name="cursor">
		/// The cursor to set, or `NULL` to switch back to the default
		/// arrow cursor.
		/// </param>
		public static void glfwSetCursor(IntPtr window, IntPtr cursor)
		{
			_glfwSetCursor(window, cursor);
		}

		/// <summary>
		/// Sets the key callback.
		/// </summary>
		/// <remarks>
		/// This function sets the key callback of the specified window, which is called
		/// when a key is pressed, repeated or released.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new key callback, or `NULL` to remove the currently
		/// set callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWkeyfun glfwSetKeyCallback(IntPtr window, GLFWkeyfun cbfun)
		{
			return _glfwSetKeyCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the Unicode character callback.
		/// </summary>
		/// <remarks>
		/// This function sets the character callback of the specified window, which is
		/// called when a Unicode character is input.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWcharfun glfwSetCharCallback(IntPtr window, GLFWcharfun cbfun)
		{
			return _glfwSetCharCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the Unicode character with modifiers callback.
		/// </summary>
		/// <remarks>
		/// This function sets the character with modifiers callback of the specified
		/// window, which is called when a Unicode character is input regardless of what
		/// modifier keys are used.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static GLFWcharmodsfun glfwSetCharModsCallback(IntPtr window, GLFWcharmodsfun cbfun)
		{
			return _glfwSetCharModsCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the mouse button callback.
		/// </summary>
		/// <remarks>
		/// This function sets the mouse button callback of the specified window, which
		/// is called when a mouse button is pressed or released.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWmousebuttonfun glfwSetMouseButtonCallback(IntPtr window, GLFWmousebuttonfun cbfun)
		{
			return _glfwSetMouseButtonCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the cursor position callback.
		/// </summary>
		/// <remarks>
		/// This function sets the cursor position callback of the specified window,
		/// which is called when the cursor is moved.  The callback is provided with the
		/// position, in screen coordinates, relative to the upper-left corner of the
		/// client area of the window.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWcursorposfun glfwSetCursorPosCallback(IntPtr window, GLFWcursorposfun cbfun)
		{
			return _glfwSetCursorPosCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the cursor enter/exit callback.
		/// </summary>
		/// <remarks>
		/// This function sets the cursor boundary crossing callback of the specified
		/// window, which is called when the cursor enters or leaves the client area of
		/// the window.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWcursorenterfun glfwSetCursorEnterCallback(IntPtr window, GLFWcursorenterfun cbfun)
		{
			return _glfwSetCursorEnterCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the scroll callback.
		/// </summary>
		/// <remarks>
		/// This function sets the scroll callback of the specified window, which is
		/// called when a scrolling device is used, such as a mouse wheel or scrolling
		/// area of a touchpad.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new scroll callback, or `NULL` to remove the currently
		/// set callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWscrollfun glfwSetScrollCallback(IntPtr window, GLFWscrollfun cbfun)
		{
			return _glfwSetScrollCallback(window, cbfun);
		}

		/// <summary>
		/// Sets the file drop callback.
		/// </summary>
		/// <remarks>
		/// This function sets the file drop callback of the specified window, which is
		/// called when one or more dragged files are dropped on the window.
		/// </remarks>
		/// <param name="window">
		/// The window whose callback to set.
		/// </param>
		/// <param name="cbfun">
		/// The new file drop callback, or `NULL` to remove the
		/// currently set callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWdropfun glfwSetDropCallback(IntPtr window, GLFWdropfun cbfun)
		{
			return _glfwSetDropCallback(window, cbfun);
		}

		/// <summary>
		/// Returns whether the specified joystick is present.
		/// </summary>
		/// <remarks>
		/// This function returns whether the specified joystick is present.
		/// </remarks>
		/// <param name="joy">
		/// The [joystick](@ref joysticks) to query.
		/// </param>
		/// <returns>
		/// `GLFW_TRUE` if the joystick is present, or `GLFW_FALSE` otherwise.
		/// </returns>
		public static int glfwJoystickPresent(int joy)
		{
			return _glfwJoystickPresent(joy);
		}

		/// <summary>
		/// Returns the values of all axes of the specified joystick.
		/// </summary>
		/// <remarks>
		/// This function returns the values of all axes of the specified joystick.
		/// Each element in the array is a value between -1.0 and 1.0.
		/// </remarks>
		/// <param name="joy">
		/// The [joystick](@ref joysticks) to query.
		/// </param>
		/// <param name="count">
		/// Where to store the number of axis values in the returned
		/// array.  This is set to zero if the joystick is not present or an error
		/// occurred.
		/// </param>
		/// <returns>
		/// An array of axis values, or `NULL` if the joystick is not present or
		/// an [error](@ref error_handling) occurred.
		/// </returns>
		public static float[] glfwGetJoystickAxes(int joy)
		{
			var arrayPtr = _glfwGetJoystickAxes(joy, out int count);

			if (arrayPtr == IntPtr.Zero)
				return null;

			var result = new float[count];
			Marshal.Copy(arrayPtr, result, 0, count);

			return result;
		}

		/// <summary>
		/// Returns the state of all buttons of the specified joystick.
		/// </summary>
		/// <remarks>
		/// This function returns the state of all buttons of the specified joystick.
		/// Each element in the array is either `GLFW_PRESS` or `GLFW_RELEASE`.
		/// </remarks>
		/// <param name="joy">
		/// The [joystick](@ref joysticks) to query.
		/// </param>
		/// <param name="count">
		/// Where to store the number of button states in the returned
		/// array.  This is set to zero if the joystick is not present or an error
		/// occurred.
		/// </param>
		/// <returns>
		/// An array of button states, or `NULL` if the joystick is not present
		/// or an [error](@ref error_handling) occurred.
		/// </returns>
		public static byte[] glfwGetJoystickButtons(int joy)
		{
			var arrayPtr = _glfwGetJoystickButtons(joy, out int count);

			var result = new byte[count];
			Marshal.Copy(arrayPtr, result, 0, count);

			return result;
		}

		/// <summary>
		/// Returns the name of the specified joystick.
		/// </summary>
		/// <remarks>
		/// This function returns the name, encoded as UTF-8, of the specified joystick.
		/// The returned string is allocated and freed by GLFW.  You should not free it
		/// yourself.
		/// </remarks>
		/// <param name="joy">
		/// The [joystick](@ref joysticks) to query.
		/// </param>
		/// <returns>
		/// The UTF-8 encoded name of the joystick, or `NULL` if the joystick
		/// is not present or an [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetJoystickName(int joy)
		{
			return _glfwGetJoystickName(joy);
		}

		/// <summary>
		/// Sets the joystick configuration callback.
		/// </summary>
		/// <remarks>
		/// This function sets the joystick configuration callback, or removes the
		/// currently set callback.  This is called when a joystick is connected to or
		/// disconnected from the system.
		/// </remarks>
		/// <param name="cbfun">
		/// The new callback, or `NULL` to remove the currently set
		/// callback.
		/// </param>
		/// <returns>
		/// The previously set callback, or `NULL` if no callback was set or the
		/// library had not been [initialized](@ref intro_init).
		/// </returns>
		public static GLFWjoystickfun glfwSetJoystickCallback(GLFWjoystickfun cbfun)
		{
			return _glfwSetJoystickCallback(cbfun);
		}

		/// <summary>
		/// Sets the clipboard to the specified string.
		/// </summary>
		/// <remarks>
		/// This function sets the system clipboard to the specified, UTF-8 encoded
		/// string.
		/// </remarks>
		/// <param name="window">
		/// The window that will own the clipboard contents.
		/// </param>
		/// <param name="string">
		/// A UTF-8 encoded string.
		/// </param>
		public static void glfwSetClipboardString(IntPtr window, string @string)
		{
			_glfwSetClipboardString(window, @string);
		}

		/// <summary>
		/// Returns the contents of the clipboard as a string.
		/// </summary>
		/// <remarks>
		/// This function returns the contents of the system clipboard, if it contains
		/// or is convertible to a UTF-8 encoded string.  If the clipboard is empty or
		/// if its contents cannot be converted, `NULL` is returned and a @ref
		/// GLFW_FORMAT_UNAVAILABLE error is generated.
		/// </remarks>
		/// <param name="window">
		/// The window that will request the clipboard contents.
		/// </param>
		/// <returns>
		/// The contents of the clipboard as a UTF-8 encoded string, or `NULL`
		/// if an [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetClipboardString(IntPtr window)
		{
			return _glfwGetClipboardString(window);
		}

		/// <summary>
		/// Returns the value of the GLFW timer.
		/// </summary>
		/// <remarks>
		/// This function returns the value of the GLFW timer.  Unless the timer has
		/// been set using @ref glfwSetTime, the timer measures time elapsed since GLFW
		/// was initialized.
		/// </remarks>
		/// <returns>
		/// The current value, in seconds, or zero if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static double glfwGetTime()
		{
			return _glfwGetTime();
		}

		/// <summary>
		/// Sets the GLFW timer.
		/// </summary>
		/// <remarks>
		/// This function sets the value of the GLFW timer.  It then continues to count
		/// up from that value.  The value must be a positive finite number less than
		/// or equal to 18446744073.0, which is approximately 584.5 years.
		/// </remarks>
		/// <param name="time">
		/// The new value, in seconds.
		/// </param>
		public static void glfwSetTime(double time)
		{
			_glfwSetTime(time);
		}

		/// <summary>
		/// Returns the current value of the raw timer.
		/// </summary>
		/// <remarks>
		/// This function returns the current value of the raw timer, measured in
		/// 1&nbsp;/&nbsp;frequency seconds.  To get the frequency, call @ref
		/// glfwGetTimerFrequency.
		/// </remarks>
		/// <returns>
		/// The value of the timer, or zero if an 
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static ulong glfwGetTimerValue()
		{
			return _glfwGetTimerValue();
		}

		/// <summary>
		/// Returns the frequency, in Hz, of the raw timer.
		/// </summary>
		/// <remarks>
		/// This function returns the frequency, in Hz, of the raw timer.
		/// </remarks>
		/// <returns>
		/// The frequency of the timer, in Hz, or zero if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static ulong glfwGetTimerFrequency()
		{
			return _glfwGetTimerFrequency();
		}

		/// <summary>
		/// Makes the context of the specified window current for the calling
		/// thread.
		/// </summary>
		/// <remarks>
		/// This function makes the OpenGL or OpenGL ES context of the specified window
		/// current on the calling thread.  A context can only be made current on
		/// a single thread at a time and each thread can have only a single current
		/// context at a time.
		/// </remarks>
		/// <param name="window">
		/// The window whose context to make current, or `NULL` to
		/// detach the current context.
		/// </param>
		public static void glfwMakeContextCurrent(IntPtr window)
		{
			_glfwMakeContextCurrent(window);
		}

		/// <summary>
		/// Returns the window whose context is current on the calling thread.
		/// </summary>
		/// <remarks>
		/// This function returns the window whose OpenGL or OpenGL ES context is
		/// current on the calling thread.
		/// </remarks>
		/// <returns>
		/// The window whose context is current, or `NULL` if no window's
		/// context is current.
		/// </returns>
		public static IntPtr glfwGetCurrentContext()
		{
			return _glfwGetCurrentContext();
		}

		/// <summary>
		/// Swaps the front and back buffers of the specified window.
		/// </summary>
		/// <remarks>
		/// This function swaps the front and back buffers of the specified window when
		/// rendering with OpenGL or OpenGL ES.  If the swap interval is greater than
		/// zero, the GPU driver waits the specified number of screen updates before
		/// swapping the buffers.
		/// </remarks>
		/// <param name="window">
		/// The window whose buffers to swap.
		/// </param>
		public static void glfwSwapBuffers(IntPtr window)
		{
			_glfwSwapBuffers(window);
		}

		/// <summary>
		/// Sets the swap interval for the current context.
		/// </summary>
		/// <remarks>
		/// This function sets the swap interval for the current OpenGL or OpenGL ES
		/// context, i.e. the number of screen updates to wait from the time @ref
		/// glfwSwapBuffers was called before swapping the buffers and returning.  This
		/// is sometimes called _vertical synchronization_, _vertical retrace
		/// synchronization_ or just _vsync_.
		/// </remarks>
		/// <param name="interval">
		/// The minimum number of screen updates to wait for
		/// until the buffers are swapped by @ref glfwSwapBuffers.
		/// </param>
		public static void glfwSwapInterval(int interval)
		{
			_glfwSwapInterval(interval);
		}

		/// <summary>
		/// Returns whether the specified extension is available.
		/// </summary>
		/// <remarks>
		/// This function returns whether the specified
		/// [API extension](@ref context_glext) is supported by the current OpenGL or
		/// OpenGL ES context.  It searches both for client API extension and context
		/// creation API extensions.
		/// </remarks>
		/// <param name="extension">
		/// The ASCII encoded name of the extension.
		/// </param>
		/// <returns>
		/// `GLFW_TRUE` if the extension is available, or `GLFW_FALSE`
		/// otherwise.
		/// </returns>
		public static int glfwExtensionSupported(string extension)
		{
			return _glfwExtensionSupported(extension);
		}

		/// <summary>
		/// Returns the address of the specified function for the current
		/// context.
		/// </summary>
		/// <remarks>
		/// This function returns the address of the specified OpenGL or OpenGL ES
		/// [core or extension function](@ref context_glext), if it is supported
		/// by the current context.
		/// </remarks>
		/// <param name="procname">
		/// The ASCII encoded name of the function.
		/// </param>
		/// <returns>
		/// The address of the function, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetProcAddress(string procname)
		{
			return _glfwGetProcAddress(procname);
		}

		/// <summary>
		/// Returns whether the Vulkan loader has been found.
		/// </summary>
		/// <remarks>
		/// This function returns whether the Vulkan loader has been found.  This check
		/// is performed by @ref glfwInit.
		/// </remarks>
		/// <returns>
		/// `GLFW_TRUE` if Vulkan is available, or `GLFW_FALSE` otherwise.
		/// </returns>
		public static int glfwVulkanSupported()
		{
			return _glfwVulkanSupported();
		}

		/// <summary>
		/// Returns the Vulkan instance extensions required by GLFW.
		/// </summary>
		/// <remarks>
		/// This function returns an array of names of Vulkan instance extensions required
		/// by GLFW for creating Vulkan surfaces for GLFW windows.  If successful, the
		/// list will always contains `VK_KHR_surface`, so if you don't require any
		/// additional extensions you can pass this list directly to the
		/// `VkInstanceCreateInfo` struct.
		/// </remarks>
		/// <param name="count">
		/// Where to store the number of extensions in the returned
		/// array.  This is set to zero if an error occurred.
		/// </param>
		/// <returns>
		/// An array of ASCII encoded extension names, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static string[] glfwGetRequiredInstanceExtensions()
		{
			var arrayPtr = _glfwGetRequiredInstanceExtensions(out uint count);

			if (arrayPtr == IntPtr.Zero)
				return null;

			var result = new string[count];

			for (int i = 0; i < count; i++)
			{
				IntPtr stringPtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
				result[i] = Marshal.PtrToStringAnsi(stringPtr);
			}

			return result;
		}

		/// <summary>
		/// Returns the address of the specified Vulkan instance function.
		/// </summary>
		/// <remarks>
		/// This function returns the address of the specified Vulkan core or extension
		/// function for the specified instance.  If instance is set to `NULL` it can
		/// return any function exported from the Vulkan loader, including at least the
		/// following functions:
		/// </remarks>
		/// <param name="instance">
		/// The Vulkan instance to query, or `NULL` to retrieve
		/// functions related to instance creation.
		/// </param>
		/// <param name="procname">
		/// The ASCII encoded name of the function.
		/// </param>
		/// <returns>
		/// The address of the function, or `NULL` if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static IntPtr glfwGetInstanceProcAddress(IntPtr instance, string procname)
		{
			return _glfwGetInstanceProcAddress(instance, procname);
		}

		/// <summary>
		/// Returns whether the specified queue family can present images.
		/// </summary>
		/// <remarks>
		/// This function returns whether the specified queue family of the specified
		/// physical device supports presentation to the platform GLFW was built for.
		/// </remarks>
		/// <param name="instance">
		/// The instance that the physical device belongs to.
		/// </param>
		/// <param name="device">
		/// The physical device that the queue family belongs to.
		/// </param>
		/// <param name="queuefamily">
		/// The index of the queue family to query.
		/// </param>
		/// <returns>
		/// `GLFW_TRUE` if the queue family supports presentation, or
		/// `GLFW_FALSE` otherwise.
		/// </returns>
		public static int glfwGetPhysicalDevicePresentationSupport(IntPtr instance, IntPtr device, uint queuefamily)
		{
			return _glfwGetPhysicalDevicePresentationSupport(instance, device, queuefamily);
		}

		/// <summary>
		/// Creates a Vulkan surface for the specified window.
		/// </summary>
		/// <remarks>
		/// This function creates a Vulkan surface for the specified window.
		/// </remarks>
		/// <param name="instance">
		/// The Vulkan instance to create the surface in.
		/// </param>
		/// <param name="window">
		/// The window to create the surface for.
		/// </param>
		/// <param name="allocator">
		/// The allocator to use, or `NULL` to use the default
		/// allocator.
		/// </param>
		/// <param name="surface">
		/// Where to store the handle of the surface.  This is set
		/// to `VK_NULL_HANDLE` if an error occurred.
		/// </param>
		/// <returns>
		/// `VK_SUCCESS` if successful, or a Vulkan error code if an
		/// [error](@ref error_handling) occurred.
		/// </returns>
		public static int glfwCreateWindowSurface(IntPtr instance, IntPtr window, IntPtr allocator, out IntPtr surface)
		{
			return _glfwCreateWindowSurface(instance, window, allocator, out surface);
		}

	}
}
