#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texCoord;
layout (location = 2) in vec3 normal;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;

uniform mat4 model;
uniform mat4 transform;

void main()
{
    TexCoord = texCoord;
    Normal = normal;
    
    gl_Position = transform * model * vec4(position, 1.0f);

    vec4 pos = model * vec4(position, 1.0f);
    FragPos = vec3(pos.x, pos.y, pos.z);
}
