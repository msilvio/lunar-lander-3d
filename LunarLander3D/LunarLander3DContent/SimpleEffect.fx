float4x4 World;
float4x4 View;
float4x4 Projection;

texture BasicTexture;

sampler BasicTextureSampler = sampler_state {
	texture = <BasicTexture>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

bool TextureEnabled = false;

float3 DiffuseColor = float3(1, 1, 1);
float3 AmbientColor = float3(.15, .15, .15);

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    float4 worldPosition = mul(input.Position, World);
    float4x4 viewProjection = mul(View, Projection);
    
    output.Position = mul(worldPosition, viewProjection);

	output.UV = input.UV;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 output = DiffuseColor + AmbientColor;

	if (TextureEnabled)
		output *= tex2D(BasicTextureSampler, input.UV);

    return float4(output, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}