using ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace REPOSITORIES.IRepositories.Clients
{
    public interface IAccountClientRepository
    {
        AccountClient GetByUsername(string username);
        int UpdatePassword(string email, string password);
    }
}
