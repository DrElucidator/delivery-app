using DeliveryApp.Dominio.Compartilhado;

namespace DeliveryApp.Dominio.Modulos.Clientes;

public sealed class Cliente : EntidadeBase<Cliente>
{
    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;

    private Cliente() { }

    public Cliente(Guid id, string nome, string cpf)
    {
        Id = id;
        Nome = nome.Trim();
        Cpf = ValidadorDocumento.Normalizar(cpf);
    }

    public override IReadOnlyList<ErroValidacao> Validar()
    {
        List<ErroValidacao> erros = [];

        if (Nome.Length is < 2 or > 100)
        {
            erros.Add(new ErroValidacao(
                nameof(Nome),
                "O nome deve possuir entre 2 e 100 caracteres."
            ));
        }

        if (!ValidadorDocumento.CpfValido(Cpf))
        {
            erros.Add(new ErroValidacao(
                nameof(Cpf),
                "O CPF informado é inválido."
            ));
        }

        return erros;
    }

    public override void Atualizar(Cliente entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome.Trim();
        Cpf = ValidadorDocumento.Normalizar(entidadeAtualizada.Cpf);
    }
}
