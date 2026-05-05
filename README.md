# UserTracker — Rastreador de Acessos em .NET 8 + SQLite

Sistema completo de rastreamento de dispositivos que acessa sua aplicação.
Combina dados do **servidor** (headers HTTP) com dados do **cliente** (JavaScript APIs).

---

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 🚀 Como rodar

```bash
# 1. Restaurar dependências
dotnet restore

# 2. Executar (o banco SQLite é criado automaticamente)
dotnet run

# 3. Acessar no navegador
# Dashboard: http://localhost:5000
# API Docs:  http://localhost:5000/swagger
```

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

---

## ⚠️ Por que não tem MAC Address?

O MAC Address **não é transmitido** em requisições HTTP/HTTPS — ele opera na camada
de rede local (Layer 2 / Ethernet) e nunca chega ao servidor web.

Para capturar o MAC, seria necessário um **agente/software instalado** na máquina
do usuário (ex: aplicação desktop, extensão de browser com permissões elevadas,
ou acesso direto à rede via ARP em ambiente corporativo).

---

## 🔒 Sobre privacidade e LGPD

Este sistema coleta dados de identificação de dispositivos.
Em produção, certifique-se de:
- Ter base legal para o tratamento (LGPD, Art. 7º)
- Informar os usuários via Política de Privacidade
- Implementar mecanismo de consentimento se necessário
- Definir prazo de retenção e exclusão dos dados
