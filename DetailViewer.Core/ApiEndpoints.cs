namespace DetailViewer.Core
{
    public static class ApiEndpoints
    {
        public const string Assemblies = "api/Assemblies";
        public const string AssemblyDetails = "api/AssemblyDetails";
        public const string AssemblyParents = "api/AssemblyParents";
        public const string ChangeLogs = "api/ChangeLogs";
        public const string Classifiers = "api/Classifiers";
        public const string ConflictLogs = "api/ConflictLogs";
        public const string DocumentDetailRecords = "api/DocumentDetailRecords";
        public const string ESKDNumbers = "api/ESKDNumbers";
        public const string ProductAssemblies = "api/ProductAssemblies";
        public const string Products = "api/Products";
        public const string Profiles = "api/Profiles";

        public static string GetChangesSince(System.DateTime timestamp) => $"{ChangeLogs}/since/{timestamp:O}";
        public static string GetParentAssemblies(string entity, int id) => $"api/{entity}/{id}/parents";
        public static string GetRelatedProducts(int assemblyId) => $"{Assemblies}/{assemblyId}/products";
        public static string ConvertProductToAssembly(int productId) => $"{Products}/{productId}/convertToAssembly";
    }
}
