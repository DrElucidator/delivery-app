using DeliveryApp.Dominio.Modulos.Clientes;

namespace DeliveryApp.Testes.Unidade.Modulos.Clientes;

[TestClass]
public sealed class ClienteTests
{
    [TestMethod]
    public void Deve_CriarCliente_ComDadosValidos()
    {
        Cliente cliente = new(Guid.CreateVersion7(), "Ana Silva", "529.982.247-25");

        Assert.AreEqual("Ana Silva", cliente.Nome);
        Assert.AreEqual("52998224725", cliente.Cpf);
        Assert.IsEmpty(cliente.Validar());
    }

    [TestMethod]
    public void Deve_RejeitarCliente_ComCpfInvalido()
    {
        Cliente cliente = new(Guid.CreateVersion7(), "Ana Silva", "12345678901");

        Assert.IsTrue(cliente.Validar().Any(e => e.Campo == nameof(Cliente.Cpf)));
    }

    [TestMethod]
    public void Deve_AtualizarCliente_ComDadosValidos()
    {
        Cliente cliente = new(Guid.CreateVersion7(), "Ana Silva", "52998224725");
        Cliente clienteAtualizado = new(cliente.Id, "Ana Souza", "11144477735");

        cliente.Atualizar(clienteAtualizado);

        Assert.AreEqual("Ana Souza", cliente.Nome);
        Assert.AreEqual("11144477735", cliente.Cpf);
    }
}
