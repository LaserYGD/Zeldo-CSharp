#version 440 core

in vec2 fTexCoords;
in vec3 fNormal;

out vec4 fragColor;

uniform float ambientIntensity;
uniform vec4 lightColor;
uniform vec3 lightDirection;

void main()
{
	float intensity = 0;

	fragColor = intensity * lightColor;
}
