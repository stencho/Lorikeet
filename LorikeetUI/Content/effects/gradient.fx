#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

float2 resolution;
float angle = 0;
float offset = 0;

float4 colorA;
float4 colorB;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Convert screen UV (0–1) into normalized space centered at 0
    float2 centered = (input.TextureCoordinates - 0.5) * float2(resolution.x / resolution.y, 1.0);

    // Compute gradient direction from angle
    float2 dir = float2(cos(angle), sin(angle));

    // Project point onto gradient axis
    float proj = dot(centered, dir);

    // Shift along axis
    proj += offset;

    // Normalize to 0–1 range for output gradient
    float grad = saturate(proj * 0.5 + 0.5);

    // Make gradient output (black → white)
    return float4(grad, grad, grad, 1.0);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
