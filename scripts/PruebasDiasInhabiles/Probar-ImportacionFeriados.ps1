#requires -Version 5.1
# Prueba de punta a punta de la importacion de feriados nacionales contra la
# fuente oficial real. Escribe dias inhabiles, un tipo de agenda, una regla y
# entradas de agenda: usar solo con una API local apuntando a una base aislada.
[CmdletBinding()]
param(
    [string]$ApiBaseUrl = "http://localhost:5099",
    [Parameter(Mandatory = $true)]
    [hashtable]$Headers,
    [int]$Anio = 2026,
    [switch]$ConfirmarEscrituras
)

$ErrorActionPreference = "Stop"
if (-not $ConfirmarEscrituras) {
    throw "Confirmar primero que la API apunta a una base aislada y usar -ConfirmarEscrituras."
}
$destino = [Uri]$ApiBaseUrl
if (-not $destino.IsAbsoluteUri -or -not $destino.IsLoopback) {
    throw "Solo se admite una API local."
}
if ([string]::IsNullOrWhiteSpace([string]$Headers["Authorization"])) {
    throw "Falta la sesion en Headers."
}
$ApiBaseUrl = $ApiBaseUrl.TrimEnd("/")
$marca = [Guid]::NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant()
$script:verificaciones = @()

function Comprobar([bool]$Condicion, [string]$Nombre) {
    if (-not $Condicion) { throw "Fallo: $Nombre" }
    $script:verificaciones += [PSCustomObject]@{ Prueba = $Nombre; Resultado = "OK" }
}

function Invoke-Api([string]$Metodo, [string]$Ruta, $Cuerpo = $null) {
    $parametros = @{
        Method = $Metodo; Uri = "$ApiBaseUrl$Ruta"; Headers = $Headers
        UseBasicParsing = $true; TimeoutSec = 60; ErrorAction = "Stop"
    }
    if ($null -ne $Cuerpo) {
        $parametros.ContentType = "application/json; charset=utf-8"
        $parametros.Body = [Text.Encoding]::UTF8.GetBytes((ConvertTo-Json -InputObject $Cuerpo -Depth 10 -Compress))
    }
    try {
        $respuesta = Invoke-WebRequest @parametros
        $texto = [Text.Encoding]::UTF8.GetString($respuesta.RawContentStream.ToArray())
        return [PSCustomObject]@{ Status = [int]$respuesta.StatusCode; Data = ($texto | ConvertFrom-Json) }
    }
    catch {
        if ($null -eq $_.Exception.Response) { throw }
        $lector = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream(), [Text.Encoding]::UTF8)
        $texto = $lector.ReadToEnd()
        $datos = $null
        if ($texto) { try { $datos = $texto | ConvertFrom-Json } catch { } }
        return [PSCustomObject]@{ Status = [int]$_.Exception.Response.StatusCode; Data = $datos }
    }
}

$base = "/api/dias-inhabiles/importacion-nacional"

$r = Invoke-Api GET "$base/1999/vista-previa"
Comprobar ($r.Status -eq 400 -and $r.Data.errors[0].code -eq "DiasInhabiles.DatosInvalidos") "Anio fuera de rango -> 400"

$r = Invoke-Api GET "$base/2027/vista-previa"
Comprobar ($r.Status -eq 404 -and $r.Data.errors[0].code -eq "DiasInhabiles.AnioNoDisponible") "Anio no publicado (2027) -> 404"

$estadoInicial = (Invoke-Api GET "$base/estado?anio=$Anio").Data
Write-Host "Estado inicial ${Anio}: calendarioCargado=$($estadoInicial.calendarioCargado)"

$r = Invoke-Api GET "$base/$Anio/vista-previa"
Comprobar ($r.Status -eq 200) "Vista previa $Anio -> 200"
$vista = $r.Data
Write-Host ("Vista previa: nuevos={0} existentes={1} actualizables={2} conflictos={3} revision={4} omitidos={5}" -f `
    $vista.resumen.nuevos, $vista.resumen.existentes, $vista.resumen.actualizables, `
    $vista.resumen.conflictos, $vista.resumen.requierenRevision, $vista.resumen.omitidos)
Comprobar ($vista.huella -match "^[0-9a-f]{64}$") "La vista previa devuelve huella"
Comprobar (@($vista.items | Where-Object { $_.categoria -ne "FeriadoNacional" }).Count -eq 0) "Items contiene solo feriados nacionales"
Comprobar (@($vista.items | Where-Object { $_.registrosFuente.tipo -notcontains "inamovible" -and $_.registrosFuente.tipo -notcontains "trasladable" }).Count -eq 0) "Items sin feriado nacional van a revision"

$r = Invoke-Api POST "$base/$Anio" @{ huella = ("0" * 64) }
Comprobar ($r.Status -eq 409 -and $r.Data.errors[0].code -eq "DiasInhabiles.VistaPreviaDesactualizada") "Huella ajena -> 409"

$r = Invoke-Api POST "$base/$Anio" @{ huella = $vista.huella }
# Si el calendario no cambio desde la ultima confirmacion, la huella coincide y
# la API responde yaAplicada (re-ejecucion del script).
Comprobar ($r.Status -eq 200) "Confirmacion -> 200"
Comprobar ($r.Data.yaAplicada -or $r.Data.creados -eq $vista.resumen.nuevos) "Crea exactamente los nuevos de la vista previa"
Write-Host "Confirmacion: creados=$($r.Data.creados) existentes=$($r.Data.existentes) conflictos=$($r.Data.conflictos)"

$r = Invoke-Api POST "$base/$Anio" @{ huella = $vista.huella }
Comprobar ($r.Status -eq 200 -and $r.Data.yaAplicada) "Reintento con la misma huella -> yaAplicada"

$segunda = (Invoke-Api GET "$base/$Anio/vista-previa").Data
Comprobar ($segunda.resumen.nuevos -eq 0) "Segunda vista previa sin nuevos"
$r = Invoke-Api POST "$base/$Anio" @{ huella = $segunda.huella }
Comprobar ($r.Status -eq 200 -and $r.Data.yaAplicada -and $r.Data.creados -eq 0 -and $r.Data.actualizados -eq 0) "Segunda importacion identica sin cambios"

$estado = (Invoke-Api GET "$base/estado?anio=$Anio").Data
Comprobar ($estado.calendarioCargado -and $null -ne $estado.ultimaSincronizacion) "Estado del anio: calendario cargado"
$estadoSiguiente = (Invoke-Api GET "$base/estado?anio=$($Anio + 1)").Data
Comprobar (-not $estadoSiguiente.calendarioCargado) "Anio siguiente sin calendario"

$listado = (Invoke-Api GET "/api/dias-inhabiles?anio=$Anio&pageSize=100").Data
$importado = $listado.items | Where-Object { $_.origen -eq "FeriadoNacional" } | Select-Object -First 1
Comprobar ($null -ne $importado -and $importado.fuente -eq "argentina.gob.ar" -and $importado.fechaSincronizacion) "El listado informa origen, fuente y sincronizacion"

# Calculo que cruza de anio con el siguiente sin calendario.
$tipo = Invoke-Api POST "/api/tipos-entrada-agenda" @{ nombre = "PRUEBA FERIADOS $marca" }
Comprobar ($tipo.Status -eq 201) "Alta de tipo de agenda de prueba"
$regla = Invoke-Api POST "/api/reglas-vencimiento" @{
    tipoEntradaAgendaId = $tipo.Data.tipoEntradaAgendaId; nombre = "PRUEBA FERIADOS $marca"
    cantidadDias = 10; tipoComputo = "DiasHabiles"; sentidoCalculo = "Despues"; prioridadGenerada = "Normal"
}
Comprobar ($regla.Status -eq 201) "Alta de regla de 10 dias habiles"
$aplicacion = Invoke-Api POST "/api/reglas-vencimiento/$($regla.Data.reglaVencimientoId)/aplicar" @{
    fechaBase = "$Anio-12-21"; origenFechaBase = "Manual"; claveIdempotencia = "prueba-feriados-$marca"
}
Comprobar ($aplicacion.Status -in 200, 201) "Aplicacion de la regla"
Write-Host "Fecha calculada: $($aplicacion.Data.fechaCalculada); advertencias: $(@($aplicacion.Data.advertencias).Count)"
Comprobar (@($aplicacion.Data.advertencias | Where-Object { $_.anio -eq ($Anio + 1) -and $_.codigo -eq "DiasInhabiles.AnioSinCalendario" }).Count -eq 1) "Advierte el anio siguiente sin calendario"

$script:verificaciones | Format-Table -AutoSize
