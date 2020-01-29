Shader "Custom/CrossSection"
{
    Properties
    {
        _SectionColor ("SectionColor", Color) = (0, 1, 0, 1)
        _SectionTex("Section Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent"} 
        Pass
        {
            Cull Back
            Stencil
            {
                WriteMask 0
                ReadMask 1
                Ref 1
                PassFront Keep
                PassBack Keep
                FailFront Keep
                FailBack keep
                Comp Equal
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            uniform float4 _SectionColor;
            uniform sampler2D _SectionTex;
            uniform float4 _SectionTex_ST; // tiling and offset

            struct VERT_INPUT
            {
                float4 local_pos : POSITION;
                float3 local_normal : NORMAL;
                float2 uv_SectionTex : TEXCOORD0;
            };

            struct FRAG_INPUT
            {
                float2 uv_SectionTex : TEXCOORD0;
                float3 view_space_pos : TEXCOORD1;
                float3 view_space_normal : TEXCOORD2;
            };

            float4 vert(in VERT_INPUT VERT_IN, out FRAG_INPUT FRAG_IN) : SV_POSITION
            {
                FRAG_IN.view_space_pos = mul(UNITY_MATRIX_MV, VERT_IN.local_pos);
                FRAG_IN.uv_SectionTex = VERT_IN.uv_SectionTex;
                FRAG_IN.view_space_normal = mul(UNITY_MATRIX_MV, VERT_IN.local_normal);
                return UnityObjectToClipPos(VERT_IN.local_pos);
            }

            float4 frag(in FRAG_INPUT IN) : COLOR
            {
                float4 tex_color = tex2D(_SectionTex, IN.uv_SectionTex * _SectionTex_ST.xy) * _SectionColor;
                float3 normal = normalize(IN.view_space_normal);
                float NdotL = 1.0; //dot(normal, -normalize(IN.view_space_pos));
                return tex_color * NdotL;
            }
            ENDCG
        }
        Pass
        {
            Cull Back
            Stencil
            {
                WriteMask 255
                ReadMask 1
                Ref 1
                PassFront IncrWrap
                PassBack IncrWrap
                FailFront Keep
                FailBack keep
                Comp Equal
            }
            ColorMask 0
            ZWrite off
            ZTest Always
        }
    }
}
