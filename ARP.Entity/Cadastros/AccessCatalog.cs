namespace ARP.Entity.Cadastros
{
    public static class PersonaCodes
    {
        public const string Operator = "operador";
        public const string Collaborator = "colaborador";
        public const string Company = "empresa";
        public const string Admin = "admin";
    }

    public static class PermissionCodes
    {
        public const string CompanyView = "empresa.visualizar";
        public const string CompanyCreate = "empresa.criar";
        public const string CompanyEdit = "empresa.editar";
        public const string CompanyManage = "empresa.gerenciar";
        public const string CollaboratorView = "colaborador.visualizar";
        public const string CollaboratorManage = "colaborador.gerenciar";
        public const string SectorManage = "setor.gerenciar";
        public const string CategoryManage = "categoria.gerenciar";
        public const string StandardSurveyCreate = "pesquisa_padronizada.criar";
        public const string StandardSurveyEdit = "pesquisa_padronizada.editar";
        public const string StandardSurveyPublish = "pesquisa_padronizada.publicar";
        public const string StandardSurveyManage = "pesquisa_padronizada.gerenciar";
        public const string SurveyRespond = "pesquisa.responder";
        public const string SurveyTrack = "pesquisa.acompanhar";
        public const string TenantSwitch = "tenant.alternar";
        public const string PermissionManage = "permissao.gerenciar";
    }
}