Shader "Custom/cut_shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _SectionColor ("SectionColor", Color) = (0, 1, 0, 1)
        _CrossPlanePosition ("CrossPlanePosition", Vector) = (0, 0, 0, 1)
        _CrossPlaneVisibleNormal ("CrossPlaneVisibleDirection", Vector) = (1, 0, 0, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" }    
        Cull Off
        Pass
        {
            Stencil
            {
                WriteMask 1
                Ref 1
                PassFront IncrWrap
                PassBack IncrWrap
                ZFailFront IncrWrap
                ZFailBack IncrWrap
                Comp always
            }
            
            GLSLPROGRAM
    #ifdef VERTEX
            
            #include "UnityCG.glslinc"

            uniform mat4 _Object2World;
            
            out vec3 world_pos;
            void main()
            {
                world_pos = (unity_ObjectToWorld * gl_Vertex).xyz;
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }
    #endif

    #ifdef FRAGMENT
            uniform vec4 _CrossPlanePosition;
            uniform vec4 _CrossPlaneVisibleNormal;

            in vec3 world_pos;
            void main()
            {
                vec3 dir_from_plane = normalize(world_pos - _CrossPlanePosition.xyz);
                if(dot(dir_from_plane, _CrossPlaneVisibleNormal.xyz) > 0)
                {
                    discard;
                }
                if (gl_FrontFacing) 
                {
                   gl_FragColor = vec4(world_pos, 1); //vec4(0, 1, 0, 1);
                }
                else // fragment is part of a back face
                {
                   gl_FragColor = vec4(1, 0, 0, 1);
                }
            }
    #endif

            ENDGLSL
        }
    }
}
