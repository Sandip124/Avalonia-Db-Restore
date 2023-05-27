using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PostgresRestore.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public double HeartSize => MaxSize * 2;
    
    public MainWindowViewModel()
    {
        MaxSize = 5;
    }

    [RelayCommand]
    private void Restore()
    {
        
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeartSize))]
    private double _maxSize;
}