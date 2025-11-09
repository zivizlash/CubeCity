#version 330 core

in vec4 vertexColor;

uniform vec4 ourColor;

out vec4 color;

void main()
{
    color = ourColor;
}
