# Configuração das Credenciais do Google (OAuth)

Para que o login com o Google funcione na aplicação UserTracker, você precisa criar e configurar um projeto no Google Cloud para obter o **ClientId** e o **ClientSecret**. Siga o passo a passo abaixo:

## 1. Criar o Projeto no Google Cloud
1. Acesse o [Google Cloud Console](https://console.cloud.google.com/).
2. Faça login com sua conta do Google.
3. No topo da página (ao lado do logo do Google Cloud), clique em **Selecione um projeto** (ou no nome do projeto atual) e clique no botão **Novo Projeto**.
4. Dê um nome ao projeto (ex: `UserTracker App`) e clique em **Criar**.
5. Aguarde a criação e certifique-se de que o novo projeto está selecionado no topo da página.

## 2. Configurar a Tela de Consentimento OAuth
*Esta é a tela que os usuários verão quando clicarem em "Fazer login com o Google".*
1. No menu lateral esquerdo, vá em **APIs e Serviços** > **Tela de consentimento OAuth**.
2. Em `User Type`, selecione **Externo** (External) e clique em **Criar**.
3. Preencha as informações obrigatórias:
   - **Nome do app:** `UserTracker`
   - **E-mail de suporte do usuário:** Seu e-mail
   - **Dados de contato do desenvolvedor:** Seu e-mail
4. Clique em **Salvar e continuar** nas próximas etapas (Escopos e Usuários de teste) sem precisar alterar nada, até o Resumo. Volte para o painel.
5. *(Opcional)* Como seu aplicativo ficará publicado para você testar, você pode clicar em **Publicar Aplicativo** na tela de consentimento para não precisar adicionar os e-mails manualmente nos usuários de teste.

## 3. Criar as Credenciais (ID e Secret)
1. No menu lateral, clique em **Credenciais**.
2. No topo, clique em **+ CRIAR CREDENCIAIS** e escolha **ID do cliente OAuth**.
3. Em `Tipo de aplicativo`, selecione **Aplicativo da Web**.
4. Em `Nome`, coloque algo como `UserTracker Web`.
5. **URIs de redirecionamento autorizados** (MUITO IMPORTANTE):
   Aqui você precisa colocar as URLs exatas para onde o Google vai devolver o usuário após o login. Adicione os caminhos tanto do seu ambiente local quanto do ambiente de produção.
   - Clique em **+ ADICIONAR URI** e insira: `http://localhost:5000/signin-google`
   - Clique em **+ ADICIONAR URI** e insira: `https://localhost:5001/signin-google` *(se usar HTTPS local)*
   - Clique em **+ ADICIONAR URI** e insira o do seu ambiente de produção (ex: `https://seu-app-service.azurewebsites.net/signin-google`)
6. Clique em **Criar**.

## 4. Salvar suas chaves (Desenvolvimento Local)
O Google exibirá um pop-up contendo o **ID do cliente** e a **Chave secreta do cliente**. Copie os dois.

No terminal, na raiz do seu projeto, configure as chaves locais executando:
```bash
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "COLO_AQUI_O_ID_DO_CLIENTE"
dotnet user-secrets set "Authentication:Google:ClientSecret" "COLE_AQUI_A_CHAVE_SECRETA"
```

## 5. Configurar no Azure (Produção)
Para a aplicação funcionar em produção, você precisará inserir essas chaves de forma segura:
1. Acesse o [Portal do Azure](https://portal.azure.com/).
2. Vá até o seu App Service no Azure.
3. No menu lateral, em **Configurações** (Settings), clique em **Variáveis de Ambiente** (Environment variables).
4. Adicione duas novas variáveis de aplicativo (App settings):
   - **Nome:** `Authentication__Google__ClientId` *(no Azure usamos duplo sublinhado `__` como separador)*
   - **Valor:** `[SEU CLIENT ID]`
   - **Nome:** `Authentication__Google__ClientSecret`
   - **Valor:** `[SEU CLIENT SECRET]`
5. Clique em **Aplicar** / **Salvar** e reinicie o App Service para que as mudanças façam efeito.
