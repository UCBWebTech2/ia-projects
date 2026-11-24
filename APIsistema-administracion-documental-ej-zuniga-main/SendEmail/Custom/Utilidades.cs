using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SendEmail.Models;
using Microsoft.IdentityModel.Tokens;


namespace SendEmail.Custom
{
    public class Utilidades
    {
        private readonly IConfiguration _configuration;
        //Constructor para acceder a los datos de appsetting
        public Utilidades(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";

            // Si la longitud es menor que 8, establecer un mínimo
            if (length < 8)
            {
                length = 8;
            }

            char[] password = new char[length];
            for (int i = 0; i < length; i++)
            {
                int index = RandomNumberGenerator.GetInt32(validChars.Length);
                password[i] = validChars[index];
            }

            return new string(password);
        }
        public string encriptarSHA256(string texto) 
        {
            using (SHA256 sha256Hash = SHA256.Create()) 
            {
                //Computar el hash
                byte[] bytes=sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(texto));
                // Convertir el array de bytes a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            } 
        }
        //public static string EncriptarClave(string clave)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    using(SHA256 hash = SHA256Managed.Create())
        //    {
        //        Encoding enc = Encoding.UTF8;
        //        byte[] result = hash.ComputeHash(enc.GetBytes(clave));

        //        foreach (byte b in result)
        //            sb.Append(b.ToString("x2"));
        //    }
        //    return sb.ToString();
        //}
        public static string EncriptarClave(string clave)
        {
            ArgumentNullException.ThrowIfNull(clave);

            byte[] bytesTexto = Encoding.UTF8.GetBytes(clave);
            byte[] bytesHash = SHA256.HashData(bytesTexto);

            StringBuilder sb = new StringBuilder();

            foreach (byte b in bytesHash)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
        public string generarJWT(Usuario modelo)
        {
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, modelo.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, modelo.CorreoElectronico!),
                new Claim(ClaimTypes.Role, modelo.Rol!)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            //crear detalle del token
            var jwtConfig = new JwtSecurityToken(
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }
        public static string GenerarToken()
        {
            string token = Guid.NewGuid().ToString("N");
            return token;
        }
    }
}
