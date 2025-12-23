namespace SimulacaoEmprestimoApi.Pagination
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Dados { get; set; } = new List<T>();
        public PaginationMetadata Paginacao { get; set; } = new();
        public IEnumerable<LinkInfo> Links { get; set; } = new List<LinkInfo>();
    }

    public class PaginationMetadata
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    public class LinkInfo
    {
        public string Href { get; set; } = string.Empty;
        public string Rel { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
    }
}
