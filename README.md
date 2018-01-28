C# Job System Cookbook
=======================

This is a repo of examples i've written to learn how to use the C# job system to write systems at scale, here for reference and sharing.  Each example script has a corresponding scene where it's set up.



## Examples

Note: examples in this repo use `LateUpdate()` as an easy way to handle completing jobs later than we schedule them, but in real code you might want to schedule the jobs early in `Update` so you can use the result later in the same `Update`. 

### [Accelerate 10000 Cubes](Assets/Scripts/AccelerationParallelFor.cs)

Demonstrates a simple 2-job dependency setup.

### [Point & Bounds Intersection Checks](Assets/Scripts/CheckBoundsParallelFor.cs)

Demonstrates checking a `Vector3` and a `Bounds` for intersection against a list of 10000 `Bounds`.

### [Point Cloud Generation & Processing](Assets/Scripts/PointCloudProcessing.cs)

Generates a cloud of 10000 points, then calculates magnitudes & normalizes the points.
