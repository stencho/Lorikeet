#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

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

float3 HSVtoRGB(float h, float s, float v)
{
    float3 rgb = float3(0,0,0);

    float c = v * s;
    float hPrime = h * 6.0;           // sector 0..6
    float x = c * (1 - abs(fmod(hPrime, 2) - 1));
    float m = v - c;

    if (0 <= hPrime && hPrime < 1) rgb = float3(c, x, 0);
    else if (1 <= hPrime && hPrime < 2) rgb = float3(x, c, 0);
    else if (2 <= hPrime && hPrime < 3) rgb = float3(0, c, x);
    else if (3 <= hPrime && hPrime < 4) rgb = float3(0, x, c);
    else if (4 <= hPrime && hPrime < 5) rgb = float3(x, 0, c);
    else if (5 <= hPrime && hPrime < 6) rgb = float3(c, 0, x);

    return rgb + float3(m, m, m);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float hue = input.TextureCoordinates.x; // horizontal = hue
    float sat = input.TextureCoordinates.y; // vertical = saturation
    float val = 1.0;  // full brightness
    
    float3 col = HSVtoRGB(hue,sat,val);
        
    return float4(col, 1.0);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
