using DeliveryApp.Dominio.Compartilhado;

namespace DeliveryApp.Testes.Unidade.Compartilhado;

[TestClass]
public sealed class ValidadorDocumentoTests
{
    [TestMethod]
    [DataRow("52998224725")]
    [DataRow("529.982.247-25")]
    public void Deve_ValidarCpf_Correto(string cpf)
    {
        Assert.IsTrue(ValidadorDocumento.CpfValido(cpf));
    }

    [TestMethod]
    [DataRow("52998224724")]
    [DataRow("11111111111")]
    [DataRow("123")]
    public void Deve_RejeitarCpf_Invalido(string cpf)
    {
        Assert.IsFalse(ValidadorDocumento.CpfValido(cpf));
    }

    [TestMethod]
    [DataRow("11222333000181")]
    [DataRow("11.222.333/0001-81")]
    public void Deve_ValidarCnpj_Correto(string cnpj)
    {
        Assert.IsTrue(ValidadorDocumento.CnpjValido(cnpj));
    }

    [TestMethod]
    [DataRow("11222333000180")]
    [DataRow("00000000000000")]
    [DataRow("123")]
    public void Deve_RejeitarCnpj_Invalido(string cnpj)
    {
        Assert.IsFalse(ValidadorDocumento.CnpjValido(cnpj));
    }
}
