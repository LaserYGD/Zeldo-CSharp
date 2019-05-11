#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vSource;
layout (location = 2) in vec3 vNormal;
layout (location = 3) in ivec2 boneIndexes;
layout (location = 4) in vec2 boneWeights;

out vec2 fSource;
out vec3 fNormal;
out vec4 fShadowMapCoords;

uniform mat4 orientation;
uniform mat4 mvp;
uniform mat4 lightBiasMatrix;
uniform vec4 bones[2];

void main()
{
	vec4 position = mvp * vec4(vPosition, 1);
	vec4 normal = orientation * vec4(vNormal, 1);

	int[] indexArray = int[2];
	indexArray[0] = boneIndexes.x;
	indexArray[1] = boneIndexes.y;

	for (int i = 0; i < 2; i++)
	{
		int index = indexArray[i];

		if (index == -1)
		{
			break;
		}

		vec4 bone = bones[index] * boneWeights[i];

		position *= bone;
		normal *= bone;
	}

	fSource = vSource;
	fNormal = normal;
	fShadowMapCoords = lightBiasMatrix * position;
}
