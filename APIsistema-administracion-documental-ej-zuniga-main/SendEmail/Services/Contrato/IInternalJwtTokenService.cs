namespace APIsEstudioJuridico.Services.Contrato
{
    public interface IInternalJwtTokenService
    {
        string GenerateToken(string actorUserId, IReadOnlyCollection<string> permissions, string operation);
    }
}
