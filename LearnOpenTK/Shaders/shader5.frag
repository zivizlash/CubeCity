#version 330 core

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

struct DirectionalLight {
    vec3 direction;
    vec3 diffuse;
    float strength;
};

struct PointLight {
    vec3 position;
    vec3 diffuse;
    float linear;
    float quadratic;
};

#define POINT_LIGHTS_COUNT 4

uniform DirectionalLight directionalLight;
uniform sampler2D texture_diffuse1;

uniform PointLight pointLights[POINT_LIGHTS_COUNT];

out vec4 color;

vec3 calculateDirectionalLight(DirectionalLight dirLight) 
{
    vec3 lightDir = normalize(-dirLight.direction);
    float strength = max(dot(Normal, lightDir), 0.00f) * dirLight.strength;
    return dirLight.diffuse * strength;
}

vec3 calculatePointLight(PointLight pointLight) 
{
    vec3 lightDir = normalize(pointLight.position - FragPos);
    float diff = max(dot(Normal, lightDir), 0.00f) * 0.85f + 0.15f;
    return diff * pointLight.diffuse;
}

vec3 calculatePointLight2(PointLight pointLight) 
{
    float distance = length(pointLight.position - FragPos);
    float attenuation = 1.0 / (1 + pointLight.linear * distance + pointLight.quadratic * (distance * distance));

    vec3 lightDir = normalize(pointLight.position - FragPos);
    float diff = max(dot(Normal, lightDir), 0.00f) * 0.85f + 0.15f;
    return diff * pointLight.diffuse * attenuation;
}

void main() 
{
    vec3 lightAcc = calculateDirectionalLight(directionalLight);

    for (int i = 0; i < POINT_LIGHTS_COUNT; i++) 
    {
        lightAcc += calculatePointLight2(pointLights[i]);
    }
    
    vec4 textureColor = texture(texture_diffuse1, TexCoord);
    color = vec4(lightAcc, 1.0f) * textureColor;
}
