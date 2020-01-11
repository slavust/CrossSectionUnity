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
        Tags { "Queue" = "Geometry"}    
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
           GLSLPROGRAM
    #ifdef VERTEX
            
            #include "UnityCG.glslinc"

            uniform mat4 _Object2World;
            
            out vec3 world_pos; 
            out vec3 world_normal;

            void main()
            {
                world_pos = (unity_ObjectToWorld * gl_Vertex).xyz;
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                world_normal = normalize(gl_Normal);
            }
    #endif

    #ifdef FRAGMENT
            #include "UnityCG.glslinc"

            uniform vec4 _CrossPlanePosition;
            uniform vec4 _CrossPlaneVisibleNormal;

            in vec3 world_pos;
            in vec3 world_normal;
            void main()
            {
                vec3 dir_from_plane = normalize(world_pos - _CrossPlanePosition.xyz);
                if(dot(dir_from_plane, _CrossPlaneVisibleNormal.xyz) > 0)
                {
                    discard;
                }

                gl_FragColor = vec4(1, 1, 0, 1);
            }
    #endif

            ENDGLSL
        }
        Pass
        {
            Stencil
            {
                WriteMask 0
                Comp always
            }
            Cull Off
            GLSLPROGRAM
    #ifdef VERTEX
            
            #include "UnityCG.glslinc"

            uniform mat4 _Object2World;
            
            out vec3 world_pos; 
            out vec3 world_normal;

            void main()
            {
                world_pos = (unity_ObjectToWorld * gl_Vertex).xyz;
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                world_normal = normalize(gl_Normal);
            }
    #endif

    #ifdef FRAGMENT
            #include "UnityCG.glslinc"

            uniform vec4 _CrossPlanePosition;
            uniform vec4 _CrossPlaneVisibleNormal;

            in vec3 world_pos;
            in vec3 world_normal;
            void main()
            {
                vec3 dir_from_plane = normalize(world_pos - _CrossPlanePosition.xyz);
                if(dot(dir_from_plane, _CrossPlaneVisibleNormal.xyz) > 0)
                {
                    discard;
                }

                vec3 normal_normalized = normalize(world_normal);
                vec3 L = normalize(_WorldSpaceCameraPos - world_pos);
                float lighting = dot(L, normal_normalized);

                if (gl_FrontFacing) 
                {
                   gl_FragColor = lighting * vec4(1, 1, 1, 1); //vec4(0, 1, 0, 1);
                }
                else // fragment is part of a back face
                {
                   gl_FragColor = (-lighting) * vec4(1, 0, 0, 1);
                }
            }
    #endif

            ENDGLSL
        }
    }
}
