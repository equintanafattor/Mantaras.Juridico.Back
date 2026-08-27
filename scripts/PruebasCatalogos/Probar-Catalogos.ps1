#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$ApiBaseUrl = "http://localhost:5024",
    [Parameter(Mandatory = $true)]
    [hashtable]$Headers,
    [switch]$ConfirmarEscrituras
)

$ErrorActionPreference = "Stop"

if (-not $ConfirmarEscrituras) {
    throw "Este script crea registros de prueba. Usar -ConfirmarEscrituras solo tras comprobar que la API utiliza la BD de desarrollo."
}

$destino = [Uri]$ApiBaseUrl
if (-not $destino.IsAbsoluteUri -or -not $destino.IsLoopback) {
    throw "La prueba solo admite una API local (localhost o loopback)."
}
if ($destino.Scheme -notin @("http", "https") -or $destino.UserInfo -or $destino.Query -or $destino.Fragment -or $destino.AbsolutePath -ne "/") {
    throw "Usar una URL base HTTP/HTTPS local, sin credenciales, ruta, query ni fragmento."
}
if ([string]::IsNullOrWhiteSpace([string]$Headers["Authorization"])) {
    throw "Falta Authorization. Volver a iniciar sesion y pasar los headers, sin imprimir el token."
}

$ApiBaseUrl = $ApiBaseUrl.TrimEnd("/")
$marca = [Guid]::NewGuid().ToString("N").ToUpperInvariant()
$prefijo = "PRUEBA CATALOGO $marca"
$creados = New-Object 'System.Collections.Generic.List[object]'
$resultados = New-Object 'System.Collections.Generic.List[object]'
$fallo = $null
$limpiezaFallida = $false

function Invoke-CatalogoHttp {
    param([string]$Method, [string]$Path, $Body = $null)

    $parametros = @{
        Method = $Method
        Uri = "$ApiBaseUrl$Path"
        Headers = $Headers
        UseBasicParsing = $true
        MaximumRedirection = 0
        TimeoutSec = 30
        ErrorAction = "Stop"
    }

    if ($null -ne $Body) {
        $json = ConvertTo-Json -InputObject $Body -Depth 5 -Compress
        $parametros.ContentType = "application/json; charset=utf-8"
        $parametros.Body = [Text.Encoding]::UTF8.GetBytes($json)
    }

    try {
        $respuesta = Invoke-WebRequest @parametros
    }
    catch {
        if ($null -eq $_.Exception.Response) {
            throw "No se recibio respuesta HTTP en $Method $Path. Revisar la API y la conexion."
        }

        # Las respuestas 400/401/404/409 son resultados de prueba, no reintentos.
        return [PSCustomObject]@{
            Status = [int]$_.Exception.Response.StatusCode
            Data = $null
        }
    }

    $datos = $null
    if (-not [string]::IsNullOrWhiteSpace([string]$respuesta.Content)) {
        try {
            $datos = ConvertFrom-Json -InputObject ([string]$respuesta.Content) -ErrorAction Stop
        }
        catch {
            throw "La respuesta de $Method $Path no es JSON valido."
        }
    }

    # Registrar tambien un POST inesperadamente exitoso para intentar limpiarlo.
    if ($Method -eq "POST" -and [int]$respuesta.StatusCode -eq 201 -and ([string]$datos.nombre).StartsWith($prefijo, [StringComparison]::Ordinal)) {
        $idNuevo = 0L
        if ($Path -eq "/api/tipos-beneficio") {
            $idNuevo = [long]$datos.tipoBeneficioId
        }
        elseif ($Path -eq "/api/tipos-expediente-administrativo") {
            $idNuevo = [long]$datos.tipoExpedienteAdministrativoId
        }
        if ($idNuevo -gt 0) {
            $creados.Add(@{ Ruta = $Path; Id = $idNuevo })
        }
    }

    return [PSCustomObject]@{
        Status = [int]$respuesta.StatusCode
        Data = $datos
    }
}

function Comprobar {
    param([string]$Catalogo, [string]$Prueba, [bool]$Condicion, [int]$Http, [string]$Detalle = "")
    $resultado = "OK"
    if (-not $Condicion) { $resultado = "FALLO" }
    $resultados.Add([PSCustomObject]@{
        Catalogo = $Catalogo
        Prueba = $Prueba
        HTTP = $Http
        Resultado = $resultado
    })
    if (-not $Condicion) {
        throw "$Catalogo / $Prueba (HTTP $Http). $Detalle"
    }
}

function Texto-Fecha {
    param($Fecha)
    if ($null -eq $Fecha) { return "<null>" }
    if ($Fecha -is [DateTime] -or $Fecha -is [DateTimeOffset]) {
        return $Fecha.ToString("o", [Globalization.CultureInfo]::InvariantCulture)
    }
    return [string]$Fecha
}

function Ticks-FechaUtc {
    param($Fecha)
    if ($null -eq $Fecha -or [string]::IsNullOrWhiteSpace([string]$Fecha)) {
        throw "Fecha ausente."
    }
    if ($Fecha -is [DateTime] -or $Fecha -is [DateTimeOffset]) {
        return ([DateTimeOffset]$Fecha).UtcDateTime.Ticks
    }
    $fechaParseada = [DateTimeOffset]::Parse([string]$Fecha, [Globalization.CultureInfo]::InvariantCulture)
    return $fechaParseada.UtcDateTime.Ticks
}

function Fechas-Iguales {
    param($Esperada, $Actual)
    try {
        # Igualdad exacta en UTC: no hay tolerancia ni truncamiento de decimales.
        $ticksEsperados = Ticks-FechaUtc $Esperada
        $ticksActuales = Ticks-FechaUtc $Actual
        return ($ticksEsperados -eq $ticksActuales)
    }
    catch {
        return $false
    }
}

# Autocomprobaciones del comparador, antes de hacer cualquier solicitud HTTP.
if (-not (Fechas-Iguales "2026-08-27T14:00:00.123456Z" "2026-08-27T11:00:00.1234560-03:00")) {
    throw "Fallo interno: el comparador no reconoce el mismo instante."
}
if (Fechas-Iguales "2026-08-27T14:00:00.1234567Z" "2026-08-27T14:00:00.123456Z") {
    throw "Fallo interno: el comparador esta ignorando una diferencia de fecha."
}
if ((Fechas-Iguales $null $null) -or (Fechas-Iguales "invalida" "invalida")) {
    throw "Fallo interno: el comparador acepta fechas ausentes o invalidas."
}

$catalogos = @(
    @{ Nombre = "Beneficios"; Ruta = "/api/tipos-beneficio"; Id = "tipoBeneficioId"; Limite = 100 },
    @{ Nombre = "Tipos administrativos"; Ruta = "/api/tipos-expediente-administrativo"; Id = "tipoExpedienteAdministrativoId"; Limite = 150 }
)

Write-Host "Version de prueba: 2 (referencias persistidas y diagnostico separado)"
Write-Host "API: $ApiBaseUrl"
Write-Host "Marca de los registros de prueba: $prefijo"
Write-Host "La API local debe apuntar a desarrollo; localhost no garantiza que la BD sea local."

try {
    # Comprobar la sesion antes de escribir en cualquiera de los dos catalogos.
    foreach ($cat in $catalogos) {
        $r = Invoke-CatalogoHttp "GET" $cat.Ruta
        Comprobar $cat.Nombre "Sesion y listado" ($r.Status -eq 200) $r.Status
    }

    foreach ($cat in $catalogos) {
        $ruta = $cat.Ruta
        $campoId = $cat.Id
        $nombreA = "$prefijo A"
        $nombreB = "$prefijo B"

        $r = Invoke-CatalogoHttp "POST" $ruta @{ nombre = "  prueba   catalogo $($marca.ToLowerInvariant()) a  " }
        $idA = [long]$r.Data.$campoId
        Comprobar $cat.Nombre "Alta y normalizacion" ($r.Status -eq 201 -and $idA -gt 0 -and $r.Data.nombre -ceq $nombreA -and $r.Data.activo -eq $true) $r.Status
        $fechaCreacionRespuesta = $r.Data.fechaCreacion

        # La referencia para comprobar inmutabilidad es lo guardado, no el objeto
        # en memoria devuelto por POST: PostgreSQL almacena microsegundos.
        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "Alta persistida" ($r.Status -eq 200 -and $r.Data.$campoId -eq $idA -and $r.Data.nombre -ceq $nombreA -and $r.Data.activo -eq $true) $r.Status
        $fechaCreacion = $r.Data.fechaCreacion
        Comprobar $cat.Nombre "Fecha alta persistida valida" (Fechas-Iguales $fechaCreacion $fechaCreacion) $r.Status "Fecha recibida: $(Texto-Fecha $fechaCreacion)"
        if (-not (Fechas-Iguales $fechaCreacionRespuesta $fechaCreacion)) {
            Write-Host "INFO $($cat.Nombre): fecha alta POST=$(Texto-Fecha $fechaCreacionRespuesta); GET=$(Texto-Fecha $fechaCreacion)"
        }

        $r = Invoke-CatalogoHttp "POST" $ruta @{ nombre = $nombreA.ToLowerInvariant() }
        Comprobar $cat.Nombre "Nombre duplicado" ($r.Status -eq 409) $r.Status

        $r = Invoke-CatalogoHttp "POST" $ruta @{ nombre = $nombreB }
        $idB = [long]$r.Data.$campoId
        Comprobar $cat.Nombre "Segunda alta" ($r.Status -eq 201 -and $idB -gt 0 -and $idB -ne $idA -and $r.Data.nombre -ceq $nombreB) $r.Status

        $r = Invoke-CatalogoHttp "PUT" "$ruta/$idB" @{ nombre = $nombreA }
        Comprobar $cat.Nombre "Edicion duplicada" ($r.Status -eq 409) $r.Status
        $r = Invoke-CatalogoHttp "GET" "$ruta/$idB"
        Comprobar $cat.Nombre "Conflicto no altera nombre" ($r.Status -eq 200 -and $r.Data.nombre -ceq $nombreB) $r.Status

        $nombreEditado = "$prefijo EDITADO"
        $r = Invoke-CatalogoHttp "PUT" "$ruta/$idA" @{ nombre = "  $($nombreEditado.ToLowerInvariant())  " }
        Comprobar $cat.Nombre "Editar: respuesta HTTP" ($r.Status -eq 200) $r.Status
        Comprobar $cat.Nombre "Editar: nombre normalizado" ($r.Data.nombre -ceq $nombreEditado) $r.Status "Esperado: $nombreEditado; recibido: $($r.Data.nombre)"
        Comprobar $cat.Nombre "Editar: conserva fecha alta" (Fechas-Iguales $fechaCreacion $r.Data.fechaCreacion) $r.Status "Antes: $(Texto-Fecha $fechaCreacion); despues: $(Texto-Fecha $r.Data.fechaCreacion)"
        Comprobar $cat.Nombre "Editar: fecha modificacion valida" (Fechas-Iguales $r.Data.fechaModificacion $r.Data.fechaModificacion) $r.Status "Recibida: $(Texto-Fecha $r.Data.fechaModificacion)"
        $fechaEdicionRespuesta = $r.Data.fechaModificacion

        # Tomar tambien la fecha de modificacion desde la BD para comprobar
        # que un PUT sin cambios no vuelva a escribir la auditoria.
        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "Edicion persistida" ($r.Status -eq 200 -and $r.Data.$campoId -eq $idA -and $r.Data.nombre -ceq $nombreEditado -and $r.Data.activo -eq $true) $r.Status
        Comprobar $cat.Nombre "BD conserva fecha alta" (Fechas-Iguales $fechaCreacion $r.Data.fechaCreacion) $r.Status "Antes: $(Texto-Fecha $fechaCreacion); guardada: $(Texto-Fecha $r.Data.fechaCreacion)"
        $fechaEdicion = $r.Data.fechaModificacion
        Comprobar $cat.Nombre "Fecha edicion persistida valida" (Fechas-Iguales $fechaEdicion $fechaEdicion) $r.Status "Recibida: $(Texto-Fecha $fechaEdicion)"
        if (-not (Fechas-Iguales $fechaEdicionRespuesta $fechaEdicion)) {
            Write-Host "INFO $($cat.Nombre): fecha edicion PUT=$(Texto-Fecha $fechaEdicionRespuesta); GET=$(Texto-Fecha $fechaEdicion)"
        }

        $r = Invoke-CatalogoHttp "PUT" "$ruta/$idA" @{ nombre = $nombreEditado }
        Comprobar $cat.Nombre "Edicion sin cambios" ($r.Status -eq 200 -and (Fechas-Iguales $fechaEdicion $r.Data.fechaModificacion)) $r.Status "Antes: $(Texto-Fecha $fechaEdicion); despues: $(Texto-Fecha $r.Data.fechaModificacion)"

        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "PUT sin cambios conserva BD" ($r.Status -eq 200 -and $r.Data.nombre -ceq $nombreEditado -and (Fechas-Iguales $fechaCreacion $r.Data.fechaCreacion) -and (Fechas-Iguales $fechaEdicion $r.Data.fechaModificacion)) $r.Status "Alta esperada: $(Texto-Fecha $fechaCreacion); alta guardada: $(Texto-Fecha $r.Data.fechaCreacion); edicion esperada: $(Texto-Fecha $fechaEdicion); edicion guardada: $(Texto-Fecha $r.Data.fechaModificacion)"

        $r = Invoke-CatalogoHttp "DELETE" "$ruta/$idA"
        Comprobar $cat.Nombre "Baja logica" ($r.Status -eq 204) $r.Status
        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "Detalle inactivo conservado" ($r.Status -eq 200 -and $r.Data.activo -eq $false -and $r.Data.nombre -ceq $nombreEditado) $r.Status
        $fechaBaja = $r.Data.fechaModificacion

        $r = Invoke-CatalogoHttp "DELETE" "$ruta/$idA"
        Comprobar $cat.Nombre "Baja repetida" ($r.Status -eq 204) $r.Status
        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "Baja repetida sin nueva auditoria" ($r.Status -eq 200 -and (Fechas-Iguales $fechaBaja $r.Data.fechaModificacion) -and $r.Data.activo -eq $false) $r.Status "Antes: $(Texto-Fecha $fechaBaja); despues: $(Texto-Fecha $r.Data.fechaModificacion); activo: $($r.Data.activo)"

        $busqueda = [Uri]::EscapeDataString($nombreEditado)
        $r = Invoke-CatalogoHttp "GET" "${ruta}?busqueda=$busqueda&page=1&pageSize=100"
        Comprobar $cat.Nombre "Listado omite inactivos" ($r.Status -eq 200 -and $r.Data.totalItems -eq 0) $r.Status
        $r = Invoke-CatalogoHttp "GET" "${ruta}?busqueda=$busqueda&soloActivos=false&pageSize=100"
        Comprobar $cat.Nombre "Listado incluye inactivos" ($r.Status -eq 200 -and $r.Data.totalItems -eq 1) $r.Status

        $r = Invoke-CatalogoHttp "POST" $ruta @{ nombre = $nombreEditado }
        Comprobar $cat.Nombre "Inactivo reserva nombre" ($r.Status -eq 409) $r.Status

        $r = Invoke-CatalogoHttp "PATCH" "$ruta/$idA/reactivar"
        Comprobar $cat.Nombre "Reactivar" ($r.Status -eq 204) $r.Status
        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "Detalle activo" ($r.Status -eq 200 -and $r.Data.activo -eq $true) $r.Status
        $fechaReactivacion = $r.Data.fechaModificacion
        $r = Invoke-CatalogoHttp "PATCH" "$ruta/$idA/reactivar"
        Comprobar $cat.Nombre "Reactivacion repetida" ($r.Status -eq 204) $r.Status
        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "Reactivacion sin nueva auditoria" ($r.Status -eq 200 -and (Fechas-Iguales $fechaReactivacion $r.Data.fechaModificacion) -and $r.Data.activo -eq $true) $r.Status "Antes: $(Texto-Fecha $fechaReactivacion); despues: $(Texto-Fecha $r.Data.fechaModificacion); activo: $($r.Data.activo)"

        foreach ($entrada in @(@{ nombre = "   " }, @{ nombre = $null }, @{ nombre = ("X" * ($cat.Limite + 1)) })) {
            $r = Invoke-CatalogoHttp "POST" $ruta $entrada
            Comprobar $cat.Nombre "Alta invalida" ($r.Status -eq 400) $r.Status
            $r = Invoke-CatalogoHttp "PUT" "$ruta/$idA" $entrada
            Comprobar $cat.Nombre "Edicion invalida" ($r.Status -eq 400) $r.Status
        }

        $r = Invoke-CatalogoHttp "GET" "$ruta/$idA"
        Comprobar $cat.Nombre "Validaciones no alteran registro" ($r.Status -eq 200 -and $r.Data.nombre -ceq $nombreEditado -and $r.Data.activo -eq $true -and (Fechas-Iguales $fechaReactivacion $r.Data.fechaModificacion)) $r.Status "Nombre: $($r.Data.nombre); activo: $($r.Data.activo); fecha antes: $(Texto-Fecha $fechaReactivacion); despues: $(Texto-Fecha $r.Data.fechaModificacion)"

        foreach ($query in @("page=0", "pageSize=101", "page=2147483647&pageSize=100")) {
            $r = Invoke-CatalogoHttp "GET" "${ruta}?$query"
            Comprobar $cat.Nombre "Paginacion invalida" ($r.Status -eq 400) $r.Status
        }

        $r = Invoke-CatalogoHttp "GET" "$ruta/-1"
        Comprobar $cat.Nombre "ID inexistente" ($r.Status -eq 404) $r.Status
    }
}
catch {
    $fallo = $_.Exception.Message
}
finally {
    # Solo desactivar IDs creados en esta ejecucion y que aun conservan su marca.
    foreach ($registro in $creados) {
        try {
            $path = "$($registro.Ruta)/$($registro.Id)"
            $r = Invoke-CatalogoHttp "GET" $path
            if ($r.Status -ne 200 -or -not ([string]$r.Data.nombre).StartsWith($prefijo, [StringComparison]::Ordinal)) {
                throw "No se pudo verificar la marca del registro."
            }
            $r = Invoke-CatalogoHttp "DELETE" $path
            if ($r.Status -ne 204) { throw "HTTP $($r.Status) al desactivar." }
            $r = Invoke-CatalogoHttp "GET" $path
            if ($r.Status -ne 200 -or $r.Data.activo -ne $false) {
                throw "No se pudo confirmar la baja logica."
            }
            Write-Host "Prueba desactivada: $path"
        }
        catch {
            $limpiezaFallida = $true
            Write-Warning "Revisar manualmente $($registro.Ruta)/$($registro.Id). No se pudo confirmar su baja."
        }
    }
}

$resultados | Format-Table -Property @("Catalogo", "Prueba", "HTTP", "Resultado") -AutoSize

if ($fallo) {
    throw "Prueba interrumpida: $fallo. Marca para revisar: $prefijo"
}
if ($limpiezaFallida) {
    throw "Las verificaciones terminaron, pero quedaron bajas de prueba por confirmar. Marca: $prefijo"
}

Write-Host "OK: $($resultados.Count) verificaciones. Los $($creados.Count) registros de prueba quedaron inactivos."
