using DeliveryApp.Aplicacao.Modulos.Clientes.Util;
using DeliveryApp.Dominio.Compartilhado;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Clientes;

public sealed record EnderecoClienteDto(Guid Id, string Endereco);

public sealed record CadastrarEnderecoClienteCommand(
    Guid ClienteId,
    string Endereco
) : IRequest<Result<Guid>>;

public sealed class CadastrarEnderecoClienteCommandHandler(
    IRepositorioEnderecoCliente repositorioEndereco,
    IProvedorDeUsuario provedorDeUsuario
) : IRequestHandler<CadastrarEnderecoClienteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CadastrarEnderecoClienteCommand command,
        CancellationToken cancellationToken
    )
    {
        if (provedorDeUsuario.Id != command.ClienteId)
            return Result.Fail<Guid>(ErrosDeCliente.NaoAutorizado(command.ClienteId));

        var endereco = new EnderecoCliente(
            Guid.CreateVersion7(),
            command.ClienteId,
            command.Endereco
        );

        var erros = endereco.Validar();

        if (erros.Count > 0)
            return Result.Fail<Guid>(ErrosDeCliente.Validacao(erros));

        try
        {
            await repositorioEndereco.CadastrarAsync(endereco, cancellationToken);
        }
        catch (ConflitoDePersistenciaException)
        {
            return Result.Fail<Guid>(ErrosDeCliente.EnderecoDuplicado());
        }

        return Result.Ok(endereco.Id);
    }
}

public sealed record ListarEnderecosClienteQuery(Guid ClienteId)
    : IRequest<Result<IReadOnlyList<EnderecoClienteDto>>>;

public sealed class ListarEnderecosClienteQueryHandler(
    IRepositorioEnderecoCliente repositorioEndereco,
    IProvedorDeUsuario provedorDeUsuario
) : IRequestHandler<ListarEnderecosClienteQuery, Result<IReadOnlyList<EnderecoClienteDto>>>
{
    public async Task<Result<IReadOnlyList<EnderecoClienteDto>>> Handle(
        ListarEnderecosClienteQuery query,
        CancellationToken cancellationToken
    )
    {
        if (provedorDeUsuario.Id != query.ClienteId)
            return Result.Fail<IReadOnlyList<EnderecoClienteDto>>(
                ErrosDeCliente.NaoAutorizado(query.ClienteId)
            );

        var enderecos = await repositorioEndereco.ListarDoClienteAsync(
            query.ClienteId,
            cancellationToken
        );

        return Result.Ok<IReadOnlyList<EnderecoClienteDto>>(
            enderecos.Select(e => new EnderecoClienteDto(e.Id, e.Endereco)).ToList()
        );
    }
}
