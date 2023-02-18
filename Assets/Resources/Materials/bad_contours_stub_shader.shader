Shader "CrossSections/_BadContourStub"
{
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque"}    
        Pass
        {
            Cull Off
            Stencil
            {
                WriteMask 255
                PassFront IncrWrap
                PassBack IncrWrap
                Comp Always
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

            void vert(float4 vertex_local : POSITION, out VERT_OUT OUT) 
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

            float4 frag(in VERT_OUT IN) : COLOR
            {
                if(IsFragmentClipped(IN.world_pos))
                    discard;
                return float4(1, 1, 1, 1);
            }

            ENDCG
        }
    }
}
