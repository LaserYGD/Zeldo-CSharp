#version 440 core

in vec2 fTexCoords;
in vec4 fColor;

out vec4 fragColor;

uniform sampler2D shadowMap;

void main()
{
	float depth = texture(shadowMap, fTexCoords).r;
	float t = 0.01;

	if (depth > 0 && depth <= t)
	{
		fragColor = vec4(0, 1, 0, 1);
	}
	else if (depth < 1 && depth > t)
	{
		fragColor = vec4(1, 0, 0, 1);
	}
	else
	{
		fragColor = fColor * vec4(vec3(depth), 1);
	}
}
