using System.Security;

namespace Gold_Client.ViewModel
{
    public class RegistrationPresenter : ObservableObject
    {
        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }
    }
}
