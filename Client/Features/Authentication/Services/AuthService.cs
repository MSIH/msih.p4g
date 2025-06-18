using msih.p4g.Server.Features.Base.UserService.Models;

namespace msih.p4g.Client.Features.Authentication.Services
{
    public class AuthService
    {
        private User? _currentUser;
        public event Action? AuthStateChanged;

        public User? CurrentUser => _currentUser;

        public bool IsAuthenticated => _currentUser != null;

        public void Login(User user)
        {
            _currentUser = user;
            NotifyAuthStateChanged();
        }

        public void Logout()
        {
            _currentUser = null;
            NotifyAuthStateChanged();
        }

        private void NotifyAuthStateChanged()
        {
            AuthStateChanged?.Invoke();
        }
    }
}
