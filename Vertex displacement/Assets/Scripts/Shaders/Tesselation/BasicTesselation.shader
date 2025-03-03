// This shader adds tessellation in URP
Shader "Custom/URPUnlitShaderTessellated"
{
 
	// The properties block of the Unity shader. In this example this block is empty
	// because the output color is predefined in the fragment shader code.
	Properties
	{
		_Tess("Tessellation", Range(1, 32)) = 20
		_MaxTessDistance("Max Tess Distance", Range(1, 32)) = 20
	}
 
	// The SubShader block containing the Shader code. 
	SubShader
	{
		// SubShader Tags define when and under which conditions a SubShader block or
		// a pass is executed.
		Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
 
		Pass
		{
			Tags{ "LightMode" = "UniversalForward" }
 
 
			// The HLSL code block. Unity SRP uses the HLSL language.
			HLSLPROGRAM
			// The Core.hlsl file contains definitions of frequently used HLSL
			// macros and functions, and also contains #include references to other
			// HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"    
			#include "CustomTessellation.hlsl"
 
			// This line defines the name of the hull shader. 
			#pragma hull hull
			// This line defines the name of the domain shader. 
			#pragma domain domain
			// This line defines the name of the vertex shader. 
			#pragma vertex TessellationVertexProgram
			// This line defines the name of the fragment shader. 
			#pragma fragment frag
 
			// pre tesselation vertex program
			ControlPoint TessellationVertexProgram(Attributes v)
			{
				ControlPoint p;
 
				p.vertex = v.vertex;
				p.uv = v.uv;
				p.normal = v.normal;
				p.color = v.color;
 
				return p;
			}
 
			// The fragment shader definition.            
			half4 frag(Varyings IN) : SV_Target
			{
				return float4(1, 1, 1, 1);
			}
			ENDHLSL
		}
	}
}