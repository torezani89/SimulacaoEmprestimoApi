namespace SimulacaoEmprestimoApi.Models
{
    public static class UsuarioMappingExtensionDTO
    {
        public static UsuarioResponseDto ToUsuarioResponseDto(this UsuarioModel usuario)
        {
            //if(usuario is null) return null;

            return new UsuarioResponseDto
            {
                Usuario = usuario.Usuario,
                Nome = usuario.Nome,
                Sobrenome = usuario.Sobrenome,
                Email = usuario.Email,
                DataCriacao = usuario.DataCriacao
            };
        }

        public static UsuarioModel ToUsuarioModel(this UsuarioCriacaoDto usuarioDto, byte[] senhaHash, byte[] senhaSalt)
        {
            //if (usuarioDto is null) return null;

            return new UsuarioModel
            {
                Usuario = usuarioDto.Usuario,
                Nome = usuarioDto.Nome,
                Sobrenome = usuarioDto.Sobrenome,
                Email = usuarioDto.Email,
                SenhaHash = senhaHash,
                SenhaSalt = senhaSalt
            };
        }

        //public static IEnumerable<UsuarioResponseDto>? ToUsuariosResponseList(this IEnumerable<UsuarioModel> usuarios)
        //{
        //    if (usuarios is null || !usuarios.Any()) return new List<UsuarioResponseDto>();

        //    return usuarios.Select(usuario => new UsuarioResponseDto
        //    {
        //        Usuario = usuario.Usuario,
        //        Nome = usuario.Nome,
        //        Sobrenome = usuario.Sobrenome,
        //        Email = usuario.Email,
        //        DataCriacao = usuario.DataCriacao
        //    }).ToList();
        //}
    }
}
