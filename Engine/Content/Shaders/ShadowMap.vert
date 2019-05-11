#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in ivec2 boneIndexes;
layout (location = 2) in vec2 boneWeights;

uniform mat4 lightMatrix;
uniform vec4 bones[2];

void main()
{
	vec4 position = lightMatrix * vec4(vPosition, 1);

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

		position *= bones[index] * boneWeights[i];
	}

	gl_Position = position;
}
