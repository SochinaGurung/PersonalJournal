using Coursework.Models;

namespace Coursework.Services
{
    public class AuthenticationService
    {
        public User? CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;
        public string UserName => CurrentUser?.Username ?? "Guest";

        public void Login(User user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
