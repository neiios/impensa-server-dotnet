# Impensa WebAPI

This repository contains the backend implementation for [Impensa](https://github.com/richard96292/impensa).

## Prerequisites

- .NET Core SDK 7.0

## Development Setup & Installation

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
dotnet restore

# Run the project
dotnet run
```

After running these commands, you should be able to access the API at `http://localhost:5274`.

## License

This project is free software and is distributed under the [AGPL (GNU Affero General Public License)](https://www.gnu.org/licenses/agpl-3.0.en.html).
