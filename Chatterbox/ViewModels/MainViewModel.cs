namespace Chatterbox.ViewModels;

public class MainViewModel : BaseViewModel
{

    private bool _enableConnectionInput = true;
    private bool _enableMessageSending;

    public bool EnableConnectionInput
    {
        get => _enableConnectionInput;
        set => UpdateProperty(ref _enableConnectionInput, value);
    }

    public bool EnableMessageSending
    {
        get => _enableMessageSending;
        set => UpdateProperty(ref _enableMessageSending, value);
    }

}