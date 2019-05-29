#version 440 core

in vec2 fTexCoords;
in vec4 fColor;

out vec4 fragColor;

uniform sampler2D image;
uniform vec3 lightDirection;
uniform mat3 orientation;

void main()
{
	vec3 normal = texture(image, fTexCoords).rgb;
	normal = normalize(normal * 2 - 1) * orientation;

	float d = dot(normal, lightDirection);

	fragColor = fColor * d;
}
