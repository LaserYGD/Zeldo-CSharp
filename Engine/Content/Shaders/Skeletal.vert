#version 440 core

const int Bones = 40;

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vSource;
layout (location = 2) in vec3 vNormal;
layout (location = 3) in vec2 boneWeights;
layout (location = 4) in ivec2 boneIndexes;

out vec2 fSource;
out vec3 fNormal;
out vec4 fShadowMapCoords;

uniform mat4 orientation;
uniform mat4 mvp;
uniform mat4 lightBiasMatrix;
uniform vec3 poseOrigin;
uniform vec3 defaultPose[Bones];
uniform vec3 bonePositions[Bones];
uniform vec4 boneOrientations[Bones];

vec3 quatMultiply(vec4 q, vec3 v) {
	// See https://community.khronos.org/t/quaternion-functions-for-glsl/50140/3.
	return v + 2 * cross(cross(v, q.xyz ) + q.w * v, q.xyz);
} 

void main()
{
	vec4 position = vec4(vPosition, 1);
	vec4 normal = vec4(vNormal, 1);
	
	float w1 = boneWeights.x;
	float w2 = boneWeights.y;

	vec4 q1 = boneOrientations[boneIndexes.x];
	vec4 q2 = boneOrientations[boneIndexes.y];

	int index1 = boneIndexes.x;
	int index2 = boneIndexes.y;

	vec3 pose1 = defaultPose[index1];
	vec3 pose2 = defaultPose[index2];
	vec3 localPosition1 = (vPosition + poseOrigin) - pose1;
	vec3 localPosition2 = (vPosition + poseOrigin) - pose2;
	vec3 rotated1 = (quatMultiply(q1, localPosition1) - localPosition1) * w1;
	vec3 rotated2 = (quatMultiply(q2, localPosition2) - localPosition2) * w2;
	vec3 localBone1 = bonePositions[index1] - pose1;
	vec3 localBone2 = bonePositions[index2] - pose2;

	position.xyz += poseOrigin + localBone1 * w1 + localBone2 * w2 + rotated1 + rotated2;
	
	vec3 n1 = (quatMultiply(q1, vNormal) - vNormal) * w1;
	vec3 n2 = (quatMultiply(q2, vNormal) - vNormal) * w2;

	normal.xyz += n1 + n2;

	gl_Position = mvp * position;

	fSource = vSource;
	fNormal = normalize((orientation * normal).xyz);
	fShadowMapCoords = lightBiasMatrix * position;
}
