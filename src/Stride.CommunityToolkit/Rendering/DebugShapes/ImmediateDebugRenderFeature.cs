// Copyright (c) Stride contributors (https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.CommunityToolkit.Extensions;
using Stride.Core.Collections;
using Stride.Core.Threading;
using Stride.DebugRendering;
using Stride.Graphics;
using Stride.Rendering;
using System.Runtime.InteropServices;
using Buffer = Stride.Graphics.Buffer;

namespace Stride.CommunityToolkit.Rendering.DebugShapes;

public class ImmediateDebugRenderFeature : RootRenderFeature
{

    internal enum DebugRenderStage
    {
        Opaque,
        Transparent
    }

    public override Type SupportedRenderObjectType => typeof(ImmediateDebugRenderObject);

    internal struct Primitives
    {

        public int Quads;
        public int Circles;
        public int Spheres;
        public int HalfSpheres;
        public int Cubes;
        public int Capsules;
        public int Cylinders;
        public int Cones;
        public int Lines;

        public void Clear()
        {
            Quads = 0;
            Circles = 0;
            Spheres = 0;
            HalfSpheres = 0;
            Cubes = 0;
            Capsules = 0;
            Cylinders = 0;
            Cones = 0;
            Lines = 0;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct LineVertex
    {

        public static readonly VertexDeclaration Layout = new VertexDeclaration(VertexElement.Position<Vector3>(), VertexElement.Color<Color>());

        public Vector3 Position;
        public Color Color;

    }

    internal enum RenderableType : byte
    {
        Quad,
        Circle,
        Sphere,
        HalfSphere,
        Cube,
        Capsule,
        Cylinder,
        Cone,
        Line
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct Renderable
    {

        public Renderable(ref Quad q) : this()
        {
            Type = RenderableType.Quad;
            QuadData = q;
        }

        public Renderable(ref Circle c) : this()
        {
            Type = RenderableType.Circle;
            CircleData = c;
        }

        public Renderable(ref Sphere s) : this()
        {
            Type = RenderableType.Sphere;
            SphereData = s;
        }

        public Renderable(ref HalfSphere h) : this()
        {
            Type = RenderableType.HalfSphere;
            HalfSphereData = h;
        }

        public Renderable(ref Cube c) : this()
        {
            Type = RenderableType.Cube;
            CubeData = c;
        }

        public Renderable(ref Capsule c) : this()
        {
            Type = RenderableType.Capsule;
            CapsuleData = c;
        }

        public Renderable(ref Cylinder c) : this()
        {
            Type = RenderableType.Cylinder;
            CylinderData = c;
        }

        public Renderable(ref Cone c) : this()
        {
            Type = RenderableType.Cone;
            ConeData = c;
        }

        public Renderable(ref Line l) : this()
        {
            Type = RenderableType.Line;
            LineData = l;
        }

        [FieldOffset(0)]
        public RenderableType Type;

        [FieldOffset(1)]
        public Quad QuadData;

        [FieldOffset(1)]
        public Circle CircleData;

        [FieldOffset(1)]
        public Sphere SphereData;

        [FieldOffset(1)]
        public HalfSphere HalfSphereData;

        [FieldOffset(1)]
        public Cube CubeData;

        [FieldOffset(1)]
        public Capsule CapsuleData;

        [FieldOffset(1)]
        public Cylinder CylinderData;

        [FieldOffset(1)]
        public Cone ConeData;

        [FieldOffset(1)]
        public Line LineData;

    }

    internal struct Quad
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector2 Size;
        public Color Color;
    }

    internal struct Circle
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Radius;
        public Color Color;
    }

    internal struct Sphere
    {
        public Vector3 Position;
        public float Radius;
        public Color Color;
    }

    internal struct HalfSphere
    {
        public Vector3 Position;
        public float Radius;
        public Quaternion Rotation;
        public Color Color;
    }

    internal struct Cube
    {
        public Vector3 Start;
        public Vector3 End;
        public Quaternion Rotation;
        public Color Color;
    }

    internal struct Capsule
    {
        public Vector3 Position;
        public float Height;
        public float Radius;
        public Quaternion Rotation;
        public Color Color;
    }

    internal struct Cylinder
    {
        public Vector3 Position;
        public float Height;
        public float Radius;
        public Quaternion Rotation;
        public Color Color;
    }

    internal struct Cone
    {
        public Vector3 Position;
        public float Height;
        public float Radius;
        public Quaternion Rotation;
        public Color Color;
    }

    internal struct Line
    {
        public Vector3 Start;
        public Vector3 End;
        public Color Color;
    }

    internal struct InstanceData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public Color Color;
    }

    private const float DefaultCircleRadius = 0.5f;
    private const float DefaultSphereRadius = 0.5f;
    private const float DefaultCubeSize = 1.0f;
    private const float DefaultCapsuleLength = 1.0f;
    private const float DefaultCapsuleRadius = 0.5f;
    private const float DefaultCylinderHeight = 1.0f;
    private const float DefaultCylinderRadius = 0.5f;
    private const float DefaultPlaneSize = 1.0f;
    private const float DefaultConeRadius = 0.5f;
    private const float DefaultConeHeight = 1.0f;

    private const int CircleTesselation = 16;
    private const int SphereTesselation = 8;
    private const int CapsuleTesselation = 8;
    private const int CylinderTesselation = 16;
    private const int ConeTesselation = 16;

    /* mesh data we will use when stuffing things in vertex buffers */
    private readonly (VertexPositionTexture[] Vertices, int[] Indices) _circle = ImmediateDebugPrimitives.GenerateCircle(DefaultCircleRadius, CircleTesselation);
    private readonly (VertexPositionTexture[] Vertices, int[] Indices) _plane = ImmediateDebugPrimitives.GenerateQuad(DefaultPlaneSize, DefaultPlaneSize);
    private readonly (VertexPositionTexture[] Vertices, int[] Indices) _sphere = ImmediateDebugPrimitives.GenerateSphere(DefaultSphereRadius, SphereTesselation, uvSplitOffsetVertical: 1);
    private readonly (VertexPositionTexture[] Vertices, int[] Indices) _cube = ImmediateDebugPrimitives.GenerateCube(DefaultCubeSize);
    private readonly (VertexPositionTexture[] Vertices, int[] Indices) _capsule = ImmediateDebugPrimitives.GenerateCapsule(DefaultCapsuleLength, DefaultCapsuleRadius, CapsuleTesselation);
    private readonly (VertexPositionTexture[] Vertices, int[] Indices) _cylinder = ImmediateDebugPrimitives.GenerateCylinder(DefaultCylinderHeight, DefaultCylinderRadius, CylinderTesselation);
    private readonly (VertexPositionTexture[] Vertices, int[] Indices) _cone = ImmediateDebugPrimitives.GenerateCone(DefaultConeHeight, DefaultConeRadius, ConeTesselation, uvSplits: 8);

    /* vertex and index buffer for our primitive data */
    private Buffer _vertexBuffer;
    private Buffer _indexBuffer;

    /* vertex buffer for line rendering */
    private Buffer _lineVertexBuffer;

    /* offsets into our vertex/index buffer */
    private Primitives _primitiveVertexOffsets;
    private Primitives _primitiveIndexOffsets;

    /* other gpu related data */
    private MutablePipelineState _pipelineState;
    private InputElementDescription[] _inputElements;
    private InputElementDescription[] _lineInputElements;
    private DynamicEffectInstance _primitiveEffect;
    private DynamicEffectInstance _lineEffect;
    private Buffer _transformBuffer;
    private Buffer _colorBuffer;

    /* intermediate message related data, written to in extract */
    private readonly List<InstanceData> _instances = [];

    /* data written to buffers in prepare */
    private readonly List<Matrix> _transforms = new(1);
    private readonly List<Color> _colors = new(1);

    /* data only for line rendering */
    private readonly List<LineVertex> _lineVertices = new();

    public ImmediateDebugRenderFeature()
    {
        SortKey = 0xFF; // render last! .. or things without depth testing in the opaque stage will have the background rendered over them
    }

    protected override void InitializeCore()
    {

        var device = Context.GraphicsDevice;

        _inputElements = VertexPositionTexture.Layout.CreateInputElements();
        _lineInputElements = LineVertex.Layout.CreateInputElements();

        // create our pipeline state object
        _pipelineState = new MutablePipelineState(device);
        _pipelineState.State.SetDefaults();

        // TODO: create our associated effect
        _primitiveEffect = new DynamicEffectInstance("PrimitiveShader");
        _primitiveEffect.Initialize(Context.Services);
        _primitiveEffect.UpdateEffect(device);

        _lineEffect = new DynamicEffectInstance("LinePrimitiveShader");
        _lineEffect.Initialize(Context.Services);
        _lineEffect.UpdateEffect(device);

        {

            // create initial vertex and index buffers
            var vertexData = new VertexPositionTexture[
                _circle.Vertices.Length +
                _plane.Vertices.Length +
                _sphere.Vertices.Length +
                _cube.Vertices.Length +
                _capsule.Vertices.Length +
                _cylinder.Vertices.Length +
                _cone.Vertices.Length
            ];

            /* set up vertex buffer data */

            int vertexBufferOffset = 0;

            Array.Copy(_circle.Vertices, vertexData, _circle.Vertices.Length);
            _primitiveVertexOffsets.Circles = vertexBufferOffset;
            vertexBufferOffset += _circle.Vertices.Length;

            Array.Copy(_plane.Vertices, 0, vertexData, vertexBufferOffset, _plane.Vertices.Length);
            _primitiveVertexOffsets.Quads = vertexBufferOffset;
            vertexBufferOffset += _plane.Vertices.Length;

            Array.Copy(_sphere.Vertices, 0, vertexData, vertexBufferOffset, _sphere.Vertices.Length);
            _primitiveVertexOffsets.Spheres = vertexBufferOffset;
            _primitiveVertexOffsets.HalfSpheres = vertexBufferOffset; // same as spheres
            vertexBufferOffset += _sphere.Vertices.Length;

            Array.Copy(_cube.Vertices, 0, vertexData, vertexBufferOffset, _cube.Vertices.Length);
            _primitiveVertexOffsets.Cubes = vertexBufferOffset;
            vertexBufferOffset += _cube.Vertices.Length;

            Array.Copy(_capsule.Vertices, 0, vertexData, vertexBufferOffset, _capsule.Vertices.Length);
            _primitiveVertexOffsets.Capsules = vertexBufferOffset;
            vertexBufferOffset += _capsule.Vertices.Length;

            Array.Copy(_cylinder.Vertices, 0, vertexData, vertexBufferOffset, _cylinder.Vertices.Length);
            _primitiveVertexOffsets.Cylinders = vertexBufferOffset;
            vertexBufferOffset += _cylinder.Vertices.Length;

            Array.Copy(_cone.Vertices, 0, vertexData, vertexBufferOffset, _cone.Vertices.Length);
            _primitiveVertexOffsets.Cones = vertexBufferOffset;
            vertexBufferOffset += _cone.Vertices.Length;

            var newVertexBuffer = Buffer.Vertex.New(device, vertexData);
            _vertexBuffer = newVertexBuffer;

        }

        {

            /* set up index buffer data */

            var indexData = new int[
                _circle.Indices.Length +
                _plane.Indices.Length +
                _sphere.Indices.Length +
                _cube.Indices.Length +
                _capsule.Indices.Length +
                _cylinder.Indices.Length +
                _cone.Indices.Length
            ];

            if (indexData.Length >= 0xFFFF && device.Features.CurrentProfile <= GraphicsProfile.Level_9_3)
            {
                throw new InvalidOperationException("Cannot generate more than 65535 indices on feature level HW <= 9.3");
            }

            // copy all our primitive data into the buffers

            int indexBufferOffset = 0;

            Array.Copy(_circle.Indices, indexData, _circle.Indices.Length);
            _primitiveIndexOffsets.Circles = indexBufferOffset;
            indexBufferOffset += _circle.Indices.Length;

            Array.Copy(_plane.Indices, 0, indexData, indexBufferOffset, _plane.Indices.Length);
            _primitiveIndexOffsets.Quads = indexBufferOffset;
            indexBufferOffset += _plane.Indices.Length;

            Array.Copy(_sphere.Indices, 0, indexData, indexBufferOffset, _sphere.Indices.Length);
            _primitiveIndexOffsets.Spheres = indexBufferOffset;
            _primitiveIndexOffsets.HalfSpheres = indexBufferOffset; // same as spheres
            indexBufferOffset += _sphere.Indices.Length;

            Array.Copy(_cube.Indices, 0, indexData, indexBufferOffset, _cube.Indices.Length);
            _primitiveIndexOffsets.Cubes = indexBufferOffset;
            indexBufferOffset += _cube.Indices.Length;

            Array.Copy(_capsule.Indices, 0, indexData, indexBufferOffset, _capsule.Indices.Length);
            _primitiveIndexOffsets.Capsules = indexBufferOffset;
            indexBufferOffset += _capsule.Indices.Length;

            Array.Copy(_cylinder.Indices, 0, indexData, indexBufferOffset, _cylinder.Indices.Length);
            _primitiveIndexOffsets.Cylinders = indexBufferOffset;
            indexBufferOffset += _cylinder.Indices.Length;

            Array.Copy(_cone.Indices, 0, indexData, indexBufferOffset, _cone.Indices.Length);
            _primitiveIndexOffsets.Cones = indexBufferOffset;
            indexBufferOffset += _cone.Indices.Length;

            var newIndexBuffer = Buffer.Index.New(device, indexData);
            _indexBuffer = newIndexBuffer;

        }

        // allocate our buffers with position/colour etc data
        _transforms.Add(new Matrix());
        var newTransformBuffer = Buffer.Structured.New<Matrix>(device, 1);
        _transformBuffer = newTransformBuffer;

        _colors.Add(new Color());
        var newColourBuffer = Buffer.Structured.New<Color>(device, 1);
        _colorBuffer = newColourBuffer;

        // Add a default value to give the buffer a default unit size
        _lineVertices.Add(new LineVertex());
        var newLineVertexBuffer = Buffer.Vertex.New(device, _lineVertices.ToArray(), GraphicsResourceUsage.Dynamic);
        _lineVertexBuffer = newLineVertexBuffer;
    }

    public override void Extract()
    {

        void ProcessRenderables(List<Renderable> renderables, ref Primitives offsets)
        {

            for (int i = 0; i < renderables.Count; ++i)
            {
                var cmd = renderables[i];
                switch (cmd.Type)
                {
                    case RenderableType.Quad:
                        var quad = new InstanceData();
                        quad.Position = cmd.QuadData.Position;
                        quad.Rotation = cmd.QuadData.Rotation;
                        quad.Scale = new Vector3(cmd.QuadData.Size.X, 1.0f, cmd.QuadData.Size.Y);
                        quad.Color = cmd.QuadData.Color;
                        _instances.Add(quad);
                        offsets.Quads++;
                        break;
                    case RenderableType.Circle:
                        var circle = new InstanceData();
                        circle.Position = cmd.CircleData.Position;
                        circle.Rotation = cmd.CircleData.Rotation;
                        circle.Scale = new Vector3(cmd.CircleData.Radius * 2.0f, 0.0f, cmd.CircleData.Radius * 2.0f);
                        circle.Color = cmd.CircleData.Color;
                        _instances.Add(circle);
                        offsets.Circles++;
                        break;
                    case RenderableType.Sphere:
                        var sphere = new InstanceData();
                        sphere.Position = cmd.SphereData.Position;
                        sphere.Rotation = Quaternion.Identity;
                        sphere.Scale = new Vector3(cmd.SphereData.Radius * 2);
                        sphere.Color = cmd.SphereData.Color;
                        _instances.Add(sphere);
                        offsets.Spheres++;
                        break;
                    case RenderableType.HalfSphere:
                        var halfSphere = new InstanceData();
                        halfSphere.Position = cmd.HalfSphereData.Position;
                        halfSphere.Rotation = cmd.HalfSphereData.Rotation;
                        halfSphere.Scale = new Vector3(cmd.HalfSphereData.Radius * 2);
                        halfSphere.Color = cmd.HalfSphereData.Color;
                        _instances.Add(halfSphere);
                        offsets.HalfSpheres++;
                        break;
                    case RenderableType.Cube:
                        ref var start = ref cmd.CubeData.Start;
                        ref var end = ref cmd.CubeData.End;
                        var cube = new InstanceData();
                        cube.Position = start;
                        cube.Rotation = cmd.CubeData.Rotation;
                        cube.Scale = end - start;
                        cube.Color = cmd.CubeData.Color;
                        _instances.Add(cube);
                        offsets.Cubes++;
                        break;
                    case RenderableType.Capsule:
                        var capsule = new InstanceData();
                        capsule.Position = cmd.CapsuleData.Position;
                        capsule.Rotation = cmd.CapsuleData.Rotation;
                        capsule.Scale = new Vector3(cmd.CapsuleData.Radius * 2.0f, cmd.CapsuleData.Height, cmd.CapsuleData.Radius * 2.0f);
                        capsule.Color = cmd.CapsuleData.Color;
                        _instances.Add(capsule);
                        offsets.Capsules++;
                        break;
                    case RenderableType.Cylinder:
                        var cylinder = new InstanceData();
                        cylinder.Position = cmd.CylinderData.Position;
                        cylinder.Rotation = cmd.CylinderData.Rotation;
                        cylinder.Scale = new Vector3(cmd.CylinderData.Radius * 2.0f, cmd.CylinderData.Height, cmd.CylinderData.Radius * 2.0f);
                        cylinder.Color = cmd.CylinderData.Color;
                        _instances.Add(cylinder);
                        offsets.Cylinders++;
                        break;
                    case RenderableType.Cone:
                        var cone = new InstanceData();
                        cone.Position = cmd.ConeData.Position;
                        cone.Rotation = cmd.ConeData.Rotation;
                        cone.Scale = new Vector3(cmd.ConeData.Radius * 2.0f, cmd.ConeData.Height, cmd.ConeData.Radius * 2.0f);
                        cone.Color = cmd.ConeData.Color;
                        _instances.Add(cone);
                        offsets.Cones++;
                        break;
                    case RenderableType.Line:
                        var lineStart = new LineVertex();
                        lineStart.Position = cmd.LineData.Start;
                        lineStart.Color = cmd.LineData.Color;
                        _lineVertices.Add(lineStart);
                        offsets.Lines++;
                        var lineEnd = new LineVertex();
                        lineEnd.Position = cmd.LineData.End;
                        lineEnd.Color = cmd.LineData.Color;
                        _lineVertices.Add(lineEnd);
                        offsets.Lines++;
                        break;
                }
                renderables[i] = cmd;
            }

        }

        int SumBasicPrimitives(ref Primitives primitives)
        {
            return primitives.Quads
                + primitives.Circles
                + primitives.Spheres
                + primitives.HalfSpheres
                + primitives.Cubes
                + primitives.Capsules
                + primitives.Cylinders
                + primitives.Cones;
        }

        Primitives SetupPrimitiveOffsets(ref Primitives counts, int offset = 0)
        {
            var offsets = new Primitives();
            offsets.Quads = 0 + offset;
            offsets.Circles = offsets.Quads + counts.Quads;
            offsets.Spheres = offsets.Circles + counts.Circles;
            offsets.HalfSpheres = offsets.Spheres + counts.Spheres;
            offsets.Cubes = offsets.HalfSpheres + counts.HalfSpheres;
            offsets.Capsules = offsets.Cubes + counts.Cubes;
            offsets.Cylinders = offsets.Capsules + counts.Capsules;
            offsets.Cones = offsets.Cylinders + counts.Cylinders;
            return offsets;
        }

        int lastOffset = 0;
        int lastLineOffset = 0;
        foreach (RenderObject renderObject in RenderObjects)
        {

            ImmediateDebugRenderObject debugObject = (ImmediateDebugRenderObject)renderObject;

            /* everything except lines is included here, as lines just get accumulated into a buffer directly */
            int primitivesWithDepth = SumBasicPrimitives(ref debugObject.totalPrimitives);
            int primitivesWithoutDepth = SumBasicPrimitives(ref debugObject.totalPrimitivesNoDepth);
            int totalThingsToDraw = primitivesWithDepth + primitivesWithoutDepth;

            //instances.Resize(instances.Count + totalThingsToDraw, true);

            for(int i = 0;  i < totalThingsToDraw; i++)
            {
                _instances.Add(new InstanceData());
            }

            //_lineVertices.Resize(lineVertices.Count + debugObject.totalPrimitives.Lines * 2 + debugObject.totalPrimitivesNoDepth.Lines * 2, true);

            var test = _lineVertices.AsSpan();

            for(int i = 0; i < debugObject.totalPrimitives.Lines * 2 + debugObject.totalPrimitivesNoDepth.Lines * 2; i++)
            {
                _lineVertices.Add(new LineVertex());
            }

            var primitiveOffsets = SetupPrimitiveOffsets(ref debugObject.totalPrimitives, lastOffset);
            var primitiveOffsetsNoDepth = SetupPrimitiveOffsets(ref debugObject.totalPrimitivesNoDepth, primitiveOffsets.Cones + debugObject.totalPrimitives.Cones);

            /* line rendering data, separate buffer so offset isnt relative to the other data */
            primitiveOffsets.Lines = 0 + lastLineOffset;
            primitiveOffsetsNoDepth.Lines = debugObject.totalPrimitives.Lines * 2 + lastLineOffset;

            /* save instance offsets before we mutate them as we need them when rendering */
            debugObject.instanceOffsets = primitiveOffsets;
            debugObject.instanceOffsetsNoDepth = primitiveOffsetsNoDepth;

            ProcessRenderables(debugObject.renderablesWithDepth, ref primitiveOffsets);
            ProcessRenderables(debugObject.renderablesNoDepth, ref primitiveOffsetsNoDepth);

            debugObject.primitivesToDraw = debugObject.totalPrimitives;
            debugObject.primitivesToDrawNoDepth = debugObject.totalPrimitivesNoDepth;

            // store the last offsets, so we can start from there next iteration
            lastOffset = debugObject.instanceOffsetsNoDepth.Cones + debugObject.totalPrimitivesNoDepth.Cones;
            lastLineOffset = debugObject.instanceOffsetsNoDepth.Lines + debugObject.totalPrimitivesNoDepth.Lines * 2;

            // only now clear this data...
            debugObject.renderablesWithDepth.Clear();
            debugObject.renderablesNoDepth.Clear();
            debugObject.totalPrimitives.Clear();
            debugObject.totalPrimitivesNoDepth.Clear();

        }
    }

    private unsafe static void UpdateBufferIfNecessary(GraphicsDevice device, CommandList commandList, ref Buffer buffer, DataPointer dataPtr, int elementSize)
    {
        int neededBufferSize = dataPtr.Size / elementSize;
        if (neededBufferSize > buffer.ElementCount)
        {
            buffer.Dispose();
            var newBuffer = Stride.Graphics.Buffer.New(
                device,
                dataPtr,
                buffer.StructureByteStride,
                buffer.Flags
            );
            buffer = newBuffer;
        }
        else
        {
            buffer.SetData(commandList, dataPtr);
        }
    }

    private void CheckBuffers(RenderDrawContext context)
    {
        unsafe
        {
            fixed (Matrix* transformsPtr = _transforms.AsSpan())
            {
                UpdateBufferIfNecessary(
                    context.GraphicsDevice, context.CommandList, buffer: ref _transformBuffer,
                    dataPtr: new DataPointer(transformsPtr, _transforms.Count * Marshal.SizeOf<Matrix>()),
                    elementSize: Marshal.SizeOf<Matrix>()
                );
            }

            fixed (Color* colorsPtr = _colors.AsSpan())
            {
                UpdateBufferIfNecessary(
                    context.GraphicsDevice, context.CommandList, buffer: ref _colorBuffer,
                    dataPtr: new DataPointer(colorsPtr, _colors.Count * Marshal.SizeOf<Color>()),
                    elementSize: Marshal.SizeOf<Color>()
                );
            }

            fixed (LineVertex* lineVertsPtr = _lineVertices.AsSpan())
            {
                UpdateBufferIfNecessary(
                    context.GraphicsDevice, context.CommandList, buffer: ref _lineVertexBuffer,
                    dataPtr: new DataPointer(lineVertsPtr, _lineVertices.Count * Marshal.SizeOf<LineVertex>()),
                    elementSize: Marshal.SizeOf<LineVertex>()
                );
            }
        }
    }

    public override void Prepare(RenderDrawContext context)
    {
        //_transforms.Resize(_instances.Count, true);
        //_colors.Resize(_instances.Count, true);

        var difference = _instances.Count - _transforms.Count;

        for(int i = 0; i < difference; i++)
        {
            _transforms.Add(new Matrix());
            _colors.Add(new Color());
        }

        if (_instances.Count > 0)
        {

            Dispatcher.For(0, _transforms.Count, (i) =>
            {
                var instance = _instances[i];
                Matrix.Transformation(ref instance.Scale, ref instance.Rotation, ref instance.Position, out var transform);
                _transforms[i] = transform;
                _colors[i] = instance.Color;
                _instances[i] = instance;
            });
        }

        CheckBuffers(context);
        _lineVertices.Clear();
        _instances.Clear();
    }

    private void SetPrimitiveRenderingPipelineState(CommandList commandList, bool depthTest, FillMode selectedFillMode, bool isDoubleSided = false, bool hasTransparency = false)
    {
        _pipelineState.State.SetDefaults();
        _pipelineState.State.PrimitiveType = PrimitiveType.TriangleList;
        _pipelineState.State.RootSignature = _primitiveEffect.RootSignature;
        _pipelineState.State.EffectBytecode = _primitiveEffect.Effect.Bytecode;
        _pipelineState.State.DepthStencilState = depthTest ? hasTransparency ? DepthStencilStates.DepthRead : DepthStencilStates.Default : DepthStencilStates.None;
        _pipelineState.State.RasterizerState.FillMode = selectedFillMode;
        _pipelineState.State.RasterizerState.CullMode = selectedFillMode == FillMode.Solid && !isDoubleSided ? CullMode.Back : CullMode.None;
        _pipelineState.State.BlendState = hasTransparency ? BlendStates.NonPremultiplied : BlendStates.Opaque;
        _pipelineState.State.Output.CaptureState(commandList);
        _pipelineState.State.InputElements = _inputElements;
        _pipelineState.Update();
    }

    private void SetLineRenderingPipelineState(CommandList commandList, bool depthTest, bool hasTransparency = false)
    {
        _pipelineState.State.SetDefaults();
        _pipelineState.State.PrimitiveType = PrimitiveType.LineList;
        _pipelineState.State.RootSignature = _lineEffect.RootSignature;
        _pipelineState.State.EffectBytecode = _lineEffect.Effect.Bytecode;
        _pipelineState.State.DepthStencilState = depthTest ? hasTransparency ? DepthStencilStates.DepthRead : DepthStencilStates.Default : DepthStencilStates.None;
        _pipelineState.State.RasterizerState.FillMode = FillMode.Solid;
        _pipelineState.State.RasterizerState.CullMode = CullMode.None;
        _pipelineState.State.BlendState = hasTransparency ? BlendStates.NonPremultiplied : BlendStates.Opaque;
        _pipelineState.State.Output.CaptureState(commandList);
        _pipelineState.State.InputElements = _lineInputElements;
        _pipelineState.Update();
    }

    private void RenderPrimitives(RenderDrawContext context, RenderView renderView, ref Primitives offsets, ref Primitives counts, bool depthTest, FillMode fillMode, bool hasTransparency)
    {
        var commandList = context.CommandList;

        // set buffers and our current pipeline state
        commandList.SetVertexBuffer(0, _vertexBuffer, 0, VertexPositionTexture.Layout.VertexStride);
        commandList.SetIndexBuffer(_indexBuffer, 0, is32bits: true);
        commandList.SetPipelineState(_pipelineState.CurrentState);

        // we set line width to something absurdly high to avoid having to alter our shader substantially for now
        _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.LineWidthMultiplier, fillMode == FillMode.Solid ? 10000.0f : 1.0f);
        _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.ViewProjection, renderView.ViewProjection);
        _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.Transforms, _transformBuffer);
        _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.Colors, _colorBuffer);

        _primitiveEffect.UpdateEffect(context.GraphicsDevice);
        _primitiveEffect.Apply(context.GraphicsContext);

        // draw spheres
        if (counts.Spheres > 0)
        {
            SetPrimitiveRenderingPipelineState(commandList, depthTest, fillMode, isDoubleSided: false, hasTransparency: hasTransparency);
            commandList.SetPipelineState(_pipelineState.CurrentState);

            _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.Spheres);
            _primitiveEffect.Apply(context.GraphicsContext);

            commandList.DrawIndexedInstanced(_sphere.Indices.Length, counts.Spheres, _primitiveIndexOffsets.Spheres, _primitiveVertexOffsets.Spheres);
        }

        if (counts.Quads > 0 || counts.Circles > 0 || counts.HalfSpheres > 0)
        {
            SetPrimitiveRenderingPipelineState(commandList, depthTest, fillMode, isDoubleSided: true, hasTransparency: hasTransparency);
            commandList.SetPipelineState(_pipelineState.CurrentState);

            // draw quads
            if (counts.Quads > 0)
            {

                _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.Quads);
                _primitiveEffect.Apply(context.GraphicsContext);

                commandList.DrawIndexedInstanced(_plane.Indices.Length, counts.Quads, _primitiveIndexOffsets.Quads, _primitiveVertexOffsets.Quads);

            }

            // draw circles
            if (counts.Circles > 0)
            {

                _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.Circles);
                _primitiveEffect.Apply(context.GraphicsContext);

                commandList.DrawIndexedInstanced(_circle.Indices.Length, counts.Circles, _primitiveIndexOffsets.Circles, _primitiveVertexOffsets.Circles);

            }

            // draw half spheres
            if (counts.HalfSpheres > 0)
            {

                _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.HalfSpheres);
                _primitiveEffect.Apply(context.GraphicsContext);

                // HACK: we sort of abuse knowledge of the mesh primitive here.. :P
                commandList.DrawIndexedInstanced(_sphere.Indices.Length / 2, counts.HalfSpheres, _primitiveIndexOffsets.HalfSpheres, _primitiveVertexOffsets.HalfSpheres);

            }
        }

        if (counts.Cubes > 0 || counts.Capsules > 0 || counts.Cylinders > 0 || counts.Cones > 0)
        {
            SetPrimitiveRenderingPipelineState(commandList, depthTest, fillMode, isDoubleSided: false, hasTransparency: hasTransparency);
            commandList.SetPipelineState(_pipelineState.CurrentState);

            // draw cubes
            if (counts.Cubes > 0)
            {

                _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.Cubes);
                _primitiveEffect.Apply(context.GraphicsContext);

                commandList.DrawIndexedInstanced(_cube.Indices.Length, counts.Cubes, _primitiveIndexOffsets.Cubes, _primitiveVertexOffsets.Cubes);

            }

            // draw capsules
            if (counts.Capsules > 0)
            {

                _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.Capsules);
                _primitiveEffect.Apply(context.GraphicsContext);

                commandList.DrawIndexedInstanced(_capsule.Indices.Length, counts.Capsules, _primitiveIndexOffsets.Capsules, _primitiveVertexOffsets.Capsules);

            }

            // draw cylinders
            if (counts.Cylinders > 0)
            {

                _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.Cylinders);
                _primitiveEffect.Apply(context.GraphicsContext);

                commandList.DrawIndexedInstanced(_cylinder.Indices.Length, counts.Cylinders, _primitiveIndexOffsets.Cylinders, _primitiveVertexOffsets.Cylinders);

            }

            // draw cones
            if (counts.Cones > 0)
            {

                _primitiveEffect.Parameters.Set(PrimitiveShaderKeys.InstanceOffset, offsets.Cones);
                _primitiveEffect.Apply(context.GraphicsContext);

                commandList.DrawIndexedInstanced(_cone.Indices.Length, counts.Cones, _primitiveIndexOffsets.Cones, _primitiveVertexOffsets.Cones);

            }
        }

        // draw lines
        if (counts.Lines > 0)
        {

            SetLineRenderingPipelineState(commandList, depthTest, hasTransparency);
            commandList.SetVertexBuffer(0, _lineVertexBuffer, 0, LineVertex.Layout.VertexStride);
            commandList.SetPipelineState(_pipelineState.CurrentState);

            _lineEffect.Parameters.Set(LinePrimitiveShaderKeys.ViewProjection, renderView.ViewProjection);
            _lineEffect.UpdateEffect(context.GraphicsDevice);
            _lineEffect.Apply(context.GraphicsContext);

            commandList.Draw(counts.Lines * 2, offsets.Lines);

        }
    }

    public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
    {
        var commandList = context.CommandList;

        for (int index = startIndex; index < endIndex; index++)
        {

            var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
            var debugObject = (ImmediateDebugRenderObject)GetRenderNode(renderNodeReference).RenderObject;
            bool objectHasTransparency = debugObject.Stage == DebugRenderStage.Transparent;

            // update pipeline state, render with depth test first
            SetPrimitiveRenderingPipelineState(commandList, depthTest: true, selectedFillMode: debugObject.CurrentFillMode, hasTransparency: objectHasTransparency);
            RenderPrimitives(context, renderView, ref debugObject.instanceOffsets, ref debugObject.primitivesToDraw, depthTest: true, fillMode: debugObject.CurrentFillMode, hasTransparency: objectHasTransparency);

            // render without depth test second
            SetPrimitiveRenderingPipelineState(commandList, depthTest: false, selectedFillMode: debugObject.CurrentFillMode, hasTransparency: objectHasTransparency);
            RenderPrimitives(context, renderView, offsets: ref debugObject.instanceOffsetsNoDepth, counts: ref debugObject.primitivesToDrawNoDepth, depthTest: false, fillMode: debugObject.CurrentFillMode, hasTransparency: objectHasTransparency);

        }
    }

    public override void Flush(RenderDrawContext context)
    {
    }

    /* FIXME: is there a nicer way to handle dispose, some xenko idiom? */

    public override void Unload()
    {
        base.Unload();
        _transformBuffer.Dispose();
        _colorBuffer.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _lineVertexBuffer.Dispose();
    }
}