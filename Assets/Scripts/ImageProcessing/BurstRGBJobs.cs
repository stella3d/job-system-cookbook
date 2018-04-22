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
public struct ComplementNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], ~data[i], data[i] > threshold);
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
public struct LeftShiftNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] << data[i], data[i] > threshold);
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
public struct RightShiftNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] >> data[i], data[i] > threshold);
    }
}


[ComputeJobOptimization(Accuracy.Low, Support.Relaxed)]
public struct SelfExclusiveOrBurstJob : IJobParallelFor
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

[ComputeJobOptimization(Accuracy.Low, Support.Relaxed)]
public struct SelfExclusiveOrNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] ^ data[i], data[i] > threshold);
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

[ComputeJobOptimization]
public struct ThresholdExclusiveOrNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] ^ threshold, data[i] > threshold);
    }
}


