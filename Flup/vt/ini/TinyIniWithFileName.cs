namespace vt.ini
{
    /// <summary>
    /// ini s rucne definovanym filename
    /// </summary>
    public abstract class TinyIniWithFileName : TinyIni
    {
        public string FileName { get; protected set; }

        public void Save()
        {
            Save(FileName);
        }
    }
}
