namespace DeliveryApp.WebApi.Modulos.Clientes;

public sealed record CadastrarClienteRequest(
    string Nome,
    string Cpf,
    string Email,
    string Senha
);

public sealed record CadastrarClienteResponse(
    Guid Id,
    string Nome
);

public sealed record AutenticarClienteRequest(string Email, string Senha);

public sealed record AutenticarClienteResponse(
    Guid ClienteId,
    string AccessToken,
    DateTime DataExpiracaoEmUtc
);

public sealed record ClienteResponse(Guid Id, string Nome, string Cpf, string Email);

public sealed record EditarClienteRequest(string Nome, string Cpf);

public sealed record CadastrarEnderecoClienteRequest(string Endereco);

public sealed record CadastrarEnderecoClienteResponse(Guid EnderecoId);

public sealed record EnderecoClienteResponse(Guid Id, string Endereco);
