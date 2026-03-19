# CryptoTracker

Sistema de coleta e visualização de cotações de criptomoedas em tempo real. Um worker coleta os dados da CoinGecko a cada 5 minutos e os expõe via API REST, consumida por um dashboard Angular.

## Arquitetura

```
┌──────────────────────────────────────────────────────────┐
│                         Docker                           │
│                                                          │
│  ┌─────────────┐      ┌─────────────┐                   │
│  │   Worker    │      │     API     │                    │
│  │   :5001     │      │    :8080    │                    │
│  │  Scraper    │      │ /api/crypto │                    │
│  │  (5 min)    │      │   /health   │                    │
│  └──────┬──────┘      └──────┬──────┘                   │
│         │                    │                           │
│         └─────────┬──────────┘                          │
│                   │                                      │
│            ┌──────▼──────┐    ┌──────────────┐          │
│            │   SQLite    │    │  Web (Nginx) │          │
│            │  crypto.db  │    │     :80      │          │
│            └─────────────┘    └──────┬───────┘          │
│                                      │                   │
└──────────────────────────────────────┼───────────────────┘
                                       │ :4200
                                   Browser
```

| Serviço | Tecnologia | Responsabilidade |
|---|---|---|
| **Worker** | .NET 8 Background Service | Scraping da CoinGecko, persistência no banco |
| **API** | ASP.NET Core 8 | Endpoints REST para consulta dos dados |
| **Shared** | Class Library .NET 8 | Entidades, repositório e `AppDbContext` compartilhados |
| **Web** | Angular 21 + Nginx | Dashboard com tabela, gráfico e top movers |

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Como rodar

```bash
git clone <url-do-repositorio>
cd DataCollector

docker compose up --build
```

Acesse **http://localhost:4200**

O worker executa um scrape imediatamente ao iniciar — os dados já estão disponíveis na primeira abertura do site.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/crypto` | Lista cotações mais recentes (paginado) |
| `GET` | `/api/crypto/stats` | Total de registros e horário da última coleta |
| `GET` | `/api/crypto/top-gainers` | Maiores altas nas últimas 24h |
| `GET` | `/api/crypto/top-losers` | Maiores baixas nas últimas 24h |
| `GET` | `/api/crypto/history/{symbol}` | Histórico de preços de uma moeda |
| `POST` | `/trigger` | Força um scrape imediato (via worker) |
| `GET` | `/health` | Health check dos serviços |

**Parâmetros de paginação:** `?page=1&pageSize=50` (máx. 100 por página)

### Exemplos

```bash
# Cotações mais recentes
curl "http://localhost:8080/api/crypto?page=1&pageSize=50"

# Estatísticas
curl http://localhost:8080/api/crypto/stats

# Top gainers
curl "http://localhost:8080/api/crypto/top-gainers?limit=10"

# Histórico do Bitcoin
curl http://localhost:8080/api/crypto/history/btc

# Forçar scrape imediato
curl -X POST http://localhost:5001/trigger
```

## Variáveis de ambiente

| Variável | Padrão | Descrição |
|---|---|---|
| `Scraper__TargetUrl` | `https://api.coingecko.com/api/v3` | URL base da CoinGecko |
| `Scraper__IntervalMinutes` | `5` | Intervalo entre coletas |
| `ConnectionStrings__DefaultConnection` | `Data Source=/data/crypto.db` | Conexão SQLite |
| `ASPNETCORE_URLS` | varia por serviço | Porta de escuta |

## Parar os serviços

```bash
# Apenas parar
docker compose down

# Parar e apagar o banco de dados
docker compose down -v
```

## Testes

```bash
dotnet test src/DataCollector.Tests/DataCollector.Tests.csproj
```
