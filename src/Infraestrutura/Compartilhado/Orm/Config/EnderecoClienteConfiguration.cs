using DeliveryApp.Dominio.Modulos.Clientes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infraestrutura.Compartilhado.Orm.Config;

public sealed class EnderecoClienteConfiguration : IEntityTypeConfiguration<EnderecoCliente>
{
    public void Configure(EntityTypeBuilder<EnderecoCliente> builder)
    {
        builder.ToTable("TBEnderecosCliente");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Endereco)
            .HasMaxLength(EnderecoCliente.TamanhoMaximo)
            .IsRequired();

        builder.HasIndex(e => new { e.ClienteId, e.Endereco }).IsUnique();

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
