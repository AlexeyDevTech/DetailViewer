namespace DetailViewer.Modules.Dialogs
{
    /// <summary>
    /// Содержит ключи для параметров, передаваемых между диалоговыми окнами Prism.
    /// </summary>
    public static class DialogParameterKeys
    {
        /// <summary>
        /// Ключ для передачи объекта записи документа (DocumentDetailRecord).
        /// </summary>
        public const string Record = "record";

        /// <summary>
        /// Ключ для передачи полного имени активного пользователя.
        /// </summary>
        public const string ActiveUserFullName = "activeUserFullName";

        /// <summary>
        /// Ключ для передачи кода компании.
        /// </summary>
        public const string CompanyCode = "companyCode";

        /// <summary>
        /// Ключ для передачи списка выбранных сборок.
        /// </summary>
        public const string SelectedAssemblies = "selectedAssemblies";

        /// <summary>
        /// Ключ для передачи списка выбранных продуктов.
        /// </summary>
        public const string SelectedProducts = "selectedProducts";

        /// <summary>
        /// Ключ для передачи объекта продукта (Product).
        /// </summary>
        public const string Product = "product";

        
    }
}