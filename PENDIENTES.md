# Pendientes

Registro de cosas detectadas que quedan para resolver después (no se tocan hasta que
se decida explícitamente hacerlo).

---

## 1. Diferencia mal calculada en el cierre de caja del 21 de julio de 2026 (producción)

**Estado:** Pendiente — el usuario decidió corregirlo al final del día, no ahora (2026-07-22).

### Qué pasó

El 21 de julio se registró un egreso de caja chica de **S/ 15.00** ("FLETE - TRANSPORTE DE
PINTURA, ESCALERA Y ACCESORIOS PARA TERMA"). Ese mismo día se cerró la caja, pero el cierre
se hizo **antes** de que estuviera desplegado en producción el fix que hace que la
"Diferencia" del cierre reste los egresos del día (commit `ba7f140` en el backend,
"Corregir cuadre de caja: descontar egresos y encontrar apertura/cierre").

Por eso el cierre del 21 quedó guardado con la fórmula vieja (`Diferencia = TotalContado -
VentasSistema`, sin restar egresos), en vez de la fórmula correcta (`Diferencia =
TotalContado - (MontoApertura + VentasSistema - Egresos)`).

**Importante:** `CierreCaja.Diferencia` es un valor que se **guarda en el momento del
cierre** — no se recalcula solo después. Aunque el backend ya tiene el fix corriendo, un
cierre que ya existe en la base de datos se queda con el número viejo para siempre, a
menos que se corrija manualmente. Los cierres de hoy en adelante ya calculan bien.

### Datos exactos (de producción, consultados el 2026-07-22 vía
`GET /api/reportes/liquidacion-caja?fecha=2026-07-21`)

| Campo | Valor |
|---|---|
| Fecha | 2026-07-21 |
| Monto apertura | S/ 0.00 |
| Ventas del sistema | S/ 1,495.50 |
| Egresos caja chica | S/ 15.00 (1 egreso: "FLETE - TRANSPORTE DE PINTURA, ESCALERA Y ACCESORIOS PARA TERMA") |
| Efectivo contado (físico, ingresado por el cajero al cerrar) | S/ 1,081.00 |
| Yape contado (físico) | S/ 414.50 |
| Total contado | S/ 1,495.50 |
| **Diferencia guardada actualmente** | **S/ 0.00** ← incorrecta |
| **Diferencia correcta** | **S/ 15.00** (sobrante) |

### Por qué da "sobrante" y no "faltante"

```
Esperado en caja = Apertura + Ventas - Egresos
                  = 0 + 1,495.50 - 15.00
                  = 1,480.50

Diferencia real = Total contado - Esperado
                = 1,495.50 - 1,480.50
                = +15.00  (sobra S/15 respecto a lo esperado)
```

Esto sugiere que el egreso se registró en el sistema, pero el dinero de ese flete
probablemente **no se sacó físicamente de la caja** al momento de contar (o se contó la
caja antes de sacar ese efectivo) — vale la pena revisarlo con quien cerró esa caja antes
de corregir el número a ciegas.

### Cómo corregirlo cuando se decida hacerlo

1. Identificar el `cierre_caja_id` exacto del 21 de julio:
   ```bash
   sudo -u postgres psql -d collpa_db -c "SELECT cierre_caja_id, fecha, diferencia, total_egresos FROM caja.cierres_caja WHERE fecha::date = '2026-07-21';"
   ```
2. Backup antes de tocar nada:
   ```bash
   sudo -u postgres pg_dump collpa_db -F c -f /tmp/collpa_db_$(date +%Y%m%d%H%M).dump
   ```
3. Actualizar la diferencia (reemplazar `<id>` por el que salga en el paso 1):
   ```bash
   sudo -u postgres psql -d collpa_db -c "UPDATE caja.cierres_caja SET diferencia = 15.00 WHERE cierre_caja_id = <id>;"
   ```
4. Verificar con el mismo endpoint de Liquidación de Caja que ahora sí muestre
   `"diferencia":15.00` en vez de `0.00`.

No hace falta migración ni cambio de código — es solo corregir el dato guardado de ese
día puntual.

---

## 2. Ticket de Baños Termales — CANT ahora muestra 1 en vez de la cantidad de personas

**Estado:** Aplicado en el frontend (2026-07-23), a pedido del usuario. Queda anotado como
incidente por si se decide revertir — ver "Cómo volver a como estaba antes".

### Qué pasó

Los pozeros se quejaban de que el ticket de Baños Termales mostraba, por ejemplo:

```
CANT  DESCRIPCIÓN         P.U.     TOTAL
3     Piscina (3 pers.)   S/5.00   S/15.00
```

y tenían que explicarle al cliente que "CANT 3" no eran 3 piscinas, sino 1 piscina para 3
personas (el precio es por persona). El problema es que `Cantidad` se usaba para dos cosas
a la vez: cuántas personas entran, y la columna "CANT" del ticket (que naturalmente se lee
como "cuántas unidades de este producto").

### Qué se cambió (solo frontend, solo el ticket recién emitido)

En `collpa-front/src/pages/caja/VentasPage.tsx`, función `handleCobrar` de `TabBanios`, el
armado de `itemsRecibo` que alimenta el ticket impreso (`buildHtml`/`printTicket`) pasó de:

```ts
descripcion:    `${i.nombre} (${i.cantidad} pers.)`,
cantidad:       i.cantidad,
precioUnitario: i.precioUnitario,
subtotal:       i.precioUnitario * i.cantidad,
```

a:

```ts
descripcion:    `${i.nombre} (${i.cantidad} pers.)`,
cantidad:       1,
precioUnitario: i.precioUnitario * i.cantidad,
subtotal:       i.precioUnitario * i.cantidad,
```

Ahora el ticket muestra `CANT 1 | Piscina (3 pers.) | S/15.00 | S/15.00` — una sola línea
(el pase del grupo), con la cantidad de personas solo en el texto, no en la columna CANT.

### Lo que NO se tocó (limitación conocida)

El backend (`Termales.BLL/Services/ComprobanteService.cs`, método `GenerarComprobanteBanio`)
sigue guardando en `ComprobanteDetalle.Cantidad` el número de **personas** (no 1), con
`PrecioUnitario` = precio por persona. Esto significa que:

- Reimprimir un comprobante de Baños desde Facturación (`ObtenerDetalleAsync`, que lee
  `ComprobanteDetalle` directo de la base de datos) sigue mostrando "CANT 8" (o el número
  de personas real), porque ese endpoint no pasa por el cambio de frontend.
- La Boleta/Factura oficial (PDF SUNAT), si se genera a partir de estos mismos
  `ComprobanteDetalle`, probablemente también sigue mostrando la cantidad de personas en
  vez de 1.

No se corrigió ese lado a propósito: `ComprobanteDetalle.Cantidad` también alimenta el
reporte "Productos Más Vendidos" (cuenta personas atendidas por ambiente) — cambiarlo en el
origen a `Cantidad = 1` haría que ese reporte deje de reflejar cuántas personas entraron
realmente a cada servicio. El usuario decidió dejarlo así por ahora (2026-07-23) y no tocar
el backend.

### Cómo volver a como estaba antes

Si se decide revertir, en `VentasPage.tsx` (`TabBanios.handleCobrar`) volver a:

```ts
const itemsRecibo: ReciboItem[] = carrito.map(i => ({
  descripcion:    `${i.nombre} (${i.cantidad} pers.)`,
  cantidad:       i.cantidad,
  precioUnitario: i.precioUnitario,
  subtotal:       i.precioUnitario * i.cantidad,
}))
```

(Es decir, quitar el `cantidad: 1` y `precioUnitario: i.precioUnitario * i.cantidad` que se
agregaron, y devolver `cantidad`/`precioUnitario` a sus valores originales de `i`.)

---

## 3. `ImpresoraComanda:Activa` sigue en `true` en el `appsettings.json` local

**Estado:** Pendiente — el usuario decidió no tocarlo por ahora (2026-07-24).

### Qué pasó

Con `Activa: true` y `Modo: usb` (impresora `POS-80-Series`) configurado en
`Termales.API/appsettings.json` local, cada comprobante/comanda emitido durante pruebas locales
(incluidas las que hice yo vía Playwright a lo largo de esta conversación) intenta mandarse a esa
impresora por el spooler de Windows. Como la impresora física no estaba conectada la mayor parte
del tiempo, Windows fue acumulando esos trabajos en cola en vez de fallar. Al conectar el cable el
2026-07-24, salieron **44 comandas** de golpe (acumuladas desde el 12 de julio). Se vaciaron con
`Get-PrintJob -PrinterName "POS-80-Series" | Remove-PrintJob`.

### Cómo evitar que se repita

Mientras la impresora física no esté conectada durante pruebas locales, poner
`"ImpresoraComanda": { "Activa": false, ... }` en el `appsettings.json` local (nunca en el de
producción) antes de probar — así el backend no intenta mandar nada a la cola de Windows. El
usuario prefirió no hacer este cambio por ahora.

### Nota importante

Vaciar la cola solo borra lo que ya estaba pendiente. Si `Activa` se queda en `true` y se sigue
emitiendo/probando localmente sin la impresora conectada, la cola va a volver a acumularse.

---

## 4. Rediseño de la comanda de cocina — reaplicado (2026-07-25)

**Estado:** Reaplicado. Se había revertido el 2026-07-24 por sospecha de que causaba que dejara de
imprimir en producción; luego se confirmó que la causa real era otra (el usuario la resolvió por su
cuenta sin tocar este archivo), así que el 2026-07-25 se reaplicó sin cambios adicionales.
`ComandaPrinterService.cs` quedó restaurado exactamente a la versión del commit `9d6a13e` (la que ya
incluye el fix de fecha sin `CultureInfo`, para evitar el problema de ICU en Linux). Ojo al desplegar:
vigilar que las comandas sí impriman en producción tras este cambio.

### Qué pasó

Se rediseñó `ComandaPrinterService.cs` (commits `7b834b1`, `c1d9aef`, `9d6a13e`) para que la
comanda de cocina se pareciera a `ticketComanda.jpeg`: encabezado grande (Orden #, hora-mesero,
mesa, "AMBIENTE: COMEDOR"), cabecera de columnas "Cant./Producto", letra más grande, y fecha larga
en español al pie. Tras desplegarlo, dejó de imprimir **cualquier cosa** en producción.

Se sospechó que `CultureInfo.GetCultureInfo("es-PE")` (usado para el día/mes en español) lanzaba
una excepción por falta de datos ICU en el servidor Linux, silenciosamente atrapada por el
`try/catch` de `ImprimirAsync` — se corrigió reemplazándolo por arreglos fijos de días/meses
(commit `9d6a13e`). **Aun así siguió sin imprimir nada** después de ese fix, así que la causa real
todavía no está identificada; puede ser algo distinto (el bridge/SignalR, el formato de fecha
`HH:mm:ss`, algún carácter en `QuitarTildes`, u otra cosa no relacionada con el código en absoluto).

Ante esto, el usuario pidió revertir todo el archivo a como estaba antes de este rediseño
(commit `8e069d8`, antes de `7b834b1`) en vez de seguir depurando a ciegas en producción.

### Qué quedó

`ComandaPrinterService.cs` está de vuelta en su versión original (formato viejo: "Mesero: X",
"Orden #N dd/MM HH:mm — titulo", "MESA X" en doble alto/doble ancho, ítems con prefijo "x",
sin cabecera de columnas, sin fecha larga, sin "AMBIENTE: COMEDOR").

### Si se retoma este rediseño más adelante

Ya se descartó que el código del rediseño fuera la causa del problema de impresión (el usuario lo
resolvió por su cuenta, sin tocar `ComandaPrinterService.cs`). Al reaplicar, basta con volver a
traer los cambios de los commits `7b834b1` (rediseño), `c1d9aef` (letra más grande) y `9d6a13e`
(fecha sin `CultureInfo`, para evitar el problema de ICU en Linux — esa parte sí vale la pena
mantenerla al reaplicar).
