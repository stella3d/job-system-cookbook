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
}

public class WebcamProcessing : MonoBehaviour
{
    [SerializeField]
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

        m_CamDevice = WebCamTexture.devices[1];
        m_CamTexture = new WebCamTexture(m_CamDevice.name, m_WebcamTextureSize.x, m_WebcamTextureSize.y);
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = m_CamTexture;

        m_CamTexture.Play();
    }

    void OnDisable()
    {
        m_NativeColors.Dispose();
    }

    void Update ()
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
            case ExampleEffect.ExclusiveOrThreshold:
                BurstExclusiveOrProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
            case ExampleEffect.LeftShiftThreshold:
                BurstLeftShiftProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
            case ExampleEffect.RightShiftThreshold:
                BurstRightShiftProcessing(m_NativeRed, m_NativeGreen, m_NativeBlue, ref m_RGBComplementBurstJobHandle);
                break;
            case ExampleEffect.ComplementThreshold:
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
        var redJob = new RedThresholdComplementBurstJob()
        {
            data = r,
            redThreshold = m_ColorThreshold.r,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var greenJob = new GreenThresholdComplementBurstJob()
        {
            data = g,
            greenThreshold = m_ColorThreshold.g,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var blueJob = new BlueThresholdComplementBurstJob()
        {
            data = b,
            blueThreshold = m_ColorThreshold.b,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 128);
        var gHandle = greenJob.Schedule(length, 128, rHandle);
        handle = blueJob.Schedule(length, 128, gHandle);
    }

    void BurstLeftShiftProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new RedThresholdLeftShiftBurstJob()
        {
            data = r,
            redThreshold = m_ColorThreshold.r,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var greenJob = new GreenThresholdLeftShiftBurstJob()
        {
            data = g,
            greenThreshold = m_ColorThreshold.g,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var blueJob = new BlueThresholdLeftShiftBurstJob()
        {
            data = b,
            blueThreshold = m_ColorThreshold.b,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 128);
        var gHandle = greenJob.Schedule(length, 128, rHandle);
        handle = blueJob.Schedule(length, 128, gHandle);
    }

    void BurstRightShiftProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new RedThresholdRightShiftBurstJob()
        {
            data = r,
            redThreshold = m_ColorThreshold.r,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var greenJob = new GreenThresholdRightShiftBurstJob()
        {
            data = g,
            greenThreshold = m_ColorThreshold.g,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var blueJob = new BlueThresholdRightShiftBurstJob()
        {
            data = b,
            blueThreshold = m_ColorThreshold.b,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 128);
        var gHandle = greenJob.Schedule(length, 128, rHandle);
        handle = blueJob.Schedule(length, 128, gHandle);
    }

    void BurstExclusiveOrProcessing(NativeSlice<byte> r, NativeSlice<byte> g, NativeSlice<byte> b, ref JobHandle handle)
    {
        var redJob = new RedThresholdExclusiveOrBurstJob()
        {
            data = r,
            redThreshold = m_ColorThreshold.r,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var greenJob = new GreenThresholdExclusiveOrBurstJob()
        {
            data = g,
            greenThreshold = m_ColorThreshold.g,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var blueJob = new BlueThresholdExclusiveOrBurstJob()
        {
            data = b,
            blueThreshold = m_ColorThreshold.b,
            width = m_WebcamTextureSize.x,
            height = m_WebcamTextureSize.y,
            lineSkip = lineSkip
        };

        var length = m_NativeRed.Length;
        var rHandle = redJob.Schedule(length, 128);
        var gHandle = greenJob.Schedule(length, 128, rHandle);
        handle = blueJob.Schedule(length, 128, gHandle);
    }

}