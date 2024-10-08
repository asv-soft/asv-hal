namespace Asv.Hal;

public interface IScreen:IRenderContext
{
    IDisposable BeginRenderLoop();
}