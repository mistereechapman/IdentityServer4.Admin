# 1. create infrastructure
# remove docker network if exists
#docker network remove my-development-network

# create docker network
#docker network create my-development-network --subnet

# create docker volume for sql containter
#below has been commented out as the creation of a persistent volume to local windows file system from an unix fs is freaking difficult to create
#instead its managed by an env variable and injected to the compose script
#docker volume create --name my-identityserver-sql

# get password from local file and write to configs
$envFileContent = Get-Content .env
$passwordLine = $envFileContent | Where-Object { $_ -like 'SA_PASSWORD*'} | Select-Object -First 1
$persistentStorageDir = $envFileContent | Where-Object { $_ -like 'PERSISTENT_STORAGE_LOCATION*' } | Select-Object -First 1
$type = [System.Security.Cryptography.X509Certificates.X509ContentType]::Cert

$destAdminFileName = "src/Skoruba.IdentityServer4.Admin/appsettings.json"
$srcAdminFileName = "src/Skoruba.IdentityServer4.Admin/appsettings.json.orig"

$destSTSFileName = "src/Skoruba.IdentityServer4.STS.Identity/appsettings.json"
$srcSTSFileName = "src/Skoruba.IdentityServer4.STS.Identity/appsettings.json.orig"

if(!($passwordLine) -or !($persistentStorageDir))
{
    Write-Error "No SA_PASSWORD or PERSISTENT_STORAGE_LOCATION set in environment file."
    Write-Error "Add these variables and retry"

}else{

    if(!(Test-Path -Path certs))
    {
        New-Item -ItemType Directory -Force -Path certs
        #needs to be at least 1 file in there - even if its of an invalid type - in order for the copy routine to work
        New-Item -ItemType File -Force -Path certs -Value "" -Name "fake"

        Add-Content -Path C:\Windows\System32\drivers\etc\hosts -Value "127.0.0.1`tmy.IdentityServer.com" -Force
    }

    $proxyInfo = (docker info | Select-String -Pattern "HTTP Proxy:")
    $regexForProxyAddress = '(?<=PROXY_ADDRESS=)*'
    if(($proxyInfo))
    {
        Write-Host "Proxy found - setting environment file"

        $proxyPartsFromDocker = (($proxyInfo -split ':')[1]).trim()

        (Get-Content .env) -replace $regexForProxyAddress,"$proxyPartsFromDocker" | Set-Content .env
    } else
    {
        Write-Host "No Proxy found - setting environment file"
        (Get-Content .env) -replace $regexForProxyAddress,"" | Set-Content .env
    }

    #ifproxy exists then store aside for attaching to build host
    Set-Location certs

    Get-Childitem -path Cert:\LocalMachine\Root\  | Where-Object {
        $_.Subject -like '*CN=Zscaler*'
    } | ForEach-Object {
            
        $hash = $_.GetCertHashString()

        [System.IO.File]::WriteAllBytes("$hash.cer", $_.export($type) )
        
        $cert = get-content "..\$hash.cer" -Encoding Byte
        $content = @(
        '-----BEGIN CERTIFICATE-----'
        [System.Convert]::ToBase64String($cert, 'InsertLineBreaks')
        '-----END CERTIFICATE-----'
        )

        $content | Out-File -FilePath "$hash.cer" -Encoding ASCII
    }

    Set-Location ../ 
    
    $storageDirParts = $persistentStorageDir.split("=")
    New-Item -ItemType Directory -Force -Path "$($storageDirParts[1])\my-identityserver4-sql"
    New-Item -ItemType Directory -Force -Path "$($storageDirParts[1])\my-identityserver4-webapp"

    $parts = $passwordLine.split("=")

    if(-not(Test-Path -Path $srcAdminFileName)){
        Copy-Item $destAdminFileName $srcAdminFileName
    }else{
        #restore original
        Remove-Item $destAdminFileName
        Copy-Item $srcAdminFileName $destAdminFileName
    }

    $configFileContent = Get-Content -Path $destAdminFileName
    $configFileContent | ForEach-Object{ $_ -replace "#REPLACE_SAPASSWORD#", $parts[1] } | Set-Content $destAdminFileName

    if(-not(Test-Path -Path $srcSTSFileName)){
        Copy-Item $destSTSFileName $srcSTSFileName
    }else{
        #restore original
        Remove-Item $destSTSFileName
        Copy-Item $srcSTSFileName $destSTSFileName
    }
    
    $configFileContent = Get-Content -Path $destSTSFileName
    $configFileContent | ForEach-Object{ $_ -replace "#REPLACE_SAPASSWORD#", $parts[1] } | Set-Content $destSTSFileName


    dotnet user-secrets set "IDS:AdminSecret" $parts[1] --project ".\src\Skoruba.IdentityServer4.Admin"
    dotnet user-secrets set "IDS:STSSecret" $parts[1] --project ".\src\Skoruba.IdentityServer4.STS.Identity"
}


docker-compose down

docker-compose up -d --build --quiet-pull

Write-Host "Note that you only really need to run this script if something changes with the docker running environment" -ForegroundColor DarkYellow
Write-Host "If you need to start the containers up again without re-configuring the networks and storage again then run redeploy.cmd" -ForegroundColor DarkYellow

#return environment to normal
#restore original
Copy-Item $srcSTSFileName $destSTSFileName
Remove-Item $srcSTSFileName

Copy-Item $srcAdminFileName $destAdminFileName
Remove-Item $srcAdminFileName