using Stride.Core.Mathematics;
using Stride.Engine;

namespace Example_CubicleCalamity.Components;

public class CubeComponent : EntityComponent
{
    public Color Color { get; set; }

    public CubeComponent(Color color) => Color = color;
}