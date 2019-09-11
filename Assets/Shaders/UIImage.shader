// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UpitSoft/UIImage"{
	Properties{
		//[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Stencil ("Stencil ID", Float) = 0
		_StencilComp ("Stencil Comparison", Float) = 8
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader{
		Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True"	}
		
		Stencil{Ref [_Stencil]	Comp [_StencilComp]	Pass [_StencilOp] }

		Cull Back
		Lighting Off
		ZWrite Off
		//ZTest [unity_GUIZTestMode]
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha
		//ColorMask[_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			//#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			v2f vert(appdata_t IN){
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);

				OUT.texcoord = IN.texcoord;

				OUT.color = IN.color;
				return OUT;
			}

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;

			fixed4 frag(v2f IN) : COLOR{
				half4 color = (tex2D(_MainTex, IN.texcoord)+ _TextureSampleAdd) * IN.color;
				
				//#ifdef UNITY_UI_ALPHACLIP
				//clip (color.a - 0.001);
				//#endif

				return color;
			}

		ENDCG
		}
	}
}
