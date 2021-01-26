Shader "Custom/GPUInstanceShader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _DefaultColor("Default Color", Color) = (1,1,1,1)
        _RainbowMode("Rainbow Mode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        
        #pragma surface surf SimpleUnlit vertex:vert
        #pragma instancing_options procedural:CalculateProcedural
        
        struct ParticleStruct {
            float3 pos;
            float3 vel;
            float time;
        };

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<ParticleStruct> _Particles;
            float _StartScale;
            float _EndScale;
            float _TimeToSet;
        #endif
       
        sampler2D _MainTex;
        half4 _DefaultColor;
        fixed _RainbowMode;

        struct Input 
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

        void CalculateProcedural() {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                float3 position = _Particles[unity_InstanceID].pos;
                float scale = lerp(_StartScale, _EndScale, 1-_Particles[unity_InstanceID].time / _TimeToSet);
                unity_ObjectToWorld = 0.0;
                unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
                unity_ObjectToWorld._m00_m11_m22 = scale;
            #endif
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float3 forward = -normalize(UNITY_MATRIX_V._m20_m21_m22);
            float3 up = normalize(UNITY_MATRIX_V._m10_m11_m12);
            float3 right = normalize(UNITY_MATRIX_V._m00_m01_m02);

            float4x4 rotationMatrix = float4x4(right, 0,
                up, 0,
                forward, 0,
                0, 0, 0, 1);
            v.vertex = mul(v.vertex, rotationMatrix);
            v.normal = mul(v.normal, rotationMatrix);
        }

        void surf(Input IN, inout SurfaceOutput surface) 
        {
            surface.Albedo = tex2D(_MainTex,IN.uv_MainTex)*_DefaultColor.rgb*((1-_RainbowMode)+_RainbowMode*frac(IN.worldPos));
        }

        half4 LightingSimpleUnlit(SurfaceOutput s, half3 lightDir, half atten) {
            half4 c = half4(s.Albedo, 1);
            return c;
        }
        ENDCG
    }
}
