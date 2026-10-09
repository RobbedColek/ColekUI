using Avalonia.Controls;
using Avalonia.Media;

namespace ColekUI.Controls;

internal sealed class BlurState
{
    private const double BlurRadius = 10;

    private int _count;

    public event Action? Changed;

    public bool IsBlurred => _count > 0;

    public IDisposable Push()
    {
        if (++_count == 1)
            Changed?.Invoke();

        return new Token(this);
    }

    public static void ApplyEffect(Control? host, bool blurred) =>
        host?.Effect = new BlurEffect { Radius = blurred ? BlurRadius : 0 };

    private void Release()
    {
        if (--_count == 0)
            Changed?.Invoke();
    }

    private sealed class Token(BlurState owner) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            owner.Release();
        }
    }
}
