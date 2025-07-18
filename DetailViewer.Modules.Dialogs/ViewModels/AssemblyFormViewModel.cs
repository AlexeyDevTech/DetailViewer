using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class AssemblyFormViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;

        public string Title => "Форма сборки";

        public event Action<IDialogResult> RequestClose;

        private Assembly _assembly;
        public Assembly Assembly
        {
            get { return _assembly; }
            set { SetProperty(ref _assembly, value); }
        }

        // ESKD Number Properties
        private string _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        public string CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnESKDNumberPartChanged); }
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged); }
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0)
                {
                    return string.Empty;
                }

                string baseCode = $"{CompanyCode}.{int.Parse(ClassNumberString):D6}.{DetailNumber:D3}";
                return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode;
            }
        }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AssemblyFormViewModel(IDocumentDataService documentDataService)
        {
            _documentDataService = documentDataService;

            Assembly = new Assembly
            {
                EskdNumber = new ESKDNumber()
            };

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        private async void Save()
        {
            Assembly.EskdNumber.CompanyCode = CompanyCode;
            if (int.TryParse(ClassNumberString, out int classNumber))
            {
                Assembly.EskdNumber.ClassNumber = new Classifier { Number = classNumber };
            }
            Assembly.EskdNumber.DetailNumber = DetailNumber;
            Assembly.EskdNumber.Version = Version;

            if (Assembly.Id == 0)
            {
                await _documentDataService.AddAssemblyAsync(Assembly);
            }
            else
            {
                await _documentDataService.UpdateAssemblyAsync(Assembly);
            }

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("assembly"))
            {
                Assembly = parameters.GetValue<Assembly>("assembly");
                CompanyCode = Assembly.EskdNumber.CompanyCode;
                ClassNumberString = Assembly.EskdNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = Assembly.EskdNumber.DetailNumber;
                Version = Assembly.EskdNumber.Version;
            }
        }
    }
}
