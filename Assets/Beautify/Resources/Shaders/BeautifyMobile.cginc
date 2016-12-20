	// Copyright 2016 Kronnect - All Rights Reserved.
	
	#include "UnityCG.cginc"

	uniform sampler2D      _MainTex;
	uniform sampler2D_float _CameraDepthTexture;
	uniform sampler2D       _OverlayTex;
	uniform sampler2D       _ScreenLum;
	uniform sampler2D       _BloomTex;
	uniform sampler2D       _EALumSrc;
	uniform sampler2D       _EAHist;
	
	uniform float4 _MainTex_TexelSize;
	uniform float4 _MainTex_ST;
	uniform float4 _ColorBoost; // x = Brightness, y = Contrast, z = Saturate, w = Daltonize;
	uniform float4 _Sharpen;
	uniform float4 _Dither;
	uniform float4 _FXColor;
	uniform float4 _Vignetting;
	uniform float4 _Frame;
	uniform float4 _Outline;
	uniform float4 _Dirt;		
    uniform float3 _Bloom;
    uniform float4 _CompareParams;
	uniform float  _VignettingAspectRatio;
   	uniform float4 _BokehData;
	uniform float4 _BokehData2;

	#if BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT || BEAUTIFY_DEPTH_OF_FIELD
	uniform sampler2D       _DoFTex;
	uniform float4 _DoFTex_TexelSize;
	#endif
	#if BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
	uniform sampler2D       _DepthTexture;
	#endif
		
    struct appdata {
    	float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
    };
    
	struct v2f {
	    float4 pos : SV_POSITION;
	    float2 uv: TEXCOORD0;
    	float2 depthUV : TEXCOORD1;	 
    	#if BEAUTIFY_DIRT || BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT   
	    float2 uvNonStereo: TEXCOORD2;
	    #endif
	};

	v2f vert(appdata v) {
    	v2f o;
    	o.pos = UnityObjectToClipPos(v.vertex);
   		o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    	o.depthUV = o.uv;
    	#if BEAUTIFY_DIRT || BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
   		o.uvNonStereo = v.texcoord;
   		#endif
    	#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Depth texture is inverted WRT the main texture
    	    o.depthUV.y = 1.0 - o.depthUV.y;
    	}
    	#endif
    	return o;
	}

	float getLuma(float3 rgb) { 
		const float3 lum = float3(0.299, 0.587, 0.114);
		return dot(rgb, lum);
	}
	
	float3 getNormal(float depth, float depth1, float depth2, float2 offset1, float2 offset2) {
  		float3 p1 = float3(offset1, depth1 - depth);
  		float3 p2 = float3(offset2, depth2 - depth);
  		float3 normal = cross(p1, p2);
	  	return normalize(normal);
	}
		
	float getRandomFast(float2 uv) {
		float2 p = uv + _Time.yy;
		p -= floor(p * 0.01408450704) * 71.0;    
		p += float2( 26.0, 161.0 );                                
		p *= p;                                          
		return frac(p.x * p.y * 0.001051374728);
	}

	float getCoc(v2f i) {
	#if BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
		float depth  = UNITY_SAMPLE_DEPTH(tex2Dlod(_CameraDepthTexture, float4(i.depthUV, 0, 0)));
	    float depthTex = UNITY_SAMPLE_DEPTH(tex2Dlod(_DepthTexture, float4(i.uvNonStereo, 0, 0)));
		#if defined(UNITY_REVERSED_Z)
	    	depth = max(depth, depthTex);
		#else
	    	depth = min(depth, depthTex);
		#endif
	    depth = LinearEyeDepth(depth);
	#else
		float depth  = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dlod(_CameraDepthTexture, float4(i.depthUV, 0, 0))));
	#endif
		float xd     = abs(depth - _BokehData.x) - _BokehData2.x * (depth < _BokehData.x);
		return 0.5 * _BokehData.y * xd/depth;	// radius of CoC
	}
		
	void beautifyPassFast(v2f i, inout float3 rgbM) {

	    const float3 halves = float3(0.5, 0.5, 0.5);
	    const float4 ones   = float4(1.0,1.0,1.0,1.0);

		// Grab scene info
		float3 uvInc      = float3(_MainTex_TexelSize.x, _MainTex_TexelSize.y, 0);
		float  depthN     = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV + uvInc.zy)));
		float  depthW     = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV - uvInc.xz)));
		float  lumaM      = getLuma(rgbM);
		
		#if !BEAUTIFY_NIGHT_VISION && !BEAUTIFY_THERMAL_VISION
		// 0. RGB Dither		
		float3 dither     = dot(float2(171.0, 231.0), i.uv * _ScreenParams.xy).xxx;
		      dither     = frac(dither / float3(103.0, 71.0, 97.0)) - halves;
		      rgbM      *= 1.0 + step(_Dither.y, depthW) * dither * _Dither.x;

		// 1. Daltonize
		#if BEAUTIFY_DALTONIZE
		float3 rgb0       = ones.xyz - saturate(rgbM.rgb);
		      rgbM.r    *= 1.0 + rgbM.r * rgb0.g * rgb0.b * _ColorBoost.w;
			  rgbM.g    *= 1.0 + rgbM.g * rgb0.r * rgb0.b * _ColorBoost.w;
			  rgbM.b    *= 1.0 + rgbM.b * rgb0.r * rgb0.g * _ColorBoost.w;	
			  rgbM      *= lumaM / (getLuma(rgbM) + 0.0001);
		#endif
		
		// 2. Sharpen
		float  depthClamp = abs(depthW - _Dither.z) < _Dither.w;
		float  maxDepth   = max(depthN, depthW);
		float  minDepth   = min(depthN, depthW);
		float  dDepth     = maxDepth - minDepth + 0.00001;
		float  lumaDepth  = saturate(_Sharpen.y / dDepth);
	    float3 rgbN       = tex2D(_MainTex, i.uv + uvInc.zy).rgb;
		float3 rgbS       = tex2D(_MainTex, i.uv - uvInc.zy).rgb;
	    float3 rgbW       = tex2D(_MainTex, i.uv - uvInc.xz).rgb;
    	float  lumaN      = getLuma(rgbN);
    	float  lumaW      = getLuma(rgbW);
    	float  lumaS      = getLuma(rgbS);
    	float  maxLuma    = max(lumaN,lumaS);
    	       maxLuma    = max(maxLuma, lumaW);
	    float  minLuma    = min(lumaN,lumaS);
	           minLuma    = min(minLuma, lumaW) - 0.000001;
	    float  lumaPower  = 2 * lumaM - minLuma - maxLuma;
		float  lumaAtten  = saturate(_Sharpen.w / (maxLuma - minLuma));
		       rgbM      *= 1.0 + clamp(lumaPower * lumaAtten * lumaDepth * _Sharpen.x, -_Sharpen.z, _Sharpen.z) * depthClamp;

		// 3. DOF
		#if BEAUTIFY_DEPTH_OF_FIELD || BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
		float4 dofPix     = tex2D(_DoFTex, i.uv);
   		#if UNITY_COLORSPACE_GAMMA
			   dofPix.rgb = LinearToGammaSpace(dofPix.rgb);
		#endif
		if (_DoFTex_TexelSize.z < _MainTex_TexelSize.z) {
		float  CoC        = getCoc(i);
		       dofPix.a   = lerp(CoC, dofPix.a, _DoFTex_TexelSize.z / _MainTex_TexelSize.z);
		}
		       rgbM       = lerp(rgbM, dofPix.rgb, saturate(dofPix.a));
		#endif		

		// 4. Vibrance
		float3 maxComponent = max(rgbM.r, max(rgbM.g, rgbM.b));
 		float3 minComponent = min(rgbM.r, min(rgbM.g, rgbM.b));
 		float  sat          = saturate(maxComponent - minComponent);
		      rgbM         *= 1.0 + _ColorBoost.z * (1.0 - sat) * (rgbM - getLuma(rgbM));

  		#endif	// night & thermal vision exclusion
		
   	 	// 5. Bloom
   	 	#if BEAUTIFY_BLOOM
   		#if UNITY_COLORSPACE_GAMMA
		rgbM = GammaToLinearSpace(rgbM);
		#endif
		rgbM += tex2D(_BloomTex, i.uv).rgb * _Bloom.xxx;
		#if UNITY_COLORSPACE_GAMMA
			rgbM    = LinearToGammaSpace(rgbM);
		#endif
		#endif
		
 	 	// 6. Lens Dirt
		#if BEAUTIFY_DIRT
		float4 scrLum     = tex2D(_ScreenLum, i.uv);
   	 	float4 dirt       = tex2D(_OverlayTex, i.uvNonStereo);
   	 	      rgbM       += saturate(halves.xxx - _Dirt.zzz + scrLum.rgb) * dirt.rgb * _Dirt.y; 
  	 	       
	   	#endif

   	 	// 8. Night Vision
   	 	#if BEAUTIFY_NIGHT_VISION
   	 	       lumaM      = getLuma(rgbM);	// updates luma
		float   depth     = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV)));
   		float3  normalNW  = getNormal(depth, depthN, depthW, uvInc.zy, -uvInc.xz);
   		float   nvbase    = saturate(normalNW.z - 0.8); // minimum ambient self radiance (useful for pitch black)
   			   nvbase    += lumaM;						// adds current lighting
   			   nvbase    *= nvbase * (0.5 + nvbase);	// increase contrast
   			   rgbM	      = nvbase * _FXColor.rgb;
   		float2  uvs       = floor(i.uv.xy * _ScreenParams.xy);
   			   rgbM      *= frac(uvs.y*0.25)>0.4;	// scan lines
   			   rgbM	     *= 1.0 + getRandomFast(uvs) * 0.3 - 0.15;				// noise
	 	#endif
   
  	 	// 9. Thermal Vision
   	 	#if BEAUTIFY_THERMAL_VISION
   	 	       lumaM      = getLuma(rgbM);	// updates luma
    	float3 tv0 	      = lerp(float3(0.0,0.0,1.0), float3(1.0,1.0,0.0), lumaM * 2.0);
    	float3 tv1	      = lerp(float3(1.0,1.0,0.0), float3(1.0,0.0,0.0), lumaM * 2.0 - 1.0);
    		  rgbM        = lerp(tv0, tv1, lumaM >= 0.5);
   		float2 uvs        = floor(i.uv.xy * _ScreenParams.xy);
   			  rgbM       *= 0.2 + frac(uvs.y*0.25)>0.4;	// scan lines
   			  rgbM		 *= 1.0 + getRandomFast(uvs) * 0.2 - 0.1;				// noise
	 	#endif
		
   		// 7. Tinting
   		#if BEAUTIFY_TINT
  		      rgbM       = lerp(rgbM, rgbM * _FXColor.rgb, _FXColor.a);
  		#endif
		
		// 10. Final contrast + brightness
  			  rgbM       = (rgbM - halves) * _ColorBoost.y + halves;
  			  rgbM      *= _ColorBoost.x;

  		// 11. Colored vignetting
  		float2 vd         = float2(i.uv.x - 0.5, (i.uv.y - 0.5) * _VignettingAspectRatio);
  		      rgbM       = lerp(rgbM, lumaM * _Vignetting.rgb, saturate(_Vignetting.a * dot(vd,vd)));

   		// 12. Sepia
   		#if BEAUTIFY_SEPIA
   		float3 sepia      = float3(
   		            	   			dot(rgbM, float3(0.393, 0.769, 0.189)),
               						dot(rgbM, float3(0.349, 0.686, 0.168)),
               						dot(rgbM, float3(0.272, 0.534, 0.131))
               					  );
               	rgbM      = lerp(rgbM, sepia, _FXColor.a);
        #endif

   		// 13. Outline
   		#if BEAUTIFY_OUTLINE
   		#if !BEAUTIFY_NIGHT_VISION
   		float depth       = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV)));
   		float3 normalNW   = getNormal(depth, depthN, depthW, uvInc.zy, -uvInc.xz);
   		#endif
		float  depthE     = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV + uvInc.xz)));		
		float  depthS     = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV - uvInc.zy)));
   		float3 normalSE   = getNormal(depth, depthS, depthE, -uvInc.zy, uvInc.xz);
		float  dnorm      = dot(normalNW, normalSE);
   		rgbM              = lerp(rgbM, _Outline.rgb, dnorm  < _Outline.a);
	 	#endif
	 	  		
  		// 14. Border
  		if (_Frame.a) {
  		      rgbM       = lerp(rgbM, _Frame.rgb, saturate( (max(abs(i.uv.x - 0.5), abs(i.uv.y - 0.5)) - _Frame.a) * 50.0));
   		}
	}

	float4 fragBeautifyFast (v2f i) : SV_Target {
   		float4 pixel = tex2D(_MainTex, i.uv);
   		beautifyPassFast(i, pixel.rgb);
   		return pixel;
	}
	
	float4 fragCompareFast (v2f i) : SV_Target {

		// separator line + antialias
		float2 dd     = i.uv - 0.5.xx;
		float  co     = dot(_CompareParams.xy, dd);
		float  dist   = distance( _CompareParams.xy * co, dd );
		float4 aa     = saturate( (_CompareParams.w - dist) / abs(_MainTex_TexelSize.y) );

		float4 pixel  = tex2D(_MainTex, i.uv);

		// are we on the beautified side?
		float s       = dot(dd, _CompareParams.yz);
		if (s>0) beautifyPassFast(i, pixel.rgb);

		return pixel + aa;

	}
