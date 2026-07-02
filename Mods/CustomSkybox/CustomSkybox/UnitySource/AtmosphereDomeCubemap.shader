Shader "7DTD/AtmosphereDomeCubemap"
{
    Properties
    {
        [NoScaleOffset] _Tex  ("Cubemap A", Cube) = "" {}
        [NoScaleOffset] _TexB ("Cubemap B", Cube) = "" {}
        _Blend    ("Blend A->B", Range(0,1)) = 0.0
        _Tint     ("Tint", Color) = (1,1,1,1)
        _Exposure ("Exposure", Range(0,8)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0
            #include "UnityCG.cginc"

            UNITY_DECLARE_TEXCUBE(_Tex);
            half4 _Tex_HDR;
            UNITY_DECLARE_TEXCUBE(_TexB);
            half4 _TexB_HDR;
            half  _Blend;
            half4 _Tint;
            half  _Exposure;

            struct appdata { float4 vertex : POSITION; };
            struct v2f     { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                // Bypass the AtmosphereSphere's transform entirely. SkyManager rotates
                // that transform every frame, which would drag the cubemap around with
                // it. Instead, treat each mesh vertex as a world-relative direction from
                // the camera, and build clip-space directly from (cameraPos + v.vertex)
                // using only the view-projection matrix.
                float3 worldPos = _WorldSpaceCameraPos + v.vertex.xyz;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

                // Force depth to the far plane so the sphere is neither clipped by the
                // camera's far plane nor drawn in front of world geometry.
                #if defined(UNITY_REVERSED_Z)
                    o.pos.z = 0.0;
                #else
                    o.pos.z = o.pos.w;
                #endif

                // v.vertex.xyz is the mesh-local direction, which is now also the
                // world-space direction since we never applied the sphere's rotation.
                o.dir = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 texA = UNITY_SAMPLE_TEXCUBE(_Tex,  i.dir);
                half3 colA = DecodeHDR(texA, _Tex_HDR);
                half4 texB = UNITY_SAMPLE_TEXCUBE(_TexB, i.dir);
                half3 colB = DecodeHDR(texB, _TexB_HDR);
                half3 c = lerp(colA, colB, saturate(_Blend)) * _Tint.rgb * _Exposure;
                return half4(c, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
