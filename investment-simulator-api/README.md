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

## Validações de domínio (ERS §§5 e 27)

As regras são aplicadas nos construtores das entidades. Violações lançam `DomainValidationException`.

| Regra | Onde |
| ----- | ---- |
| Valor do aporte > 0 | `Contribution` |
| Data do aporte válida | `Contribution`, `Simulation` |
| Aporte entre data inicial e resgate | `Simulation` |
| Aportes em ordem cronológica | `Simulation` |
| Resgate ≥ aporte inicial | `Simulation` |
| Valor inicial > 0 | `Simulation` |
| Taxas anuais ≥ 0 | `AnnualRate` |
| Percentual de rentabilidade > 0 | `Simulation` |
| Custos ≥ 0 | `Simulation` |
| Coleções e entradas obrigatórias | `Simulation` |

## Calendário financeiro (ERS §29)

Implementado em `InvestmentSimulator.Domain.Calendar`:

| Conceito | Detalhe |
| -------- | ------- |
| Dias úteis/ano | `FinancialCalendar.BusinessDaysPerYear` = **252** (conversão de taxas, ERS §8) |
| Dia útil | Segunda–sexta, excluindo feriados nacionais |
| Feriados | `BrazilianNationalHolidays` — fixos + móveis (Carnaval, Sexta Santa, Corpus Christi); Consciência Negra a partir de 2024 |
| Aporte em dia não útil | `NonBusinessDayContributionRule`: adiar para o próximo dia útil (padrão) ou manter a data |
| Dias corridos | `CountCalendarDays` — base para IR/IOF |
| Dias úteis | `CountBusinessDays` / `EnumerateBusinessDays` — base para rentabilidade (intervalo `(início, fim]`) |

## Taxas anuais e conversão (ERS §§6–8)

Implementado em `InvestmentSimulator.Domain.Rates`:

| Conceito | Detalhe |
| -------- | ------- |
| Conversão anual → diária | `RateConverter.AnnualToDaily` — `(1 + taxa)^(1/252) − 1` |
| Gerador de anos | `YearGenerator.Generate` — anos inclusivos entre data inicial e final |
| Taxa única | `RateSchedule.FromSingleRate` — mesma taxa expandida para todos os anos |
| Taxa por ano | `RateSchedule.FromPerYear` — uma taxa por ano do período (sem lacunas/duplicatas) |
| Modo de entrada | `RateEntryMode`: `SingleRate` ou `PerYear` |

## Motor de cálculo base (ERS §§9–11)

Implementado em `InvestmentSimulator.Domain.Calculation`:

| Conceito | Detalhe |
| -------- | ------- |
| Posição por aporte | `ContributionPosition` — saldo, rendimento, dias investidos, IR e IOF (cada aporte é independente) |
| Loop diário | `DailyCalculationEngine` — para cada dia útil e cada aporte ativo: aplica rendimento, atualiza saldo e dias |
| Troca de taxas | `SimulationRateContext.AdvanceToYear` — troca CDI/Selic, IPCA e Taxa B3 na virada do ano |
| Taxa efetiva diária | `IDailyYieldRateProvider` — estratégia plugável por produto |

O intervalo de acumulação por aporte é meio-aberto `(data do aporte, data final]` em dias úteis, alinhado ao `FinancialCalendar`.

## Calculadora CDB (ERS §12)

| Conceito | Detalhe |
| -------- | ------- |
| Fórmula | `CdbCalculator.CalculateDailyYieldRate` — **CDI diário × rentabilidade contratada** |
| Exemplo anual | CDI 15% × 110% = 16,5% (`CalculateEffectiveAnnualRate`) |
| Provider | `CdbDailyYieldRateProvider` — pluga a fórmula no `DailyCalculationEngine` |
| Rentabilidade | Fração decimal (ex.: `1.10` = 110% do CDI), mesma convenção de `Simulation.ProfitabilityPercentage` |

## Calculadora Tesouro Selic (ERS §13)

| Conceito | Detalhe |
| -------- | ------- |
| Fórmula | `TesouroSelicCalculator.CalculateDailyYieldRate` — **(1 + Selic diária) × (1 + ágio diário) − 1** |
| Taxa anual efetiva | `(1 + Selic anual) × (1 + ágio anual) − 1` (`CalculateEffectiveAnnualRate`) |
| Provider | `TesouroSelicDailyYieldRateProvider` — pluga a fórmula no `DailyCalculationEngine` |
| Ágio/deságio | Fração decimal anual (ex.: `0.001` = +0,1% ágio; negativo = deságio); convertido para diário via `RateConverter` |

## Custódia B3 (ERS §14)

Implementado em `InvestmentSimulator.Domain.Calculation`:

| Conceito | Detalhe |
| -------- | ------- |
| Isenção | Saldo ≤ **R$10.000** → taxa 0 (`B3CustodyCalculator.ExemptionThreshold`) |
| Base de cálculo | Excedente: `max(0, saldo − 10.000)` |
| Provisionamento diário | `excedente × taxa B3 diária` (`CalculateDailyProvision`) |
| Cobrança semestral | 1º dia útil de **janeiro** e de **julho** (`IsSemiannualCollectionDate`) |
| Cobrança no resgate | `B3CustodyProvisioner.CollectOnRedemption` — liquida o provisionado restante |
| Acúmulo | `B3CustodyProvisioner` — provisiona por dia útil e cobra nas datas devidas |

A taxa B3 anual/diária já é exposta por `SimulationRateContext` (`CurrentB3AnnualRate` / `CurrentB3DailyRate`).

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
