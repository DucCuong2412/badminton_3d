Shader "Toon/Basic Outline"
{
  Properties
  {
    _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
    _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    _Outline ("Outline width", Range(0.002, 0.03)) = 0.005
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _ToonShade ("ToonShader Cubemap(RGB)", Cube) = "" {}
  }
  SubShader
  {
    Tags
    { 
      "RenderType" = "Opaque"
    }
    Pass // ind: 1, name: 
    {
      Tags
      { 
      }
      ZClip Off
      ZWrite Off
      Cull Off
      Stencil
      { 
        Ref 0
        ReadMask 0
        WriteMask 0
        Pass Keep
        Fail Keep
        ZFail Keep
        PassFront Keep
        FailFront Keep
        ZFailFront Keep
        PassBack Keep
        FailBack Keep
        ZFailBack Keep
      } 
      Fog
      { 
        Mode  Off
      } 
      // m_ProgramMask = 0
      
    } // end phase
    Pass // ind: 2, name: OUTLINE
    {
      Name "OUTLINE"
      Tags
      { 
        "LIGHTMODE" = "ALWAYS"
        "RenderType" = "Opaque"
      }
      ZClip Off
      Cull Front
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB
      // m_ProgramMask = 6
      CGPROGRAM
      //#pragma target 4.0
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      #define conv_mxt4x4_0(mat4x4) float4(mat4x4[0].x,mat4x4[1].x,mat4x4[2].x,mat4x4[3].x)
      #define conv_mxt4x4_1(mat4x4) float4(mat4x4[0].y,mat4x4[1].y,mat4x4[2].y,mat4x4[3].y)
      #define conv_mxt4x4_2(mat4x4) float4(mat4x4[0].z,mat4x4[1].z,mat4x4[2].z,mat4x4[3].z)
      #define conv_mxt4x4_3(mat4x4) float4(mat4x4[0].w,mat4x4[1].w,mat4x4[2].w,mat4x4[3].w)
      
      
      #define CODE_BLOCK_VERTEX
      //uniform float4x4 unity_ObjectToWorld;
      //uniform float4x4 unity_WorldToObject;
      //uniform float4x4 UNITY_MATRIX_P;
      //uniform float4x4 unity_MatrixInvV;
      //uniform float4x4 unity_MatrixVP;
      uniform float _Outline;
      uniform float4 _OutlineColor;
      struct appdata_t
      {
          float4 vertex :POSITION;
          float3 normal :NORMAL;
      };
      
      struct OUT_Data_Vert
      {
          float4 xlv_COLOR :COLOR;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float4 xlv_COLOR :COLOR;
      };
      
      struct OUT_Data_Frag
      {
          float4 color :SV_Target0;
      };
      
      OUT_Data_Vert vert(appdata_t in_v)
      {
          OUT_Data_Vert out_v;
          float4 tmpvar_1;
          float4 tmpvar_2;
          float4 tmpvar_3;
          tmpvar_3.w = 1;
          tmpvar_3.xyz = in_v.vertex.xyz;
          tmpvar_2 = mul(unity_MatrixVP, mul(unity_ObjectToWorld, tmpvar_3));
          tmpvar_1.zw = tmpvar_2.zw;
          float4x4 m_4;
          m_4 = mul(unity_WorldToObject, unity_MatrixInvV);
          float4 tmpvar_5;
          float4 tmpvar_6;
          float4 tmpvar_7;
          tmpvar_5.x = conv_mxt4x4_0(m_4).x;
          tmpvar_5.y = conv_mxt4x4_1(m_4).x;
          tmpvar_5.z = conv_mxt4x4_2(m_4).x;
          tmpvar_5.w = conv_mxt4x4_3(m_4).x;
          tmpvar_6.x = conv_mxt4x4_0(m_4).y;
          tmpvar_6.y = conv_mxt4x4_1(m_4).y;
          tmpvar_6.z = conv_mxt4x4_2(m_4).y;
          tmpvar_6.w = conv_mxt4x4_3(m_4).y;
          tmpvar_7.x = conv_mxt4x4_0(m_4).z;
          tmpvar_7.y = conv_mxt4x4_1(m_4).z;
          tmpvar_7.z = conv_mxt4x4_2(m_4).z;
          tmpvar_7.w = conv_mxt4x4_3(m_4).z;
          float3x3 tmpvar_8;
          tmpvar_8[0] = tmpvar_5.xyz;
          tmpvar_8[1] = tmpvar_6.xyz;
          tmpvar_8[2] = tmpvar_7.xyz;
          float2x2 tmpvar_9;
          tmpvar_9[0] = conv_mxt4x4_0(UNITY_MATRIX_P).xy;
          tmpvar_9[1] = conv_mxt4x4_1(UNITY_MATRIX_P).xy;
          tmpvar_1.xy = (tmpvar_2.xy + mul(mul(mul(tmpvar_9, mul(tmpvar_8, in_v.normal).xy), tmpvar_2.z), _Outline));
          out_v.vertex = tmpvar_1;
          out_v.xlv_COLOR = _OutlineColor;
          return out_v;
      }
      
      #define CODE_BLOCK_FRAGMENT
      OUT_Data_Frag frag(v2f in_f)
      {
          OUT_Data_Frag out_f;
          float4 tmpvar_1;
          tmpvar_1 = in_f.xlv_COLOR;
          out_f.color = tmpvar_1;
          return out_f;
      }
      
      
      ENDCG
      
    } // end phase
  }
  SubShader
  {
    Tags
    { 
      "RenderType" = "Opaque"
    }
    Pass // ind: 1, name: 
    {
      Tags
      { 
      }
      LOD 1065353216
      ZClip Off
      ZWrite Off
      Cull Off
      Stencil
      { 
        Ref 0
        ReadMask 0
        WriteMask 0
        Pass Keep
        Fail Keep
        ZFail Keep
        PassFront Keep
        FailFront Keep
        ZFailFront Keep
        PassBack Keep
        FailBack Keep
        ZFailBack Keep
      } 
      // m_ProgramMask = 0
      
    } // end phase
    Pass // ind: 2, name: OUTLINE
    {
      Name "OUTLINE"
      Tags
      { 
        "LIGHTMODE" = "ALWAYS"
        "RenderType" = "Opaque"
      }
      ZClip Off
      Cull Front
      Fog
      { 
        Mode  Off
      } 
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB
      // m_ProgramMask = 2
      
    } // end phase
  }
  FallBack "Toon/Basic"
}
