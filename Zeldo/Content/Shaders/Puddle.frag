#version 440 core

in vec2 fTexCoords;

out vec4 fragColor;

uniform sampler2D skyImage;
uniform sampler2D modelImage;

void main()
{
	// TODO: Modify tex coords based on ripple.
	vec4 model = texture(modelImage, fTexCoords);
	vec4 reflection = model.a == 1 ? model : texture(skyImage, fTexCoords);

	// TODO: Combine with the ground color (beneath the puddle).
	vec4 final = reflection;
	
	fragColor = final;
}
