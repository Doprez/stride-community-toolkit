using Stride.CommunityToolkit.Scripts;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Stride.CommunityToolkit.Bullet;

/// <summary>
/// Provides extension methods for the <see cref="Simulation"/> class to perform raycasting operations in a game simulation fro the Bullet physics.
/// </summary>
public static class SimulationExtensions
{
    /// <summary>
    /// Performs a raycast from the given entity's position in the direction the entity is facing, with the specified length.
    /// </summary>
    /// <param name="simulation">The <see cref="Simulation"/> instance in which the raycast is performed.</param>
    /// <param name="entityPosition">The <see cref="Entity"/> from which the ray starts. The ray is cast from the entity's current world position and direction.</param>
    /// <param name="length">The length of the ray, which determines how far it should extend from the entity. Defaults to 1.</param>
    /// <param name="collisionFlags">Specifies which collision groups to include in the raycast. Defaults to <see cref="CollisionFilterGroupFlags.AllFilter"/>.</param>
    /// <param name="eFlags">Additional raycasting flags for fine-tuning the behavior. Defaults to <see cref="EFlags.None"/>.</param>
    /// <returns>A <see cref="HitResult"/> that contains information about the first object hit by the ray, or an empty result if nothing is hit.</returns>
    /// <remarks>
    /// Ensure that you are using the actual rotating entity, as debugging with the wrong entity can lead to unexpected results.
    /// </remarks>
    public static HitResult Raycast(this Simulation simulation, Entity entityPosition, float length = 1, CollisionFilterGroupFlags collisionFlags = CollisionFilterGroupFlags.AllFilter, EFlags eFlags = EFlags.None)
    {
        var raycastStart = entityPosition.Transform.WorldMatrix.TranslationVector;
        var forward = entityPosition.Transform.WorldMatrix.Forward;
        var raycastEnd = raycastStart + forward * length;

        var result = simulation.Raycast(raycastStart, raycastEnd, filterFlags: collisionFlags, eFlags: eFlags);

        return result;
    }

    /// <summary>
    /// Performs a raycast from the given entity's position in the specified direction, with the specified length.
    /// </summary>
    /// <param name="simulation">The <see cref="Simulation"/> instance in which the raycast is performed.</param>
    /// <param name="entityPosition">The <see cref="Entity"/> from which the ray starts. The ray is cast from the entity's current world position.</param>
    /// <param name="direction">The direction in which the ray is cast.</param>
    /// <param name="length">The length of the ray, which determines how far it should extend from the entity. Defaults to 1.</param>
    /// <param name="collisionFlags">Specifies which collision groups to include in the raycast. Defaults to <see cref="CollisionFilterGroupFlags.AllFilter"/>.</param>
    /// <param name="eFlags">Additional raycasting flags for fine-tuning the behavior. Defaults to <see cref="EFlags.None"/>.</param>
    /// <returns>A <see cref="HitResult"/> that contains information about the first object hit by the ray, or an empty result if nothing is hit.</returns>
    public static HitResult Raycast(this Simulation simulation, Entity entityPosition, Vector3 direction, float length = 1, CollisionFilterGroupFlags collisionFlags = CollisionFilterGroupFlags.AllFilter, EFlags eFlags = EFlags.None)
    {
        var raycastStart = entityPosition.Transform.WorldMatrix.TranslationVector;
        var raycastEnd = raycastStart + direction * length;

        var result = simulation.Raycast(raycastStart, raycastEnd, filterFlags: collisionFlags, eFlags: eFlags);

        return result;
    }

    /// <summary>
    /// Raycasts and stops at the first hit.
    /// </summary>
    /// <param name="simulation">Physics simulation.</param>
    /// <param name="raySegment">Ray.</param>
    /// <returns>The hit results.</returns>
    /// <exception cref="ArgumentNullException">If the simulation argument is null.</exception>
    public static HitResult Raycast(this Simulation simulation, RaySegment raySegment)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        return simulation.Raycast(raySegment.Start, raySegment.End);
    }

    /// <summary>
    /// Raycasts and stops at the first hit.
    /// </summary>
    /// <param name="simulation">Physics simulation.</param>
    /// <param name="raySegment">Ray.</param>
    /// <param name="collisionFilterGroups">The collision group of this shape sweep</param>
    /// <param name="collisionFilterGroupFlags">The collision group that this shape sweep can collide with</param>
    /// <returns>The hit results.</returns>
    /// <exception cref="ArgumentNullException">If the simulation argument is null.</exception>
    public static HitResult Raycast(this Simulation simulation, RaySegment raySegment, CollisionFilterGroups collisionFilterGroups, CollisionFilterGroupFlags collisionFilterGroupFlags)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        return simulation.Raycast(raySegment.Start, raySegment.End, collisionFilterGroups, collisionFilterGroupFlags);
    }

    /// <summary>
    /// Raycasts penetrating any shape the ray encounters.
    /// </summary>
    /// <param name="simulation">Physics simulation.</param>
    /// <param name="raySegment">Ray.</param>
    /// <param name="resultsOutput">The list to fill with results.</param>
    /// <exception cref="ArgumentNullException">If the simulation argument is null.</exception>
    public static void RaycastPenetrating(this Simulation simulation, RaySegment raySegment, IList<HitResult> resultsOutput)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        simulation.RaycastPenetrating(raySegment.Start, raySegment.End, resultsOutput);
    }

    /// <summary>
    /// Raycasts penetrating any shape the ray encounters.
    /// </summary>
    /// <param name="simulation">Physics simulation.</param>
    /// <param name="raySegment">Ray.</param>
    /// <returns>The list with hit results.</returns>
    /// <exception cref="ArgumentNullException">If the simulation argument is null.</exception>
    public static List<HitResult> RaycastPenetrating(this Simulation simulation, RaySegment raySegment)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        var resultsOutput = new List<HitResult>();

        simulation.RaycastPenetrating(raySegment.Start, raySegment.End, resultsOutput);

        return resultsOutput;
    }

    /// <summary>
    /// Raycasts penetrating any shape the ray encounters.
    /// Filtering by CollisionGroup
    /// </summary>
    /// <param name="simulation">Physics simulation.</param>
    /// <param name="raySegment">Ray.</param>
    /// <param name="resultsOutput">The list to fill with results.</param>
    /// <param name="collisionFilterGroups">The collision group of this shape sweep</param>
    /// <param name="collisionFilterGroupFlags">The collision group that this shape sweep can collide with</param>
    /// <exception cref="ArgumentNullException">If the simulation argument is null.</exception>
    public static void RaycastPenetrating(this Simulation simulation, RaySegment raySegment, IList<HitResult> resultsOutput, CollisionFilterGroups collisionFilterGroups, CollisionFilterGroupFlags collisionFilterGroupFlags)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        simulation.RaycastPenetrating(raySegment.Start, raySegment.End, resultsOutput, collisionFilterGroups, collisionFilterGroupFlags);
    }

    /// <summary>
    /// Raycasts penetrating any shape the ray encounters.
    /// </summary>
    /// <param name="simulation">Physics simulation.</param>
    /// <param name="raySegment">Ray.</param>
    /// <param name="collisionFilterGroups">The collision group of this shape sweep</param>
    /// <param name="collisionFilterGroupFlags">The collision group that this shape sweep can collide with</param>
    /// <returns>The list with hit results.</returns>
    /// <exception cref="ArgumentNullException">If the simulation argument is null.</exception>
    public static List<HitResult> RaycastPenetrating(this Simulation simulation, RaySegment raySegment, CollisionFilterGroups collisionFilterGroups, CollisionFilterGroupFlags collisionFilterGroupFlags)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        var result = new List<HitResult>();

        simulation.RaycastPenetrating(raySegment.Start, raySegment.End, result, collisionFilterGroups, collisionFilterGroupFlags);

        return result;
    }
}