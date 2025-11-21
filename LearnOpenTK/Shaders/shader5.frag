#version 330 core

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

uniform vec3 lightPos;
uniform vec3 lightColor;
uniform sampler2D ourTexture;

out vec4 color;

void main()
{
    // мы получаем вектор который указывает на наш фрагмент
    vec3 lightDir = normalize(lightPos - FragPos);

    // вычисляем степень прямоты падения света
    float diff = max(dot(Normal, lightDir), 0.00f) * 0.85f + 0.15f;
    vec4 diffuse = vec4(diff * lightColor, 1.0f);

    vec4 textureColor = texture(ourTexture, TexCoord);
    color = diffuse * textureColor;
}
