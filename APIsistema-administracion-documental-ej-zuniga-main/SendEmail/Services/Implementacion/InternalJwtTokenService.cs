using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using APIsEstudioJuridico.Services.Contrato;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SendEmail.Custom;

namespace APIsEstudioJuridico.Services.Implementacion
{
    public sealed class InternalJwtTokenService : IInternalJwtTokenService, IDisposable
    {
        private readonly InternalJwtOptions _options;
        private readonly string _privateKeyPath;
        private readonly Lazy<RSA> _rsa;
        private readonly Lazy<SigningCredentials> _signingCredentials;

        public InternalJwtTokenService(
            IOptions<InternalJwtOptions> options,
            IHostEnvironment hostEnvironment)
        {
            _options = options.Value;

            if (_options.LifetimeSeconds is < 1 or > 60)
            {
                throw new InvalidOperationException("InternalJwt:LifetimeSeconds debe estar entre 1 y 60 segundos.");
            }

            if (string.IsNullOrWhiteSpace(_options.PrivateKeyPath))
            {
                throw new InvalidOperationException("InternalJwt:PrivateKeyPath no está configurado.");
            }

            _privateKeyPath = Path.IsPathRooted(_options.PrivateKeyPath)
                ? _options.PrivateKeyPath
                : Path.Combine(hostEnvironment.ContentRootPath, _options.PrivateKeyPath);

            _privateKeyPath = Path.GetFullPath(_privateKeyPath);
            _rsa = new Lazy<RSA>(LoadPrivateKey);
            _signingCredentials = new Lazy<SigningCredentials>(() =>
                new SigningCredentials(
                    new RsaSecurityKey(_rsa.Value),
                    SecurityAlgorithms.RsaSha256));
        }

        private RSA LoadPrivateKey()
        {
            if (!File.Exists(_privateKeyPath))
            {
                throw new FileNotFoundException("No se encontró la clave privada del JWT interno.", _privateKeyPath);
            }

            var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(_privateKeyPath));
            return rsa;
        }

        public string GenerateToken(
            string actorUserId,
            IReadOnlyCollection<string> permissions,
            string operation)
        {
            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                throw new ArgumentException("El actor_user_id es obligatorio.", nameof(actorUserId));
            }

            if (permissions.Count == 0)
            {
                throw new ArgumentException("Debe incluir al menos un permiso.", nameof(permissions));
            }

            if (string.IsNullOrWhiteSpace(operation))
            {
                throw new ArgumentException("La operación es obligatoria.", nameof(operation));
            }

            var now = DateTime.UtcNow;
            var payload = new JwtPayload
            {
                { JwtRegisteredClaimNames.Sub, _options.Subject },
                { JwtRegisteredClaimNames.Iss, _options.Issuer },
                { JwtRegisteredClaimNames.Aud, _options.Audience },
                { "permissions", permissions.ToArray() },
                { "actor_user_id", actorUserId },
                { "operation", operation },
                { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() },
                { JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now) },
                { JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(now.AddSeconds(_options.LifetimeSeconds)) }
            };

            var token = new JwtSecurityToken(
                new JwtHeader(_signingCredentials.Value),
                payload);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void Dispose()
        {
            if (_rsa.IsValueCreated)
            {
                _rsa.Value.Dispose();
            }
        }
    }
}
