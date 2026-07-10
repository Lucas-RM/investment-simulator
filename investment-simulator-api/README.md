# Investment Simulator API

Backend do Simulador de Investimentos em **C# .NET 10**, organizado em camadas.

## Estrutura da solução

```
investment-simulator-api/
├── src/
│   ├── InvestmentSimulator.Domain/          # Entidades, value objects e regras de domínio
│   ├── InvestmentSimulator.Application/     # Casos de uso e serviços de aplicação
│   ├── InvestmentSimulator.Infrastructure/    # Exportação, persistência/histórico
│   └── InvestmentSimulator.Api/               # Controllers HTTP + Swagger (ASP.NET Core)
└── tests/
    ├── InvestmentSimulator.Domain.Tests/           # Testes unitários do domínio
    ├── InvestmentSimulator.Application.Tests/      # Testes do serviço de orquestração
    ├── InvestmentSimulator.Infrastructure.Tests/   # Testes de exportação e persistência
    └── InvestmentSimulator.Api.Tests/              # Testes de integração dos controllers
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
| Valor inicial ≥ 0 (se 0, exige aportes) | `Simulation` |
| Taxas anuais ≥ 0 | `AnnualRate` |
| Percentual de rentabilidade > 0 | `Simulation` |
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
| Rentabilidade | Fração decimal (ex.: `1.10` = 110% do CDI), mesma convenção de `Simulation.ProfitabilityPercentage` / API `cdiPercentage` |
| Custos | Sempre **zero** — CDB não possui custódia B3 |
| Taxas na API | CDI/IPCA em **percentual** (ex.: `14.15`); convertidos para fração no mapper |

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

## Calculadora de IOF (ERS §15)

Implementado em `InvestmentSimulator.Domain.Calculation.IofCalculator`:

| Conceito | Detalhe |
| -------- | ------- |
| Incidência | Somente se **dias corridos investidos &lt; 30** (`ExemptionDays`) |
| Base de cálculo | **Somente o rendimento** (yield) — nunca o principal |
| Tabela | Regressiva oficial (Decreto 6.306/2007): dia 1 = 96% … dia 29 = 3%; ≥ 30 dias = 0% |
| Fórmula | `IOF = rendimento × alíquota(dias)` |
| Precisão | Resultado com 8 casas decimais intermediárias (`MonetaryPrecision`) |

A contagem de dias usa dias corridos (já definida em `FinancialCalendar.CountCalendarDays` / `ContributionPosition.DaysInvested`).

## Calculadora de IR (ERS §16)

Implementado em `InvestmentSimulator.Domain.Calculation.IncomeTaxCalculator`:

| Conceito | Detalhe |
| -------- | ------- |
| Incidência | **Por aporte individual** (cada contribuição tem sua própria alíquota conforme dias corridos) |
| Base de cálculo | Rendimento (yield) — quando houver IOF, o orquestrador deve passar o rendimento já líquido de IOF |
| Tabela | Regressiva oficial: ≤180 → **22,5%**; 181–360 → **20%**; 361–720 → **17,5%**; &gt;720 → **15%** |
| Fórmula | `IR = rendimento × alíquota(dias)` |
| Precisão | Resultado com 8 casas decimais intermediárias (`MonetaryPrecision`) |

## Calculadora de Inflação (ERS §§17–18)

Implementado em `InvestmentSimulator.Domain.Calculation.InflationCalculator`:

| Conceito | Detalhe |
| -------- | ------- |
| Inflação acumulada | `∏(1 + IPCAᵢ) − 1` — produto composto das taxas anuais do período |
| Exemplo ERS | 5%, 4%, 4,5% → `(1,05 × 1,04 × 1,045) − 1` = **14,114%** |
| Poder de compra | `Valor líquido ÷ (1 + inflação acumulada)` — valor real ajustado |
| Atalho | `CalculateInflationAdjustedAmount` — acumula e ajusta em uma chamada |
| Precisão | Resultado com 8 casas decimais intermediárias (`MonetaryPrecision`) |

As taxas IPCA anuais já existem em `Simulation.IpcaRates` / `SimulationRateContext`; esta calculadora consome a sequência de frações decimais ao final da simulação.

## SimulacaoService — orquestração (ERS §§19–20)

Implementado em `InvestmentSimulator.Application.Simulations.SimulationService`:

| Etapa | Detalhe |
| ----- | ------- |
| Motor diário | `DailyCalculationEngine` + provider CDB ou Tesouro Selic conforme `InvestmentType` |
| Custódia B3 | Opcional via `SimulationOptions.B3CustodyRates` — **apenas Tesouro Selic**; CDB ignora |
| IOF → IR | Por aporte: IOF sobre o rendimento; IR sobre o rendimento já líquido de IOF |
| Inflação | `InflationCalculator` sobre o valor líquido final |
| Resumo | `SimulationResult` — valor inicial, aportes, total investido, bruto, rentabilidades, custos (B3), IR, IOF, líquido, lucro, valor real |
| Detalhamento | `ContributionDetail` por aporte (data, valor, dias corridos/úteis, IR, IOF, saldo/rendimento bruto) |

Parâmetros extras (ágio Tesouro e taxas B3) ficam em `SimulationOptions`. Valor inicial pode ser zero se houver aportes adicionais.

## Comparação de simulações (ERS §24)

Implementado em `InvestmentSimulator.Application.Simulations.SimulationComparisonService`:

| Conceito | Detalhe |
| -------- | ------- |
| Entrada | Duas simulações (`left` / `right`) com `SimulationOptions` opcionais por lado |
| Execução | Cada lado passa pelo `SimulationService.Run` |
| Métricas | Valor líquido, IR, custos, lucro líquido, rentabilidade líquida, valor real (inflação) |
| Diferenças | `right − left` para cada métrica (`*Difference`) |
| Tipos | `SimulationComparisonSide` (um lado) e `SimulationComparisonResult` (lado a lado) |

Exemplo típico: **CDB** vs **Tesouro Selic** com os mesmos aportes e período.

## Exportação de resultados (ERS §25)

Implementado em `InvestmentSimulator.Infrastructure.Export` (porta em `Application.Export`):

| Formato | Detalhe |
| ------- | ------- |
| CSV | UTF-8 com BOM; separador `;` (compatível com Excel pt-BR) |
| Excel | `.xlsx` via ClosedXML — abas **Resumo** e **Aportes** |
| PDF | QuestPDF (licença Community) — resumo + tabela por aporte |
| Conteúdo | Campos do resumo (ERS §19) + detalhamento por aporte (ERS §20) |
| Precisão | Arredondamento só na apresentação: 2 casas (moeda), 4 (percentuais) |
| API | `ISimulationExportService.Export(result, format)` → `ExportDocument` |

## Histórico / Persistência (ERS §26)

Implementado em `InvestmentSimulator.Infrastructure.Persistence` (porta em `Application.History`):

| Conceito | Detalhe |
| -------- | ------- |
| Entrada | `SimulationHistoryEntry` — nome, data, tipo, observações + agregado `Simulation` |
| Salvar | `ISimulationHistoryRepository.Save` — persiste (ou sobrescreve pelo `Id`) |
| Carregar | `ISimulationHistoryRepository.GetById` — retorna a entrada ou `null` |
| Listar | `ISimulationHistoryRepository.List` — ordenado por data desc., depois nome |
| Implementação | `InMemorySimulationHistoryRepository` — armazenamento em memória (dev/testes) |
| Validações | Nome obrigatório; data válida; observações não nulas (podem ser vazias) |

O tipo (`InvestmentType`) é derivado da simulação salva. O agregado completo permite recarregar e reexecutar o cálculo.

## API HTTP — Controllers + Swagger

Expostos via controllers MVC em `InvestmentSimulator.Api/Controllers` (rotas em português; código em inglês).

| Método | Rota | Descrição |
| ------ | ---- | --------- |
| `POST` | `/simular/cdb` | Simula CDB pós-fixado (CDI × percentual) |
| `POST` | `/simular/tesouro` | Simula Tesouro Selic (Selic + ágio/deságio) |
| `POST` | `/comparar` | Compara duas simulações lado a lado |
| `POST` | `/exportar` | Exporta um resultado em CSV, Excel ou PDF |
| `GET` | `/historico` | Lista simulações salvas |
| `GET` | `/historico/{id}` | Carrega uma entrada do histórico |
| `POST` | `/historico` | Salva (ou sobrescreve) uma simulação no histórico |

**Taxas anuais na API** entram em **percentual** (ex.: `14.15` = 14,15% a.a.); o mapper converte para fração no domínio. O campo `cdiPercentage` é multiplicador (`1.20` = 120% do CDI). `initialAmount` pode ser `0` se houver aportes. CDB **não** aplica custódia B3 (`costs` = 0).

Validações de domínio retornam **HTTP 400** com `{ "error": "..." }`. Enums como string (`Cdb`, `Csv`, etc.).

Documentação detalhada (request/response): [`docs/endpoints.md`](docs/endpoints.md). Em Development: Swagger UI em `/swagger`. Exemplos em `InvestmentSimulator.Api.http`.

## Testes automatizados das calculadoras (ERS §30)

Suíte em `InvestmentSimulator.Domain.Tests/Calculation` para validar os cálculos financeiros:

| Área | Arquivo de testes |
| ---- | ----------------- |
| IR (tabela regressiva) | `IncomeTaxCalculatorTests` |
| IOF (até 30 dias, só rendimento) | `IofCalculatorTests` |
| Custódia B3 (isenção + semestral) | `B3CustodyCalculatorTests` |
| Inflação / poder de compra | `InflationCalculatorTests` |
| Motor diário (aportes, taxas, feriados) | `DailyCalculationEngineTests` |
| Cenários compostos (IOF→IR, B3+motor, 50 anos) | `FinancialCalculatorsValidationTests` |

Os cenários compostos cobrem a composição usada na orquestração (IOF sobre rendimento, IR sobre rendimento líquido de IOF, custódia via hook do motor, ajuste pela inflação) e o requisito de simulações longas (até 50 anos) em poucos segundos.

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

# Apenas testes das calculadoras / motor
dotnet test --filter "FullyQualifiedName~Calculation"
```
