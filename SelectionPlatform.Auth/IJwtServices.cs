using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Auth
{
    public interface IJwtServices
    {
        string GenerateToken(string userId, string userRole);
    }
}
