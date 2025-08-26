using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    /// <summary>
    /// ViewModel для диалогового окна выбора листа Excel и опций импорта.
    /// </summary>
    public class SelectSheetDialogViewModel : BindableBase, IDialogAware
    {
        /// <summary>
        /// Команда для подтверждения выбора.
        /// </summary>
        public DelegateCommand OkCommand { get; }

        /// <summary>
        /// Команда для отмены выбора.
        /// </summary>
        public DelegateCommand CancelCommand { get; }

        private List<string> _sheetNames = new List<string>();
        /// <summary>
        /// Список названий листов, доступных для выбора.
        /// </summary>
        public List<string> SheetNames
        {
            get { return _sheetNames; }
            set { SetProperty(ref _sheetNames, value); }
        }

        private string _selectedSheet = string.Empty;
        /// <summary>
        /// Выбранное название листа.
        /// </summary>
        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set { SetProperty(ref _selectedSheet, value); }
        }

        private bool _createRelationships;
        /// <summary>
        /// Флаг, указывающий, нужно ли создавать связи при импорте.
        /// </summary>
        public bool CreateRelationships
        {
            get { return _createRelationships; }
            set { SetProperty(ref _createRelationships, value); }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SelectSheetDialogViewModel"/>.
        /// </summary>
        public SelectSheetDialogViewModel()
        {
            OkCommand = new DelegateCommand(OnOk);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        /// <summary>
        /// Обрабатывает команду OK, возвращая выбранный лист и опции.
        /// </summary>
        private void OnOk()
        {
            var parameters = new DialogParameters
            {
                { "selectedSheet", SelectedSheet },
                { "createRelationships", CreateRelationships }
            };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
        }

        /// <summary>
        /// Обрабатывает команду Cancel.
        /// </summary>
        private void OnCancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Выбор листа";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event System.Action<IDialogResult> RequestClose;

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
        /// <param name="parameters">Параметры диалогового окна. Ожидается параметр "sheetNames" (List&lt;string&gt;).</param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            SheetNames = parameters.GetValue<List<string>>("sheetNames");
        }
    }
}