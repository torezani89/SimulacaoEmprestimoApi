using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SimulacaoEmprestimoApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SimulacaoEmprestimoApi.Services
{
    public class SenhaService : ISenhaservice
    {
        private readonly IConfiguration _config;
        public SenhaService(IConfiguration config)
        {
            _config = config;
        }

        public void CriarSenhaHash(string senha, out byte[] senhaHash, out byte[] senhaSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                senhaSalt = hmac.Key;
                senhaHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
            }
        }
        public bool VerificaSenhaHash(string senha, byte[] senhaHash, byte[] senhaSalt)
        {
            using (var hmac = new HMACSHA512(senhaSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
                return computedHash.SequenceEqual(senhaHash);
            }
        }
        public string CriarToken(UsuarioModel usuario)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("Email", usuario.Email),
                new Claim("Username", usuario.Usuario)
            };

            var jwtSettings = _config.GetSection("Jwt");
            var jwtSettingsKey = jwtSettings["Key"];

            if (string.IsNullOrEmpty(jwtSettingsKey))
            {
                throw new InvalidOperationException("A chave JWT não foi configurada em appsettings.json");
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettingsKey));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims, // informações do usuário que vão dentro do token.
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
                //expires: DateTime.Now.AddDays(1), // data de expiração → 1 dia a partir de agora.
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                signingCredentials: credentials // assinatura com a chave secreta (Chave Simétrica) e o algoritmo escolhido (HmacSha512).
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

    }
}
