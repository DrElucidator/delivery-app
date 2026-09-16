namespace DeliveryApp.Dominio.Modulos.Clientes;

public interface IRepositorioEnderecoCliente
{
    Task CadastrarAsync(
        EnderecoCliente endereco,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<EnderecoCliente>> ListarDoClienteAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default
    );

    Task<EnderecoCliente?> SelecionarDoClienteAsync(
        Guid enderecoId,
        Guid clienteId,
        CancellationToken cancellationToken = default
    );
}
