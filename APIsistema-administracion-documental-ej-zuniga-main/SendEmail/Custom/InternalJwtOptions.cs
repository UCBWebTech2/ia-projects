namespace SendEmail.Custom
{
    public class InternalJwtOptions
    {
        public const string SectionName = "InternalJwt";

        public string Issuer { get; set; } = "estudio-juridico-api";
        public string Audience { get; set; } = "ocr-api";
        public string Subject { get; set; } = "estudio-juridico-api";
        public string PrivateKeyPath { get; set; } = string.Empty;
        public int LifetimeSeconds { get; set; } = 60;
    }
}
