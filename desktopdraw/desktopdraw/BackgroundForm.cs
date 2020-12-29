using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using System.Threading;
using SharpDX.Direct2D1.Effects;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;

namespace desktopdraw
{
    class BackgroundForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        private RenderTarget renderTarget = null;
        private DeviceContext deviceContext = null;
        private Device d2dDevice = null;
        private SharpDX.Direct3D11.Device d3dDevice = null;
        private SharpDX.DXGI.Factory2 dxgiFactory = null;
        private SharpDX.DXGI.Surface dxgiSurface = null;
        private SharpDX.DXGI.SwapChain1 sw = null;
        private SpinWait spin = new SpinWait(); 
        private Factory1 factory;
        private SharpDX.Direct2D1.Bitmap rtBitmap;
        private MultimediaTimer mmt = new MultimediaTimer();
        private Thread altThread;

        private LivingBackground livingBackground = null;

        private LivingBackground initialBackground;

        public LivingBackground Background
        {
            get
            {
                return livingBackground;
            }
            set
            {
                value.Initialize(deviceContext, renderTarget, factory, dxgiSurface, d3dDevice, sw);
                livingBackground = value;
            }
        }

        public BackgroundForm(LivingBackground InitialBackground)
        {
            initialBackground = InitialBackground;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Location = screenBounds.Location;
            this.Size = new Size(3840, 1080);

            IntPtr backgroundWindow = BackgroundWindow.ObtainBackgroundHandle();
            SetParent(this.Handle, backgroundWindow);
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            d3dDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.Debug | SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport | SharpDX.Direct3D11.DeviceCreationFlags.VideoSupport);
            dxgiFactory = new SharpDX.DXGI.Factory2();

            SharpDX.DXGI.SwapChainDescription1 scd1 = new SharpDX.DXGI.SwapChainDescription1();
            scd1.AlphaMode = SharpDX.DXGI.AlphaMode.Ignore;
            scd1.BufferCount = 2;
            scd1.Flags = SharpDX.DXGI.SwapChainFlags.None;
            scd1.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            scd1.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            scd1.Width = 3840;
            scd1.Height = 1080;
            scd1.Scaling = SharpDX.DXGI.Scaling.Stretch;
            scd1.Stereo = false;
            scd1.SwapEffect = SharpDX.DXGI.SwapEffect.Sequential;
            scd1.Usage = SharpDX.DXGI.Usage.RenderTargetOutput;
            sw = new SharpDX.DXGI.SwapChain1(dxgiFactory, d3dDevice, this.Handle, ref scd1);

            SharpDX.DXGI.Device dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
            factory = new Factory1(FactoryType.MultiThreaded, DebugLevel.Information);
            d2dDevice = new Device(factory, dxgiDevice);
            deviceContext = new DeviceContext(d2dDevice, DeviceContextOptions.None);
            dxgiSurface = sw.GetBackBuffer<SharpDX.DXGI.Surface>(0);

            BitmapProperties1 bp = new BitmapProperties1();
            bp.BitmapOptions = BitmapOptions.CannotDraw | BitmapOptions.Target;
            bp.PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore);
            rtBitmap = new SharpDX.Direct2D1.Bitmap1(deviceContext, dxgiSurface, bp);
            deviceContext.Target = rtBitmap;

            renderTarget = deviceContext;
            Background = initialBackground;

            //mmt.Elapsed += FrameDraw;
            //mmt.Interval = 16;
            //mmt.Resolution = 0;
            //mmt.Start();
            while (true)
            {
                Application.DoEvents();
                FrameDraw(null, null);
            }
        }
        private void FrameDraw(object sender, EventArgs e)
        {
            POINT mousePt;
            GetCursorPos(out mousePt);
            RawVector2 mousePtD2D = new RawVector2(mousePt.X + 1920, mousePt.Y);
            if (livingBackground.PreDraw(deviceContext, renderTarget, factory, mousePtD2D))
            {
                if (livingBackground.RTDraw)
                {
                    renderTarget.BeginDraw();
                    livingBackground.Draw(deviceContext, renderTarget, factory, mousePtD2D);
                    renderTarget.EndDraw();
                }
                else
                {
                    livingBackground.Draw(deviceContext, renderTarget, factory, mousePtD2D);
                }
            }
            sw.Present(1, SharpDX.DXGI.PresentFlags.None);
            //livingBackground.AfterDraw();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // stop the painting of the background
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
        }

        private void Draw()
        {
            
        }
    }
}
