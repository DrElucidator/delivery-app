using DeliveryApp.Aplicacao.Modulos.Clientes.Util;
using DeliveryApp.Dominio.Compartilhado;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Clientes;

public sealed record EditarClienteCommand(
    Guid ClienteId,
    string Nome,
    string Cpf
) : IRequest<Result>;

public sealed class EditarClienteCommandHandler(
    IRepositorioCliente repositorioCliente,
    IProvedorDeUsuario provedorDeUsuario
) : IRequestHandler<EditarClienteCommand, Result>
{
    public async Task<Result> Handle(
        EditarClienteCommand command,
        CancellationToken cancellationToken
    )
    {
        if (provedorDeUsuario.Id != command.ClienteId)
            return Result.Fail(ErrosDeCliente.NaoAutorizado(command.ClienteId));

        var clienteAtualizado = new Cliente(command.ClienteId, command.Nome, command.Cpf);
        var erros = clienteAtualizado.Validar();

        if (erros.Count > 0)
            return Result.Fail(ErrosDeCliente.Validacao(erros));

        if (await repositorioCliente.ExisteRegistroComCpfAsync(
            clienteAtualizado.Cpf,
            command.ClienteId,
            cancellationToken
        ))
        {
            return Result.Fail(ErrosDeCliente.CpfDuplicado());
        }

        try
        {
            bool editado = await repositorioCliente.EditarAsync(
                command.ClienteId,
                clienteAtualizado,
                cancellationToken
            );

            return editado
                ? Result.Ok()
                : Result.Fail(ErrosDeCliente.NaoEncontrado(command.ClienteId));
        }
        catch (ConflitoDePersistenciaException)
        {
            return Result.Fail(ErrosDeCliente.CpfDuplicado());
        }
    }
}
