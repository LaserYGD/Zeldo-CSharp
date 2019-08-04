#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vSource;

out vec2 fSource;

uniform mat4 lightMatrix;

void main()
{
	gl_Position = lightMatrix * vec4(vPosition, 1);
	fSource = vSource;
}
