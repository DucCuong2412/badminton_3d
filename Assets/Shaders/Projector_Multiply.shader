Shader "Projector/Multiply"
{
  Properties
  {
    _ShadowTex ("Cookie", 2D) = "gray" {}
    _FalloffTex ("FallOff", 2D) = "white" {}
  }
  SubShader
  {
    Tags
    { 
      "QUEUE" = "Transparent"
    }
    Pass // ind: 1, name: 
    {
      Tags
      { 
        "QUEUE" = "Transparent"
      }
      ZClip Off
      ZWrite Off
      Offset -1, -1
      Blend DstColor Zero
      ColorMask RGB
      // m_ProgramMask = 6
      CGPROGRAM
      //#pragma target 4.0
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      
      #define CODE_BLOCK_VERTEX
      //uniform float4x4 unity_ObjectToWorld;
      //uniform float4x4 unity_MatrixVP;
      uniform float4x4 unity_Projector;
      uniform float4x4 unity_ProjectorClip;
      uniform sampler2D _ShadowTex;
      uniform sampler2D _FalloffTex;
      struct appdata_t
      {
          float4 vertex :POSITION;
      };
      
      struct OUT_Data_Vert
      {
          float4 xlv_TEXCOORD0 :TEXCOORD0;
          float4 xlv_TEXCOORD1 :TEXCOORD1;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float4 xlv_TEXCOORD0 :TEXCOORD0;
          float4 xlv_TEXCOORD1 :TEXCOORD1;
      };
      
      struct OUT_Data_Frag
      {
          float4 color :SV_Target0;
      };
      
      OUT_Data_Vert vert(appdata_t in_v)
      {
          OUT_Data_Vert out_v;
          float4 tmpvar_1;
          tmpvar_1.w = 1;
          tmpvar_1.xyz = in_v.vertex.xyz;
          out_v.xlv_TEXCOORD0 = mul(unity_Projector, in_v.vertex);
          out_v.xlv_TEXCOORD1 = mul(unity_ProjectorClip, in_v.vertex);
          out_v.vertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, tmpvar_1));
          return out_v;
      }
      
      #define CODE_BLOCK_FRAGMENT
      OUT_Data_Frag frag(v2f in_f)
      {
          OUT_Data_Frag out_f;
          float4 texS_1;
          float4 tmpvar_2;
          tmpvar_2 = tex2D(_ShadowTex, in_f.xlv_TEXCOORD0);
          texS_1.xyz = tmpvar_2.xyz;
          texS_1.w = (1 - tmpvar_2.w);
          float4 tmpvar_3;
          tmpvar_3 = lerp(float4(1, 1, 1, 0), texS_1, tex2D(_FalloffTex, in_f.xlv_TEXCOORD1).wwww);
          out_f.color = tmpvar_3;
          return out_f;
      }
      
      
      ENDCG
      
    } // end phase
  }
  FallBack Off
}
