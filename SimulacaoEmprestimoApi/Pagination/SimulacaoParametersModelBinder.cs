using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SimulacaoEmprestimoApi.Pagination
{
    public class SimulacaoParametersModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext context)
        {
            var pageNumber = Convert.ToInt32(context.ValueProvider.GetValue("pageNumber").FirstValue ?? "1");
            var pageSize = Convert.ToInt32(context.ValueProvider.GetValue("pageSize").FirstValue ?? "10");

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            context.Result = ModelBindingResult.Success(new SimulacaoParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            return Task.CompletedTask;
        }
    }
}
