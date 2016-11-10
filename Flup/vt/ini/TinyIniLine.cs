namespace vt.ini
{
    /// <summary>
    /// rozparsovana radka INI souboru
    /// </summary>
    internal class TinyIniLine
    {
        public TinyIniLineType LineType { get; set; }

        public string SourceLine { get; set; }

        public string Section { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
