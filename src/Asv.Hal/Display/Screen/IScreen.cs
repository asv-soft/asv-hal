namespace Asv.Hal;

public interface IScreen:IRenderContext
{
    IDisposable BeginRenderLoop();
    void Debug(string key,string value);
    void DebugWrite(string message);
    void DebugWriteLine(string message);
}