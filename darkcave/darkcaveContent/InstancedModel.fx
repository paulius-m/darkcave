//-----------------------------------------------------------------------------
// InstancedModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;

texture Texture;

sampler diffuse = sampler_state
{
    Texture = (Texture);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};


struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};


// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    // Apply the world and camera matrices to compute the output position.
    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);


    output.Color = float4(0,0,0,0);

    // Copy across the input texture coordinate.
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float3 color: COLOR1, float3 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput o = VertexShaderCommon(input, transpose(instanceTransform));

	o.Color.xyz  = color.xyz * light.xyz;
	o.TextureCoordinate = (text + o.TextureCoordinate)* 0.1f;
	return o; 
}


// When instancing is disabled we take the world transform from an effect parameter.
VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
    return VertexShaderCommon(input, World);
}


// Both techniques share this same pixel shader.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 map = tex2D(diffuse, input.TextureCoordinate.xy);
	map.xyz *= input.Color.xyz;
	return map;
}


// Hardware instancing technique.
technique HardwareInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}


// For rendering without instancing.
technique NoInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 NoInstancingVertexShader();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
