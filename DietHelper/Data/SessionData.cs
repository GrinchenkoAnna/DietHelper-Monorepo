using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Data
{
    public class SessionData
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
