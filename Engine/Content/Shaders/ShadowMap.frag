#version 440 core

//in vec2 fSource;

out float fragDepth;

//uniform sampler2D image;

void main()
{
	//vec4 color = texture(image, fSource);

	// Only non-transparent pixels cast shadows. The transparency value is close to float's max value.
	//fragDepth = color == vec4(0, 0, 0, 0) ? 3.4e38 : gl_FragCoord.z;
	fragDepth = gl_FragCoord.z;
}
