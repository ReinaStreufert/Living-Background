using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using System.IO;

namespace desktopdraw
{
    class VideoLoopBackground : LivingBackground
    {
        public string VideoPath = "cool.wmv";

        private MediaEngineClassFactory mecf;
        private DXGIDeviceManager deviceMan;
        private MediaEngine mediaEngine;
        private MediaEngineEx mediaEngineEx;
        private FileStream videoFile;
        private ByteStream videoStream;
        private SharpDX.DXGI.Surface dxgiSurface;
        private SharpDX.Direct3D11.Texture2D texture;
        private SharpDX.Direct3D11.Device mediaDevice;
        private RawRectangle rr;
        private long ts;
        //private SharpDX.DXGI.Surface dxgiSurface;
        //private Bitmap1 frameBmp;

        //private SharpDX.DXGI.Surface surface;


        private SharpDX.Size2 videoDimensions;

        public override void Draw(DeviceContext dc, RenderTarget rt, Factory factory, RawVector2 mousePt)
        {
            //dc.Clear(new RawColor4(1F, 1F, 1F, 1F));
            if (mediaEngine.OnVideoStreamTick(out ts))
            {
                mediaEngineEx.TransferVideoFrame(dxgiSurface, null, rr, null);
            }
            //dc.DrawBitmap(frameBmp, 1F, InterpolationMode.Linear);
        }

        public override void AfterDraw()
        {
        }

        public override void Initialize(DeviceContext dc, RenderTarget rt, Factory factory, SharpDX.DXGI.Surface surface, SharpDX.Direct3D11.Device d3dDevice, SharpDX.DXGI.SwapChain1 sw)
        {
            Console.WriteLine("init");
            RTDraw = false;
            dxgiSurface = surface;
            MediaManager.Startup();

            mediaDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.VideoSupport | SharpDX.Direct3D11.DeviceCreationFlags.Debug | SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport);

            mecf = new MediaEngineClassFactory();
            deviceMan = new DXGIDeviceManager();
            deviceMan.ResetDevice(mediaDevice);
            MediaEngineAttributes attributes = new MediaEngineAttributes();
            attributes.VideoOutputFormat = (int)SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            attributes.DxgiManager = deviceMan;

            mediaEngine = new MediaEngine(mecf, attributes, MediaEngineCreateFlags.None);

            mediaEngine.PlaybackEvent += MediaEngineEx_PlaybackEvent;

            mediaEngineEx = mediaEngine.QueryInterface<MediaEngineEx>();

            //videoFile = new FileStream(VideoPath, FileMode.Open, FileAccess.Read);
            //videoStream = new ByteStream(videoFile);

            //mediaEngineEx.SetSourceFromByteStream(videoStream, new Uri(Path.GetFullPath(VideoPath), UriKind.RelativeOrAbsolute).AbsoluteUri);
            mediaEngineEx.Source = "file:///" + Path.GetFullPath(VideoPath);
            mediaEngineEx.Load();

            while (mediaEngineEx.ReadyState != 4)
            {
                //Console.WriteLine(mediaEngine.ReadyState);
            }

            int width;
            int height;

            mediaEngineEx.GetNativeVideoSize(out width, out height);
            videoDimensions = new SharpDX.Size2(width, height);
            rr = new RawRectangle(0, 0, videoDimensions.Width, videoDimensions.Height);

            //frameBmp = new Bitmap1(dc, new SharpDX.Size2(width, height));
            //dxgiSurface = frameBmp.Surface;
            mediaEngineEx.Volume = 0F;
            mediaEngineEx.Loop = true;
            mediaEngineEx.Play();

        }

        private void MediaEngineEx_PlaybackEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            Console.WriteLine(mediaEvent + " " + param1);
        }

        public override bool PreDraw(DeviceContext dc, RenderTarget rt, Factory factory, RawVector2 mousePt)
        {
            
            return true;
        }
    }
}
