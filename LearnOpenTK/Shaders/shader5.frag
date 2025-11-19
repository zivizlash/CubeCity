#version 330 core

in vec2 TexCoord;

uniform sampler2D ourTexture;
uniform vec3 lightColor;

out vec4 color;

void main()
{
    color = texture(ourTexture, TexCoord) * vec4(lightColor, 1.0f);
}
