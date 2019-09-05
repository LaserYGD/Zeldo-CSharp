#version 440 core

const int Bones = 40;

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vSource;
layout (location = 2) in vec2 boneWeights;
layout (location = 3) in ivec2 boneIndexes;

out vec2 fSource;

uniform mat4 lightMatrix;
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
	
	gl_Position = lightMatrix * position;
	fSource = vSource;
}
