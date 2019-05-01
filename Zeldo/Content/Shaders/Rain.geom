#version 440 core

layout (points) in;
layout (line_strip, max_vertices = 2) out;

// The half vector is assumed to point in the direction the rain is falling (i.e. its velocity).
uniform vec3 halfVector;
uniform mat4 mvp;

void main()
{
	vec4 p = gl_in[0].gl_Position;
	vec4 v = mvp * vec4(halfVector, 0);

	gl_Position = p - v;

	EmitVertex();

	gl_Position = p + v;

	EmitVertex();
	EndPrimitive();
}
