# Configuração de CI/CD (GitHub Actions para Azure)

Este documento descreve como configurar a esteira de integração e entrega contínua (CI/CD) para publicar automaticamente o projeto no **Azure App Service** sempre que houver um novo envio (push) para a branch `main`.

## 1. O Fluxo de Publicação
A nossa esteira está definida no arquivo `.github/workflows/main_usertracker-app.yml`. 
O fluxo realiza os seguintes passos:
1. Instala o SDK do .NET 8.
2. Realiza o build (compilação) e publish do projeto.
3. Conecta-se ao Azure utilizando um **Perfil de Publicação (Publish Profile)** de forma segura.
4. Faz o deploy dos arquivos compilados diretamente para o App Service.

## 2. Como configurar a conexão com o Azure
Para que o GitHub Actions tenha permissão para enviar os arquivos para o seu servidor, você precisa configurar um "Secret" (Segredo) nas configurações do repositório.

### Passo 2.1: Obter o Perfil de Publicação no Azure
1. Acesse o [Portal do Azure](https://portal.azure.com/).
2. Vá até o seu App Service.
3. Na página de Visão Geral (Overview), procure no menu superior pelo botão **Obter perfil de publicação** (Get publish profile).
   - > ⚠️ **Atenção:** Se ao clicar você receber o erro *"A autenticação básica está desabilitada"*, você precisa habilitá-la primeiro.
   - > **Como habilitar:** No menu lateral do seu App Service, vá em **Configurações > Configuração** (Configuration). Clique na aba **Configurações Gerais** (General settings). Role a página até encontrar **SCM Basic Auth Publishing Credentials** (Credenciais de publicação da Autenticação Básica SCM) e marque como **Ativado (On)**. Salve as alterações no topo da página e tente baixar o perfil novamente.
4. Com o download concluído, um arquivo com a extensão `.PublishSettings` será salvo no seu computador.
5. Abra esse arquivo no Bloco de Notas (ou qualquer editor de texto) e **copie todo o conteúdo**.

### Passo 2.2: Configurar o Secret no GitHub
1. Vá até a página principal do seu **repositório** no [GitHub](https://github.com) (a página onde ficam os seus arquivos de código).
2. Logo abaixo do nome do seu repositório, clique na aba **Settings** (com o ícone de engrenagem).
   - > ⚠️ **Importante:** Esta é a aba de configurações exclusivas do repositório, na mesma barra onde ficam as abas "Code", "Issues" e "Pull requests". **Não** clique na engrenagem do seu perfil pessoal no canto superior direito.
3. No menu lateral esquerdo dessa nova página, desça até a seção "Security", expanda a opção **Secrets and variables** e clique em **Actions**.
4. Clique no botão verde **New repository secret** (Novo segredo de repositório).
5. Preencha os campos da seguinte forma:
   - **Name:** `AZURE_PUBLISH_PROFILE`
   - **Secret:** *Cole aqui todo o conteúdo que você copiou do arquivo .PublishSettings.*
6. Clique em **Add secret**.

Pronto! A partir de agora, qualquer `git push` para a branch `main` irá disparar a esteira, compilar o código e publicá-lo automaticamente no Azure.

## 3. Segurança de Variáveis e Segredos (Environment Variables)
É uma **excelente prática de segurança** nunca colocar credenciais do sistema (como as chaves do Google Auth ou Connection Strings de banco de dados) no repositório do GitHub ou nos arquivos YAML do GitHub Actions.

A esteira de CI/CD tem apenas o papel de empacotar o seu código executável e enviá-lo para o servidor. As chaves sensíveis devem ser armazenadas e injetadas de forma isolada diretamente pelo painel do Azure App Service.

**Para configurar chaves da aplicação (ex: Google ClientId/Secret) no servidor:**
1. Acesse o [Portal do Azure](https://portal.azure.com/).
2. Vá até o seu App Service.
3. No menu lateral, em **Configurações** (Settings), clique em **Variáveis de Ambiente** (Environment variables) ou Configuração.
4. Adicione suas chaves criando novas "App Settings". Lembre-se que no Azure, a notação de hierarquia usa um **duplo sublinhado** `__` ao invés de dois pontos `:`.
   - Exemplo de Nome: `Authentication__Google__ClientId`
   - Exemplo de Valor: `12345-abcdef.apps.googleusercontent.com`
5. Salve as alterações (o Azure reiniciará a aplicação automaticamente para aplicar as novas chaves).

Dessa forma, seu repositório Git permanece limpo e 100% seguro contra o vazamento de credenciais em código público.
