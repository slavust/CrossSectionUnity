Shader "Custom/bad_contour_stub_shader"
{
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
                FailFront IncrWrap
                FailBack IncrWrap
                ZFailFront IncrWrap
                ZFailBack IncrWrap
                Comp Always
            }
	        ZTest Never
            ZWrite Off
        }
    }
}
