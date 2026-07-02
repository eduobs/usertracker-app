# UserTracker — Rastreador de Acessos em .NET 8 + SQLite

Sistema completo de rastreamento de dispositivos que acessa sua aplicação.
Combina dados do **servidor** (headers HTTP) com dados do **cliente** (JavaScript APIs).

---

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 🚀 Como rodar

> **Nota sobre Login (User Secrets)**: Caso você não possua as credenciais do Google (`ClientId` e `ClientSecret`), consulte o [guia detalhado de configuração do Google Auth](docs/google_auth_setup.md).

```bash
# 1. Restaurar dependências
dotnet restore

# 2. Configurar credenciais do Google para o Login
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "COLOQUE_SEU_CLIENT_ID_AQUI"
dotnet user-secrets set "Authentication:Google:ClientSecret" "COLOQUE_SEU_CLIENT_SECRET_AQUI"

# 3. Executar (o banco SQLite é criado automaticamente)
dotnet run

# 4. Acessar no navegador
# Portal:    http://localhost:5000/
# Dashboard: http://localhost:5000/dashboard
# API Docs:  http://localhost:5000/swagger
```

---

## ⚙️ Deploy Automático (CI/CD)

Este repositório possui uma esteira configurada com GitHub Actions para deploy automático no Azure App Service.
Para aprender a autorizar o GitHub a publicar no Azure e como proteger as variáveis de ambiente (Secrets), consulte o [Guia de Configuração de CI/CD e Secrets](docs/cicd_azure_setup.md).

---

## 📦 O que é coletado

### Via servidor (headers HTTP)
| Campo | Descrição |
|-------|-----------|
| IP Address | IP real do cliente (suporta X-Forwarded-For, X-Real-IP) |
| User-Agent | String completa do navegador |
| Accept-Language | Idioma configurado no OS |
| Referer / Origin | Página de origem |
| Sec-CH-UA* | Client Hints (Chrome/Edge) — OS, mobile, versão |
| Sec-Fetch-* | Tipo de requisição |
| Session ID | ID de sessão gerado pelo servidor |

### Via JavaScript (browser APIs)
| Campo | Descrição |
|-------|-----------|
| Fingerprint Hash | SHA-256 único do conjunto de atributos |
| Timezone | Fuso horário do sistema |
| Screen Width/Height | Resolução real da tela |
| Device Pixel Ratio | Densidade de pixels (identifica telas Retina) |
| Hardware Concurrency | Número de CPUs/threads |
| Device Memory | RAM disponível (em GB) |
| Max Touch Points | Se é touchscreen e quantos pontos |
| Canvas Hash | Hash de renderização do canvas 2D |
| WebGL Vendor/Renderer | GPU utilizada (identifica o hardware) |
| Plugins | Lista de plugins do navegador |
| Connection Type | Tipo de rede (4g, wifi, etc.) |
| localStorage/IndexedDB | Capacidades do navegador |

---

## 🏗️ Arquitetura

```
UserTracker/
├── Controllers/
│   └── TrackingController.cs   # API REST (collect, list, stats, delete)
├── Data/
│   └── AppDbContext.cs          # EF Core + SQLite
├── Models/
│   └── UserAccess.cs            # Entidade com todos os campos
├── Pages/
│   ├── Index.cshtml             # Dashboard com estatísticas
│   ├── Accesses.cshtml          # Listagem paginada e filtrada
│   └── AccessDetail.cshtml      # Detalhe completo de um acesso
├── Services/
│   └── FingerprintService.cs   # Lógica de coleta server-side
├── wwwroot/js/
│   └── fingerprint.js           # Coleta client-side + POST para API
└── Program.cs                   # Configuração da aplicação
```

---

## 🔌 Endpoints da API

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/tracking/collect` | Recebe fingerprint do JS |
| GET | `/api/tracking/accesses` | Lista acessos (paginado, filtrável) |
| GET | `/api/tracking/accesses/{id}` | Detalhe de um acesso |
| DELETE | `/api/tracking/accesses/{id}` | Remove um acesso |
| GET | `/api/tracking/stats` | Estatísticas gerais |
