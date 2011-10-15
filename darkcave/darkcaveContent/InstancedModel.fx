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
float3 Light;
texture Texture;
texture Shadow;

sampler diffuse = sampler_state
{
    Texture = (Texture);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler shadow = sampler_state
{
    Texture = (Shadow);
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
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
	float4 Pos : COLOR1;
    float2 TextureCoordinate : TEXCOORD0;
};


VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);


    output.Color = float4(1, 1, 1, 0);
	output.Pos = output.Position;
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}


// Hardware instancing reads the yper-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float3 color: COLOR1, float3 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput o = VertexShaderCommon(input, transpose(instanceTransform));

	o.Color.xyz  = color.xyz * light.xyz;
	o.TextureCoordinate = (text + o.TextureCoordinate)* 0.0625f;
	return o; 
}

VertexShaderOutput ShadowMappingVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float3 color: COLOR1, float3 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput output = VertexShaderCommon(input, transpose(instanceTransform));
	//output.Color.xyz  = cohlor.xyz * light.xyz;

	float4 worldPosition = mul(input.Position, transpose(instanceTransform));

	output.TextureCoordinate = (text + output.TextureCoordinate)* 0.0625f;
	return output; 
}

// Both techniques share this same pixel shader.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 pos = -input.Pos;
	//pos.y = - pos.y;
	float2 shadowPos = float2( atan2( pos.y, pos.x)/6.28, 0);
	float shadowy = sqrt(pow(pos.x, 2) + pow(pos.y, 2));
	if (shadowPos.x < 0) shadowPos.x +=1;
	//float2 shadowPos = input.TextureCoordinate.xy;
	float4 shad = tex2D(shadow, shadowPos);

	float4 map = tex2D(diffuse, input.TextureCoordinate.xy);

	if (shadowy < shad.x )
		return map;
	map.xyz *=0.1;
	return map;
}

float4 ShadowMapShader(VertexShaderOutput input) : COLOR0
{
	float4 map = tex2D(diffuse, input.TextureCoordinate.xy);
	map.xyz = map.a;//input.Color.xyz;
	if (map.a != 0)
	{
		map.xyz = sqrt(pow(input.Pos.x,2) + pow(input.Pos.y,2));
		}
	return map;
}

VertexShaderOutput SimpleVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float3 color: COLOR1, float3 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput o = VertexShaderCommon(input, transpose(instanceTransform));
	return o; 
}


float4 PolarShader(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.Pos.xy;
	float r = -pos.y/2 +0.5;
	//text = float2(r * cos(text.x*6.28) + .5, r * sin(text.x*6.28) + 0.5f);
	float2 text = float2(r * cos(pos.x*3.14) + .5, r * sin(-pos.x*3.14) + 0.5);
	float4 map = tex2D(shadow, text);
	return map;
}


float4 ZMapShader(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.TextureCoordinate.xy;
	float r = 0;

	//text = float2(r * cos(text.x*6.28) + .5, r * sin(text.x*6.28) + 0.5f);
	float2 text = float2(pos.x, r);
	float4 map = tex2D(shadow, text);
	float minimum  = 1;
	//return map;
	for ( r = 0; r <= 0.5; r +=0.002)
	{
		text.y = r;
		map = tex2D(shadow, text);
		if ((map.x == map.y)  && map.x < minimum)
			minimum = map.x;
	}

	for ( r = 0.5; r <= 1; r +=0.002)
	{
		text.y = r;
		map = tex2D(shadow, text);
		if ((map.x == map.y)  && map.x < minimum)
			minimum = map.x;
	}

	return float4(minimum,minimum,minimum,1);
}

/*
float4 ZMapShader(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.Pos.xy;
	float r = 0;

	//text = float2(r * cos(text.x*6.28) + .5, r * sin(text.x*6.28) + 0.5f);
	float2 text = float2(0 * cos(pos.x*3.14) + .5, 0 * sin(-pos.x*3.14) + 0.5);
	float4 map = tex2D(shadow, text);
	//return map;s
	do {

		text = float2(r * cos(pos.x*3.14) + .5, r * sin(-pos.x*3.14) + 0.5f);
		map = tex2D(shadow, text);
		r += 0.009;
	} while ((map.x != map.y ) && r <= 0.5);

	if (map.x == map.y )
		return map;
	return float4(1,1,1,1);
}*/

// Hardware instancing technique.
technique ShadowMapInstancing 
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ShadowMappingVertexShader();
        PixelShader = compile ps_3_0 ShadowMapShader();
    }
}

technique ZMap
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 SimpleVertexShader();
		PixelShader = compile ps_3_0 PolarShader();
	}
	
	pass Pass2
	{
		VertexShader = compile vs_3_0 SimpleVertexShader();
		PixelShader = compile ps_3_0 ZMapShader();
	}
}

technique HardwareInstancing
{
	pass Pass1
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique Final
{
	pass Pass1
    {
        VertexShader = compile vs_3_0 SimpleVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}