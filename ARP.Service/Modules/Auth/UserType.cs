namespace ARP.Service.Modules.Auth
{
    public class UserType
    {
        [GraphQLType(typeof(IdType))]
        public long Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
