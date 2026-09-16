using DeliveryApp.Aplicacao.Modulos.Clientes;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using FluentResults;
using Moq;

namespace DeliveryApp.Testes.Unidade.Modulos.Clientes;

[TestClass]
public sealed class EditarClienteCommandHandlerTests
{
    [TestMethod]
    public async Task Deve_EditarCliente_ComDadosValidos()
    {
        Guid clienteId = Guid.CreateVersion7();
        Mock<IRepositorioCliente> repositorioMock = new();
        Mock<IProvedorDeUsuario> provedorMock = new();
        provedorMock.SetupGet(p => p.Id).Returns(clienteId);
        repositorioMock
            .Setup(r => r.ExisteRegistroComCpfAsync("11144477735", clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repositorioMock
            .Setup(r => r.EditarAsync(clienteId, It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        EditarClienteCommandHandler handler = new(repositorioMock.Object, provedorMock.Object);

        Result resultado = await handler.Handle(
            new EditarClienteCommand(clienteId, "Ana Souza", "11144477735"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsSuccess);
        repositorioMock.Verify(r => r.EditarAsync(
            clienteId,
            It.Is<Cliente>(c => c.Nome == "Ana Souza" && c.Cpf == "11144477735"),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task Deve_RejeitarEdicao_DeOutroCliente()
    {
        Guid clienteId = Guid.CreateVersion7();
        Mock<IRepositorioCliente> repositorioMock = new();
        Mock<IProvedorDeUsuario> provedorMock = new();
        provedorMock.SetupGet(p => p.Id).Returns(Guid.CreateVersion7());
        EditarClienteCommandHandler handler = new(repositorioMock.Object, provedorMock.Object);

        Result resultado = await handler.Handle(
            new EditarClienteCommand(clienteId, "Ana Souza", "11144477735"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
        repositorioMock.Verify(r => r.EditarAsync(
            It.IsAny<Guid>(),
            It.IsAny<Cliente>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }
}
