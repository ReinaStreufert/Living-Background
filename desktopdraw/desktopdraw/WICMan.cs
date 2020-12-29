using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopdraw
{
    static class WICMan
    {
        public static Bitmap CreateD2DBitmapFromImagePath(string Path, RenderTarget rt)
        {
            SharpDX.WIC.Bitmap wicBmp;
            Bitmap bmp;
            using (SharpDX.WIC.ImagingFactory imgFactory = new SharpDX.WIC.ImagingFactory())
            {
                using (SharpDX.WIC.BitmapDecoder decoder = new SharpDX.WIC.BitmapDecoder(imgFactory, Path, SharpDX.WIC.DecodeOptions.CacheOnDemand))
                {
                    using (SharpDX.WIC.BitmapFrameDecode bfd = decoder.GetFrame(0))
                    {
                        wicBmp = new SharpDX.WIC.Bitmap(imgFactory, bfd, new RawBox(0, 0, bfd.Size.Width, bfd.Size.Height));
                    }
                    SharpDX.WIC.Bitmap oldBg = wicBmp;
                    using (SharpDX.WIC.FormatConverter fc = new SharpDX.WIC.FormatConverter(imgFactory))
                    {
                        fc.Initialize(wicBmp, SharpDX.WIC.PixelFormat.Format32bppBGRA);
                        wicBmp = new SharpDX.WIC.Bitmap(imgFactory, fc, new RawBox(0, 0, fc.Size.Width, fc.Size.Height));
                        oldBg.Dispose();
                    }
                }
                BitmapProperties bp = new BitmapProperties();
                bp.DpiX = 0;
                bp.DpiY = 0;
                bp.PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore);
                bmp = new SharpDX.Direct2D1.Bitmap(rt, wicBmp.Size, bp);
                using (SharpDX.WIC.BitmapLock bmpLock = wicBmp.Lock(SharpDX.WIC.BitmapLockFlags.Read))
                {
                    bmp.CopyFromMemory(bmpLock.Data.DataPointer, bmpLock.Data.Pitch);
                }
            }
            return bmp;
        }
    }
}
