Shader "Unlit/ObjectA"
{
    Properties
    {
        _ColorRed ("Red", Color) = (1,0,0,1)
        _ColorBlue ("Blue", Color) = (0,0,1,1)
        _Blend ("Blend", float) = 0.0

        _Displ ("Displace", float) = 0.0
        _Freq ("Noise Frequency", float) = 1.0
        _Speed ("Speed", float) = 1.0
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _ColorRed;
            float4 _ColorBlue;
            float _Blend;
            float _Displ;
            float _Freq;
            float _Speed;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float hash (float3 p)
            {
                p=frac(p*0.3183099+0.1);
                p*=17.0;
                return frac(p.x*p.y*p.z*(p.x+p.y+p.z));
            }

            float noise(float3 p)
            {
                float3 i=floor(p);
                float3 f=frac(p);
                f = f*f*(3.0-2.0*f);
                float n000 = hash(i+float3(0.0,0.0,0.0));
                float n100 = hash(i+float3(1.0,0.0,0.0));
                float n010 = hash(i+float3(0.0,1.0,0.0));
                float n110 = hash(i+float3(1.0,1.0,0.0));
                float n001 = hash(i+float3(0.0,0.0,1.0));
                float n101 = hash(i+float3(1.0,0.0,1.0));
                float n011 = hash(i+float3(0.0,1.0,1.0));
                float n111 = hash(i+float3(1.0,1.0,1.0));

                float n00 = lerp(n000,n100,f.x);
                float n10 = lerp(n010,n110,f.x);
                float n01 = lerp(n001,n101,f.x);
                float n11 = lerp(n011,n111,f.x);

                float n0 = lerp(n00,n10,f.y);
                float n1 = lerp(n01,n11,f.y);

                return lerp(n0,n1,f.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float3 worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
                float n = noise(worldPos*_Freq+_Time.y*_Speed);
                o.vertex.xyz += v.normal*(n*_Displ);
                return o;
            }



            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = lerp(_ColorBlue,_ColorRed,_Blend);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
