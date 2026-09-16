using DeliveryApp.Dominio.Modulos.Clientes;

namespace DeliveryApp.Testes.Unidade.Modulos.Clientes;

[TestClass]
public sealed class EnderecoClienteTests
{
    [TestMethod]
    public void Deve_CriarEndereco_ComDadosValidos()
    {
        Guid clienteId = Guid.CreateVersion7();
        EnderecoCliente endereco = new(
            Guid.CreateVersion7(),
            clienteId,
            "  Rua das Flores, 100  "
        );

        Assert.AreEqual(clienteId, endereco.ClienteId);
        Assert.AreEqual("Rua das Flores, 100", endereco.Endereco);
        Assert.IsEmpty(endereco.Validar());
    }

    [TestMethod]
    public void Deve_RejeitarEndereco_SemCliente()
    {
        EnderecoCliente endereco = new(Guid.CreateVersion7(), Guid.Empty, "Rua das Flores, 100");

        Assert.IsTrue(endereco.Validar().Any(e => e.Campo == nameof(EnderecoCliente.ClienteId)));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Rua")]
    public void Deve_RejeitarEndereco_ComDescricaoInvalida(string descricao)
    {
        EnderecoCliente endereco = new(Guid.CreateVersion7(), Guid.CreateVersion7(), descricao);

        Assert.IsTrue(endereco.Validar().Any(e => e.Campo == nameof(EnderecoCliente.Endereco)));
    }
}
