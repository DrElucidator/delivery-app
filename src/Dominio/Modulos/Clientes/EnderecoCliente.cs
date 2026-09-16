using DeliveryApp.Dominio.Compartilhado;

namespace DeliveryApp.Dominio.Modulos.Clientes;

public sealed class EnderecoCliente : EntidadeBase<EnderecoCliente>
{
    public const int TamanhoMinimo = 5;
    public const int TamanhoMaximo = 500;

    public Guid ClienteId { get; private set; }
    public string Endereco { get; private set; } = string.Empty;

    private EnderecoCliente() { }

    public EnderecoCliente(Guid id, Guid clienteId, string endereco)
    {
        Id = id;
        ClienteId = clienteId;
        Endereco = endereco.Trim();
    }

    public override IReadOnlyList<ErroValidacao> Validar()
    {
        List<ErroValidacao> erros = [];

        if (ClienteId == Guid.Empty)
            erros.Add(new(nameof(ClienteId), "O cliente é obrigatório."));

        if (Endereco.Length is < TamanhoMinimo or > TamanhoMaximo)
            erros.Add(new(nameof(Endereco), $"O endereço deve possuir entre {TamanhoMinimo} e {TamanhoMaximo} caracteres."));

        return erros;
    }

    public override void Atualizar(EnderecoCliente entidadeAtualizada)
    {
        Endereco = entidadeAtualizada.Endereco.Trim();
    }
}
