using DeliveryApp.Dominio.Modulos.Estabelecimentos;

namespace DeliveryApp.Testes.Unidade.Modulos.Estabelecimentos;

[TestClass]
public sealed class EstabelecimentoTests
{
    [TestMethod]
    public void Deve_AceitarCnpjValido()
    {
        Estabelecimento estabelecimento = CriarEstabelecimento("11.222.333/0001-81");

        Assert.IsFalse(estabelecimento.Validar().Any(e => e.Campo == nameof(Estabelecimento.Documento)));
    }

    [TestMethod]
    public void Deve_RejeitarCnpjInvalido()
    {
        Estabelecimento estabelecimento = CriarEstabelecimento("11.222.333/0001-80");

        Assert.IsTrue(estabelecimento.Validar().Any(e => e.Campo == nameof(Estabelecimento.Documento)));
    }

    [TestMethod]
    public void Deve_EstarDisponivel_DuranteHorarioDeAtendimento()
    {
        Estabelecimento estabelecimento = CriarEstabelecimento("11222333000181");

        Assert.IsTrue(estabelecimento.EstaDisponivel(new TimeOnly(12, 0)));
    }

    [TestMethod]
    public void Deve_EstarIndisponivel_ForaDoHorarioDeAtendimento()
    {
        Estabelecimento estabelecimento = CriarEstabelecimento("11222333000181");

        Assert.IsFalse(estabelecimento.EstaDisponivel(new TimeOnly(22, 0)));
    }

    [TestMethod]
    public void Deve_EstarIndisponivel_QuandoDesativado()
    {
        Estabelecimento estabelecimento = CriarEstabelecimento("11222333000181");
        estabelecimento.Desativar();

        Assert.IsFalse(estabelecimento.EstaDisponivel(new TimeOnly(12, 0)));
    }

    private static Estabelecimento CriarEstabelecimento(string documento)
    {
        return new Estabelecimento(
            Guid.CreateVersion7(),
            "Google",
            documento,
            "Rua das Flores, 100",
            "48999999999",
            "Centro",
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            5m
        );
    }
}
