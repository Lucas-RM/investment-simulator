# Investment Simulator UI

Frontend do Simulador de Investimentos em **React 19 + TypeScript + Vite**.

## Estrutura de pastas

```
investment-simulator-ui/
├── public/                 # Assets estáticos
├── src/
│   ├── assets/             # Imagens e mídia importadas pelo bundler
│   ├── components/         # Componentes reutilizáveis (ex.: navegação)
│   ├── hooks/              # Hooks customizados (commits futuros)
│   ├── layouts/            # Layouts de página (shell + header)
│   ├── pages/              # Páginas por rota (stubs iniciais)
│   ├── routes/             # Definição de rotas e paths
│   ├── services/           # Clientes HTTP / API (commits futuros)
│   ├── styles/             # Tema base (CSS variables) e estilos globais
│   ├── test/               # Setup do Vitest
│   ├── types/              # Tipos compartilhados (commits futuros)
│   ├── utils/              # Utilitários (commits futuros)
│   ├── App.tsx
│   └── main.tsx
├── index.html
├── package.json
└── vite.config.ts
```

## Rotas iniciais

| Rota | Página |
| ---- | ------ |
| `/` | Início |
| `/simulate/cdb` | Simulação CDB (stub) |
| `/simulate/tesouro` | Simulação Tesouro Selic (stub) |
| `/compare` | Comparação (stub) |
| `/history` | Histórico (stub) |

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
