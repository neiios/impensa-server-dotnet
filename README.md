# Impensa dotNet

This repository contains the backend implementation for [Impensa](https://github.com/richard96292/impensa).

## Prerequisites

- dotNET Core SDK 8.0 with the ASP dotNET Core runtime
- Docker, Podman or other container runtime

## Development Setup

Create a `.env` file in the root of the repository.
The example is provided in the `.env.example` file.

```bash
# Create a new directory for Impensa and navigate into it
# The client can be cloned to the same directory
mkdir impensa
cd impensa

# Clone the server repository
git clone https://github.com/richard96292/impensa-server-dotnet server-dotnet
cd server-dotnet

# Run postgres container
docker compose up -d

# Restore the necessary dependencies
dotnet tool restore
dotnet restore

dotnet ef database update

# Run the project
dotnet run
```

After running these commands, you should be able to access the API at `http://localhost:5274`.

## Deployment

Ensure the docker images are built locally before deploying.
Put the Dockerfile in the folder above both the client and server repos.

```text
impensa
├── client
├── server-dotnet
└── Dockerfile
```

```bash
sudo docker build -t impensa-server-dotnet:latest .
```

Use the docker-compose file in the `deploy` folder to deploy the application.
Fill in the necessary environment variables before deploying.

## Second Increment

- [x] Add email notifications on registration
- [x] Migrate to .NET 8
- [x] Allow users to specify expense date
- [x] Notifications for user actions
- [x] Account removal
- [x] Contact/bug report form
- [ ] Admin panel with reports
- [ ] Password recovery
- [x] oAuth with github to create an account
- [x] Docker support
- [x] Deploy the application

## License

This project is free software and is distributed under
the [AGPL (GNU Affero General Public License)](https://www.gnu.org/licenses/agpl-3.0.en.html).
