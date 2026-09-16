using DeliveryApp.Dominio.Modulos.Clientes;
using DeliveryApp.Dominio.Compartilhado;
using DeliveryApp.Infraestrutura.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infraestrutura.Modulos.Clientes;

public sealed class RepositorioEnderecoClienteEmOrm(DeliveryAppDbContext dbContext)
    : IRepositorioEnderecoCliente
{
    public async Task CadastrarAsync(
        EnderecoCliente endereco,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.EnderecosCliente.Add(endereco);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            dbContext.ChangeTracker.Clear();
            throw new ConflitoDePersistenciaException(
                "Ocorreu um erro ao persistir o endereço.",
                ex
            );
        }
    }

    public async Task<IReadOnlyList<EnderecoCliente>> ListarDoClienteAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.EnderecosCliente
            .Where(e => e.ClienteId == clienteId)
            .OrderBy(e => e.Endereco)
            .ToListAsync(cancellationToken);
    }

    public async Task<EnderecoCliente?> SelecionarDoClienteAsync(
        Guid enderecoId,
        Guid clienteId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.EnderecosCliente.SingleOrDefaultAsync(
            e => e.Id == enderecoId && e.ClienteId == clienteId,
            cancellationToken
        );
    }
}
