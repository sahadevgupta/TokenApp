using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using TokenApp.Models;

namespace TokenApp.ViewModels
{
    /// <summary>One chip in the history filter row. <see cref="Type"/> null means "All".</summary>
    public partial class TokenFilterOption : ObservableObject
    {
        public TokenFilterOption(string label, TokenType? type, ICommand selectCommand)
        {
            Label = label;
            Type = type;
            SelectCommand = selectCommand;
        }

        public string Label { get; }
        public TokenType? Type { get; }
        public ICommand SelectCommand { get; }

        [ObservableProperty]
        bool isSelected;
    }
}
