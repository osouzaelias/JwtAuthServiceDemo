#!/bin/bash
# Script para inicializar segredos no LocalStack

# Verifique se o LocalStack está rodando
curl -s http://localhost:4566/health > /dev/null
if [ $? -ne 0 ]; then
  echo "LocalStack não está rodando. Inicie-o primeiro."
  exit 1
fi

# Crie os segredos no Secrets Manager do LocalStack
echo "Criando segredos no LocalStack..."

# Segredo da chave de assinatura JWT
aws --endpoint-url=http://localhost:4566 secretsmanager create-secret \
  --name jwt-signing-key \
  --secret-string "YourSuperSecureJwtSigningKey_AtLeast32CharactersLong!" \
  --region us-east-1

# Segredo das chaves de API dos backends
aws --endpoint-url=http://localhost:4566 secretsmanager create-secret \
  --name backend-api-keys \
  --secret-string '[{"clientId":"backend-service-1","clientSecret":"YourStrongSecret123!","roles":["Service","DataReader"]},{"clientId":"admin-service","clientSecret":"AdminStrongSecret456!","roles":["Service","Admin"]}]' \
  --region us-east-1

echo "Segredos criados com sucesso!"