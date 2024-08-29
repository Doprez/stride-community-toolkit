// Copyright (c) Stride contributors (https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Graphics;
using Stride.Rendering;
using static Stride.CommunityToolkit.Rendering.DebugShapes.ImmediateDebugRenderFeature;
using Capsule = Stride.CommunityToolkit.Rendering.DebugShapes.ImmediateDebugRenderFeature.Capsule;
using Cone = Stride.CommunityToolkit.Rendering.DebugShapes.ImmediateDebugRenderFeature.Cone;
using Cube = Stride.CommunityToolkit.Rendering.DebugShapes.ImmediateDebugRenderFeature.Cube;
using Cylinder = Stride.CommunityToolkit.Rendering.DebugShapes.ImmediateDebugRenderFeature.Cylinder;
using Sphere = Stride.CommunityToolkit.Rendering.DebugShapes.ImmediateDebugRenderFeature.Sphere;

namespace Stride.CommunityToolkit.Rendering.DebugShapes;

public class ImmediateDebugRenderObject : RenderObject
{
    /* messages */
    internal List<Renderable> RenderablesWithDepth = [];
    internal List<Renderable> RenderablesNoDepth = [];

    /* accumulators used when data is being pushed to the system */
    internal Primitives TotalPrimitives, TotalPrimitivesNoDepth;

    /* used to specify offset into instance data buffers when drawing */
    internal Primitives InstanceOffsets, InstanceOffsetsNoDepth;

    /* used in render stage to know how many of each instance to draw */
    internal Primitives PrimitivesToDraw, PrimitivesToDrawNoDepth;

    /* state set from outside */
    internal FillMode CurrentFillMode { get; set; } = FillMode.Wireframe;

    internal DebugRenderStage Stage { get; set; }

    public void DrawQuad(ref Vector3 position, ref Vector2 size, ref Quaternion rotation, ref Color color, bool depthTest = true)
    {
        var cmd = new Quad() { Position = position, Size = size, Rotation = rotation, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Quads++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Quads++;
        }
    }

    public void DrawCircle(ref Vector3 position, float radius, ref Quaternion rotation, ref Color color, bool depthTest = true)
    {
        var cmd = new Circle() { Position = position, Radius = radius, Rotation = rotation, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Circles++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Circles++;
        }
    }

    public void DrawSphere(ref Vector3 position, float radius, ref Color color, bool depthTest = true)
    {
        var cmd = new Sphere() { Position = position, Radius = radius, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Spheres++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Spheres++;
        }
    }

    public void DrawHalfSphere(ref Vector3 position, float radius, ref Quaternion rotation, ref Color color, bool depthTest = true)
    {
        var cmd = new HalfSphere() { Position = position, Radius = radius, Rotation = rotation, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.HalfSpheres++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.HalfSpheres++;
        }
    }

    public void DrawCube(ref Vector3 start, ref Vector3 end, ref Quaternion rotation, ref Color color, bool depthTest = true)
    {
        var cmd = new Cube() { Start = start, End = end, Rotation = rotation, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Cubes++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Cubes++;
        }
    }

    public void DrawCapsule(ref Vector3 position, float height, float radius, ref Quaternion rotation, ref Color color, bool depthTest = true)
    {
        var cmd = new Capsule() { Position = position, Height = height, Radius = radius, Rotation = rotation, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Capsules++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Capsules++;
        }
    }

    public void DrawCylinder(ref Vector3 position, float height, float radius, ref Quaternion rotation, ref Color color, bool depthTest = true)
    {
        var cmd = new Cylinder() { Position = position, Height = height, Radius = radius, Rotation = rotation, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Cylinders++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Cylinders++;
        }
    }

    public void DrawCone(ref Vector3 position, float height, float radius, ref Quaternion rotation, ref Color color, bool depthTest = true)
    {
        var cmd = new Cone() { Position = position, Height = height, Radius = radius, Rotation = rotation, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Cones++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Cones++;
        }
    }

    public void DrawLine(ref Vector3 start, ref Vector3 end, ref Color color, bool depthTest = true)
    {
        var cmd = new Line() { Start = start, End = end, Color = color };
        var msg = new Renderable(ref cmd);
        if (depthTest)
        {
            RenderablesWithDepth.Add(msg);
            TotalPrimitives.Lines++;
        }
        else
        {
            RenderablesNoDepth.Add(msg);
            TotalPrimitivesNoDepth.Lines++;
        }
    }
}