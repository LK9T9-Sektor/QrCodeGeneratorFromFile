namespace QrCodeGeneratorFromFile
{
    public class Config
    {
        public string Filename { get; set; }
        public string FixedPart { get; set; }
        public int GenerationPartLength { get; set; }
        public int StartNumber { get; set; }
        public int LastNumber { get; set; }
        public int QrSideSize { get; set; }
    }
}
