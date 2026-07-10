# Endpoints da API — Simulador de Investimentos

Documentação dos endpoints HTTP expostos pelos controllers ASP.NET Core.
A documentação interativa (Swagger UI) fica disponível em `/swagger` quando a API roda em ambiente **Development**.

Rotas em português; payloads e código em inglês. Enums são serializados como string (`Cdb`, `Csv`, `TesouroSelic`, etc.).
Validações de domínio (`DomainValidationException`) retornam **HTTP 400** com `{ "error": "..." }`.

**Convenção de taxas anuais na API:** valores em **percentual** (ex.: `14.15` = 14,15% a.a.). O domínio converte internamente para fração decimal (`0.1415`).
O multiplicador `cdiPercentage` continua como fração (ex.: `1.20` = 120% do CDI).

---

## Visão geral

| Método | Rota | Controller | Descrição |
| ------ | ---- | ---------- | --------- |
| `POST` | `/simular/cdb` | `SimulationController` | Simula CDB pós-fixado (CDI × percentual) |
| `POST` | `/simular/tesouro` | `SimulationController` | Simula Tesouro Selic (Selic + ágio/deságio) |
| `POST` | `/comparar` | `ComparisonController` | Compara duas simulações lado a lado |
| `POST` | `/exportar` | `ExportController` | Exporta um resultado em CSV, Excel ou PDF |
| `GET` | `/historico` | `HistoryController` | Lista simulações salvas |
| `GET` | `/historico/{id}` | `HistoryController` | Carrega uma entrada do histórico |
| `POST` | `/historico` | `HistoryController` | Salva (ou sobrescreve) uma simulação no histórico |

---

## Simulação

### `POST /simular/cdb`

Simula um CDB pós-fixado. **Não há custos operacionais nem custódia B3** (`costs` no response é sempre `0`).

**Corpo (JSON)** — `SimulateCdbRequest`:

| Campo | Tipo | Descrição |
| ----- | ---- | --------- |
| `initialAmount` | `decimal` | Valor inicial em R$ (pode ser `0` se houver aportes) |
| `startDate` | `date` | Data inicial / aporte inicial |
| `endDate` | `date` | Data de resgate |
| `contributions` | `ContributionRequest[]` | Aportes adicionais (`date`, `amount`) |
| `cdiAnnualRates` | `AnnualRateRequest[]` | CDI anual em **%** (`year`, `rate`) |
| `ipcaRates` | `AnnualRateRequest[]` | IPCA anual em **%** |
| `cdiPercentage` | `decimal` | Multiplicador do CDI (ex.: `1.20` = 120%) |

**Resposta 200** — `SimulationResultResponse`:

| Campo | Descrição |
| ----- | --------- |
| `initialAmount` | Valor inicial investido |
| `totalAdditionalContributions` | Soma dos aportes adicionais |
| `totalInvested` | Inicial + aportes |
| `grossAmount` | Valor bruto ao resgate |
| `grossReturnPercentage` | Rentabilidade bruta (fração, ex.: `0.15` = 15%) |
| `costs` | Sempre `0` no CDB |
| `incomeTax` | Total de IR |
| `iof` | Total de IOF |
| `netAmount` | Valor líquido |
| `netReturnPercentage` | Rentabilidade líquida (fração) |
| `totalNetYield` | Lucro líquido (líquido − investido) |
| `netAmountInflationAdjusted` | Valor líquido ajustado pelo IPCA |
| `contributionDetails[]` | Detalhe por aporte (ver abaixo) |

**Detalhe por aporte** (`contributionDetails`):

| Campo | Descrição |
| ----- | --------- |
| `date` | Data do aporte (ajustada a dia útil, se aplicável) |
| `amount` | Valor aportado |
| `grossBalance` | Saldo bruto ao final |
| `grossYield` | Rendimento bruto |
| `calendarDaysInvested` | Dias corridos (base IR/IOF) |
| `businessDaysInvested` | Dias úteis com rendimento aplicado |
| `incomeTax` | IR do aporte |
| `iof` | IOF do aporte |

### `POST /simular/tesouro`

Simula Tesouro Selic.

**Corpo (JSON)** — `SimulateTesouroRequest`: mesmos campos de datas/aportes/IPCA, com:

| Campo | Tipo | Descrição |
| ----- | ---- | --------- |
| `selicAnnualRates` | `AnnualRateRequest[]` | Selic Over anual em **%** |
| `annualAgioRate` | `decimal` | Ágio/deságio como **fração** (ex.: `0.001` = +0,1%) |
| `b3CustodyRates` | `AnnualRateRequest[]?` | Custódia B3 anual em **%** (ex.: `0.2` = 0,2%); omitir para não cobrar |

**Resposta 200:** mesmo `SimulationResultResponse`. `costs` reflete a custódia B3 quando informada.

---

## Comparação

### `POST /comparar`

**Corpo:** `left` e `right` (`CompareSideRequest`), cada um com `type` (`Cdb` ou `TesouroSelic`) e os campos do produto (`cdiAnnualRates` / `selicAnnualRates`, `cdiPercentage`, `b3CustodyRates`, etc.).

**Resposta 200:** métricas por lado + diferenças (`*Difference`).

---

## Exportação

### `POST /exportar`

| Campo | Descrição |
| ----- | --------- |
| `format` | `Csv`, `Excel` ou `Pdf` |
| `result` | Resultado no shape de `SimulationResultResponse` |

**Resposta 200:** arquivo binário com `Content-Disposition`.

---

## Histórico

| Método | Rota | Resposta |
| ------ | ---- | -------- |
| `GET` | `/historico` | `200` — lista de `HistoryEntryResponse` |
| `GET` | `/historico/{id}` | `200` ou `404` |
| `POST` | `/historico` | `201 Created` |

Taxas no snapshot do histórico são devolvidas em **percentual** (`indexAnnualRates`, `ipcaRates`).

---

## Swagger

| Recurso | URL |
| ------- | --- |
| Swagger UI | `/swagger` |
| OpenAPI JSON | `/swagger/v1/swagger.json` |

Descrições em português dos parâmetros e do schema de resposta 200 vêm dos comentários XML dos contracts. Exemplos também em `InvestmentSimulator.Api.http`.
