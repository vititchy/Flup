using vt.ini;

namespace FlickrNetExtender.Ini
{
    /// <summary>
    /// ini se secret tokeny vygenerovanymi Flickrem pro uzivatele - budu ho skladovat v app folderu usera
    /// </summary>
    public class FlickrExtenderSecretIni : TinyIniAppFolder
    {
        private static string  SECRET_INI_FILE_NAME = "Flup.Secret.ini";
        private const string INI_SECTION = "FlupSecret";

        internal bool SecretExists {  get { return !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(TokenSecret); } }

        public string Token
        {
            get { return GetString(INI_SECTION, "Token"); }
            set { AddValue(INI_SECTION, "Token", value); }
        }

        public string TokenSecret
        {
            get { return GetString(INI_SECTION, "TokenSecret"); }
            set { AddValue(INI_SECTION, "TokenSecret", value); }
        }
          
        /// <summary>
        /// konstruktor
        /// </summary>
        public FlickrExtenderSecretIni() : base("Flup", SECRET_INI_FILE_NAME)
        {
        }
    }
}
