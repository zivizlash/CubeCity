#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

matrix WorldViewProjection;

sampler TextureSampler : register(s0) =
sampler_state
{
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
};
//
//struct VertexShaderInput
//{
//	float2 TexCoord : TEXCOORD0;
//	// float4 Position : POSITION0;
//};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

//
//VertexShaderOutput MainVS(in VertexShaderInput input)
//{
//	VertexShaderOutput output = (VertexShaderOutput)0;
//
//	// output.Position = mul(input.Position, WorldViewProjection);
//
//	return output;
//}

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	return tex2D(TextureSampler, texCoord);
}

technique BasicColorDrawing
{
	pass P0
	{
		//VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
