using DetailViewer.Core.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    /// <summary>
    /// ViewModel для диалогового окна разрешения конфликтов.
    /// </summary>
    public class ConflictResolutionViewModel : BindableBase, IDialogAware
    {
        private readonly ILogger _logger;
        private object? _localEntity;
        /// <summary>
        /// Локальная версия сущности, находящейся в конфликте.
        /// </summary>
        public object? LocalEntity
        {
            get { return _localEntity; }
            set { SetProperty(ref _localEntity, value); }
        }

        private object? _remoteEntity;
        /// <summary>
        /// Удаленная версия сущности, находящейся в конфликте.
        /// </summary>
        public object? RemoteEntity
        {
            get { return _remoteEntity; }
            set { SetProperty(ref _remoteEntity, value); }
        }

        /// <summary>
        /// Команда для сохранения локальной версии сущности.
        /// </summary>
        public DelegateCommand KeepLocalCommand { get; }

        /// <summary>
        /// Команда для сохранения удаленной версии сущности.
        /// </summary>
        public DelegateCommand KeepRemoteCommand { get; }

        /// <summary>
        /// Команда для откладывания разрешения конфликта.
        /// </summary>
        public DelegateCommand PostponeCommand { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ConflictResolutionViewModel"/>.
        /// </summary>
        /// <param name="logger">Сервис логирования.</param>
        public ConflictResolutionViewModel(ILogger logger)
        {
            _logger = logger;
            KeepLocalCommand = new DelegateCommand(() => CloseDialog(ButtonResult.Yes));
            KeepRemoteCommand = new DelegateCommand(() => CloseDialog(ButtonResult.No));
            PostponeCommand = new DelegateCommand(() => CloseDialog(ButtonResult.Cancel));
        }

        /// <summary>
        /// Закрывает диалоговое окно с указанным результатом.
        /// </summary>
        /// <param name="result">Результат диалогового окна.</param>
        private void CloseDialog(ButtonResult result)
        {
            RequestClose?.Invoke(new DialogResult(result));
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
        /// <param name="parameters">Параметры диалогового окна. Ожидаются параметры "localEntity" и "remoteEntity".</param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            _logger.Log("Dialog opened: ConflictResolution");
            LocalEntity = parameters.GetValue<object>("localEntity");
            RemoteEntity = parameters.GetValue<object>("remoteEntity");
        }

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Разрешение конфликта";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult> RequestClose;
    }
}