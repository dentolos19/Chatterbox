﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chatterbox.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void UpdateProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
            return;
        storage = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}