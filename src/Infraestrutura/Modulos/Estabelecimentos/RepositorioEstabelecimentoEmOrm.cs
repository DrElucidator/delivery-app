using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using DeliveryApp.Infraestrutura.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infraestrutura.Modulos.Estabelecimentos;

public sealed class RepositorioEstabelecimentoEmOrm(
    DeliveryAppDbContext dbContext
) : RepositorioBaseEmOrm<Estabelecimento>(dbContext), IRepositorioEstabelecimento
{
    public Task<List<Estabelecimento>> SelecionarDisponiveisAsync(
        CancellationToken cancellationToken = default
    )
    {
        TimeOnly horaAtual = TimeOnly.FromDateTime(DateTime.UtcNow);

        return registros
            .Where(estabelecimento => estabelecimento.Ativo &&
                (estabelecimento.HorarioAbertura < estabelecimento.HorarioFechamento
                    ? horaAtual >= estabelecimento.HorarioAbertura && horaAtual < estabelecimento.HorarioFechamento
                    : horaAtual >= estabelecimento.HorarioAbertura || horaAtual < estabelecimento.HorarioFechamento))
            .OrderBy(estabelecimento => estabelecimento.NomeComercial)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AlterarAtivoAsync(
        Guid estabelecimentoId,
        bool ativo,
        CancellationToken cancellationToken = default
    )
    {
        Estabelecimento? estabelecimento = await SelecionarPorIdAsync(
            estabelecimentoId,
            cancellationToken
        );

        if (estabelecimento is null)
            return false;

        if (ativo)
            estabelecimento.Ativar();
        else
            estabelecimento.Desativar();

        await SalvarAlteracoesAsync(cancellationToken);

        return true;
    }

    public async Task<Estabelecimento?> SelecionarParaPedidoAsync(
        Guid estabelecimentoId,
        CancellationToken cancellationToken
    )
    {
        return await registros.SingleOrDefaultAsync(
            e => e.Id == estabelecimentoId,
            cancellationToken
        );
    }
}
