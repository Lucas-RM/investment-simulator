# Investment Simulator UI

Frontend do Simulador de Investimentos em **React 19 + TypeScript + Vite**.

## Estrutura de pastas

```
investment-simulator-ui/
├── public/                 # Assets estáticos
├── src/
│   ├── assets/             # Imagens e mídia importadas pelo bundler
│   ├── components/         # Componentes reutilizáveis (navegação, formulários)
│   ├── hooks/              # Hooks (rascunho local + execução da simulação)
│   ├── layouts/            # Layouts de página (shell + header)
│   ├── pages/              # Páginas por rota
│   ├── routes/             # Definição de rotas e paths
│   ├── services/           # Cliente HTTP e chamadas aos endpoints de simulação
│   ├── styles/             # Tema base (CSS variables) e estilos globais
│   ├── types/              # Tipos compartilhados (entradas, taxas, contratos da API)
│   ├── utils/              # Validação (§27), mapeamento de request e helpers
│   ├── App.tsx
│   └── main.tsx
├── index.html
├── package.json
└── vite.config.ts
```

## Rotas iniciais

| Rota                              | Página                                      |
| --------------------------------- | ------------------------------------------- |
| `/`                               | Início                                      |
| `/simulate/cdb`                   | Simulação CDB — entradas gerais             |
| `/simulate/cdb/contributions`     | Simulação CDB — aportes                     |
| `/simulate/cdb/rates`             | Simulação CDB — taxas + chamada à API       |
| `/simulate/tesouro`               | Simulação Tesouro — entradas gerais         |
| `/simulate/tesouro/contributions` | Simulação Tesouro — aportes                 |
| `/simulate/tesouro/rates`         | Simulação Tesouro — taxas + chamada à API   |
| `/compare`                        | Comparação (stub)                           |
| `/history`                        | Histórico (stub)                            |

## Integração com a API

Na etapa de taxas, o botão **Simular** envia o rascunho para:

- `POST /simular/cdb`
- `POST /simular/tesouro`

Estados de **loading** e **erro** são exibidos no formulário; o resumo §19 aparece abaixo após sucesso (gráficos/exportação ficam para commits seguintes).

Em desenvolvimento, o Vite faz proxy de `/simular` para o valor de `VITE_API_BASE_URL` (padrão `http://localhost:5001`). Copie `.env.example` para `.env` (ignorado pelo Git). Em produção, o cliente HTTP usa `VITE_API_BASE_URL` diretamente.

Valores monetários e taxas permanecem como **string** nos formulários e só viram número JSON na borda do cliente HTTP.

## Validações (ERS §27)

Cobertas nos formulários e utilitários: datas inválidas, taxas negativas indevidas, aportes fora do período, resgate antes do início, valor inicial negativo, aportes ≤ 0, simulação sem valor investido (inicial zero sem aportes), percentuais inválidos e campos obrigatórios.
## Tema base

O tema vive em `src/styles/theme.css` com variáveis CSS para cores, tipografia, espaçamento e raios. A tipografia usa **DM Sans** (texto) e **Fraunces** (títulos), com paleta verde-escuro alinhada a um produto financeiro.

## Scripts

```bash
npm install
npm run dev      # servidor de desenvolvimento
npm run build    # build de produção
npm run preview  # preview do build
npm test         # testes (Vitest)
npm run lint     # lint (oxlint)
```

## Alias de importação

O alias `@/` aponta para `src/` (configurado em `vite.config.ts` e `tsconfig.app.json`).

Exemplo: `import { paths } from '@/routes/paths'`
