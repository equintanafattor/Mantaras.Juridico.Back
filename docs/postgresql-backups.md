# Copias de seguridad de PostgreSQL

## Objetivo

Mantener copias recuperables de la base de datos PostgreSQL de Mántaras Jurídico y comprobar que puedan restaurarse correctamente.

Railway Hobby no incluye copias de seguridad nativas ni recuperación a un punto en el tiempo. Por ese motivo, el proyecto utiliza copias lógicas generadas mediante GitHub Actions.

## Implementación

El workflow se encuentra en:

```text
.github/workflows/postgres-backup.yml
```

Cada ejecución realiza el siguiente proceso:

1. Se conecta a PostgreSQL mediante la URL pública de Railway.
2. Genera un dump en formato personalizado utilizando `pg_dump`.
3. Inspecciona el contenido del archivo.
4. Restaura el dump en una instancia temporal de PostgreSQL 18.
5. Valida las tablas, migraciones y usuarios.
6. Cifra el dump validado.
7. Calcula su suma SHA-256.
8. Publica únicamente el dump cifrado y su archivo de integridad.
9. Elimina el dump sin cifrar.

## Frecuencia y retención

| Tipo | Ejecución | Retención |
|---|---|---:|
| Diaria | Todos los días excepto el primer día del mes, 03:15 de Argentina | 14 días |
| Mensual | Primer día de cada mes, 03:45 de Argentina | 90 días |
| Manual | Desde GitHub Actions | 14 días |

Los horarios se configuran con la zona:

```text
America/Argentina/Buenos_Aires
```

## Secretos requeridos

El repositorio debe contener estos secretos de GitHub Actions:

```text
RAILWAY_DATABASE_PUBLIC_URL
BACKUP_ENCRYPTION_PASSWORD
```

### `RAILWAY_DATABASE_PUBLIC_URL`

URL pública de PostgreSQL generada mediante el TCP Proxy de Railway.

No debe utilizarse la URL interna de Railway porque GitHub Actions se ejecuta fuera de su red privada.

### `BACKUP_ENCRYPTION_PASSWORD`

Contraseña utilizada para cifrar y descifrar las copias.

Debe almacenarse fuera del repositorio y conservarse en un lugar seguro. Sin esta contraseña no es posible recuperar los backups.

Nunca se deben registrar los valores de estos secretos en archivos, commits, documentación o logs.

## Formato del backup

El dump se genera mediante PostgreSQL 18:

```text
postgres:18-alpine
```

Características:

- Formato personalizado de `pg_dump`.
- Compresión nivel 9.
- Sin propietario.
- Sin privilegios.
- Cifrado AES-256-CBC.
- Derivación de clave PBKDF2.
- 200.000 iteraciones.
- Integridad mediante SHA-256.

El artefacto contiene dos archivos:

```text
mantaras-juridico-AAAAMMDD-HHMMSS.dump.enc
mantaras-juridico-AAAAMMDD-HHMMSS.dump.enc.sha256
```

## Ejecutar una copia manual

1. Abrir el repositorio en GitHub.
2. Ingresar en **Actions**.
3. Seleccionar **PostgreSQL backup**.
4. Presionar **Run workflow**.
5. Esperar que el workflow finalice correctamente.
6. Descargar el artefacto generado.

Una ejecución exitosa confirma que el dump pudo restaurarse automáticamente antes de ser cifrado y publicado.

## Validar una restauración manual

### Requisitos

- Windows PowerShell.
- Docker Desktop iniciado.
- Contenedores Linux habilitados.
- Artefacto descargado y descomprimido.
- Contraseña configurada en `BACKUP_ENCRYPTION_PASSWORD`.
- Script:

```text
scripts/validate-postgres-backup.ps1
```

No es necesario instalar PostgreSQL ni OpenSSL localmente. El procedimiento utiliza contenedores Docker.

### Contenido del directorio

El directorio del backup debe contener exactamente un dump cifrado y su archivo SHA-256:

```text
mantaras-juridico-AAAAMMDD-HHMMSS.dump.enc
mantaras-juridico-AAAAMMDD-HHMMSS.dump.enc.sha256
```

### Ejecución

Desde la raíz del repositorio:

```powershell
Set-ExecutionPolicy `
  -Scope Process `
  -ExecutionPolicy Bypass

.\scripts\validate-postgres-backup.ps1 `
  -BackupDirectory "C:\ruta\al\backup"
```

El script solicitará la contraseña de cifrado de manera segura.

### Validaciones realizadas

El script:

1. Verifica que Docker esté disponible.
2. Busca el dump cifrado y su archivo SHA-256.
3. Comprueba la integridad del archivo.
4. Solicita la contraseña sin mostrarla.
5. Descifra el dump dentro de un contenedor Alpine.
6. Inicia PostgreSQL 18 temporal.
7. Restaura el dump mediante `pg_restore`.
8. Verifica clientes, casos, expedientes, usuarios y migraciones.
9. Elimina el contenedor temporal.
10. Elimina el dump sin cifrar.
11. Elimina la contraseña de las variables del proceso.

Una ejecución correcta finaliza con:

```text
Restauración manual validada correctamente.
Limpieza finalizada.
```

## Validación inicial

Primera restauración manual validada:

```text
Fecha: 13/08/2026
Clientes: 1
Casos: 1
Expedientes: 1
Usuarios: 1
Migraciones: 9
Resultado: correcto
```

También se verificó que:

- La suma SHA-256 coincidiera.
- El backup pudiera descifrarse.
- `pg_restore` finalizara correctamente.
- Las tablas conservaran sus datos.
- El dump sin cifrar fuera eliminado.
- El contenedor temporal fuera eliminado.

## Recuperación ante un incidente

No se debe restaurar directamente sobre producción como primera medida.

Procedimiento recomendado:

1. Descargar una copia apropiada.
2. Verificar su suma SHA-256.
3. Restaurarla en una base temporal.
4. Revisar tablas, migraciones y datos.
5. Determinar el alcance del incidente.
6. Generar una copia adicional del estado actual de producción si todavía es accesible.
7. Definir si corresponde reemplazar la base completa o recuperar información puntual.
8. Programar la intervención sobre producción.
9. Verificar la aplicación después de la recuperación.
10. Registrar la fecha, el backup utilizado y el resultado.

## Limitaciones

Esta estrategia utiliza copias lógicas periódicas y no ofrece recuperación a un instante exacto entre dos ejecuciones.

El máximo de información potencialmente perdida depende del último backup disponible:

- Hasta aproximadamente 24 horas para las copias diarias.
- Más tiempo si las ejecuciones fallan y no se detecta el problema.

Los backups dependen de:

- Disponibilidad de GitHub Actions.
- Disponibilidad del TCP Proxy de Railway.
- Conservación de la contraseña de cifrado.
- Retención de los artefactos en GitHub.

Los fallos del workflow deben investigarse lo antes posible.