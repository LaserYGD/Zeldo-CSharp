#version 440 core

in vec2 fSource;
in vec4 fShadowMapCoords;

out vec4 fragColor;

uniform vec4 tint;
uniform mat4 orientation;
uniform vec3 lightColor;
uniform vec3 lightDirection;
uniform float ambientIntensity;
uniform sampler2D shadowSampler;
uniform sampler2D textureSampler;

void main()
{
	vec4 color = texture(textureSampler, fSource) * tint;
	vec3 normal = (vec4(0, 0, 1, 1) * orientation).xyz;

	float shadowValue = texture(shadowSampler, fShadowMapCoords.xy).r;

	// Sprites are lit from both sides (i.e. their normal kinda faces in both directions), hence the absolute value.
	float d = abs(dot(-lightDirection, normal));
	float bias = 0.001;
	float lightIntensity;

	if (fShadowMapCoords.z - bias > shadowValue)
	{
		lightIntensity = ambientIntensity;
	}
	else
	{
		float diffuse = clamp(d, 0, 1);
		float combined = clamp(ambientIntensity + diffuse, 0, 1);

		lightIntensity = combined;
	}

	fragColor = color * vec4(lightColor * lightIntensity, 1);
}
