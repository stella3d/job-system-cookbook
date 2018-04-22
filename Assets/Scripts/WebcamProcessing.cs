using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public enum ExampleEffect
{
    LeftShiftThreshold,
    RightShiftThreshold,
    ComplementThreshold,
    ExclusiveOrThreshold,
    ExclusiveOrSelf,
}

public class WebcamProcessing : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Which webcam to use")]
    int m_WebcamIndex;

    WebCamDevice m_CamDevice;
    WebCamTexture m_CamTexture;

    [SerializeField]
    [Tooltip("the color that sets our threshold values above which we effect a given pixel")]
    Color32 m_ColorThreshold;

    [SerializeField]
    ExampleEffect effect = ExampleEffect.LeftShiftThreshold;

    [Range(1, 4)]
    [SerializeField]
    [Tooltip("the interval of horizontal lines to skip - 1 means process all lines, 2 & 4 create a scanline effect")]
    int lineSkip = 2;

    [SerializeField]
    [Tooltip("select a resolution compatible with your webcam.  you will need to move the camera if you change this")]
    Vector2Int m_WebcamTextureSize = new Vector2Int(1024, 576);

    [SerializeField]
    [Tooltip("The texture we will copy our process data into")]
    Texture2D m_Texture;

    JobHandle m_RGBComplementBurstJobHandle;

    NativeArray<Color32> m_NativeColors;

    NativeSlice<byte> m_NativeRed;
    NativeSlice<byte> m_NativeGreen;
    NativeSlice<byte> m_NativeBlue;

    Color32[] m_Data;

    void OnEnable()
    {
        m_Data = new Color32[m_WebcamTextureSize.x * m_WebcamTextureSize.y];
        m_NativeColors = new NativeArray<Color32>(m_Data, Allocator.Persistent);

        var slice = new NativeSlice<Color32>(m_NativeColors);
        m_NativeRed = slice.SliceWithStride<byte>(0);
        m_NativeGreen = slice.SliceWithStride<byte>(1);
        m_NativeBlue = slice.SliceWithStride<byte>(2);

        if (m_WebcamIndex >= WebCamTexture.devices.Length)
            m_WebcamIndex = WebCamTexture.devices.Length - 1;

        m_CamDevice = WebCamTexture.devices[m_WebcamIndex];
        m_CamTexture = new WebCamTexture(m_CamDevice.name, m_WebcamTextureSize.x, m_WebcamTextureSize.y);
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = m_CamTexture;

        m_CamTexture.Play();
    }

    void OnDisable()
    {
        m_NativeColors.Dispose();
    }

    void Update()
    {
        // load is one half of our big bottleneck with this method - copying data
        m_CamTexture.GetPixels32(m_Data);
        m_NativeColors.CopyFrom(m_Data);

        // lineskip can only be 1, 2, or 4 - past that the effect doesn't cover the screen
        if (lineSkip > 4)
            lineSkip = 4;
        else if (lineSkip == 3)
            lineSkip = 2;
        else if (lineSkip == 0)
            lineSkip = 1;

        switch (effect)
        {
            case ExampleEffect.ExclusiveOrSelf:
                if (lineSkip == 1)
                    ExclusiveOrSelfProcessWithoutLineSkip(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                else
                    SelfExclusiveOrProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
            case ExampleEffect.ExclusiveOrThreshold:
                if (lineSkip == 1)
                    ExclusiveOrProcessWithoutLineSkip(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                else
                    BurstExclusiveOrProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
            case ExampleEffect.LeftShiftThreshold:
                if (lineSkip == 1)
                    LeftShiftProcessWithoutLineSkip(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                else
                    BurstLeftShiftProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
            case ExampleEffect.RightShiftThreshold:
                if(lineSkip == 1)
                    RightShiftProcessWithoutLineSkip(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                else
                    BurstRightShiftProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
            case ExampleEffect.ComplementThreshold:
                if (lineSkip == 1)
                    ComplementWithoutLineSkip(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                else
                    BurstComplementProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
        }
    }

    private void LateUpdate()
    {
        m_RGBComplementBurstJobHandle.Complete();

        m_NativeColors.CopyTo(m_Data);

        m_Texture.SetPixels32(0, 0, m_WebcamTextureSize.x, m_WebcamTextureSize.y, m_Data);
        m_Texture.Apply(false);
    }

    void BurstComplementProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new SelfComplementWithSkipJob()
        {
            data = r,
            threshold = m_ColorThreshold.r,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var greenJob = new SelfComplementWithSkipJob()
        {
            data = g,
            threshold = m_ColorThreshold.g,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var blueJob = new SelfComplementWithSkipJob()
        {
            data = b,
            threshold = m_ColorThreshold.b,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 256);
        var gHandle = greenJob.Schedule(length, 256, rHandle);
        handle = blueJob.Schedule(length, 256, gHandle);
    }

    void BurstLeftShiftProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new SelfLeftShiftBurstJob()
        {
            data = r,
            threshold = m_ColorThreshold.r,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var greenJob = new SelfLeftShiftBurstJob()
        {
            data = g,
            threshold = m_ColorThreshold.g,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var blueJob = new SelfLeftShiftBurstJob()
        {
            data = b,
            threshold = m_ColorThreshold.b,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 256);
        var gHandle = greenJob.Schedule(length, 256, rHandle);
        handle = blueJob.Schedule(length, 256, gHandle);
    }

    void BurstRightShiftProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new ThresholdRightShiftBurstJob()
        {
            data = r,
            threshold = m_ColorThreshold.r,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y
        };

        var greenJob = new ThresholdRightShiftBurstJob()
        {
            data = g,
            threshold = m_ColorThreshold.g,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var blueJob = new ThresholdRightShiftBurstJob()
        {
            data = b,
            threshold = m_ColorThreshold.b,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 256);
        var gHandle = greenJob.Schedule(length, 256, rHandle);
        handle = blueJob.Schedule(length, 256, gHandle);
    }

    void BurstExclusiveOrProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new ThresholdExclusiveOrBurstJob()
        {
            data = r,
            threshold = m_ColorThreshold.r,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var greenJob = new ThresholdExclusiveOrBurstJob()
        {
            data = g,
            threshold = m_ColorThreshold.g,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var blueJob = new ThresholdExclusiveOrBurstJob()
        {
            data = b,
            threshold = m_ColorThreshold.b,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 1024);
        var gHandle = greenJob.Schedule(length, 1024, rHandle);
        handle = blueJob.Schedule(length, 1024, gHandle);
    }

    void SelfExclusiveOrProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new SelfExclusiveOrBurstJob()
        {
            data = r,
            threshold = m_ColorThreshold.r,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var greenJob = new SelfExclusiveOrBurstJob()
        {
            data = g,
            threshold = m_ColorThreshold.g,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var blueJob = new SelfExclusiveOrBurstJob()
        {
            data = b,
            threshold = m_ColorThreshold.b,
            widthOverLineSkip = m_WebcamTextureSize.x / lineSkip,
            height = m_WebcamTextureSize.y,
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 1024);
        var gHandle = greenJob.Schedule(length, 1024, rHandle);
        handle = blueJob.Schedule(length, 1024, gHandle);
    }

    void ExclusiveOrProcessWithoutLineSkip(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new ThresholdExclusiveOrNoSkipJob()
        {
            data = r,
            threshold = m_ColorThreshold.r
        };

        var greenJob = new ThresholdExclusiveOrNoSkipJob()
        {
            data = g,
            threshold = m_ColorThreshold.g
        };

        var blueJob = new ThresholdExclusiveOrNoSkipJob()
        {
            data = b,
            threshold = m_ColorThreshold.b
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 512);
        var gHandle = greenJob.Schedule(length, 512, rHandle);
        handle = blueJob.Schedule(length, 512, gHandle);
    }

    void ExclusiveOrSelfProcessWithoutLineSkip(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new SelfExclusiveOrNoSkipJob()
        {
            data = r,
            threshold = m_ColorThreshold.r
        };

        var greenJob = new SelfExclusiveOrNoSkipJob()
        {
            data = g,
            threshold = m_ColorThreshold.g
        };

        var blueJob = new SelfExclusiveOrNoSkipJob()
        {
            data = b,
            threshold = m_ColorThreshold.b
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 512);
        var gHandle = greenJob.Schedule(length, 512, rHandle);
        handle = blueJob.Schedule(length, 512, gHandle);
    }

    void RightShiftProcessWithoutLineSkip(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new RightShiftNoSkipJob()
        {
            data = r,
            threshold = m_ColorThreshold.r
        };

        var greenJob = new RightShiftNoSkipJob()
        {
            data = g,
            threshold = m_ColorThreshold.g
        };

        var blueJob = new RightShiftNoSkipJob()
        {
            data = b,
            threshold = m_ColorThreshold.b
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 512);
        var gHandle = greenJob.Schedule(length, 512, rHandle);
        handle = blueJob.Schedule(length, 512, gHandle);
    }

    void LeftShiftProcessWithoutLineSkip(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new LeftShiftNoSkipJob()
        {
            data = r,
            threshold = m_ColorThreshold.r
        };

        var greenJob = new LeftShiftNoSkipJob()
        {
            data = g,
            threshold = m_ColorThreshold.g
        };

        var blueJob = new LeftShiftNoSkipJob()
        {
            data = b,
            threshold = m_ColorThreshold.b
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 512);
        var gHandle = greenJob.Schedule(length, 512, rHandle);
        handle = blueJob.Schedule(length, 512, gHandle);
    }

    void ComplementWithoutLineSkip(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new SelfComplementNoSkipJob()
        {
            data = r,
            threshold = m_ColorThreshold.r
        };

        var greenJob = new SelfComplementNoSkipJob()
        {
            data = g,
            threshold = m_ColorThreshold.g
        };

        var blueJob = new SelfComplementNoSkipJob()
        {
            data = b,
            threshold = m_ColorThreshold.b
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 512);
        var gHandle = greenJob.Schedule(length, 512, rHandle);
        handle = blueJob.Schedule(length, 512, gHandle);
    }
}