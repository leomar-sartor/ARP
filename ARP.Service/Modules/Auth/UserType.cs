namespace ARP.Service.Modules.Auth
{
    public class UserType
    {
        [GraphQLType(typeof(IdType))]
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
