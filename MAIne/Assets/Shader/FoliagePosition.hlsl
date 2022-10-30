
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float4x4> _Transform;
#endif


void setup()
{
    
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		unity_ObjectToWorld = _Transform[unity_InstanceID];
#endif

}

void ShaderGraphFunction_float(float4x4 In, out float4x4 Out)
{
    Out = In;
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        Out = _Transform[unity_InstanceID];
#endif
}