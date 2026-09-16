using DeliveryApp.Aplicacao.Modulos.Pedidos;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using FluentResults;
using MassTransit;
using Moq;

namespace DeliveryApp.Testes.Unidade.Modulos.Pedidos;

[TestClass]
public sealed class CriarPedidoCommandHandlerTests
{
    [TestMethod]
    public async Task Deve_RejeitarEndereco_QueNaoPertenceAoClienteAutenticado()
    {
        Guid clienteId = Guid.CreateVersion7();
        Guid enderecoId = Guid.CreateVersion7();
        Mock<IProvedorDeUsuario> provedorMock = new();
        Mock<IRepositorioEnderecoCliente> repositorioEnderecoMock = new();
        Mock<IPublishEndpoint> publishEndpointMock = new();
        provedorMock.SetupGet(p => p.Id).Returns(clienteId);
        provedorMock.Setup(p => p.PossuiTipo(TipoUsuario.Cliente)).Returns(true);
        repositorioEnderecoMock
            .Setup(r => r.SelecionarDoClienteAsync(
                enderecoId,
                clienteId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((EnderecoCliente?)null);
        CriarPedidoCommandHandler handler = new(
            provedorMock.Object,
            repositorioEnderecoMock.Object,
            publishEndpointMock.Object
        );

        Result<Guid> resultado = await handler.Handle(
            new CriarPedidoCommand(
                Guid.CreateVersion7(),
                enderecoId,
                [new ItemCriarPedidoCommand(Guid.CreateVersion7(), 1, null, [])]
            ),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
        publishEndpointMock.VerifyNoOtherCalls();
    }
}
