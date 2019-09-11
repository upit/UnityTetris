// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UpitSoft/UIColor"{
	Properties{
		_Stencil ("Stencil ID", Float) = 0
		_StencilComp ("Stencil Comparison", Float) = 8
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader{
		Tags{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil{Ref [_Stencil]	Comp [_StencilComp]	Pass [_StencilOp] }

		Cull Back
		Lighting Off
		ZWrite Off
		ZTest Off
		//ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			//#include "UnityCG.cginc"

			struct appdata_t{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
			};

			struct v2f{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
			};

			v2f vert(appdata_t IN){
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color;
				return OUT;
			}

			fixed4 frag(v2f IN) : COLOR{
				return IN.color;
			}

		ENDCG
		}
	}
}
