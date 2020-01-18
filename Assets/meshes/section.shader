Shader "Custom/section"
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
        Tags { "Queue" = "Geometry+1" } 
        Cull Off
        Pass
        {
            Stencil
            {
                WriteMask 0
                ReadMask 1
                Ref 1
                PassFront Keep
                PassBack keep
                FailFront Keep
                FailBack keep
                ZFailFront Keep
                ZFailBack Keep
                Comp Equal
            }
            GLSLPROGRAM
    #ifdef VERTEX
            
            #include "UnityCG.glslinc"

            void main()
            {
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }
    #endif

    #ifdef FRAGMENT
            void main()
            {
                gl_FragColor = vec4(0, 1, 0, 1);
            }
    #endif
            ENDGLSL
        }
    }
}
