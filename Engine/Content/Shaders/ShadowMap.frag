#version 440 core

in vec2 fSource;

out float fragDepth;

uniform sampler2D image;

void main()
{
	vec4 color = texture(image, fSource);

	// Only non-transparent pixels cast shadows.
	fragDepth = color == vec4(0, 0, 0, 0) ? 3.4e38 : gl_FragCoord.z;
}
