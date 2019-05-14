# Local docker setup

Install docker for windows

Priority is to set a new subnet on the newly installed instance as the default is the business subnet

ssh into the vm - dockerNAT can be a bit of a pickle, best to use another switch, we'll likely create another in the future for dev only

docker-machine create -d "hyperv" --hyperv-virtual-switch "Default Switch" default

docker-machine -D ssh default

Follow below instructions:

##START - alternative way of creating a network
Below is what should be done, however it is much simplier to do it in portainer.io
https://success.docker.com/article/how-do-i-configure-the-default-bridge-docker0-network-for-docker-engine-to-a-different-subnet

run from another window outside of docker ssh session

docker-machine restart default

##END

Share both C and E drive on host

We'll also need to set the firewall correctly for the container access from the host
Set-NetConnectionProfile -interfacealias "vEthernet (Default Switch)" -NetworkCategory Private

You might also need to set a default dns of 8.8.8.8 - dont ask my why this matters, it just does. Docker on windows is just weird...

Fun stuff:

portainer is a must
https://www.portainer.io/

Run once:
docker volume create portainer_data

Run when needed:
docker run -d -p 9000:9000 -v /var/run/docker.sock:/var/run/docker.sock -v portainer_data:/data portainer/portainer

It will now be hosted on http://localhost:9000/

Create an admin account and password

Create a new network within portainer named my-development-network with a subnet of XXXX and ip range of XXXXX

Download source from repo - XXXX

Create migrations:
dotnet ef migrations add AspNetIdentityDbInit -c AdminIdentityDbContext -o Data/Migrations/Identity
dotnet ef database update -c AdminIdentityDbContext

dotnet ef migrations add LoggingDbInit -c AdminLogDbContext -o Data/Migrations/Logging
dotnet ef database update -c AdminLogDbContext

dotnet ef migrations add IdentityServerConfigurationDbInit -c IdentityServerConfigurationDbContext -o Data/Migrations/IdentityServerConfiguration
dotnet ef database update -c IdentityServerConfigurationDbContext

dotnet ef migrations add IdentityServerPersistedGrantsDbInit -c IdentityServerPersistedGrantDbContext -o Data/Migrations/IdentityServerGrants
dotnet ef database update -c IdentityServerPersistedGrantDbContext

run in command prompt - elevated:
powershell.exe .\1-init-docker.ps1

Docker will build all required resources and run migrations

Will need to possible export the asp.net cert for local host to %APPDATA%/ASP.NET/Https as localhost.pfx

WIP - docker CA has the localhost cer and pfx available and is able to pin sites to the 443 port. However they're not able to validate the certificate despite the cer being loaded into the CA on the docker instance. Issue can do away in core 3. People recommend nginx but that should not be needed. This is just alpine linux being difficult. https://github.com/dotnet/corefx/issues/35897


Open https://my.IdentityServer.com:81

You can log in with the default username and password from:
Skoruba.IdentityServer4.Admin.Configuration.Identity.Users