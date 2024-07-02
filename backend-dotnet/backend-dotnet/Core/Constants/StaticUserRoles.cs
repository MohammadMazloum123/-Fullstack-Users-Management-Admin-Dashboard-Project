namespace backend_dotnet.Core.Constants
{
    public static class StaticUserRoles
    {
        public const string OWNER = "OWNER";
        public const string USER = "USER";
        public const string ADMIN = "ADMIN";
        public const string MANAGER = "MANAGER";

        public const string OwnerAdmin = "OWNER,ADMIN";
        public const string OwnerAdminManager = "OWNER,ADMIN,MANAGER";
        public const string OwnerAdminManagerUser = "OWNER,ADMIN,MANAGER,USER";

    }
}
