//-----------------------------------------------------------------------------
// InstancedModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// Camera settings.
float4x4 View;
float4x4 Projection;
uniform float2 Offset : register(c0);

int  TileCount;
float2 Light;
const float Distance;
texture Texture;
texture Shadow;
texture Ambient;
texture Ambient2;

float4 skycolor;
float4 downcolor;

sampler diffuse = sampler_state
{
    Texture = (Texture);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler shadow = sampler_state
{
    Texture = (Shadow);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler ambient = sampler_state
{
    Texture = (Ambient);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler ambient2 = sampler_state
{
    Texture = (Ambient2);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

const float2 points[9] = 
{
	float2(-1, 1),
	float2( 0, 1),
	float2( 1, 1),
	float2(-1, 0),
	float2( 0, 0),
	float2( 1, 0),
	float2(-1, -1),
	float2( 0, -1),
	float2( 1, -1)
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

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Opacity : COLOR1;
	float4 Ambience : COLOR2;
};

struct RepeatPixelShaderOutput
{
	float4 Ambience : COLOR0;
	float4 Ambience2 : COLOR1;
};

float4 final_position(float4 screen_pos)
{
    float4 result = screen_pos;
    result.xy += Offset*result.ww;
    return result;
}
 


VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = final_position(mul(viewPosition, Projection));


    output.Color = float4(1, 1, 1, 0);
	output.Pos = output.Position;
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

/*first to run shader*/
VertexShaderOutput ColorVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float4 color: COLOR1, float4 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput output = VertexShaderCommon(input, transpose(instanceTransform));
	output.Color = color;
	output.Pos = light;
	output.TextureCoordinate = (text + output.TextureCoordinate) / TileCount;
	return output; 
}

PixelShaderOutput ColorShader(VertexShaderOutput input)
{
	PixelShaderOutput output;
	float4 map = tex2D(diffuse, input.TextureCoordinate.xy);

	if (map.a == 0)
	discard;

	output.Color.a = map.a;
	output.Color.xyz = map.xyz * input.Color.xyz;

	output.Ambience.xyz = input.Pos.xyz;
	output.Ambience.a = map.a;

	output.Opacity.xyzw = float4(0,0,0,0);
	if (map.a != 0 && input.Pos.a != 0)
	{
		output.Opacity.a = input.Pos.a;
	}

	return output;
}


VertexShaderOutput SkyVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float4 color: COLOR1, float4 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput output = VertexShaderCommon(input, transpose(instanceTransform));
	output.Color = skycolor*(1 - input.TextureCoordinate.y) + downcolor * (input.TextureCoordinate.y);
	output.Pos = light;
	output.TextureCoordinate = (text + output.TextureCoordinate) / TileCount;
	return output; 
}

PixelShaderOutput SkyShader(VertexShaderOutput input)
{
	PixelShaderOutput output;

	output.Color.xyzw = 0;
	output.Ambience.xyzw = input.Color.xyzw;
	output.Opacity.xyzw = float4(0,0,0,0);
	return output;
}

VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float3 color: COLOR1, float3 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput o = VertexShaderCommon(input, transpose(instanceTransform));

	o.Color.xyz  = color.xyz;// * light.xyz;
	o.TextureCoordinate = (text + o.TextureCoordinate)* 0.0625f;
	return o; 
}

VertexShaderOutput SimpleVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT, float3 color: COLOR1, float3 light: COLOR2, float3 text : TEXCOORD1)
{
	VertexShaderOutput o = VertexShaderCommon(input, transpose(instanceTransform));
	return o; 
}

float4 SimpleShader(VertexShaderOutput input) : COLOR0
{
	return input.Color;
}

float4 PolarShader(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.Pos.xy;
	float r = -pos.y/2 +0.5;
	//text = float2(r * cos(text.x*6.28) + .5, r * sin(text.x*6.28) + 0.5f);
	float2 text = float2(r * cos(pos.x*3.14), r * sin(-pos.x*3.14)) + Light;
	float4 map = tex2D(shadow, text);
	return map;
}

float2 toPolar(float2 coord)
{
	float2 pos = coord;
	float r = pos.y;

	float2 text = float2(r * - cos(pos.x*6.28), r * sin(pos.x*6.28)) + Light;
	return text;
}

float4 ZMapShader(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.TextureCoordinate.xy;
	float r = 0;

	float2 text = float2(pos.x, r);
	float4 map = tex2D(shadow, text);
	float minimum  = 1;

	for ( r = 0; r <= 0.5; r +=0.002)
	{
		text.y = r;
		map = tex2D(shadow, toPolar(text));
		if ((map.a == 1)  && r < minimum)
			minimum = r;
	}
	if (minimum == 1)
		for ( r = 0.5; r <= 1; r +=0.004)
		{
			text.y = r;
			map = tex2D(shadow, toPolar(text));
			if ((map.a == 1)  && r < minimum)
				minimum = r;
		}
	minimum *=2;
	return float4(minimum,minimum,minimum,1);
}

float4 ShadowAccumShader(VertexShaderOutput input): COLOR0
{
    float2 pos = -input.Pos;

	pos += float2(-1 + Light.x*2, 1 - Light.y *2);

	float2 shadowPos = float2( atan2( pos.y, pos.x)/6.28, 0);
	float shadowy = sqrt(pow(pos.x, 2) + pow(pos.y, 2));
	if (shadowPos.x < 0) shadowPos.x +=1;

	float4 shad = tex2D(shadow, shadowPos);

	float4 color = float4(0,0,0,0);

	if (shadowy > Distance)
	return color;


	if (shadowy < shad.x)
	{
		color.xyz = Distance - shadowy;
		color.a = Distance - shadowy;
	}
	return color;
}

float4 FinalShader(VertexShaderOutput input) : COLOR0
{
	float4 skycolor = float4(0.4, 0.6, 0.9, 1);//float4(0.4, 0.6, 0.9, 1);
	float4 downcolor = float4 (1,1,1,1);
	float4 color = float4(0,0,0,1);
	float4 map = tex2D(diffuse, input.TextureCoordinate.xy);
	float4 amb = tex2D(ambient, input.TextureCoordinate.xy);
	float4 amb2 = tex2D(ambient2, input.TextureCoordinate.xy);
	float4 shad = tex2D(shadow, input.TextureCoordinate.xy);

	color.xyz = (amb2.xyz * shad.xyz + amb.xyz) * map.xyz;

	return color;
}

technique Color
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ColorVertexShader();
        PixelShader = compile ps_3_0 ColorShader();
    }
}

technique Sky
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SkyVertexShader();
        PixelShader = compile ps_3_0 SkyShader();
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


technique ShadowAccum
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 SimpleVertexShader();
		PixelShader = compile ps_3_0 ShadowAccumShader();
	}
}

technique Final
{
	pass Pass1
    {
        VertexShader = compile vs_3_0 SimpleVertexShader();
        PixelShader = compile ps_3_0 FinalShader();
    }
}

technique Wire
{
	pass Pass1
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 SimpleShader();
    }
}
