# Serviço de Autenticação JWT com .NET 6

Este projeto implementa um serviço de autenticação utilizando tokens JWT (JSON Web Tokens) desenvolvido com ASP.NET Core. O serviço oferece um sistema de autenticação baseado em chaves de API com diferentes níveis de permissão, permitindo o controle de acesso para recursos protegidos.

## Características

- Autenticação baseada em JWT
- Suporte a múltiplas chaves de API com diferentes funções (roles)
- Armazenamento seguro de credenciais utilizando AWS Secrets Manager (com suporte ao LocalStack para desenvolvimento)
- Endpoints protegidos com políticas de autorização baseadas em funções
- Compatível com .NET 6

## Requisitos

- .NET 6 SDK
- Docker (para executar o LocalStack)
- AWS CLI (para configuração local)

## Arquitetura

```mermaid
flowchart TB
  classDef controllers stroke-width:1px, stroke-dasharray:none, stroke:#FF5978, fill:#FFDFE5, color:#8E2236
  classDef services stroke-width:1px, stroke-dasharray:none, stroke:#374D7C, fill:#E2EBFF, color:#374D7C
  classDef models stroke-width:1px, stroke-dasharray:none, stroke:#46EDC8, fill:#DEFFF8, color:#378E7A
  classDef external stroke-width:1px, stroke-dasharray:none, stroke:#FBB35A, fill:#FFEFDB, color:#8F632D

  Client([Cliente HTTP])
  Client -->|1. Requisição de Login| AuthController
  Client -->|5. Requisição + Token| SecuredController

  subgraph Controllers
    AuthController[Auth Controller]:::controllers
    SecuredController[Secured Controller]:::controllers
  end

  subgraph Services
    JwtService[JWT Service]:::services
    AwsSecrets[AWS Secrets Service]:::services
    IAwsSecrets[IAwsSecretsService]:::services
  end

  subgraph Models
    LoginReq[LoginRequest]:::models
    LoginResp[LoginResponse]:::models
    ApiKey[ApiKeyModel]:::models
  end

  subgraph "External Storage"
    AWS[AWS Secrets Manager]:::external
    LocalStack[LocalStack]:::external
  end

  AuthController -->|2. Valida Credenciais| JwtService
  AuthController -->|2.1 Obtém API Keys| AwsSecrets
  AuthController -->|3. Cria| LoginResp
  AuthController -->|2.2 Usa| LoginReq
  AuthController -->|4. Retorna Token| Client

  SecuredController -->|6. Verifica JWT| JwtService

  JwtService -->|Obtém Chave de Assinatura| AwsSecrets
  AwsSecrets -->|Implementa| IAwsSecrets
  AwsSecrets -->|Usa| ApiKey

  AwsSecrets -->|Produção| AWS
  AwsSecrets -->|Desenvolvimento| LocalStack
```

## Configuração e Execução

### 1. Iniciar o LocalStack

O projeto usa LocalStack para simular serviços da AWS localmente durante o desenvolvimento.

```shell script
docker run -d -p 4566:4566 -p 4571:4571 localstack/localstack
```


### 2. Configurar Segredos no LocalStack

Execute o script de configuração para criar os segredos necessários:

```shell script
chmod +x setup-localstack.sh
./setup-localstack.sh
```


### 3. Executar o Projeto

```shell script
cd JwtAuthService
dotnet run
```


A API estará disponível em: `https://localhost:7099`

## Estrutura do Projeto

- **Controllers/**
    - `AuthController.cs` - Gerencia autenticação e emissão de tokens
    - `SecuredController.cs` - Endpoints protegidos para testar autorização

- **Models/**
    - `ApiKeyModel.cs` - Representação das chaves de API
    - `LoginRequest.cs` - Modelo para requisições de login
    - `LoginResponse.cs` - Resposta contendo token JWT

- **Services/**
    - `AwsSecretsService.cs` - Acesso aos segredos no AWS Secrets Manager
    - `JwtService.cs` - Geração e validação de tokens JWT

## Endpoints da API

### Autenticação

- **POST** `/api/Auth/token`
    - Gera um token JWT baseado nas credenciais fornecidas
    - Corpo da requisição:
```json
{
      "clientId": "backend-service-1",
      "clientSecret": "YourStrongSecret123!"
    }
```

- Resposta:
```json
{
      "token": "eyJhbGciOiJIUzI1...",
      "expiration": "2023-06-15T15:30:45Z"
    }
```


### Endpoints Protegidos

- **GET** `/api/Secured`
    - Endpoint protegido acessível a qualquer cliente autenticado
    - Requer cabeçalho de autorização: `Authorization: Bearer {token}`

- **GET** `/api/Secured/admin`
    - Endpoint protegido acessível apenas a clientes com função `Admin`
    - Requer cabeçalho de autorização: `Authorization: Bearer {token}`

## Testando com cURL

### Obter Token (Cliente Regular)

```shell script
curl -X POST https://localhost:7099/api/Auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "backend-service-1",
    "clientSecret": "YourStrongSecret123!"
  }'
```


### Obter Token (Cliente Admin)

```shell script
curl -X POST https://localhost:7099/api/Auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "admin-service",
    "clientSecret": "AdminStrongSecret456!"
  }'
```


### Acessar Endpoint Protegido

```shell script
curl -X GET https://localhost:7099/api/Secured \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```


### Acessar Endpoint Admin

```shell script
curl -X GET https://localhost:7099/api/Secured/admin \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```
