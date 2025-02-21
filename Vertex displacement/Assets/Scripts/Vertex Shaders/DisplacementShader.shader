Shader "Custom/DisplacementShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            StructuredBuffer<float> displacementBuffer;
            int displacementBufferSize;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                uint id : TEXCOORD2;
            };

            v2f vert (appdata_t v, uint id : SV_VertexID)
            {
                v2f o;
                float displacement = displacementBuffer[id]; // Fetch displacement
                o.uv = v.uv;
                o.id = id;
                o.normal = UnityObjectToWorldNormal(v.normal);
                float3 normalDisplacement = UnityObjectToWorldNormal(v.normal) * displacement;
                v.vertex += float4(normalDisplacement.xyz, 0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float id = (float)i.id / displacementBufferSize;
                //return fixed4(id, 1-id, 1, 1); // White color
                return fixed4(i.normal.xyz + id, 1);
            }
            ENDCG
        }
    }
}
