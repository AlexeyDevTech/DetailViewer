using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    /// <summary>
    /// ViewModel для диалогового окна подтверждения.
    /// </summary>
    public class ConfirmationDialogViewModel : BindableBase, IDialogAware
    {
        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Подтверждение";

        private string _message = string.Empty;
        /// <summary>
        /// Сообщение, отображаемое в диалоговом окне подтверждения.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult>? RequestClose;

        /// <summary>
        /// Команда для подтверждения действия (кнопка OK).
        /// </summary>
        public DelegateCommand OkCommand { get; private set; }

        /// <summary>
        /// Команда для отмены действия (кнопка Cancel).
        /// </summary>
        public DelegateCommand CancelCommand { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ConfirmationDialogViewModel"/>.
        /// </summary>
        public ConfirmationDialogViewModel()
        {
            OkCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));
        }

        /// <summary>
        /// Определяет, можно ли закрыть диалоговое окно.
        /// </summary>
        /// <returns>Всегда true.</returns>
        public bool CanCloseDialog() => true;

        /// <summary>
        /// Вызывается после закрытия диалогового окна.
        /// </summary>
        public void OnDialogClosed() { }

        /// <summary>
        /// Вызывается при открытии диалогового окна.
        /// </summary>
        /// <param name="parameters">Параметры диалогового окна. Ожидается параметр "message" (string).</param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
        }
    }
}
