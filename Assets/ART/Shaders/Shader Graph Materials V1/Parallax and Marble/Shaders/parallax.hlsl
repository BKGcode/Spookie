void parallax_float(
  float iterations,
  float heightScale,
  float3 viewDir,
  Texture2D HeightTex,
  float2 uv,
  SamplerState sampleState,
  out float2 Out
)
{
  float numSteps = min(50, iterations); // Clamp the number of steps to a reasonable range

  // Calculate the step size and initial height
  float step = 1.0 / numSteps;
  float height = 1.0; // Start with a height of 1

  // Initialize the UV offset and height map
  float2 offset = uv.xy;
  float4 HeightMap = HeightTex.Sample(sampleState, offset);

  // Calculate the delta value for the parallax effect
  float2 delta = viewDir.xy * heightScale / (viewDir.z * numSteps);

  // Variables to store the UV coordinates and height map value before the intersection
  float2 prevOffset = offset;
  float prevHeightMap = HeightMap.r;

  // Perform the parallax mapping
  for (float i = 0; i < numSteps; i++)
  {
      if (HeightMap.r < height)
      {
          // Store previous values
          prevOffset = offset;
          prevHeightMap = HeightMap.r;

          // Update height and offset for the next step
          height -= step;
          offset += delta;
          HeightMap = HeightTex.Sample(sampleState, offset);
      }
      else
      {
        break;
      }
    }
    // Interpolate between the last two depth samples for improved accuracy
  float afterDepth = HeightMap.r - height;
  float beforeDepth = prevHeightMap - (height + step);

  // Calculate the weight for interpolation
  float weight = afterDepth / (afterDepth - beforeDepth);

  // Interpolate UV coordinates
  float2 finalOffset = prevOffset * weight + offset * (1.0 - weight);

  // Output the final UV offset
  Out = finalOffset;
}