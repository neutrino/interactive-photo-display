Shader "Animated/Text"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_AnimationXSpeed("Animation Horizontal Speed", Float) = 0
		_AnimationYSpeed("Animation Vertical Speed", Float) = 0
		_AnimationXMagnitude("Animation Horizontal Magnitude", Float) = 1
		_AnimationYMagnitude("Animation Vertical Magnitude", Float) = 1
		_HeightOffset("Height Offset", Float) = 0
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		CGPROGRAM
#pragma surface surf Lambert vertex:vert nofog keepalpha
#pragma multi_compile _ PIXELSNAP_ON
#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

		sampler2D _MainTex;
	fixed4 _Color;
	sampler2D _AlphaTex;
	float _AnimationXSpeed;
	float _AnimationXMagnitude;
	float _AnimationYSpeed;
	float _AnimationYMagnitude;
	float _HeightOffset;

	struct Input
	{
		float2 uv_MainTex;
		fixed4 color;
	};

	void vert(inout appdata_full v, out Input o)
	{
#if defined(PIXELSNAP_ON)
		v.vertex = UnityPixelSnap(v.vertex);
#endif
		float _Height = v.vertex.y + _HeightOffset;
		v.vertex.x += sin(_AnimationXSpeed * _Time) * _AnimationXMagnitude * _Height;
		v.vertex.y += sin(_AnimationYSpeed * _Time) * _AnimationYMagnitude * _Height;
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.color = v.color * _Color;
	}

	fixed4 SampleSpriteTexture(float2 uv)
	{
		fixed4 color = tex2D(_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
		color.a = tex2D(_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

		return color;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed4 c = IN.color;
		o.Albedo = c.rgb * c.a;
		o.Alpha = SampleSpriteTexture(IN.uv_MainTex).a;
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}
