using Server.Interfaces.LostPassword;

namespace Server.Modules.LostPasswordModule
{
    class LostPasswordEmailMessage : ILostPasswordEmailMessage
    {

        public string lostPassEmailMessage(string userName, string code)
        {
            return string.Format(@"
            <p>Witaj <strong>{0}</strong>.
            </p>
            <p><br />
                Jeśli zapomniałeś haśło wklej ten kod w oknie Lost Password : <span style='color: #ff0000;'><strong>{1}</strong></span>
            </p>
            <p>
            <p> 
                Jeśli nie zapomniałeś hasło lub nie próbowałeś go odzyskiwać, usuń tego maila
            </p>
                <strong>
                <span style='color: #ff0000;'> Pamiętaj by dokłanie skopiować KOD.</span>
                </strong>
            </p>
            <p>Dziękujemy <br /> Administracja Gold Chat.</p>", userName, code);
        }
    }
}
