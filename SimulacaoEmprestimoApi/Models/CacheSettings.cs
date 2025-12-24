namespace SimulacaoEmprestimoApi.Models
{
    public class CacheSettings // ← deve corresponder ao nome da SEÇÃO em appsettings.json
    {
        //[ConfigurationKeyName("duracao_em_minutos")] //se nome da prop em appsettings diferir
        public int DuracaoMinutos { get; set; } = 10; // ← corresponder nome da Propriedade dentro da seção

        // valor padrão = 10, caso não seja definido na seção "CacheSettings" do appsettings
        //A seção no appsettings deve corresponder ao nome da classe (ou ser configurada explicitamente)
        //As propriedades dentro da seção devem corresponder às propriedades da classe
    }
}
