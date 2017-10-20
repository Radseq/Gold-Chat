using Server.Interfaces.ClientRegistration;

namespace Server.Modules.ClientRegistrationModule
{
    public class RegistrationEmailMessage : IRegistrationMessage
    {
        public string RegistrationMessage(string UserName, string Code)
        {
            return string.Format(@"
            <p>Witaj <strong>{0}</strong>.
                <br />Dziękujemy za rejestrację w aplikacji <strong>Gold Chat</strong>.
                <br />Zanim będziesz mógł kożystać z aplikacji, musisz wykonać ostatnia operacje.
                <br />Pamiętaj - musisz to zrobić zanim staniesz sie w pełni zarejestrowanym użytkownikiem.<br />
                <span style='text-decoration: underline;'>
                    <em>Jedyne co musisz zrobić to skopiować kod aktywacyjny, oraz wkleić go w oknie <strong>Register Code</strong> okno to pojawi się gdy wpiszesz swój login i hasło w <strong>Oknie Logowania!.</strong></em>
                </span>
            </p>
            <p><br />
                A o to twój kod aktywacyjny : <span style='color: #ff0000;'><strong>{1}</strong></span>
            </p>
            <p>
                <strong>
                <span style='color: #ff0000;'> Pamiętaj by dokłanie skopiować KOD.</span>
                </strong>
            </p>
            <p>Dziękujemy <br /> Administracja Gold Chat.</p>", UserName, Code);
        }
    }
}
