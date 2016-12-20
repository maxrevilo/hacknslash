	// Copyright 2016 Kronnect - All Rights Reserved.
	
	#include "UnityCG.cginc"

	uniform sampler2D_float _CameraDepthTexture;
	uniform sampler2D_float _DepthTexture;
	uniform sampler2D _MainTex;
	uniform float4    _MainTex_TexelSize;
	uniform float4    _MainTex_ST;
	uniform float4	  _BokehData;
	uniform float4    _BokehData2;
	
    struct appdata {
    	float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
    };
    
	struct v2f {
	    float4 pos : SV_POSITION;
	    float2 uv: TEXCOORD0;
		float2 depthUV : TEXCOORD1;	    		
		float2 uvNonStereo: TEXCOORD2;
	};

	v2f vert(appdata v) {
    	v2f o;
    	o.pos = UnityObjectToClipPos(v.vertex);
		o.uvNonStereo = v.texcoord;
		o.uv  = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
		o.depthUV = o.uv;
		
    	#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
    	    o.uv.y = 1.0 - o.uv.y;
			o.uvNonStereo.y = 1.0 - o.uvNonStereo.y;
    	}
    	#endif  	
    	return o;
	}

	float getCoc(v2f i) {
	#if BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
		float depth  = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV));
	    float depthTex = UNITY_SAMPLE_DEPTH(tex2D(_DepthTexture, i.uvNonStereo));
		#if defined(UNITY_REVERSED_Z)
	    	depth = max(depth, depthTex);
		#else
		    depth = min(depth, depthTex);
		#endif
	    depth = LinearEyeDepth(depth);
	#else
		float depth  = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV)));
	#endif
		float xd     = abs(depth - _BokehData.x) - _BokehData2.x * (depth < _BokehData.x);
		return 0.5 * _BokehData.y * xd/depth;	// radius of CoC
	}
				
	float4 fragCoC (v2f i) : SV_Target {
		float4 p      = tex2D(_MainTex, i.uv);
		#if UNITY_COLORSPACE_GAMMA
		p.rgb = GammaToLinearSpace(p.rgb);
		#endif
   		return float4(p.rgb, getCoc(i));
   	}	
	
	float4 fragCoCDebug (v2f i) : SV_Target {
		float4 pixel  = tex2D(_MainTex, i.uv);
		float  CoC    = getCoc(i);
		pixel.a       = min(CoC, pixel.a);
		return pixel.aaaa;
   	}

	float4 fragBlur (v2f i): SV_Target {
		float4 sum     = tex2D(_MainTex, i.uv );
		float  samples = ceil(sum.a);
		float4 dir     = float4(_BokehData.zw * _MainTex_TexelSize.xy, 0, 0);
		#if UNITY_SINGLE_PASS_STEREO
		dir.x *= 2.0;
		#endif
		       dir    *= max(1.0, samples / _BokehData2.y);
		float  jitter  = dot(float2(2.4084507, 3.2535211), i.uv * _MainTex_TexelSize.zw);
		float2 disp0   = dir.xy * (frac(jitter) + 0.5);
		float4 disp1   = float4(i.uvNonStereo + disp0, 0, 0);
		float4 disp2   = float4(i.uvNonStereo - disp0, 0, 0);
		float  w       = 1.0;

		const int sampleCount = (int)min(_BokehData2.y, samples);
		UNITY_UNROLL
		for (int k=1;k<16;k++) {
			if (k<sampleCount) {
				#if UNITY_SINGLE_PASS_STEREO
				float4 pixel1	   = tex2Dlod(_MainTex, float4(UnityStereoScreenSpaceUVAdjust(disp1.xy, _MainTex_ST), 0, 0));
				#else
				float4 pixel1	   = tex2Dlod(_MainTex, disp1);
				#endif
				float  bt1         = pixel1.a > k;
				       pixel1.rgb += _BokehData2.www * max(pixel1.rgb - _BokehData2.zzz, 0.0.xxx);
					   sum        += pixel1 * bt1;
					   w 	      += bt1;
					   disp1      += dir;
				#if UNITY_SINGLE_PASS_STEREO
				float4 pixel2      = tex2Dlod(_MainTex, float4(UnityStereoScreenSpaceUVAdjust(disp2.xy, _MainTex_ST), 0, 0));
				#else
				float4 pixel2	   = tex2Dlod(_MainTex, disp2);
				#endif
					   float  bt2  = pixel2.a > k;
				       pixel2.rgb += _BokehData2.www * max(pixel2.rgb - _BokehData2.zzz, 0.0.xxx);
					   sum        += pixel2 * bt2;
					   w          += bt2;
					   disp2      -= dir;
			}
		}
		return sum / w;
	}

	float4 fragBlurNoBokeh (v2f i): SV_Target {
		float4 sum     = tex2D(_MainTex, i.uv );
		float samples  = ceil(sum.a);
		float4 dir     = float4(_BokehData.zw * _MainTex_TexelSize.xy, 0, 0);
		#if UNITY_SINGLE_PASS_STEREO
		dir.x *= 0.5;
		#endif
		       dir    *= max(1.0, samples / _BokehData2.y);
		float  jitter  = dot(float2(2.4084507, 3.2535211), i.uv * _MainTex_TexelSize.zw);
		float2 disp0   = dir.xy * (frac(jitter) + 0.5);
		float4 disp1   = float4(i.uvNonStereo + disp0, 0, 0);
		float4 disp2   = float4(i.uvNonStereo - disp0, 0, 0);
		float  w       = 1.0;

		const int sampleCount = (int)min(_BokehData2.y, samples);
		UNITY_UNROLL
		for (int k=1;k<16;k++) {
			if (k<sampleCount) {
				#if UNITY_SINGLE_PASS_STEREO
				float4 pixel1      = tex2Dlod(_MainTex, float4(UnityStereoScreenSpaceUVAdjust(disp1.xy, _MainTex_ST), 0, 0));
				#else
				float4 pixel1      = tex2Dlod(_MainTex, disp1);
				#endif
				float  bt1         = pixel1.a > k;
					   sum        += bt1 * pixel1;
					   w 	      += bt1;
					   disp1      += dir;
				#if UNITY_SINGLE_PASS_STEREO
				float4 pixel2      = tex2Dlod(_MainTex, float4(UnityStereoScreenSpaceUVAdjust(disp2.xy, _MainTex_ST), 0, 0));
				#else
				float4 pixel2      = tex2Dlod(_MainTex, disp2);
				#endif
				float  bt2         = pixel2.a > k;
					   sum        += bt2 * pixel2;
					   w          += bt2;
					   disp2      -= dir;
			}
		}
		return sum / w;
	}

