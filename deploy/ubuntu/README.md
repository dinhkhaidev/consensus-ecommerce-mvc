# Ubuntu Deployment

This folder contains the native Ubuntu deployment setup for the ASP.NET Core MVC app.

## Files

- `deploy.sh`: publishes the app and installs it to `/var/www/consensus`.
- `consensus.service`: systemd service that runs Kestrel on `127.0.0.1:5000` for native deploy.
- `nginx-consensus.conf`: Nginx reverse proxy to the Kestrel app.
- `consensus.env.example`: production environment variables.

## Server Prerequisites

Install these on Ubuntu before deploying:

```bash
sudo apt update
sudo apt install -y nginx
```

Install .NET 8 SDK or at least build on another machine and copy the published output. If building on the server, the `dotnet` command must be available.

## Option A: Docker App + System Nginx

This is the fastest Ubuntu setup. Docker runs the app and SQL Server, while the host Nginx proxies to the app.

```bash
docker compose up -d --build
sudo cp deploy/ubuntu/nginx-consensus.conf /etc/nginx/sites-available/consensus
sudo ln -sfn /etc/nginx/sites-available/consensus /etc/nginx/sites-enabled/consensus
sudo nginx -t
sudo systemctl reload nginx
```

The Docker MVC app listens on host port `5000`, and Nginx proxies to `127.0.0.1:5000`.

The Docker MVC app reads app settings from the project root `.env` through `env_file`. The SQL Server container still uses the compose SQL password unless you add a dedicated SQL password variable and update `docker-compose.yml`.

## Option B: Native App + Systemd + System Nginx

Use this if you want to run the .NET app directly on Ubuntu instead of inside Docker.

## Database

If using the provided Docker SQL Server for the native app, start only the database services:

```bash
docker compose up -d mssql mssql-init
```

The compose init container runs `furnish_all_in_one.sql` automatically. Your `.env` should point the native app to the host SQL port:

```text
Server=127.0.0.1,1433;Database=ShopDb;User Id=sa;Password=Strong123!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Deploy Native App

From the project root:

```bash
bash deploy/ubuntu/deploy.sh
```

If `/etc/consensus/consensus.env` does not exist, `deploy.sh` copies the project root `.env` to that path before starting the service. If neither file exists, the script stops.

Edit Nginx for your real domain if needed:

```bash
sudo nano /etc/nginx/sites-available/consensus
```

Restart after changes:

```bash
sudo systemctl restart consensus
sudo nginx -t
sudo systemctl reload nginx
```

Useful logs:

```bash
sudo systemctl status consensus
sudo journalctl -u consensus -f
```
