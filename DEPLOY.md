# Despliegue a producción — Termales Collpa

Servidor: `srv1818017` (acceso por SSH, comandos con `sudo`).

## Mapa del servidor

| Componente | Detalle |
|---|---|
| API backend | Servicio systemd `collpa-api`, corre en `/var/www/collpa-api`, ejecutable `Termales.API` (apphost linux-x64, framework-dependent), escucha en `http://localhost:5018` |
| Frontend | React + Vite, servido por nginx como estático desde `/var/www/collpa-front` (repo fuente: `D:\Proyectos\collpa-front`, **no** el `index.html` de este repo — ese es solo un mockup viejo, ignorarlo) |
| Base de datos | PostgreSQL, base `collpa_db`, usuario `postgres` (ver contraseña en `Termales.API/appsettings.json` → `ConnectionStrings:TermalesDb`) |
| nginx configs | `/etc/nginx/sites-available/collpa-api` (proxy a `localhost:5018`, dominio `api.termalescollpa.cloud`) y `/etc/nginx/sites-available/collpa-front` (root estático, dominio `termalescollpa.cloud`) |
| Runtime servidor | .NET 8.0.28 (solo runtime ASP.NET Core instalado, no SDK) — por eso el publish debe ser framework-dependent, no self-contained |

## 1. Actualizar el backend (API)

**En tu máquina:**
```bash
cd D:/Proyectos/Collpa/Termales.API
dotnet publish -c Release -r linux-x64 --self-contained false -o ../publish_linux
cd ..
tar -czf publish_linux.tar.gz -C publish_linux .
scp "D:/Proyectos/Collpa/publish_linux.tar.gz" usuario@servidor:/tmp/
```

**En el servidor:**
```bash
# Backup (por si hay que revertir)
sudo cp -r /var/www/collpa-api /var/www/collpa-api.bak.$(date +%Y%m%d%H%M)
sudo cp /var/www/collpa-api/appsettings.json /tmp/appsettings.prod.json.bak

# Extraer y reemplazar, preservando el appsettings.json real de producción
sudo mkdir -p /tmp/collpa-api-new
sudo tar -xzf /tmp/publish_linux.tar.gz -C /tmp/collpa-api-new
sudo cp /tmp/appsettings.prod.json.bak /tmp/collpa-api-new/appsettings.json
sudo rm -rf /var/www/collpa-api/*
sudo cp -r /tmp/collpa-api-new/* /var/www/collpa-api/
sudo chown -R www-data:www-data /var/www/collpa-api
sudo chmod +x /var/www/collpa-api/Termales.API   # IMPORTANTE: sin esto falla con "203/EXEC"

sudo systemctl restart collpa-api
sudo systemctl status collpa-api --no-pager
sudo journalctl -u collpa-api -n 50 --no-pager
```

⚠️ **Nunca copiar el `appsettings.json` generado localmente al servidor** — trae credenciales de desarrollo y sobrescribiría la config real de producción.

## 2. Actualizar el frontend (React)

El proyecto real está en `D:\Proyectos\collpa-front` (Vite + React + TS + antd), **no** en este repo.

**En tu máquina:**
```bash
cd D:/Proyectos/collpa-front
npm run build          # genera dist/ con .env.production (VITE_API_URL=https://api.termalescollpa.cloud/api)
tar -czf /tmp/collpa-front-dist.tar.gz -C dist .
scp /tmp/collpa-front-dist.tar.gz usuario@servidor:/tmp/
```

**En el servidor:**
```bash
sudo cp -r /var/www/collpa-front /var/www/collpa-front.bak.$(date +%Y%m%d%H%M)
sudo tar -xzf /tmp/collpa-front-dist.tar.gz -C /var/www/collpa-front
sudo chown -R www-data:www-data /var/www/collpa-front
```

Verificar:
```bash
curl -s https://termalescollpa.cloud/ | grep -o '<title>.*</title>'
```

## 3. Aplicar migraciones de base de datos

`migration.sql` (raíz de este repo) es el script acumulado de EF Core — es **idempotente**: cada bloque `DO` revisa `__EFMigrationsHistory` antes de aplicarse, así que correr el archivo completo de nuevo no rompe nada.

```bash
scp "D:/Proyectos/Collpa/migration.sql" usuario@servidor:/tmp/
```
En el servidor:
```bash
sudo -u postgres psql -d collpa_db -f /tmp/migration.sql
```

Verificar que una migración específica quedó aplicada:
```bash
sudo -u postgres psql -d collpa_db -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '<id_de_la_migracion>';"
```

Nota: `sudo -u postgres psql` usa autenticación por socket/peer (no pide contraseña), así que puede funcionar aunque la contraseña en `appsettings.json` esté desincronizada con el rol `postgres`. Si el login de la API falla con `password authentication failed for user "postgres"`, hay que alinear la contraseña real del rol con la que tiene `appsettings.json`:
```bash
sudo -u postgres psql -c "ALTER USER postgres PASSWORD '<la_misma_que_appsettings.json>';"
```

## 4. Checklist rápido antes de dar por cerrado un deploy

- [ ] `git push` del código a GitHub (`origin/main`)
- [ ] Backend: publish `linux-x64`, subir, `chmod +x`, restart, revisar `journalctl`
- [ ] Frontend: build con `.env.production` correcto, subir `dist/` completo
- [ ] Migración de BD aplicada (si hay cambios de schema)
- [ ] Probar login real: `curl -X POST https://api.termalescollpa.cloud/api/auth/login -H "Content-Type: application/json" -d '{"email":"...","password":"..."}'` → debe dar `200 OK` con token
- [ ] Probar frontend en el navegador (no solo `curl`, para pescar errores de JS/CORS/runtime)
