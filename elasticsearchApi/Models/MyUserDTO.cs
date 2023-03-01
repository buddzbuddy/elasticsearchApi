namespace elasticsearchApi.Models
{
    public class MyUserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public MyAddressDTO Address { get; set; }
        public class MyAddressDTO
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
        }
    }
}
