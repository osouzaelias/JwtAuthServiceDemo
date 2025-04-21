#!/bin/bash

echo "Testando serviço JWT Auth..."

# Definindo URL base
BASE_URL="https://localhost:7099"

# Obtendo token para cliente regular
echo "Obtendo token para cliente regular..."
REGULAR_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/Auth/token" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "backend-service-1",
    "clientSecret": "YourStrongSecret123!"
  }')

REGULAR_TOKEN=$(echo $REGULAR_RESPONSE | grep -o '"token":"[^"]*' | sed 's/"token":"//')
echo "Token obtido: ${REGULAR_TOKEN:0:20}..."

# Obtendo token para admin
echo -e "\nObtendo token para cliente admin..."
ADMIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/Auth/token" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "admin-service",
    "clientSecret": "AdminStrongSecret456!"
  }')

ADMIN_TOKEN=$(echo $ADMIN_RESPONSE | grep -o '"token":"[^"]*' | sed 's/"token":"//')
echo "Token obtido: ${ADMIN_TOKEN:0:20}..."

# Testando endpoint seguro com cliente regular
echo -e "\nAcessando endpoint seguro com cliente regular..."
curl -s -X GET "${BASE_URL}/api/Secured" \
  -H "Authorization: Bearer $REGULAR_TOKEN"

# Testando endpoint de admin com cliente regular (deve falhar)
echo -e "\n\nAcessando endpoint de admin com cliente regular (deve falhar)..."
curl -s -X GET "${BASE_URL}/api/Secured/admin" \
  -H "Authorization: Bearer $REGULAR_TOKEN"

# Testando endpoint de admin com cliente admin
echo -e "\n\nAcessando endpoint de admin com cliente admin..."
curl -s -X GET "${BASE_URL}/api/Secured/admin" \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Testando com credenciais inválidas
echo -e "\n\nTestando com credenciais inválidas..."
curl -s -X POST "${BASE_URL}/api/Auth/token" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "cliente-invalido",
    "clientSecret": "senha-errada"
  }'

echo -e "\n\nTestes concluídos!"