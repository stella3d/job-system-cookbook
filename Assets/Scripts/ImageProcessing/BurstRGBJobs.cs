using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;


[ComputeJobOptimization]
public struct ThresholdComplementBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], ~data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct ThresholdLeftShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], data[i] << data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct ThresholdRightShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], data[i] >> data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct ThresholdExclusiveOrBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], data[i] ^ threshold, overThreshold && operateOnThisPixel);
    }
}

