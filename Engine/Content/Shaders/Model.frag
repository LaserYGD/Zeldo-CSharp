#version 440 core

in vec2 fSource;
in vec3 fNormal;

out vec4 fragColor;

uniform vec3 lightColor;
uniform vec3 lightDirection;
uniform float ambientIntensity;
uniform sampler2D textureSampler;

void main()
{
	vec4 color = texture(textureSampler, fSource);

	float d = dot(-lightDirection, fNormal);
	float diffuse = clamp(d, 0, 1);
	float combined = clamp(ambientIntensity + diffuse, 0, 1);
	
	fragColor = color * vec4(lightColor * combined, 1);
}
