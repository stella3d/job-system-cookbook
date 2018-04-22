using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;


[ComputeJobOptimization]
public struct RedThresholdComplementBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte redThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > redThreshold;
        data[i] = (byte)math.select(data[i], ~data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct GreenThresholdComplementBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte greenThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > greenThreshold;
        data[i] = (byte)math.select(data[i], ~data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct BlueThresholdComplementBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte blueThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > blueThreshold;
        data[i] = (byte)math.select(data[i], ~data[i], overThreshold && operateOnThisPixel);
    }
}


[ComputeJobOptimization]
public struct RedThresholdLeftShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte redThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > redThreshold;
        data[i] = (byte)math.select(data[i], data[i] << data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct GreenThresholdLeftShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte greenThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > greenThreshold;
        data[i] = (byte)math.select(data[i], data[i] << data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct BlueThresholdLeftShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte blueThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > blueThreshold;
        data[i] = (byte)math.select(data[i], data[i] << data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct RedThresholdRightShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte redThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > redThreshold;
        data[i] = (byte)math.select(data[i], data[i] >> data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct GreenThresholdRightShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte greenThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > greenThreshold;
        data[i] = (byte)math.select(data[i], data[i] >> data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct BlueThresholdRightShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte blueThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > blueThreshold;
        data[i] = (byte)math.select(data[i], data[i] >> data[i], overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct RedThresholdExclusiveOrBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte redThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > redThreshold;
        data[i] = (byte)math.select(data[i], data[i] ^ redThreshold, overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct GreenThresholdExclusiveOrBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte greenThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > greenThreshold;
        data[i] = (byte)math.select(data[i], data[i] ^ greenThreshold, overThreshold && operateOnThisPixel);
    }
}

[ComputeJobOptimization]
public struct BlueThresholdExclusiveOrBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte blueThreshold;
    public int height;
    public int width;
    public int lineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < width / lineSkip;
        bool overThreshold = data[i] > blueThreshold;
        data[i] = (byte)math.select(data[i], data[i] ^ blueThreshold, overThreshold && operateOnThisPixel);
    }
}
