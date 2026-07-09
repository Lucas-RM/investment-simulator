# Investment Simulator API

Backend do Simulador de Investimentos em **C# .NET 10**, organizado em camadas.

## Estrutura da solução

```
investment-simulator-api/
├── src/
│   ├── InvestmentSimulator.Domain/          # Entidades, value objects e regras de domínio
│   ├── InvestmentSimulator.Application/     # Casos de uso e serviços de aplicação
│   ├── InvestmentSimulator.Infrastructure/    # Implementações externas (exportação, persistência)
│   └── InvestmentSimulator.Api/               # Endpoints HTTP (ASP.NET Core)
└── tests/
    └── InvestmentSimulator.Domain.Tests/      # Testes unitários do domínio
```

## Camadas e dependências

| Camada            | Responsabilidade                         | Depende de        |
| ----------------- | ---------------------------------------- | ----------------- |
| Domain            | Modelo de negócio e precisão matemática  | —                 |
| Application       | Orquestração dos casos de uso            | Domain            |
| Infrastructure    | Detalhes técnicos (I/O, persistência)    | Application       |
| Api               | Exposição REST                           | Application, Infrastructure |

## Precisão monetária (ERS §28)

- Valores monetários **sempre** em `decimal` — nunca `float` ou `double`.
- Cálculos intermediários com pelo menos **8 casas decimais** (`MonetaryPrecision.IntermediateDecimalPlaces`).
- Arredondamento apenas na apresentação: **2 casas** para moeda, **4** para percentuais.

As constantes estão centralizadas em `InvestmentSimulator.Domain.Common.MonetaryPrecision`.

## Convenções

- Código-fonte em **inglês**; documentação em **português**.
- `.editorconfig` e `Directory.Build.props` na raiz da API definem estilo e análise estática.
- `TreatWarningsAsErrors` habilitado em todos os projetos.

## Comandos úteis

```bash
# Restaurar e compilar
dotnet build

# Executar a API
dotnet run --project src/InvestmentSimulator.Api

# Executar testes
dotnet test
```
