Shader "Custom/CrossedSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _BackFaceColor ("BackFaceColor", Color) = (1, 0, 0, 1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }    
        Pass
        {
            Cull Off
            Stencil
            {
                WriteMask 255
                PassFront IncrWrap
                PassBack IncrWrap
                Comp always
            }
            ZTest Always
            ZWrite Off
            ColorMask 0
            
            CGPROGRAM
            #pragma vertex vert 
            #pragma fragment frag


            uniform float4 _CrossPlanePositions[3];
            uniform float4 _CrossPlaneVisibleNormals[3];

            struct VERT_OUT
            {
                float4 pos : SV_POSITION;
                float3 world_pos : TEXCOORD0;
            };

            void vert(float4 vertex_local : POSITION, inout VERT_OUT OUT) 
            {
                OUT.pos = UnityObjectToClipPos(vertex_local);
                OUT.world_pos = mul(unity_ObjectToWorld, vertex_local).xyz;
            }


            bool IsFragmentClipped(float3 world_pos)
            {
                bool is_invisible[3];
                for(int i = 0; i < 3; ++i)
                {
                    float3 dir_from_plane = normalize(world_pos - _CrossPlanePositions[i].xyz);
                    is_invisible[i] = dot(dir_from_plane, _CrossPlaneVisibleNormals[i].xyz) > 0;
                }
                return is_invisible[0] && is_invisible[1] && is_invisible[2];
            }

            float4 frag(float3 world_pos : TEXCOORD0) : COLOR
            {
                if(IsFragmentClipped(world_pos))
                    discard;
                return float4(1, 1, 1, 1);
            }

            ENDCG
        }

        Stencil
        {
            WriteMask 0
            Comp always
        }
        Cull Off

        CGPROGRAM

        #pragma target 3.0
        #pragma surface surf Standard fullforwardshadows

        uniform float3 _Color;
        uniform float3 _BackFaceColor;
        uniform float _Smoothness;
        uniform float _Metallic;
        uniform sampler2D _MainTex;


        uniform float4 _CrossPlanePositions[3];
        uniform float4 _CrossPlaneVisibleNormals[3];

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
            float facing : VFACE;
        };

        bool IsFragmentClipped(float3 world_pos)
        {
            bool is_invisible[3];
            for(int i = 0; i < 3; ++i)
            {
                float3 dir_from_plane = normalize(world_pos - _CrossPlanePositions[i].xyz);
                is_invisible[i] = dot(dir_from_plane, _CrossPlaneVisibleNormals[i].xyz) > 0;
            }
            return is_invisible[0] && is_invisible[1] && is_invisible[2];
        }

        void surf(Input IN, inout SurfaceOutputStandard OUT)
        {
            if(IsFragmentClipped(IN.worldPos))
                discard;
            
            float3 color = tex2D(_MainTex, IN.uv_MainTex).rgb;
            if(IN.facing > 0)
                color = color * _Color;
            else
                color = color * _BackFaceColor;
            OUT.Albedo = color;
            OUT.Smoothness = _Smoothness;
            OUT.Metallic = _Metallic;
            OUT.Alpha = 1.0;
        }
        ENDCG
    }
}
