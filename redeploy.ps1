docker-compose down

# $cert = Get-Content E:\Repo\github\test\certs\certificateProxy.cer -Raw
# $cert | docker create secret my_proxy_secret
# Write-Host $cert

#experimental will be available in future releases
#docker-compose build DOCKER_BUILDKIT=1

docker-compose up -d --build