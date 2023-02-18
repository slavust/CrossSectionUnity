Shader "CrossSections/_CrossSection"
{
    Properties
    {
        _SectionColor("Section Color", Color) = (0, 1, 0, 1)
        _SectionTex("Section Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" } 
        Pass
        {
            Cull Back
            Stencil
            {
                WriteMask 255
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
                float4 position : SV_POSITION;
                float2 uv_SectionTex : TEXCOORD0;
            };

            void vert(in VERT_INPUT VERT_IN, out FRAG_INPUT FRAG_IN)
            {
                FRAG_IN.uv_SectionTex = VERT_IN.uv_SectionTex;
                FRAG_IN.position = UnityObjectToClipPos(VERT_IN.local_pos);
            }

            float4 frag(in FRAG_INPUT input) : COLOR
            {
                float4 tex_color = tex2D(_SectionTex, input.uv_SectionTex * _SectionTex_ST.xy) * _SectionColor;
                return tex_color;
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
                PassFront Zero
                PassBack Zero
                FailFront Zero
                FailBack Zero
                Comp Always
            }
            ColorMask 0
            ZWrite off
            ZTest Always
        }
    }
}
