namespace ColekUI.Controls;

public interface IBlurrable
{
    IDisposable PushBlur();

    bool IsBlurred { get; }
}
