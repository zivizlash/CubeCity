#version 330 core

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

struct Light {
    vec3 position;
    vec3 diffuse;
};

uniform Light light;  
uniform sampler2D ourTexture;

out vec4 color;

void main() 
{
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(Normal, lightDir), 0.00f) * 0.85f + 0.15f;
    vec4 diffuse = vec4(diff * light.diffuse, 1.0f);

    vec4 textureColor = texture(ourTexture, TexCoord);
    color = diffuse * textureColor;
}
