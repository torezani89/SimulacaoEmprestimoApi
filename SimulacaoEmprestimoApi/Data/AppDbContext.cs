using Microsoft.EntityFrameworkCore;
using SimulacaoEmprestimoApi.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SimulacaoEmprestimoApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<SimulacaoModel> Simulacoes { get; set; }
        public DbSet<ParcelaModel> Parcelas { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Produto>().ToTable("PRODUTO");
            modelBuilder.Entity<Produto>().HasKey(p => p.CO_PRODUTO);

            modelBuilder.Entity<SimulacaoModel>(entity =>
            {
                entity.ToTable("SIMULACAO");
                entity.HasKey(e => e.IdSimulacao);
                entity.Property(e => e.IdSimulacao).HasColumnName("ID_SIMULACAO");
                entity.Property(e => e.CodigoProduto).HasColumnName("CO_PRODUTO");
                entity.Property(e => e.ValorDesejado).HasColumnName("VALOR_DESEJADO");
                entity.Property(e => e.Prazo).HasColumnName("PRAZO");
                entity.Property(e => e.TaxaJuros).HasColumnName("PC_TAXA_JUROS");
                entity.Property(e => e.DataSimulacao).HasColumnName("DATA_SIMULACAO");
                // CONFIGURAÇÃO DO RELACIONAMENTO COM A TABELA PARCELAS
                entity.HasMany(s => s.Parcelas)
                      .WithOne()
                      .HasForeignKey(p => p.IdSimulacao);
            });

            modelBuilder.Entity<ParcelaModel>(entity =>
            {
                entity.ToTable("SIMULACAO_PARCELA");
                entity.HasKey(e => e.IdParcela);
                entity.Property(e => e.IdParcela).HasColumnName("ID_PARCELA");
                entity.Property(e => e.IdSimulacao).HasColumnName("ID_SIMULACAO");
                entity.Property(e => e.TipoAmortizacao).HasColumnName("TIPO_AMORTIZACAO");
                entity.Property(e => e.Numero).HasColumnName("NUMERO");
                entity.Property(e => e.ValorAmortizacao).HasColumnName("VALOR_AMORTIZACAO");
                entity.Property(e => e.ValorJuros).HasColumnName("VALOR_JUROS");
                entity.Property(e => e.ValorPrestacao).HasColumnName("VALOR_PRESTACAO");
            });
        }
    }
}
