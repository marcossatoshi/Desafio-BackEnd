## Desafio Backend – Visão Técnica (Edição Satoshi)

Versão online em https://desafio-backend-app-4f5c452ef09b.herokuapp.com/swagger/index.html

Este documento apresenta a arquitetura e como executar localmente (Windows/Linux), como os testes estão organizados e quais padrões de projeto foram aplicados.

### Stack
- **.NET**: .NET 9, C#
- **API**: ASP.NET Core Minimal APIs, Swagger com Swashbuckle
- **Dados**: Entity Framework Core + PostgreSQL (Npgsql)
- **Mensageria**: MassTransit (In-memory se não subir com docker) ou RabbitMQ
- **Arquivos**: Disco local para imagens de CNH
- **Containers**: Docker Compose para Postgres e RabbitMQ
- **Testes**: xUnit, FluentAssertions, NSubstitute, Testcontainers (integração)

## Upload de CNH
- Antiforgery desabilitado neste endpoint para evitar erro em uploads multipart.

## Configuração

- Banco: `ConnectionStrings:Postgres` ou
- EF em memória (apenas testes/dev): `UseInMemoryEF=true`.
- MassTransit em memória (apenas testes/dev): `UseMassTransitInMemory=true`.
- RabbitMQ: `RabbitMq:HostName`, `RabbitMq:Port`, `RabbitMq:UserName`, `RabbitMq:Password`.

## Execução Local

### Windows (PowerShell)
Use `run.ps1`. Ele sobe Docker (Postgres + RabbitMQ), aplica migrações e inicia a API.

```powershell
cd D:\Projetos\Desafio-BackEnd
./run.ps1
```

Forçar modo em memória (sem Docker):
```powershell
./run.ps1 -InMemory
```

### Linux/macOS (Bash)
Use `run.sh` com comportamento equivalente ao script PowerShell.

```bash
cd /path/to/Desafio-BackEnd
chmod +x ./run.sh
./run.sh
```

Forçar modo em memória:
```bash
./run.sh --in-memory
```

### Swagger
Com a API rodando, acesse:
- `http://localhost:5000/swagger`

## Estratégia de Testes

### Testes Unitários
- xUnit + NSubstitute + FluentAssertions.
- Regras de negócio (ex.: precificação da locação para devolução antecipada/tardia).

### Testes Funcionais
- `WebApplicationFactory<Program>` com EF InMemory e MassTransit InMemory.
- Exercitam endpoints sem dependências externas reais.

### Testes de Integração
- Testcontainers sobe Postgres e RabbitMQ reais em Docker.
- `WebApplicationFactory<Program>` conecta via variáveis de ambiente.
- Migrações aplicadas no início dos testes.
- Cobrem:
  - Criação e leitura de moto por `identifier`.
  - Publicação/consumo de `MotorcycleCreatedEvent` e persistência de notificação.

Rodar todos os testes:
```powershell
dotnet test -c Debug
```
ou dotnet test -c Debug /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

### Aplicado: Strategy (Precificação de Locação)
- Interface: `IRentalPricingStrategy`
- Implementação: `DefaultRentalPricingStrategy`
- Utilizado por: `RentalService`
Motivação: isolar regras de preço (valor diário por plano e cálculo do total na devolução — normal/antecipada/tardia) para facilitar mudanças e variações sem alterar a orquestração do serviço.

## Tratamento de Erros & Status Codes
- Placa duplicada retorna `409 Conflict` ao criar/atualizar.

## Solução de Problemas
- Arquivo bloqueado no build: pare a API antes de aplicar migrações.
- Ainda em memória: abra um novo shell; os scripts limpam `UseInMemoryEF` e `UseMassTransitInMemory` em modo Docker.

## Possíveis features (mas que aumentariam bastante o tempo gasto):

#Mensageria
- Observabilidade
- Resiliência: retry/DLQ

#Armazenamento
- Utilizar um serviço adequado

#API
- Versionamento
- Tokenização para request seguros

#Geral
- Melhor observabilidade: metrics, logs mais estruturados e health checks
- Estruturar melhor as pastas
- Criptografar dados sensíveis de acesso
- ~CI~

#Produto
- ~Evitar remoção da moto/entregador se tiver alugada~