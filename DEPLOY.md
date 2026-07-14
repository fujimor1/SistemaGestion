# Despliegue a producción — Termales Collpa

Servidor: `srv1818017` (acceso por SSH como `root`).

## Mapa del servidor

| Componente | Detalle |
|---|---|
| API backend | Servicio systemd `collpa-api`, corre en `/var/www/collpa-api`, ejecutable `Termales.API` (apphost linux-x64, framework-dependent), escucha en `http://localhost:5018` |
| Frontend | React + Vite, servido por nginx como estático desde `/var/www/collpa-front` (repo: [`fujimor1/collpa-front`](https://github.com/fujimor1/collpa-front), **no** el `index.html` de este repo — ese es solo un mockup viejo, ignorarlo) |
| Base de datos | PostgreSQL, base `collpa_db`, usuario `postgres` (ver contraseña en `Termales.API/appsettings.json` → `ConnectionStrings:TermalesDb`) |
| nginx configs | `/etc/nginx/sites-available/collpa-api` (proxy a `localhost:5018`, dominio `api.termalescollpa.cloud`) y `/etc/nginx/sites-available/collpa-front` (root estático, dominio `termalescollpa.cloud`) |
| Runtime servidor | .NET 8.0.28 (solo runtime ASP.NET Core instalado, no SDK) — por eso el publish debe ser framework-dependent, no self-contained |

## 1 y 2. Backend y Frontend — deploy automático por GitHub Actions

Desde julio 2026, el backend (`fujimor1/SistemaGestion`, este repo) y el frontend (`fujimor1/collpa-front`) tienen workflows de GitHub Actions:

- **`.github/workflows/deploy-backend.yml`** (en este repo)
- **`.github/workflows/deploy-frontend.yml`** (en `collpa-front`)

Flujo:
1. `git push` a `main` → dispara solo el job de build/publish (compila y deja el artefacto listo, no toca el servidor).
2. Para desplegar de verdad: pestaña **Actions** del repo correspondiente → workflow **Deploy Backend** / **Deploy Frontend** → botón **Run workflow** → confirmar rama `main`.
3. El job `deploy` usa el environment `production` (requiere los secrets `SSH_HOST`, `SSH_USER`, `SSH_PORT`, `SSH_PRIVATE_KEY` configurados ahí — la llave privada es una llave SSH dedicada solo para Actions, distinta a la personal). En `SistemaGestion` además pide aprobación manual (Required reviewers) antes de correr.
4. El script en el servidor hace lo mismo que antes se hacía a mano: backup de la carpeta actual (`.bak.<fecha>`), reemplazo de archivos, y (solo backend) `chmod +x` + `systemctl restart collpa-api`.

⚠️ El workflow del backend preserva el `appsettings.json` real del servidor — nunca sube el de desarrollo.

### Método manual (respaldo/emergencia, si Actions no está disponible)

**Backend, en tu máquina:**
```bash
cd D:/Proyectos/Collpa/Termales.API
dotnet publish -c Release -r linux-x64 --self-contained false -o ../publish_linux
cd ..
tar -czf publish_linux.tar.gz -C publish_linux .
scp "D:/Proyectos/Collpa/publish_linux.tar.gz" root@servidor:/tmp/
```

**En el servidor:**
```bash
cp -r /var/www/collpa-api /var/www/collpa-api.bak.$(date +%Y%m%d%H%M)
cp /var/www/collpa-api/appsettings.json /tmp/appsettings.prod.json.bak

mkdir -p /tmp/collpa-api-new
tar -xzf /tmp/publish_linux.tar.gz -C /tmp/collpa-api-new
cp /tmp/appsettings.prod.json.bak /tmp/collpa-api-new/appsettings.json
rm -rf /var/www/collpa-api/*
cp -r /tmp/collpa-api-new/* /var/www/collpa-api/
chown -R www-data:www-data /var/www/collpa-api
chmod +x /var/www/collpa-api/Termales.API   # IMPORTANTE: sin esto falla con "203/EXEC"

systemctl restart collpa-api
systemctl status collpa-api --no-pager
journalctl -u collpa-api -n 50 --no-pager
```

**Frontend, en tu máquina** (proyecto real en `D:\Proyectos\collpa-front`, Vite + React + TS + antd):
```bash
cd D:/Proyectos/collpa-front
npm run build          # genera dist/ con .env.production (VITE_API_URL=https://api.termalescollpa.cloud/api)
tar -czf /tmp/collpa-front-dist.tar.gz -C dist .
scp /tmp/collpa-front-dist.tar.gz root@servidor:/tmp/
```

**En el servidor:**
```bash
cp -r /var/www/collpa-front /var/www/collpa-front.bak.$(date +%Y%m%d%H%M)
rm -rf /var/www/collpa-front/*
tar -xzf /tmp/collpa-front-dist.tar.gz -C /var/www/collpa-front
chown -R www-data:www-data /var/www/collpa-front
```

Verificar:
```bash
curl -s https://termalescollpa.cloud/ | grep -o '<title>.*</title>'
```

## 3. Aplicar migraciones de base de datos (siempre manual)

**Esto nunca se automatiza.** La base de datos de producción tiene datos reales; la local casi no tiene, así que una migración que en local corre perfecto puede fallar o perder datos en producción (columnas `NOT NULL` sin default sobre filas existentes, `DROP COLUMN` sobre datos reales, cambios de tipo incompatibles, violaciones de `UNIQUE`/`FK` que en local nunca se ven). Por eso este paso se queda 100% a mano, revisado por ti cada vez.

`migration.sql` (raíz de este repo) es el script acumulado de EF Core — es **idempotente**: cada bloque `DO` revisa `__EFMigrationsHistory` antes de aplicarse, así que correr el archivo completo de nuevo no rompe nada.

### 3.1 Antes de nada: revisa el script generado
Busca operaciones riesgosas antes de subirlo al servidor:
```bash
grep -nE "DROP COLUMN|DROP TABLE|ALTER COLUMN.*TYPE|RENAME" migration.sql
```
Si aparece algo nuevo (no aplicado ya antes) que toque una tabla con datos reales, párate ahí y piensa qué pasa con esos datos antes de continuar. Si tienes dudas, prueba la migración contra una copia real de los datos de producción (ver 3.2) antes de tocar el servidor.

### 3.2 (Opcional pero recomendado para migraciones riesgosas) Probar contra una copia de los datos reales
Como la base local casi no tiene datos, no vas a detectar estos problemas ahí. Para migraciones que toquen columnas/tablas con datos importantes:
```bash
# En el servidor: dump de producción
sudo -u postgres pg_dump collpa_db -F c -f /tmp/prod_test.dump
# Bajarlo a tu máquina
scp root@servidor:/tmp/prod_test.dump ./
# Restaurarlo en una BD local de prueba (no la que usas a diario) y correr ahí la migración primero
```

### 3.3 Backup antes de aplicar en producción (siempre, sin excepción)
```bash
sudo -u postgres pg_dump collpa_db -F c -f /tmp/collpa_db_$(date +%Y%m%d%H%M).dump
```
Así, si algo sale mal al correr `migration.sql`, se puede restaurar con `pg_restore` en minutos.

### 3.4 Aplicar
```bash
scp "D:/Proyectos/Collpa/migration.sql" root@servidor:/tmp/
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

- [ ] `git push` del código a GitHub (`origin/main`) — en el repo que corresponda (backend o `collpa-front`)
- [ ] Backend: `Run workflow` en Actions (o método manual), revisar que el job `deploy` quedó en verde y `journalctl` sin errores
- [ ] Frontend: `Run workflow` en Actions (o método manual)
- [ ] Migración de BD aplicada a mano, con backup (`pg_dump`) hecho antes (si hay cambios de schema)
- [ ] Probar login real: `curl -X POST https://api.termalescollpa.cloud/api/auth/login -H "Content-Type: application/json" -d '{"email":"...","password":"..."}'` → debe dar `200 OK` con token
- [ ] Probar frontend en el navegador (no solo `curl`, para pescar errores de JS/CORS/runtime)
