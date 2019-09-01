﻿#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vSource;
layout (location = 2) in vec3 vNormal;
layout (location = 3) in vec2 boneWeights;
layout (location = 4) in ivec2 boneIndexes;
layout (location = 5) in int boneCount;

out vec2 fSource;
out vec3 fNormal;
out vec4 fShadowMapCoords;

uniform mat4 orientation;
uniform mat4 mvp;
uniform mat4 lightBiasMatrix;
uniform mat4 bones[20];

void main()
{
	vec4 position = vec4(vPosition, 1);
	vec4 normal = orientation * vec4(vNormal, 1);

	for (int i = 0; i < boneCount; i++)
	{
		mat4 bone = bones[boneIndexes[i]] * boneWeights[i];

		position *= bone;
		normal *= bone;
	}

	gl_Position = mvp * position;

	fSource = vSource;
	fNormal = normal.xyz;
	fShadowMapCoords = lightBiasMatrix * position;
}
