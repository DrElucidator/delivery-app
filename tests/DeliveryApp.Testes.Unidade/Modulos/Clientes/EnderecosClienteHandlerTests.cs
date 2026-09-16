using DeliveryApp.Aplicacao.Modulos.Clientes;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using FluentResults;
using Moq;

namespace DeliveryApp.Testes.Unidade.Modulos.Clientes;

[TestClass]
public sealed class EnderecosClienteHandlerTests
{
    [TestMethod]
    public async Task Deve_CadastrarEndereco_ParaClienteAutenticado()
    {
        Guid clienteId = Guid.CreateVersion7();
        Mock<IRepositorioEnderecoCliente> repositorioMock = new();
        Mock<IProvedorDeUsuario> provedorMock = new();
        provedorMock.SetupGet(p => p.Id).Returns(clienteId);
        CadastrarEnderecoClienteCommandHandler handler = new(
            repositorioMock.Object,
            provedorMock.Object
        );

        Result<Guid> resultado = await handler.Handle(
            new CadastrarEnderecoClienteCommand(clienteId, "Rua das Flores, 100"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsSuccess);
        repositorioMock.Verify(r => r.CadastrarAsync(
            It.Is<EnderecoCliente>(e => e.ClienteId == clienteId),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task Deve_RejeitarCadastro_ParaOutroCliente()
    {
        Mock<IRepositorioEnderecoCliente> repositorioMock = new();
        Mock<IProvedorDeUsuario> provedorMock = new();
        provedorMock.SetupGet(p => p.Id).Returns(Guid.CreateVersion7());
        CadastrarEnderecoClienteCommandHandler handler = new(
            repositorioMock.Object,
            provedorMock.Object
        );

        Result<Guid> resultado = await handler.Handle(
            new CadastrarEnderecoClienteCommand(Guid.CreateVersion7(), "Rua das Flores, 100"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
        repositorioMock.Verify(r => r.CadastrarAsync(
            It.IsAny<EnderecoCliente>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }
}
