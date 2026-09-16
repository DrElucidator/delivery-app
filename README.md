# DeliveryApp

Desenvolvido durante o curso Fullstack da [Academia do Programador 2026](https://www.academiadoprogramador.net).

API REST em .NET 10 para gerenciamento de clientes, estabelecimentos, cardápios e pedidos de uma plataforma de entregas. A aplicação utiliza ASP.NET Core Identity, autenticação JWT, persistência em PostgreSQL e processamento assíncrono de pedidos com RabbitMQ.

## Funcionalidades

- cadastro e autenticação de clientes e estabelecimentos;
- autorização por perfil e pelo vínculo do usuário autenticado com o recurso;
- gerenciamento de estabelecimentos, categorias, produtos e complementos;
- consulta pública do cardápio de estabelecimentos ativos;
- criação de pedidos com preservação dos nomes e preços praticados no momento da compra;
- consulta e listagem de pedidos conforme o perfil autenticado;
- alteração controlada do status do pedido, com registro do histórico de transições;
- processamento assíncrono e idempotente da criação e da atualização de pedidos;
- persistência em PostgreSQL por meio do Entity Framework Core;
- documentação interativa dos endpoints pelo Swagger.

## Referência funcional

### Entidade `Cliente`

| Propriedade | Descrição                                                                    |
| ----------- | ---------------------------------------------------------------------------- |
| `Id`        | Chave primária compartilhada e chave estrangeira do usuário na relação 1:1.  |
| `Nome`      | Nome do cliente, com 2 a 100 caracteres.                                     |
| `Cpf`       | Documento único do cliente, composto por exatamente 11 dígitos.              |

### Cadastro de clientes

- cria o cliente e o usuário do Identity com o mesmo identificador (`Guid` versão 7);
- exige nome com 2 a 100 caracteres;
- exige CPF com exatamente 11 dígitos;
- impede a duplicidade de CPF por validação e índice único no banco;
- exige email único;
- exige senha com pelo menos 8 caracteres, um dígito e um caractere não alfanumérico;
- associa o usuário ao papel `Cliente`;
- retorna um token JWT após o cadastro.

### Autenticação

- autentica o cliente por email e senha;
- bloqueia a conta por 5 minutos após 5 tentativas malsucedidas;
- retorna uma mensagem genérica quando as credenciais são inválidas;
- emite um JWT assinado com HMAC SHA-256 contendo o identificador, o email e o papel do usuário;
- utiliza validade configurável, de 60 minutos por padrão, e tolerância de 30 segundos na validação.

### Endpoints

| Método | Rota                        | Acesso  | Descrição                         |
| ------ | --------------------------- | ------- | --------------------------------- |
| `POST` | `/api/clientes/cadastro`    | Público | Cadastra e autentica um cliente.  |
| `POST` | `/api/clientes/login`       | Público | Autentica um cliente.             |
| `GET`  | `/api/clientes/{clienteId}` | Cliente | Consulta um cliente pelo seu ID.  |
| `PUT`  | `/api/clientes/{clienteId}` | Cliente | Edita os dados do próprio cliente. |
| `POST` | `/api/clientes/{clienteId}/enderecos` | Cliente | Cadastra um endereço de entrega. |
| `GET`  | `/api/clientes/{clienteId}/enderecos` | Cliente | Lista os endereços do próprio cliente. |

Os demais endpoints ficam protegidos por uma política global que exige autenticação. Rotas públicas precisam ser marcadas explicitamente com `AllowAnonymous`.

Exemplo de cadastro:

```json
{
  "nome": "Cliente Exemplo",
  "cpf": "12345678901",
  "email": "cliente@example.com",
  "senha": "senha@123"
}
```

O cadastro responde com `201 Created`; o login, com `200 OK`. Ambos retornam o mesmo formato:

```json
{
  "clienteId": "01900000-0000-7000-8000-000000000000",
  "accessToken": "token-jwt",
  "dataExpiracaoEmUtc": "2026-09-01T13:00:00Z"
}
```

Erros HTTP seguem o formato Problem Details e incluem o `traceId` quando tratados pelo pipeline global.

### Entidade `Estabelecimento`

| Propriedade         | Descrição                                                                            |
| ------------------- | ------------------------------------------------------------------------------------ |
| `Id`                | Chave primária compartilhada e chave estrangeira do usuário na relação 1:1.          |
| `NomeComercial`     | Nome utilizado comercialmente.                                                       |
| `Documento`         | CPF ou CNPJ do estabelecimento.                                                       |
| `Endereco`          | Endereço do estabelecimento.                                                          |
| `Telefone`          | Telefone para contato.                                                                |
| `HorarioAbertura`   | Início do período diário de atendimento.                                              |
| `HorarioFechamento` | Final do período diário de atendimento.                                               |
| `AreaAtendimento`   | Descrição das regiões atendidas.                                                      |
| `Ativo`             | Indica se o estabelecimento está disponível para receber novos pedidos.               |
| `TaxaEntrega`       | Valor cobrado pelo estabelecimento para realizar a entrega.                           |

### Endpoints de estabelecimentos

| Método | Rota                             | Acesso  | Descrição                                |
| ------ | -------------------------------- | ------- | ---------------------------------------- |
| `POST` | `/api/estabelecimentos/cadastro` | Público | Cadastra e autentica um estabelecimento. |
| `POST` | `/api/estabelecimentos/login`    | Público | Autentica um estabelecimento.            |
| `GET`  | `/api/estabelecimentos/disponiveis` | Cliente ou Estabelecimento | Lista somente estabelecimentos ativos. |
| `GET`  | `/api/estabelecimentos/{estabelecimentoId}` | Cliente ou Estabelecimento | Consulta um estabelecimento por ID. |
| `PUT`  | `/api/estabelecimentos/{estabelecimentoId}` | Estabelecimento vinculado | Edita os dados do estabelecimento. |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/ativar` | Estabelecimento vinculado | Ativa o estabelecimento. |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/desativar` | Estabelecimento vinculado | Desativa o estabelecimento. |

Não existe uma rota `GET /api/estabelecimentos` genérica; a listagem disponível é feita exclusivamente por `/disponiveis`.

As operações de edição, ativação e desativação somente podem ser executadas pelo usuário autenticado vinculado ao estabelecimento informado na rota.

### Módulo de cardápio

O cardápio é composto por categorias, produtos e complementos. Imagens ainda não fazem parte do módulo.

#### Entidade `Categoria`

| Propriedade         | Descrição                                  |
| ------------------- | ------------------------------------------ |
| `Id`                | Identificador da categoria.                |
| `EstabelecimentoId` | Estabelecimento proprietário da categoria. |
| `Nome`              | Nome entre 2 e 100 caracteres.             |

#### Entidade `Produto`

| Propriedade         | Descrição                                          |
| ------------------- | -------------------------------------------------- |
| `Id`                | Identificador do produto.                          |
| `EstabelecimentoId` | Estabelecimento proprietário do produto.           |
| `CategoriaId`       | Categoria do produto.                              |
| `Nome`              | Nome entre 2 e 100 caracteres.                     |
| `Descricao`         | Descrição obrigatória, com até 1000 caracteres.    |
| `Preco`             | Preço maior que zero, com até duas casas decimais. |
| `Ativo`             | Define se aparece no cardápio disponível.          |

#### Entidade `Complemento`

| Propriedade        | Descrição                                |
| ------------------ | ---------------------------------------- |
| `Id`               | Identificador do complemento.            |
| `ProdutoId`        | Produto ao qual o complemento pertence. |
| `Nome`             | Nome entre 2 e 100 caracteres.           |
| `PrecoAdicional`   | Valor maior ou igual a zero.             |

Complementos são cadastrados e editados junto com o produto. Não existem grupos ou regras de quantidade nesta versão.

#### Endpoints de categorias

| Método | Rota                                                                       | Acesso                    | Descrição          |
| ------ | -------------------------------------------------------------------------- | ------------------------- | ------------------ |
| `POST` | `/api/estabelecimentos/{estabelecimentoId}/categorias`                    | Estabelecimento vinculado | Cadastra categoria. |
| `GET`  | `/api/estabelecimentos/{estabelecimentoId}/categorias`                    | Estabelecimento vinculado | Lista categorias.   |
| `PUT`  | `/api/estabelecimentos/{estabelecimentoId}/categorias/{categoriaId}`      | Estabelecimento vinculado | Edita categoria.    |

#### Endpoints de produtos

| Método  | Rota                                                                          | Acesso                    | Descrição                         |
| ------- | ----------------------------------------------------------------------------- | ------------------------- | --------------------------------- |
| `POST`  | `/api/estabelecimentos/{estabelecimentoId}/produtos`                        | Estabelecimento vinculado | Cadastra produto.                 |
| `GET`   | `/api/estabelecimentos/{estabelecimentoId}/produtos`                        | Estabelecimento vinculado | Lista produtos ativos e inativos. |
| `PUT`   | `/api/estabelecimentos/{estabelecimentoId}/produtos/{produtoId}`             | Estabelecimento vinculado | Edita produto e complementos.     |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/produtos/{produtoId}/ativar`      | Estabelecimento vinculado | Ativa produto.                    |
| `PATCH` | `/api/estabelecimentos/{estabelecimentoId}/produtos/{produtoId}/desativar`   | Estabelecimento vinculado | Desativa produto.                 |

#### Consulta do cardápio vigente

| Método | Rota                                                        | Acesso  | Descrição                                         |
| ------ | ----------------------------------------------------------- | ------- | ------------------------------------------------- |
| `GET`  | `/api/estabelecimentos/{estabelecimentoId}/cardapio`        | Público | Consulta o cardápio agrupado por categoria.       |

O cardápio público só pode ser consultado quando o estabelecimento está ativo. Produtos inativos não são retornados. O endpoint responde `404 Not Found` quando o estabelecimento não existe ou está inativo.

Somente o usuário autenticado do estabelecimento vinculado pode criar, editar, ativar ou desativar categorias e produtos. O vínculo é validado pela role `Estabelecimento` e pelo identificador do usuário autenticado.

### Módulo de pedidos

O pedido pertence a um cliente e a um estabelecimento. Ele armazena o endereço de entrega, os itens escolhidos, a taxa de entrega, os valores calculados e o histórico das alterações de status.

O endereço utilizado no pedido precisa estar previamente cadastrado e pertencer ao cliente autenticado. A criação recebe o identificador do endereço selecionado e preserva sua descrição no pedido.

Os nomes e preços dos produtos e complementos são copiados para o pedido no momento de sua criação. Dessa forma, alterações posteriores no cardápio não modificam o histórico do pedido.

#### Fluxo de status

As alterações possíveis dependem do status atual e do perfil autenticado:

| Ação                 | Perfil permitido  | Transição                               |
| -------------------- | ----------------- | --------------------------------------- |
| Aceitar pedido       | Estabelecimento   | `AguardandoAceite` para `EmPreparo`      |
| Recusar pedido       | Estabelecimento   | `AguardandoAceite` para `Recusado`       |
| Iniciar entrega      | Estabelecimento   | `EmPreparo` para `EmEntrega`             |
| Concluir entrega     | Estabelecimento   | `EmEntrega` para `Concluido`             |
| Cancelar pedido      | Cliente           | `AguardandoAceite` para `Cancelado`      |

Cada alteração válida gera uma entrada no histórico do pedido, identificando o usuário, o perfil, os estados anterior e atual, a data e o motivo quando informado.

#### Endpoints de pedidos

| Método  | Rota                                  | Acesso                  | Descrição                              |
| ------- | ------------------------------------- | ----------------------- | -------------------------------------- |
| `POST`  | `/api/pedidos`                        | Cliente                 | Solicita a criação de um pedido.       |
| `GET`   | `/api/pedidos`                        | Cliente ou Estabelecimento | Lista os pedidos vinculados ao usuário. |
| `GET`   | `/api/pedidos/{pedidoId}`             | Usuário vinculado       | Consulta os detalhes de um pedido.     |
| `PATCH` | `/api/pedidos/{pedidoId}/aceite`      | Estabelecimento vinculado | Aceita o pedido.                     |
| `PATCH` | `/api/pedidos/{pedidoId}/recusa`      | Estabelecimento vinculado | Recusa o pedido.                     |
| `PATCH` | `/api/pedidos/{pedidoId}/cancelamento` | Cliente vinculado       | Cancela o pedido.                      |
| `PATCH` | `/api/pedidos/{pedidoId}/inicio-entrega` | Estabelecimento vinculado | Inicia a entrega.                   |
| `PATCH` | `/api/pedidos/{pedidoId}/conclusao`   | Estabelecimento vinculado | Conclui a entrega.                   |

#### Processamento assíncrono

A criação e a alteração do status dos pedidos são publicadas no RabbitMQ por meio do MassTransit. Os consumidores processam as mensagens nas filas `pedidos-criados` e `pedidos-atualizados`.

O processamento verifica novamente o usuário, o estabelecimento, os produtos, os complementos e as regras de transição antes de persistir a operação. As mensagens podem ser repetidas sem duplicar a criação ou a alteração já processada, e falhas transitórias utilizam tentativas automáticas de reprocessamento.

#### Carrinho e ocorrências

O carrinho é um estado temporário da aplicação cliente. Inclusões, remoções e alterações de quantidade ocorrem antes da confirmação, e a API recebe somente a composição final do pedido. Não existe uma entidade de carrinho persistida no servidor nesta versão.

Recusas e cancelamentos aceitam um motivo e são registrados no histórico auditável do pedido. Esses registros representam as ocorrências previstas no fluxo principal, sem a necessidade de uma entidade independente.

## Arquitetura

A solução está dividida em quatro projetos:

| Projeto                      | Responsabilidade                                                          |
| ---------------------------- | ------------------------------------------------------------------------- |
| `DeliveryApp.Dominio`        | Entidades, contratos compartilhados e validações de domínio.              |
| `DeliveryApp.Aplicacao`      | Casos de uso, consultas, comandos, mensageria e tipos de resultado.        |
| `DeliveryApp.Infraestrutura` | EF Core, ASP.NET Core Identity, migrations e acesso ao PostgreSQL.         |
| `DeliveryApp.WebApi`         | Controllers, autenticação JWT, Problem Details, OpenAPI e observabilidade. |

O `DeliveryAppDbContext` herda de `IdentityDbContext` e mantém os dados de identidade e de domínio no mesmo banco. As entidades de perfil seguem o padrão de chave primária compartilhada com o Identity:

| Entidade          | Modelagem da identidade                                                               |
| ----------------- | ------------------------------------------------------------------------------------- |
| `Cliente`         | O `Id` é a chave primária e também a chave estrangeira do usuário, em uma relação 1:1. |
| `Estabelecimento` | O `Id` é a chave primária e também a chave estrangeira do usuário, em uma relação 1:1. |

Assim, `Estabelecimento` não possui um `UsuarioId` separado: seu próprio `Id` identifica tanto o perfil de domínio quanto o usuário correspondente.

O módulo de cardápio utiliza as tabelas `TBCategorias`, `TBProdutos` e `TBComplementos`. Produtos e categorias são vinculados ao estabelecimento por chave estrangeira, e produtos são vinculados à categoria do mesmo estabelecimento por uma chave estrangeira composta.

## Tecnologias

- .NET 10 e ASP.NET Core Web API;
- ASP.NET Core Identity;
- autenticação JWT Bearer;
- Entity Framework Core 10;
- PostgreSQL com Npgsql;
- MediatR para comandos e consultas;
- MassTransit com RabbitMQ para mensageria;
- FluentResults;
- Serilog com saídas para console e arquivo;
- Swagger/OpenAPI.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0);
- PostgreSQL;
- RabbitMQ;
- [EF Core CLI](https://learn.microsoft.com/ef/core/cli/dotnet), somente para gerenciar migrations manualmente.

Para instalar a CLI do EF Core:

```bash
dotnet tool install --global dotnet-ef
```

## Configuração

Em desenvolvimento, o projeto já define em `src/Api/appsettings.Development.json` a conexão local:

```text
Host=localhost;Port=5432;Database=DeliveryAppDb;Username=postgres;Password=postgres
```

Altere-a conforme o seu ambiente ou sobrescreva-a com o Secret Manager:

```bash
dotnet user-secrets set "ConnectionStrings:PostgresEF" "Host=localhost;Port=5432;Database=DeliveryAppDb;Username=postgres;Password=sua-senha" --project src/Api
```

A conexão local padrão do RabbitMQ também é definida em `src/Api/appsettings.Development.json`:

```text
amqp://guest:guest@localhost:5672
```

Ela pode ser sobrescrita sem alterar arquivos versionados:

```bash
dotnet user-secrets set "ConnectionStrings:RabbitMq" "amqp://usuario:senha@localhost:5672" --project src/Api
```

A chave de assinatura do JWT não é armazenada no repositório e precisa ser configurada:

```bash
dotnet user-secrets set "Jwt:Key" "informe-uma-chave-segura-com-pelo-menos-32-caracteres" --project src/Api
```

As demais opções do JWT ficam em `src/Api/appsettings.json`:

| Chave                    | Valor padrão          |
| ------------------------ | --------------------- |
| `Jwt:Issuer`             | `delivery-app-api`    |
| `Jwt:Audience`           | `delivery-app-client` |
| `Jwt:AccessTokenMinutes` | `60`                  |

## Execução

Na raiz da solução, execute:

```bash
dotnet restore DeliveryApp.slnx
dotnet run --project src/Api
```

No ambiente `Development`, as migrations são aplicadas automaticamente na inicialização. A API fica disponível em:

- `https://localhost:7094`;
- `http://localhost:5033`;
- Swagger UI em `/swagger`.

O PostgreSQL e o RabbitMQ precisam estar acessíveis antes da inicialização. Quando o RabbitMQ possui o plugin de gerenciamento habilitado, seu painel administrativo fica disponível por padrão em `http://localhost:15672`.

Para atualizar o banco manualmente:

```bash
dotnet ef database update --project src/Infraestrutura --startup-project src/Api
```

## Logs

O Serilog registra eventos no console e grava erros em arquivos diários. Os arquivos ficam em `DeliveryApp/erro*.log` dentro do diretório local de dados da aplicação (`LocalApplicationData`).

## Verificação

Para validar a compilação da solução:

```bash
dotnet build DeliveryApp.slnx
```

Para executar os testes automatizados:

```bash
dotnet test DeliveryApp.slnx
```
