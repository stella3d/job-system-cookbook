using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;


[BurstCompile]
public struct SelfComplementWithSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int widthOverLineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < widthOverLineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], ~data[i], overThreshold && operateOnThisPixel);
    }
}

[BurstCompile]
public struct SelfComplementNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], ~data[i], data[i] > threshold);
    }
}


[BurstCompile]
public struct SelfLeftShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int widthOverLineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < widthOverLineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], data[i] << data[i], overThreshold && operateOnThisPixel);
    }
}

[BurstCompile]
public struct LeftShiftNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] << data[i], data[i] > threshold);
    }
}

[BurstCompile]
public struct ThresholdRightShiftBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int widthOverLineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < widthOverLineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], data[i] >> data[i], overThreshold && operateOnThisPixel);
    }
}

[BurstCompile]
public struct RightShiftNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] >> data[i], data[i] > threshold);
    }
}


[BurstCompile]
public struct SelfExclusiveOrBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int widthOverLineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < widthOverLineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], data[i] ^ threshold, overThreshold && operateOnThisPixel);
    }
}

[BurstCompile]
public struct SelfExclusiveOrNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] ^ data[i], data[i] > threshold);
    }
}

[BurstCompile]
public struct ThresholdExclusiveOrBurstJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;
    public int height;
    public int widthOverLineSkip;

    public void Execute(int i)
    {
        bool operateOnThisPixel = (i % height) < widthOverLineSkip;
        bool overThreshold = data[i] > threshold;
        data[i] = (byte)math.select(data[i], data[i] ^ threshold, overThreshold && operateOnThisPixel);
    }
}

[BurstCompile]
public struct ThresholdExclusiveOrNoSkipJob : IJobParallelFor
{
    public NativeSlice<byte> data;
    public byte threshold;

    public void Execute(int i)
    {
        data[i] = (byte)math.select(data[i], data[i] ^ threshold, data[i] > threshold);
    }
}


