namespace SimulacaoEmprestimoApi.Pagination
{
    public class PagedList<T> : List<T> where T : class
    {
        public int CurrentPage { get; set; } // pagina atual
        public int TotalPages { get; set; } 
        public int PageSize { get; set; } // qtss itens por pagina
        public int TotalCount { get; set; } // total de itens na fonte de dados

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
//(double) evita arredondamento incorreto na divisão de ints: 10 / 3 = 3 (errado!) # 10 / 3.0 = 3.333...(certo)
//Math.Ceiling(): Arredonda para CIMA(3.1 → 4, 3.9 → 4)
//Math.Round(): Arredonda para o mais próximo(3.1 → 3, 3.5 → 4)
//Math.Floor(): Arredonda para BAIXO(3.9 → 3)
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items); // método nativo de List<> (PagedList<T> herda de List<T>)
        }

        public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            // Tolist(): Converte IQueriable para List (que implementa IEnumerable)
            var items = source.Skip( (pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }

}
