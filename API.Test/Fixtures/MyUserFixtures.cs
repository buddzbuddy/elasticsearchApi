using elasticsearchApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elasticsearchApi.Tests.Fixtures
{
    public static class MyUserFixtures
    {
        public static List<MyUserDTO> GetTestUsers() => new ()
        {
            new()
                {
                    Id = 1,
                    Name= "Jane",
                    Email = "tet@t.com",
                    Username = "j1",
                    Address = new MyUserDTO.MyAddressDTO
                    {
                        City = "N.A.",
                        Street = "Brown str.",
                        ZipCode = "754666"
                    }
                },
                new()
                {
                    Id = 2,
                    Name= "Bob",
                    Email = "bob@t.com",
                    Address = new MyUserDTO.MyAddressDTO
                    {
                        City = "N.A.",
                        Street = "Brown str.",
                        ZipCode = "754666"
                    }
                },
                new()
                {
                    Id = 3,
                    Name= "Smith",
                    Email = "smith@t.com",
                    Address = new MyUserDTO.MyAddressDTO
                    {
                        City = "N.A.",
                        Street = "Brown str.",
                        ZipCode = "754666"
                    }
                },
        };
    }
}
