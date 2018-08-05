C# Job System Cookbook
=======================

This is a repo of examples I've written to learn how to use the C# job system to write systems at scale, here for reference and sharing.  

The goal of this repo is making it clearer how you can _structure_ your data, _schedule_ your jobs, and use the results. 
So, the examples use easy to understand problems & algorithms. 

This repo does not cover using C# jobs with the Entity Component System. Please see the [official example repo](https://github.com/Unity-Technologies/EntityComponentSystemSamples) for more on that.

Each example script has a corresponding scene where it's set up.

## Job System Details

You can use the job system in Unity [2018.x](https://unity3d.com/get-unity/update) right now. 
I recommend 2018.2 for these examples now.

For a detailed look into how the C# job system works, please watch the [Unite Austin presentation](https://www.youtube.com/watch?v=AXUvnk7Jws4) if you haven't seen it.  There is also a [Q & A from Unite Berlin in 2018](https://www.youtube.com/watch?v=swCpyJy4FEs)

## Examples

Note: examples in this repo use `LateUpdate()` as an easy way to handle completing jobs later than we schedule them, but in real code you might want to schedule the jobs early in `Update` (using Script Execution Order maybe) so you can use the result later in the same frame.

All examples demonstrate the use of persistently-allocated job memory.

# [Realtime Image Processing (with Burst compilation)](Assets/Scripts/WebcamProcessing.cs)

Process input from a webcam in real time using Burst-compiled jobs.

the job details are all in [this file](/Assets/Scripts/ImageProcessing/BurstRGBJobs.cs), and the above file is the main script.

This demo implements 5 different effects , all based around operating on a pixel only if it's color channel value is over some threshold

To change the color thresholds, select the `WebcamDisplay` in the heirarchy of the example scene & check out the `Webcam Processing` component.  You can also change the scanline effect as well as select a webcam resolution that works for you there.

### [Change Mesh Vertices & Normals Every Frame](Assets/Scripts/MeshComplexParallel.cs)

Modify all vertices & normals of a mesh in parallel every frame.

This is the *most visually interesting example*.  Uses a more complex single job.

### [Change Mesh Vertices Every Frame](Assets/Scripts/MeshVerticesParallelUpdate.cs)

Modify all 20678 vertices of a mesh in parallel every frame, using Perlin noise & sin(time).

Uses a single job.

### [Accelerate 10000 Cubes](Assets/Scripts/AccelerationParallelFor.cs)

First determine velocities, then change positions based on those velocities.

Demonstrates using the TransformAccessArray, necessary for doing transform operations in jobs.    

### [Point & Bounds Intersection Checks](Assets/Scripts/CheckBoundsParallelFor.cs)

Check a `Vector3` and a `Bounds` for intersection against a list of 10000 `Bounds`.

Demonstrates running 2 independent jobs. 

### [Ray / Bounds Intersection Checks](Assets/Scripts/RayBoundsIntersection.cs)

Check a `Ray` for intersection with a large `Bounds` array in two steps.

Demonstrates reducing an array of checks to a smaller list, and using temporarily-allocated job memory. 

### [Point Cloud Generation & Processing](Assets/Scripts/PointCloudProcessing.cs)

Generates a cloud of 10000 points, then calculates magnitudes & normalizes the points.


## Further Examples

Keijiro Takahashi has a [great example of using the job system with ECS](https://github.com/keijiro/Voxelman)



