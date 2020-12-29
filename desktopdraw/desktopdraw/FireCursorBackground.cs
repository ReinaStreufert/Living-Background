using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct2D1.Effects;
using System.Windows.Forms;
using SharpDX.DXGI;

namespace desktopdraw
{
    class FireCursorBackground : LivingBackground
    {
        public int InitialFlameRadius = 25;
        public int FlameLifeSpanMin = 2000;
        public int FlameLifeSpanMax = 2000;
        public bool GlowEnabled = true;
        public float GlowRadius = 10F;
        public int TotalFlameTravel = 200;
        public RawColor4 BackgroundColor = new RawColor4(1F, 1F, 1F, 1F);
        // Fire
        public RawColor4 MainFlameColor = new RawColor4(1F, 0.5F, 0F, 0.5F);
        public RawColor4 MinFlameColor = new RawColor4(1F, 0F, 0F, 0.75F);
        public RawColor4 MaxFlameColor = new RawColor4(1F, 0.8F, 0F, 1F);
        // Twilight purple
        //public RawColor4 MainFlameColor = new RawColor4(0.75F, 0F, 0.55F, 0.5F);
        //public RawColor4 MinFlameColor = new RawColor4(0.5F, 0F, 0.1F, 0.5F);
        //public RawColor4 MaxFlameColor = new RawColor4(1F, 0F, 1F, 1F);
        // Blue aqua
        //public RawColor4 MainFlameColor = new RawColor4(0F, 0.55F, 0.75F, 0.5F);
        //public RawColor4 MinFlameColor = new RawColor4(0F, 0.5F, 0.1F, 0.5F);
        //public RawColor4 MaxFlameColor = new RawColor4(0F, 1F, 1F, 1F);

        public BezierCurve Curve = new BezierCurve(new[] { new System.Drawing.PointF(0F, 0F), new System.Drawing.PointF(0F, 1F), new System.Drawing.PointF(1F, 1F) });
        public string BackgroundImagePath = "";

        private DateTime lastFlameGeneration;
        private List<Flame> flames = new List<Flame>();
        private List<Flame> deadFlames = new List<Flame>();
        private RawVector2 lastMousePt = new RawVector2(0F, 0F);
        private Bitmap bg = null;

        private Random random;

        private BitmapRenderTarget brt;

        public override void Draw(DeviceContext dc, RenderTarget rt, SharpDX.Direct2D1.Factory factory, RawVector2 mousePt)
        {
            if (bg == null)
            {
                rt.Clear(BackgroundColor);
            } else
            {
                using (Tile tile = new Tile(dc))
                {
                    tile.SetInput(0, bg, false);
                    tile.Rectangle = new RawVector4(0, 0, bg.Size.Width, bg.Size.Height);
                    dc.DrawImage(tile);
                }
            }
            brt.BeginDraw();
            brt.Clear(new RawColor4(0F, 0F, 0F, 0F));
            
            using (SolidColorBrush scb = new SolidColorBrush(rt, new RawColor4(0F, 0F, 0F, 1F)))
            {
                foreach (Flame flame in flames)
                {
                    TimeSpan aliveTime = DateTime.Now - flame.CreationTime;
                    if (aliveTime.TotalMilliseconds > flame.LifeSpan)
                    {
                        deadFlames.Add(flame);
                        continue;
                    }
                    float aliveTimeNormalized = (float)(aliveTime.TotalMilliseconds / flame.LifeSpan);
                    float animationPoint = Curve.GetPointAlongCurve(aliveTimeNormalized).Y;
                    float animationPointInverted = 1F - animationPoint;

                    float currentRadius = InitialFlameRadius * animationPointInverted;
                    float currentTravelDistance = TotalFlameTravel * animationPoint;
                    RawVector2 currentLocation = pixelsInDirection(flame.InitialLocation, flame.Direction, currentTravelDistance);

                    Ellipse ellipse = new Ellipse(currentLocation, currentRadius, currentRadius);
                    scb.Color = flame.Color;

                    brt.FillEllipse(ellipse, scb);
                }
                scb.Color = MainFlameColor;
                Ellipse mainEllipse = new Ellipse(mousePt, InitialFlameRadius, InitialFlameRadius);
                brt.FillEllipse(mainEllipse, scb);
            }
            brt.EndDraw();

            if (GlowEnabled)
            {
                using (GaussianBlur blur = new GaussianBlur(dc))
                {
                    blur.StandardDeviation = GlowRadius;
                    blur.SetInput(0, brt.Bitmap, false);
                    dc.DrawImage(blur);
                }
            }
            rt.DrawBitmap(brt.Bitmap, 1F, BitmapInterpolationMode.Linear);
        }

        public override void Initialize(DeviceContext dc, RenderTarget rt, SharpDX.Direct2D1.Factory factory, SharpDX.DXGI.Surface surface, SharpDX.Direct3D11.Device d3dDevice, SharpDX.DXGI.SwapChain1 sw)
        {
            brt = new BitmapRenderTarget(rt, CompatibleRenderTargetOptions.None);
            //screenWidth = Screen.PrimaryScreen.Bounds.Width;
            //screenHeight = Screen.PrimaryScreen.Bounds.Height;
            random = new Random();

            if (BackgroundImagePath != "")
            {
                bg = WICMan.CreateD2DBitmapFromImagePath(BackgroundImagePath, rt);
            }
        }

        public override bool PreDraw(DeviceContext dc, RenderTarget rt, SharpDX.Direct2D1.Factory factory, RawVector2 mousePt)
        {
            foreach (Flame deadFlame in deadFlames)
            {
                flames.Remove(deadFlame);
            }
            deadFlames.Clear();
            if (mousePt.X != lastMousePt.X && mousePt.Y != lastMousePt.Y)
            {
                Flame flame = new Flame();
                flame.CreationTime = DateTime.Now;
                flame.InitialLocation = mousePt;
                flame.Color = new RawColor4(random.NextFloat(MinFlameColor.R, MaxFlameColor.R), random.NextFloat(MinFlameColor.G, MaxFlameColor.G), random.NextFloat(MinFlameColor.B, MaxFlameColor.B), random.NextFloat(MinFlameColor.A, MaxFlameColor.A));
                flame.Direction = random.Next(0, 359);
                flame.LifeSpan = random.Next(FlameLifeSpanMin, FlameLifeSpanMax);

                flames.Add(flame);
            } else if (flames.Count == 0)
            {
                lastMousePt = mousePt;
                return false;
            }
            lastMousePt = mousePt;
            return true;
        }

        private static RawVector2 pixelsInDirection(RawVector2 origin, int angle, float pixels)
        {
            RawVector2 b = new RawVector2();
            b.X = (float)(origin.X + Math.Sin(toRadians(angle)));
            b.Y = (float)(origin.Y + Math.Cos(toRadians(angle)));
            return pixelsToward(origin, b, pixels);
        }

        private static RawVector2 pixelsToward(RawVector2 a, RawVector2 b, float pixels)
        {
            float dist = distance(a, b);
            float rise = (b.Y - a.Y) / dist;
            float run = (b.X - a.X) / dist;
            return new RawVector2(a.X + (run * pixels), a.Y + (rise * pixels));
        }

        private static float distance(RawVector2 a, RawVector2 b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private static float toRadians(int degrees)
        {
            return degrees * ((float)Math.PI / 180F);
        }

        public override void AfterDraw()
        {
            
        }

        private class Flame
        {
            public DateTime CreationTime;
            public RawVector2 InitialLocation;
            public RawColor4 Color;
            //public RawVector2 Direction;
            public int Direction;
            public int LifeSpan;
        }
    }
}
