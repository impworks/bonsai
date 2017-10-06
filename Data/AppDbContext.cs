using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Bonsai.Data
{
    public class AppDbContext: IdentityDbContext<AppUser>
    {
    }
}
