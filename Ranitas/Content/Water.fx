﻿#if OPENGL
	//#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 SurfaceColor;
float4 BottomColor;
matrix WorldViewProjection;
float Time;

struct VertexShaderInput
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float depth = input.Color.g;

	float ambientClamp = 0.2;
	float interpolation = ambientClamp + pow(depth, 1.0 / 3.0) * (1.0 - ambientClamp);

	float4 color = (1.0 - interpolation) * SurfaceColor + interpolation * BottomColor;

	return color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};