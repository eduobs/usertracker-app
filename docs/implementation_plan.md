# Implementação de Autenticação, Rotas e Pipeline CI/CD

O objetivo desta implementação é reorganizar as rotas do projeto, integrar login com o Google para proteger o Dashboard, criar um sistema de aprovação de usuários com separação de papel (Role) e status de aprovação (IsApproved), além de proteger dados sensíveis através de um pipeline CI/CD via GitHub Actions.

## User Review Required

> [!IMPORTANT]
> **Proteção de Segredos e CI/CD**: Seus dados sigilosos (ClientId e ClientSecret) **não** estarão no código-fonte, nem no arquivo `appsettings.json`, e nem serão passados pelo GitHub Actions. 
> A estratégia será:
> 1. **Desenvolvimento Local**: Utilizaremos a ferramenta `dotnet user-secrets` para armazenar as chaves de forma segura na sua máquina.
> 2. **Produção (Azure)**: Você irá configurar essas variáveis diretamente no painel do Azure (App Service > Configurações > Variáveis de Ambiente). O código ASP.NET Core puxará essas configurações automaticamente no ambiente de produção.
> 3. **Pipeline (GitHub Actions)**: Criarei um arquivo `.github/workflows/main_usertracker-app.yml` básico para compilar e fazer o deploy contínuo do seu código para o Azure.

> [!TIP]
> **Primeiro Usuário (Admin)**: A lógica definirá **automaticamente o primeiro usuário que fizer login pelo Google como Admin e Aprovado**. Todos os usuários subsequentes serão cadastrados com a role `Common` e status `IsApproved = false` (pendente), necessitando da aprovação do Admin.

## Proposed Changes

### 1. Dependências e Secrets Locais
- Adicionar o pacote `Microsoft.AspNetCore.Authentication.Google`.
- Configurar o projeto para aceitar `user-secrets` para o desenvolvimento local.

### 2. Data & Models
#### [NEW] `Models/AppUser.cs` e `Models/UserRole.cs`
- Entidade do usuário com propriedades separadas para Role (usando enum) e Status:
  - `Id` (Guid)
  - `GoogleSubjectId` (string)
  - `Email` (string)
  - `Name` (string)
  - `Role` (UserRole enum) -> Admin ou Common
  - `IsApproved` (bool) -> true ou false
  - `CreatedAt` (DateTime)

#### [MODIFY] `Data/AppDbContext.cs`
- Adição do `DbSet<AppUser> Users`.

### 3. Autenticação & Controllers
#### [MODIFY] `Program.cs`
- Configurar autenticação via Cookies e Google. As credenciais do Google serão lidas das configurações (injetadas via user-secrets ou Azure).
- Adicionar Authorization Policies. Criaremos uma política para garantir que apenas usuários **aprovados** acessem o dashboard.

#### [NEW] `Controllers/AccountController.cs`
- Endpoints `Login`, `Logout` e o callback `GoogleResponse`.
- Lógica no callback: Se for o primeiro do banco, cria como `Role=UserRole.Admin` e `IsApproved=true`. Se não, cria como `Role=UserRole.Common` e `IsApproved=false`.
- Adiciona um `Claim` personalizado para indicar se o usuário está aprovado.

### 4. Atualização de Páginas e Rotas (Dashboard e Portal)
#### [MODIFY] `Pages/Portal.cshtml` e `.cs`
- Rota para `@page "/"`. Botão redireciona para login.

#### [MODIFY] `Pages/Index.cshtml` e `.cs`
- Rota para `@page "/dashboard"`.
- Adicionar `[Authorize(Policy = "ApprovedUser")]`.

#### [MODIFY] `Pages/Accesses.cshtml` e `Pages/AccessDetail.cshtml` 
- Adicionar `[Authorize(Policy = "ApprovedUser")]`.

#### [MODIFY] `Pages/Shared/_Layout.cshtml`
- Ajustar links do menu para `/dashboard`.
- Mostrar item "Gerenciar Usuários" apenas se `User.IsInRole("Admin")`.
- Exibir usuário logado e botão "Sair".

### 5. Novas Páginas (Admin e Pendente)
#### [NEW] `Pages/Users.cshtml` (e `.cs`)
- Rota `@page "/dashboard/users"`.
- Protegida por `[Authorize(Roles = "Admin")]`.
- Listagem e botões para aprovar/rejeitar e alterar permissões (Admin/Common).

#### [NEW] `Pages/PendingApproval.cshtml` (e `.cs`)
- Rota `@page "/pending"`.
- Acesso restrito a usuários autenticados mas *não aprovados*.

### 6. Pipeline CI/CD (GitHub Actions)
#### [NEW] `.github/workflows/main_usertracker-app.yml`
- Workflow automatizado. Quando houver `push` na branch `main`, ele fará o `dotnet build`, `dotnet publish` e usará a action `azure/webapps-deploy` para enviar os artefatos diretamente para a URL `usertracker-app.azurewebsites.net`.

## Verification Plan

### Testes Manuais
- Verificar navegação deslogada em `/` (Portal).
- Login com o 1º usuário (Admin automático).
- Teste de visualização do dashboard.
- Login com um 2º usuário (deverá cair na tela de `PendingApproval`).
- Login com Admin, aprovar o 2º usuário, alterar Role.
- Verificar que o CI/CD passa as variáveis de ambiente sem armazená-las no repositório.
