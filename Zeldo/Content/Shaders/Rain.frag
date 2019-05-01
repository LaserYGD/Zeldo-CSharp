#version 440 core

out vec4 fragColor;

// The color given is assumed to already have partial transparency applied.
uniform vec4 color;

void main()
{
	fragColor = color;
}
