#if OPENGL
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

float LightShaft(float2 position, float factor, float frequency) {
	float2 direction = normalize(float2(0.8, -0.05));
	float spaceEvaluationPoint = dot(position, direction);
	float sinValue = sin(factor * frequency * spaceEvaluationPoint) * cos(frequency * Time);
	float clamped = sinValue * float(sinValue > 0.6);
	float intensity = pow(clamped, 7.5);
	return intensity;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float depth = input.Color.g;
	if (depth <= 0.0001)
	{
		float4 boundaryColor = SurfaceColor;
		boundaryColor.a = 1.0;
		return boundaryColor;
	}

	float ambientClamp = 0.2;
	float interpolation = ambientClamp + pow(depth, 0.25) * (1.0 - ambientClamp);

	float2 position = {input.Color.r, input.Color.g};
	float lightIntensity = 0.0;
	for (int i = 3; i <= 10; ++i)
	{
		lightIntensity += LightShaft(position, 22.0, i * 0.174);
	}

	float lostLight = pow(position.y, 0.0025);
	lightIntensity = lightIntensity - lostLight;

	float dim = pow(position.y, 0.02);
	lightIntensity = saturate((1.0 - dim) * lightIntensity);

	interpolation = (1.0 - lightIntensity) * interpolation;

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