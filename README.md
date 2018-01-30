C# Job System Cookbook
=======================

This is a repo of examples I've written to learn how to use the C# job system to write systems at scale, here for reference and sharing.  

The goal of this repo is making it clearer how you can _structure_ your data, _schedule_ your jobs, and use the results. 
So, the examples use easy to understand problems & algorithms. 

This doesn't (yet!) cover things related to using C# jobs with the upcoming Entity Component System.

Each example script has a corresponding scene where it's set up.

## Job System Details

You can use the job system in the [2018.1 beta](https://unity3d.com/unity/beta) right now. 
Examples written here are written & tested on the latest beta version, starting with beta 4.

For a detailed look into how the C# job system works, please watch the [Unite Austin presentation](https://www.youtube.com/watch?v=AXUvnk7Jws4) if you haven't seen it.  

## Examples

Note: examples in this repo use `LateUpdate()` as an easy way to handle completing jobs later than we schedule them, but in real code you might want to schedule the jobs early in `Update` (using Script Execution Order maybe) so you can use the result later in the same frame.

10000 is the default number of items to have to iterate over to demonstrate how they perform at scale, but it's adjustable in the Inspector for each example.

All examples demonstrate the use of persistently-allocated job memory.

### [Change Mesh Vertices Every Frame](Assets/Scripts/MeshVerticesParallelUpdate.cs)

Modify all 20678 vertices of a mesh in parellel every frame, using Perlin noise & sin(time).

Uses a single job.

### [Change Mesh Vertices & Normals Every Frame](Assets/Scripts/MeshVerticesParallelUpdate.cs)

Modify all vertices & normals of a mesh in parellel every frame.

This is the *most visually interesting example*.  Uses a more complex single job.

### [Accelerate 10000 Cubes](Assets/Scripts/AccelerationParallelFor.cs)

First determine velocities, then change positions based on those velocities.

Demonstrates a simple 2-job dependency setup. 


### [Point & Bounds Intersection Checks](Assets/Scripts/CheckBoundsParallelFor.cs)

Check a `Vector3` and a `Bounds` for intersection against a list of 10000 `Bounds`.

Demonstrates running 2 independent jobs. 

### [Ray / Bounds Intersection Checks](Assets/Scripts/RayBoundsIntersection.cs)

Check a `Ray` for intersection with a large `Bounds` array in two steps.

Demonstrates reducing an array of checks to a smaller list, and using temporarily-allocated job memory. 

### [Point Cloud Generation & Processing](Assets/Scripts/PointCloudProcessing.cs)

Generates a cloud of 10000 points, then calculates magnitudes & normalizes the points.



