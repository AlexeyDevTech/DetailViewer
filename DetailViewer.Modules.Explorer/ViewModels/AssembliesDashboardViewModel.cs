using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class AssembliesDashboardViewModel : BindableBase
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IDialogService _dialogService;

        private ObservableCollection<Assembly> _assemblies;
        public ObservableCollection<Assembly> Assemblies
        {
            get { return _assemblies; }
            set { SetProperty(ref _assemblies, value); }
        }

        public DelegateCommand AddAssemblyCommand { get; private set; }
        public DelegateCommand EditAssemblyCommand { get; private set; }
        public DelegateCommand DeleteAssemblyCommand { get; private set; }

        public AssembliesDashboardViewModel(IDocumentDataService documentDataService, IDialogService dialogService)
        {
            _documentDataService = documentDataService;
            _dialogService = dialogService;

            AddAssemblyCommand = new DelegateCommand(AddAssembly);
            EditAssemblyCommand = new DelegateCommand(EditAssembly, () => SelectedAssembly != null).ObservesProperty(() => SelectedAssembly);
            DeleteAssemblyCommand = new DelegateCommand(DeleteAssembly, () => SelectedAssembly != null).ObservesProperty(() => SelectedAssembly);

            LoadAssemblies();
        }

        private async Task LoadAssemblies()
        {
            var assemblies = await _documentDataService.GetAssembliesAsync();
            Assemblies = new ObservableCollection<Assembly>(assemblies);
        }

        private void AddAssembly()
        {
            _dialogService.ShowDialog("AssemblyForm", null, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadAssemblies();
                }
            });
        }

        private void EditAssembly()
        {
            var parameters = new DialogParameters { { "assembly", SelectedAssembly } };
            _dialogService.ShowDialog("AssemblyForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadAssemblies();
                }
            });
        }

        private async void DeleteAssembly()
        {
            // Confirmation dialog can be added here
            await _documentDataService.DeleteAssemblyAsync(SelectedAssembly.Id);
            await LoadAssemblies();
        }

        private Assembly _selectedAssembly;
        public Assembly SelectedAssembly
        {
            get { return _selectedAssembly; }
            set { SetProperty(ref _selectedAssembly, value); }
        }
    }
}
