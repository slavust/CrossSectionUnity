Shader "Custom/BadContourStub"
{
    SubShader
    {
        Tags { "Queue" = "Geometry" }    
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
        }
    }
}
