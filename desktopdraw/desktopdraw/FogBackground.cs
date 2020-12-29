using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct2D1.Effects;

namespace desktopdraw
{
    class FogBackground : LivingBackground
    {
        public string BackgroundImagePath = "universe.jpg";
        public float BlurStandardDeviation = 20F;
        public int BlurRegionRadius = 200;
        public float BlendAmount = 40F;

        private Bitmap bg;
        private BitmapRenderTarget brt;
        private RawVector2 lastMousePt;

        public override void Draw(DeviceContext dc, RenderTarget rt, Factory factory, RawVector2 mousePt)
        {
            brt.BeginDraw();
            brt.Clear(new RawColor4(0F, 0F, 0F, 0F));
            using (SolidColorBrush scb = new SolidColorBrush(rt, new RawColor4(0F, 0F, 0F, 1F)))
            {
                brt.FillEllipse(new Ellipse(mousePt, BlurRegionRadius, BlurRegionRadius), scb);
            }
            brt.EndDraw();

            using (Tile tileEffect = new Tile(dc))
            {
                tileEffect.SetInput(0, bg, false);
                tileEffect.Rectangle = new RawVector4(0, 0, bg.Size.Width, bg.Size.Height);
                dc.DrawImage(tileEffect);

                using (GaussianBlur universeBlurEffect = new GaussianBlur(dc))
                {
                    universeBlurEffect.SetInput(0, tileEffect.Output, true);
                    universeBlurEffect.StandardDeviation = BlurStandardDeviation;

                    using (GaussianBlur maskBlurEffect = new GaussianBlur(dc))
                    {
                        maskBlurEffect.SetInput(0, brt.Bitmap, false);
                        maskBlurEffect.StandardDeviation = BlendAmount;

                        using (Composite compositeEffect = new Composite(dc))
                        {
                            compositeEffect.Mode = CompositeMode.SourceIn;
                            compositeEffect.SetInput(0, maskBlurEffect.Output, false);
                            compositeEffect.SetInput(1, universeBlurEffect.Output, true);
                            dc.DrawImage(compositeEffect);
                        }
                    }
                }
            }
        }

        public override void AfterDraw()
        {
            
        }

        public override void Initialize(DeviceContext dc, RenderTarget rt, Factory factory, SharpDX.DXGI.Surface surface, SharpDX.Direct3D11.Device d3dDevice, SharpDX.DXGI.SwapChain1 sw)
        {
            bg = WICMan.CreateD2DBitmapFromImagePath(BackgroundImagePath, rt);
            brt = new BitmapRenderTarget(rt, CompatibleRenderTargetOptions.None);
        }

        public override bool PreDraw(DeviceContext dc, RenderTarget rt, Factory factory, RawVector2 mousePt)
        {
            if (mousePt.X == lastMousePt.X && mousePt.Y == lastMousePt.Y)
            {
                return false;
            }
            lastMousePt = mousePt;
            return true;
        }
    }
}
