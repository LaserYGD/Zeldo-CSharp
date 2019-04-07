﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Exceptions;
using static Engine.GL;

namespace Engine.Core._2D
{
	public class RenderTarget : QuadSource
	{
		private uint framebufferId;
		private uint renderbufferId;
		private uint textureId;

		private bool colorEnabled;
		private bool depthEnabled;
		private bool stencilEnabled;

		public unsafe RenderTarget(int width, int height, RenderTargetFlags flags) : base(width, height)
		{
			fixed (uint* address = &framebufferId)
			{
				glGenFramebuffers(1, address);
			}

			glBindFramebuffer(GL_FRAMEBUFFER, framebufferId);

			fixed (uint* address = &textureId)
			{
				glGenTextures(1, address);
			}

			glBindTexture(GL_TEXTURE_2D, textureId);

			colorEnabled = (flags & RenderTargetFlags.Color) > 0;
			depthEnabled = (flags & RenderTargetFlags.Depth) > 0;
			stencilEnabled = (flags & RenderTargetFlags.Stencil) > 0;

			uint texFormat;
			uint texType;
			uint texAttachment;

			if (colorEnabled)
			{
				texFormat = GL_RGBA;
				texType = GL_UNSIGNED_BYTE;
				texAttachment = GL_COLOR_ATTACHMENT0;
			}
			else
			{
				texFormat = GL_DEPTH_COMPONENT;
				texType = GL_FLOAT;
				texAttachment = GL_DEPTH_ATTACHMENT;
			}

			glTexImage2D(GL_TEXTURE_2D, 0, (int)texFormat, (uint)width, (uint)height, 0, texFormat, texType, null);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
			glFramebufferTexture2D(GL_FRAMEBUFFER, texAttachment, GL_TEXTURE_2D, textureId, 0);

			if (!colorEnabled)
			{
				glDrawBuffer(GL_NONE);
				glReadBuffer(GL_NONE);
			}
			else if (depthEnabled)
			{
				uint format = (uint)(stencilEnabled ? GL_DEPTH24_STENCIL8 : GL_DEPTH_COMPONENT);
				uint attachment = (uint)(stencilEnabled ? GL_DEPTH_STENCIL_ATTACHMENT : GL_DEPTH_ATTACHMENT);

				fixed (uint* address = &renderbufferId)
				{
					glGenRenderbuffers(1, address);
				}

				glBindRenderbuffer(GL_RENDERBUFFER, renderbufferId);
				glRenderbufferStorage(GL_RENDERBUFFER, format, (uint)width, (uint)height);
				glFramebufferRenderbuffer(GL_FRAMEBUFFER, attachment, GL_RENDERBUFFER, renderbufferId);
				glBindRenderbuffer(GL_RENDERBUFFER, 0);
			}

			if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
			{
				throw new RenderTargetException("Error creating render target.");
			}

			glBindFramebuffer(GL_FRAMEBUFFER, 0);
		}
	}
}
