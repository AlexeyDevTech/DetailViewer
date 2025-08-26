using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    /// <summary>
    /// ViewModel для диалогового окна "О программе".
    /// </summary>
    public class AboutViewModel : BindableBase, IDialogAware
    {
        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "О программе";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult>? RequestClose;

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
        /// <param name="parameters">Параметры диалогового окна.</param>
        public void OnDialogOpened(IDialogParameters parameters) { }

        /// <summary>
        /// Версия программы.
        /// </summary>
        public string Version => "1.0.0";

        /// <summary>
        /// Описание программы.
        /// </summary>
        public string Description => "Интерактивный реестр записей деталей";

        /// <summary>
        /// Команда для закрытия диалогового окна.
        /// </summary>
        public DelegateCommand CloseDialogCommand { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AboutViewModel"/>.
        /// </summary>
        public AboutViewModel()
        {
            CloseDialogCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));
        }
    }
}