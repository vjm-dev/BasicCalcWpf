using BasicCalcWpf.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Globalization;

namespace BasicCalcWpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _displayContent = "0";
        private string _currentOperation = "";
        private string? _pendingOperation;
        private double _storedValue;
        private bool _isNewValue = true;
        private bool _operationJustPressed = false;
        private readonly CalculatorModel _calculator;

        public MainViewModel()
        {
            _calculator = new CalculatorModel();
            DigitCommand = new RelayCommand<string>(AddDigit);
            OperationCommand = new RelayCommand<string>(ExecuteOperation);
            ClearCommand = new RelayCommand(Clear);
            DecimalCommand = new RelayCommand(AddDecimal);
            ClearEntryCommand = new RelayCommand(ClearEntry);
        }

        public string DisplayContent
        {
            get => _displayContent;
            set
            {
                _displayContent = value;
                OnPropertyChanged();
            }
        }

        public string CurrentOperation
        {
            get => _currentOperation;
            set
            {
                _currentOperation = value;
                OnPropertyChanged();
            }
        }

        public ICommand DigitCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand DecimalCommand { get; }

        private void AddDigit(string digit)
        {
            if (_operationJustPressed)
            {
                _operationJustPressed = false;
            }

            if (_isNewValue || DisplayContent == "0")
            {
                DisplayContent = digit;
                _isNewValue = false;
            }
            else
            {
                DisplayContent += digit;
            }
            UpdateOperationDisplay();
        }

        private void ExecuteOperation(string operation)
        {
            if (DisplayContent.EndsWith("."))
            {
                DisplayContent = DisplayContent.TrimEnd('.');
            }

            if (double.TryParse(DisplayContent, NumberStyles.Any, CultureInfo.InvariantCulture, out double currentValue))
            {
                if (_pendingOperation != null && !_isNewValue)
                {
                    try
                    {
                        var result = _calculator.Calculate(_storedValue, currentValue, _pendingOperation);
                        DisplayContent = FormatNumber(result);
                        _storedValue = result;

                        if (operation == "=")
                        {
                            CurrentOperation = $"{FormatDisplay(_storedValue)}";
                            _pendingOperation = null;
                        }
                        else
                        {
                            CurrentOperation = $"{FormatDisplay(_storedValue)} {GetOperationSymbol(operation)}";
                            _pendingOperation = operation;
                        }
                    }
                    catch (DivideByZeroException)
                    {
                        DisplayContent = "UNDEFINED";
                        CurrentOperation = "Error: Division by zero";
                        _pendingOperation = null;
                        _storedValue = 0;
                    }
                }
                else
                {
                    _storedValue = currentValue;

                    if (operation == "=")
                    {
                        CurrentOperation = $"{FormatDisplay(_storedValue)}";
                        _pendingOperation = null;
                    }
                    else
                    {
                        CurrentOperation = $"{FormatDisplay(_storedValue)} {GetOperationSymbol(operation)}";
                        _pendingOperation = operation;
                    }
                }

                _isNewValue = true;
                _operationJustPressed = true;
            }
        }

        private void Clear()
        {
            DisplayContent = "0";
            CurrentOperation = "";
            _storedValue = 0;
            _pendingOperation = null;
            _isNewValue = true;
            _operationJustPressed = false;
        }

        private void ClearEntry()
        {
            DisplayContent = "0";
            _isNewValue = true;
            UpdateOperationDisplay();
        }

        private void AddDecimal()
        {
            if (_operationJustPressed)
            {
                _operationJustPressed = false;
            }

            if (_isNewValue)
            {
                DisplayContent = "0.";
                _isNewValue = false;
            }
            else if (!DisplayContent.Contains("."))
            {
                DisplayContent += ".";
            }
            UpdateOperationDisplay();
        }

        private void UpdateOperationDisplay()
        {
            if (_pendingOperation == null || _pendingOperation == "=")
            {
                CurrentOperation = DisplayContent;
            }
            else
            {
                CurrentOperation = $"{FormatDisplay(_storedValue)} {GetOperationSymbol(_pendingOperation)} {DisplayContent}";
            }
        }

        private string FormatNumber(double number)
        {
            // Para números enteros, mostrar sin decimales
            if (number % 1 == 0)
                return number.ToString("F0", CultureInfo.InvariantCulture);

            // Para números decimales, mostrar hasta 10 decimales y quitar ceros a la derecha
            return number.ToString("F10", CultureInfo.InvariantCulture)
                        .TrimEnd('0')
                        .TrimEnd('.');
        }

        private string FormatDisplay(double value)
        {
            return FormatNumber(value);
        }

        private string GetOperationSymbol(string operation)
        {
            return operation switch
            {
                "+" => "+",
                "-" => "-",
                "*" => "×",
                "/" => "÷",
                "=" => "=",
                _ => operation
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) =>
            parameter is T typedParameter && (_canExecute?.Invoke(typedParameter) ?? true);

        public void Execute(object? parameter)
        {
            if (parameter is T typedParameter)
                _execute(typedParameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
