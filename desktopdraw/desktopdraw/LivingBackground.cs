using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopdraw
{
    abstract class LivingBackground
    {
        public bool RTDraw = true;
        public abstract void Initialize(DeviceContext dc, RenderTarget rt, Factory factory, SharpDX.DXGI.Surface dxgiSurface, SharpDX.Direct3D11.Device d3dDevice, SharpDX.DXGI.SwapChain1 sw);
        public abstract bool PreDraw(DeviceContext dc, RenderTarget rt, Factory factory, RawVector2 mousePt);
        public abstract void Draw(DeviceContext dc, RenderTarget rt, Factory factory, RawVector2 mousePt);
        public abstract void AfterDraw();
    }
}
