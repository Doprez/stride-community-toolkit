using Silk.NET.OpenXR;
using Stride.Assets.Media;
using Stride.CommunityToolkit.Bullet;
using Stride.CommunityToolkit.Engine;
using Stride.CommunityToolkit.Games;
using Stride.CommunityToolkit.Skyboxes;
using Stride.Core.Assets;
using Stride.Core.Assets.Compiler;
using Stride.Core.Diagnostics;
using Stride.Core.IO;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Sprites;
using Stride.Video;

using var game = new Game();

game.Run(start: Start);

void Start(Scene scene)
{
    // Setup the base 3D scene with default lighting, camera, etc.
    game.SetupBase3DScene();

    // Add debugging aids: entity names, positions
    game.AddEntityDebugSceneRenderer(new()
    {
        ShowFontBackground = true
    });

    game.AddSkybox();
    game.AddProfiler();

    LoadTexture(scene);
}

void LoadTexture(Scene scene)
{
    using (StreamReader reader = new StreamReader("\\dot-NET-Core.png"))
    {
        var texture = Texture.Load(game.GraphicsDevice, reader.BaseStream);
        var sprite = new SpriteFromTexture();
        sprite.Texture = texture;
        var entity = new Entity("Sprite test")
        {
            new SpriteComponent { SpriteProvider = sprite }
        };

        entity.Transform.Position = new Vector3(0, 3, 0);

        scene.Entities.Add(entity);
    }
}