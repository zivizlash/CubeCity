#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

struct CreateShadowMap_VSOut
{
    float4 Position : POSITION;
    float Depth : TEXCOORD0;
};

//  CREATE SHADOW MAP
CreateShadowMap_VSOut CreateShadowMap_VertexShader(float4 Position : SV_POSITION)
{
    CreateShadowMap_VSOut Out;
    Out.Position    = mul(Position, mul(World, LightViewProj));
    Out.Depth       = Out.Position.z / Out.Position.w;
    
    return Out;
}

float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
{
    return float4(input.Depth, 0, 0, 0);
}

// Technique for creating the shadow map
technique CreateShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 CreateShadowMap_VertexShader();
        PixelShader = compile ps_3_0 CreateShadowMap_PixelShader();
    }
}
