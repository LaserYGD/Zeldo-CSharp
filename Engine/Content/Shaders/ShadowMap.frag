#version 440 core

in vec2 fSource;

uniform sampler2D image;

void main()
{
	vec4 color = texture(image, fSource);

	// Only non-transparent pixels cast shadows.
	gl_FragDepth = color.a == 0 ? 1 : gl_FragCoord.z;
}
