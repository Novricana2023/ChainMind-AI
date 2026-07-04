# ChainMind AI

Smart contract development platform — generate, explain, audit, deploy, test, and optimize Solidity contracts.

## Features

| Tool | What it does |
|------|----------------|
| **Generator** | Create production-ready Solidity from a natural language prompt |
| **Explainer** | Plain-English breakdown of any contract with security notes |
| **Auditor** | Scans for reentrancy, access control, overflow, and unsafe calls |
| **Deploy** | Step-by-step deployment guides for Ethereum, Polygon, Base, Arbitrum |
| **Tests** | Hardhat unit, integration, and security test suites |
| **Gas** | Gas usage estimates and optimization recommendations |
| **Agent** | Full security review with risk score and structured report |
| **Docs** | Markdown documentation generated from contract source |

## Stack

- **Frontend:** Next.js, TypeScript, Tailwind CSS, ShadCN UI, Framer Motion
- **Backend:** ASP.NET Core 8, clean architecture, dependency injection
- **AI:** ChainMind engine (default), OpenAI, or Claude via API keys

## Setup

### Backend

```bash
cd backend
dotnet run --project src/ChainMind.Api --launch-profile http
```

Runs at `http://localhost:5080`

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Runs at `http://localhost:3000`

## Environment

**Frontend** — `NEXT_PUBLIC_API_URL=https://chainmind-ai.onrender.com` (production) or `http://localhost:5080` (local)

**Vercel (recommended):** Project → Settings → Environment Variables →  
`NEXT_PUBLIC_API_URL` = `https://chainmind-ai.onrender.com` → Redeploy

**Render backend:** Live API: `https://chainmind-ai.onrender.com`  
- Root: `https://chainmind-ai.onrender.com/` (status page)  
- Health: `https://chainmind-ai.onrender.com/health`  
Do **not** use `chainmind-api.onrender.com` (not deployed).

If the app shows **Failed to fetch**, the frontend cannot reach the API. Usually:
1. Wrong API URL (`chainmind-api.onrender.com` is not deployed — use `chainmind-ai.onrender.com`)
2. Render free tier is sleeping — wait ~60 seconds and try again
3. `NEXT_PUBLIC_API_URL` missing on Vercel (the app falls back to `chainmind-ai.onrender.com` on `*.vercel.app`)

**Backend (recommended for best generation quality):**

| Variable | Values |
|----------|--------|
| `AI__Provider` | `OpenAI` or `Claude` for custom contracts; `ChainMind` is template-only (ERC-20/NFT) |
| `AI__OpenAIApiKey` | OpenAI key (required when Provider=OpenAI) |
| `AI__OpenAIModel` | `gpt-4o` (recommended) |
| `AI__ClaudeApiKey` | Anthropic key |
| `Cors__AllowedOrigins__0` | Frontend URL |

Generation uses a **two-pass pipeline** when OpenAI/Claude is configured: generate → audit → auto-fix critical issues.
Use **Generator templates** in the UI (Crop Insurance, Voting, HealingToken, etc.) for best results.

## Deployment

**Vercel** — root `frontend`, set `NEXT_PUBLIC_API_URL` to `https://chainmind-ai.onrender.com`, then redeploy.

**Render** — root `backend`, build `dotnet publish src/ChainMind.Api/ChainMind.Api.csproj -c Release -o out`, start `dotnet out/ChainMind.Api.dll`, set `ASPNETCORE_URLS=http://0.0.0.0:10000`.

## API

| Method | Endpoint |
|--------|----------|
| POST | `/api/contracts/generate` |
| POST | `/api/contracts/explain` |
| POST | `/api/contracts/audit` |
| POST | `/api/contracts/deploy` |
| POST | `/api/contracts/tests` |
| POST | `/api/contracts/gas` |
| POST | `/api/contracts/agent` |
| POST | `/api/contracts/documentation` |
| GET | `/api/dashboard/stats` |
| GET | `/health` |

## License

MIT
