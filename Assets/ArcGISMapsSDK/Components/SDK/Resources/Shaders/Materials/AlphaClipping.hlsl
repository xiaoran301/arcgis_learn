void AlphaClipping_float(float3 worldPosition, float4x4 objectToWorldMatrix, int clippingMode, float3 mapAreaMin, float3 mapAreaMax, out float alpha)
{
	if (clippingMode == 1)
	{
		float3 minE = mul(objectToWorldMatrix, float4(mapAreaMin, 1.0f));
		float3 maxE = mul(objectToWorldMatrix, float4(mapAreaMax, 1.0f));

		float radius = (maxE.x - minE.x) * 0.5f;
		float3 center = (minE + maxE) * 0.5f;

		alpha = distance(center, float3(worldPosition.x, center.y, worldPosition.z)) < radius ? 1.0f : 0.0f;
	}
	else if(clippingMode == 2)
	{
		float3 minE = mul(objectToWorldMatrix, float4(mapAreaMin, 1.0f));
		float3 maxE = mul(objectToWorldMatrix, float4(mapAreaMax, 1.0f));

		alpha = minE.x < worldPosition.x && minE.z < worldPosition.z && maxE.x > worldPosition.x && maxE.z > worldPosition.z ? 1.0f : 0.0f;
	}
	else
	{
		alpha = 1.0f;
	}
}
