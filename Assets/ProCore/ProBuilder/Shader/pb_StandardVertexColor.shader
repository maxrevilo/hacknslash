// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.30 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.30;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:7835,x:32760,y:32661,varname:node_7835,prsc:2|diff-8101-OUT,normal-9360-RGB;n:type:ShaderForge.SFN_VertexColor,id:2604,x:32223,y:32783,varname:node_2604,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:1274,x:32223,y:32613,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_1274,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:9470468fda72f4773a90a0306decbf0f,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9360,x:32223,y:32954,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_9360,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8101,x:32497,y:32667,varname:node_8101,prsc:2|A-1274-RGB,B-2604-RGB;proporder:1274-9360;pass:END;sub:END;*/

Shader "ProBuilder/Standard Vertex Color" {
	Properties{
		_MainTex("MainTex", 2D) = "white" {}
	_Normal("Normal", 2D) = "white" {}
	}
		SubShader{
		Tags{
		"RenderType" = "Opaque"
	}
		LOD 200
		Pass{
		Name "FORWARD"
		Tags{
		"LightMode" = "ForwardBase"
	}


		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#pragma multi_compile_fwdbase_fullshadows
#pragma multi_compile_fog
#pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
#pragma target 3.0
		uniform float4 _LightColor0;
	uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
	uniform sampler2D _Normal; uniform float4 _Normal_ST;
	struct VertexInput {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float2 texcoord0 : TEXCOORD0;
		float4 vertexColor : COLOR;
	};
	struct VertexOutput {
		float4 pos : SV_POSITION;
		float2 uv0 : TEXCOORD0;
		float4 posWorld : TEXCOORD1;
		float3 normalDir : TEXCOORD2;
		float3 tangentDir : TEXCOORD3;
		float3 bitangentDir : TEXCOORD4;
		float4 vertexColor : COLOR;
		LIGHTING_COORDS(5,6)
			UNITY_FOG_COORDS(7)
	};
	VertexOutput vert(VertexInput v) {
		VertexOutput o = (VertexOutput)0;
		o.uv0 = v.texcoord0;
		o.vertexColor = v.vertexColor;
		o.normalDir = UnityObjectToWorldNormal(v.normal);
		o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
		o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		float3 lightColor = _LightColor0.rgb;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		UNITY_TRANSFER_FOG(o,o.pos);
		TRANSFER_VERTEX_TO_FRAGMENT(o)
			return o;
	}
	float4 frag(VertexOutput i) : COLOR{
		i.normalDir = normalize(i.normalDir);
	float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
	float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	float4 _Normal_var = tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal));
	float3 normalLocal = _Normal_var.rgb;
	float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
	float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
	float3 lightColor = _LightColor0.rgb;
	////// Lighting:
	float attenuation = LIGHT_ATTENUATION(i);
	float3 attenColor = attenuation * _LightColor0.xyz;
	/////// Diffuse:
	float NdotL = max(0.0,dot(normalDirection, lightDirection));
	float3 directDiffuse = max(0.0, NdotL) * attenColor;
	float3 indirectDiffuse = float3(0,0,0);
	indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
	float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
	float3 diffuseColor = (_MainTex_var.rgb*i.vertexColor.rgb);
	float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
	/// Final Color:
	float3 finalColor = diffuse;
	fixed4 finalRGBA = fixed4(finalColor,1);
	UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
	return finalRGBA;
	}
		ENDCG
	}
		Pass{
		Name "FORWARD_DELTA"
		Tags{
		"LightMode" = "ForwardAdd"
	}
		Blend One One


		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#define UNITY_PASS_FORWARDADD
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#pragma multi_compile_fwdadd_fullshadows
#pragma multi_compile_fog
#pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
#pragma target 3.0
		uniform float4 _LightColor0;
	uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
	uniform sampler2D _Normal; uniform float4 _Normal_ST;
	struct VertexInput {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float2 texcoord0 : TEXCOORD0;
		float4 vertexColor : COLOR;
	};
	struct VertexOutput {
		float4 pos : SV_POSITION;
		float2 uv0 : TEXCOORD0;
		float4 posWorld : TEXCOORD1;
		float3 normalDir : TEXCOORD2;
		float3 tangentDir : TEXCOORD3;
		float3 bitangentDir : TEXCOORD4;
		float4 vertexColor : COLOR;
		LIGHTING_COORDS(5,6)
			UNITY_FOG_COORDS(7)
	};
	VertexOutput vert(VertexInput v) {
		VertexOutput o = (VertexOutput)0;
		o.uv0 = v.texcoord0;
		o.vertexColor = v.vertexColor;
		o.normalDir = UnityObjectToWorldNormal(v.normal);
		o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
		o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		float3 lightColor = _LightColor0.rgb;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		UNITY_TRANSFER_FOG(o,o.pos);
		TRANSFER_VERTEX_TO_FRAGMENT(o)
			return o;
	}
	float4 frag(VertexOutput i) : COLOR{
		i.normalDir = normalize(i.normalDir);
	float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
	float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	float4 _Normal_var = tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal));
	float3 normalLocal = _Normal_var.rgb;
	float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
	float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
	float3 lightColor = _LightColor0.rgb;
	////// Lighting:
	float attenuation = LIGHT_ATTENUATION(i);
	float3 attenColor = attenuation * _LightColor0.xyz;
	/////// Diffuse:
	float NdotL = max(0.0,dot(normalDirection, lightDirection));
	float3 directDiffuse = max(0.0, NdotL) * attenColor;
	float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
	float3 diffuseColor = (_MainTex_var.rgb*i.vertexColor.rgb);
	float3 diffuse = directDiffuse * diffuseColor;
	/// Final Color:
	float3 finalColor = diffuse;
	fixed4 finalRGBA = fixed4(finalColor * 1,0);
	UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
	return finalRGBA;
	}
		ENDCG
	}
	}
		FallBack "Diffuse"
		CustomEditor "ShaderForgeMaterialInspector"
}