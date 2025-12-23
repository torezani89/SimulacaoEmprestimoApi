using Microsoft.AspNetCore.Http;
using SimulacaoEmprestimoApi.Pagination;
using System.Text.Json;

namespace SimulacaoEmprestimoApi.Extensions
{
    public static class PaginationExtensions
    {
        //Compiler Error CS0452: The type 'T' must be a reference type in order to use it as parameter 'T' in the generic type or method 'PagedList<T>'.
        //O método de extensão não repete a restrição de tipo presente na classe PagedList<T> (where T : class).
        //O compilador exige que o método também declare a mesma restrição, senão ele assume que T pode ser value type, o que quebra o contrato do PagedList<T>.
        public static void AddPaginationHeader<T>(this HttpResponse response, PagedList<T> list) where T : class
        {
            var metadata = new
            {
                list.TotalCount,
                list.TotalPages,
                list.PageSize,
                list.CurrentPage,
                list.HasNext,
                list.HasPrevious
            };

            //Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata)); // usando package Newtonsoft.Json

            var options = new JsonSerializerOptions // definir options se usar System.Text.Json.JsonSerializer
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Com opções para CamelCase (igual Newtonsoft)
                WriteIndented = false // false para headers (otimizado)
            };

            response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata, options));
        }
    }
}
