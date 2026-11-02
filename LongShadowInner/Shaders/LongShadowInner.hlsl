// 伸びる影（内側） / Long shadow (inside)
// 標準の「伸びる影」と同じサンプリングを行い、影をテクスチャの内側にだけ描く。

Texture2D InputTexture : register(t0);
SamplerState InputSampler : register(s0);

cbuffer constants : register(b0)
{
	float angle : packoffset(c0.x);
	float length : packoffset(c0.y);
	float opacity : packoffset(c0.z);
	float attenuation : packoffset(c0.w);

	float4 color1 : packoffset(c1);
	float4 color2 : packoffset(c2);
};

float4 main(
	float4 pos : SV_POSITION,
	float4 posScene : SCENE_POSITION,
	float4 uv0 : TEXCOORD0
) : SV_Target
{
	float4 front = InputTexture.Sample(InputSampler, uv0.xy);

	float safeLength = max(length, 0.001);
	float a = 0;
	float4 back = 0;
	// 画像モード用: 影の色に使う図形側の色。
	// 半透明(アンチエイリアス)の画素をそのまま使うと影が暗くなるので、
	// 不透明度1の色に戻してから使う。標準の伸びる影も図形の上では色を変えない。
	float4 shadowSource = front.a > 0 ? float4(saturate(front.rgb / front.a), 1) : 0;
	bool isOutside = false;

	[loop]
	for (int i = 0; i <= length; i++)
	{
		// 標準の伸びる影と同じ向き・同じ式でサンプリングする
		float x = -sin(angle) * -i;
		float y = cos(angle) * -i;
		float2 delta = float2(x, y);

		float4 current = InputTexture.SampleLevel(InputSampler, uv0.xy - delta * uv0.zw, 0);

		// 図形の外に出るまでは、その色を影の色として覚えておく
		if (!isOutside)
		{
			if (current.a >= 0.5)
				shadowSource = float4(saturate(current.rgb / current.a), 1);
			else
				isOutside = true;
		}

		// 内側用: 進行方向が外側(透明)であるほど濃くなる
		float currentAlpha = (1 - saturate(current.a)) * opacity;
		currentAlpha *= (1 - attenuation) + attenuation * (length - i) / safeLength;

		if (a < currentAlpha)
		{
			float4 color = color1 + (color2 - color1) * i / safeLength;
			a = max(a, currentAlpha);
			back = color + shadowSource * (1 - color.a);
		}

		// 完全に透明な画素より先は必ず薄くなるので打ち切る
		if (current.a <= 0)
			break;
	}
	a = min(a, 1);

	// テクスチャの内側にだけ描き、元の画像の上に重ねる
	a *= saturate(front.a);
	return front * (1 - a) + back * a;
}
