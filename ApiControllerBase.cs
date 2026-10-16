using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers
{
    // Shared helpers for reading who is logged in (from the JWT claims)
    public class ApiControllerBase : ControllerBase
    {
        protected int CurrentUserId()
        {
            var claim = User.FindFirst("uid");
            if (claim == null) return 0;
            return int.Parse(claim.Value);
        }

        protected string CurrentRole()
        {
            var claim = User.FindFirst("role");
            if (claim == null) return "";
            return claim.Value;
        }
    }
}
