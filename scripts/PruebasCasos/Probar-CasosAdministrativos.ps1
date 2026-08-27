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
    throw "Confirmar primero que la API apunta a desarrollo y usar -ConfirmarEscrituras."
}
$destino = [Uri]$ApiBaseUrl
if (-not $destino.IsAbsoluteUri -or -not $destino.IsLoopback) {
    throw "Solo se admite una API local. Localhost no garantiza que la base sea de desarrollo."
}
if ($destino.Scheme -notin @("http", "https") -or $destino.UserInfo -or $destino.Query -or $destino.Fragment -or $destino.AbsolutePath -ne "/") {
    throw "La URL debe ser HTTP/HTTPS local, sin ruta, credenciales, query ni fragmento."
}
if ([string]::IsNullOrWhiteSpace([string]$Headers["Authorization"])) {
    throw "Falta la sesion. Renovar headers sin imprimir el token."
}
$ApiBaseUrl = $ApiBaseUrl.TrimEnd("/")
$marca = [Guid]::NewGuid().ToString("N").ToUpperInvariant()
$prefijo = "PRUEBA CASO $marca"
$anses = "ANSES-" + [Guid]::NewGuid().ToString("N").ToUpperInvariant()
$creados = New-Object 'System.Collections.Generic.List[object]'
$resultados = New-Object 'System.Collections.Generic.List[object]'
$fallo = $null
$limpiezaFallida = $false
$registroIncompleto = $false
$diario = Join-Path $PSScriptRoot "registros-casos-$marca.csv"

function Registrar-Creado {
    param([string]$Ruta, $Id, [string]$CampoMarca, [int]$Orden)
    if ($null -eq $Id -or [long]$Id -le 0) {
        $script:registroIncompleto = $true
        Write-Warning "Alta sin ID identificable en $Ruta. Revisar manualmente la marca $prefijo."
        return
    }
    $creados.Add([PSCustomObject]@{
        Api = $ApiBaseUrl; Ruta = $Ruta; Id = [long]$Id
        CampoMarca = $CampoMarca; Orden = $Orden; Marca = $prefijo
    })
    # Guardar solo IDs y marcas, nunca headers ni credenciales.
    $creados | Export-Csv -LiteralPath $diario -NoTypeInformation -Encoding UTF8
    Write-Host "Creado para prueba: $Ruta/$Id"
}

function Invoke-PruebaHttp {
    param([string]$Method, [string]$Path, $Body = $null)
    $parametros = @{
        Method = $Method; Uri = "$ApiBaseUrl$Path"; Headers = $Headers
        UseBasicParsing = $true; MaximumRedirection = 0
        TimeoutSec = 30; ErrorAction = "Stop"
    }
    if ($null -ne $Body) {
        $parametros.ContentType = "application/json; charset=utf-8"
        $parametros.Body = [Text.Encoding]::UTF8.GetBytes((ConvertTo-Json -InputObject $Body -Depth 12 -Compress))
    }
    try {
        $respuesta = Invoke-WebRequest @parametros
    }
    catch {
        if ($null -eq $_.Exception.Response) {
            if ($Method -eq "POST") { $script:registroIncompleto = $true }
            throw "Sin respuesta HTTP en $Method $Path. No reintentar automaticamente; revisar la marca $prefijo."
        }
        return [PSCustomObject]@{ Status = [int]$_.Exception.Response.StatusCode; Data = $null }
    }
    $datos = $null
    if (-not [string]::IsNullOrWhiteSpace([string]$respuesta.Content)) {
        try { $datos = ConvertFrom-Json -InputObject ([string]$respuesta.Content) }
        catch {
            if ($Method -eq "POST") { $script:registroIncompleto = $true }
            throw "Respuesta no JSON en $Method $Path. Revisar la marca $prefijo."
        }
    }
    # Registrar altas incluso si una prueba esperaba 400 pero el servidor creo datos.
    if ($Method -eq "POST" -and [int]$respuesta.StatusCode -ge 200 -and [int]$respuesta.StatusCode -lt 300) {
        switch ($Path) {
            "/api/clientes" { Registrar-Creado $Path $datos.clienteId "apellido" 2 }
            "/api/tipos-beneficio" { Registrar-Creado $Path $datos.tipoBeneficioId "nombre" 3 }
            "/api/tipos-expediente-administrativo" { Registrar-Creado $Path $datos.tipoExpedienteAdministrativoId "nombre" 3 }
            "/api/casos" { Registrar-Creado $Path $datos.casoId "titulo" 1 }
            "/api/casos/con-expediente-principal" {
                Registrar-Creado "/api/casos" $datos.casoId "titulo" 1
                Registrar-Creado "/api/expedientes" $datos.expedienteId "caratula" 0
            }
        }
    }
    return [PSCustomObject]@{ Status = [int]$respuesta.StatusCode; Data = $datos }
}

function Comprobar {
    param([string]$Prueba, [bool]$Condicion)
    $estado = "OK"
    if (-not $Condicion) { $estado = "FALLO" }
    $resultados.Add([PSCustomObject]@{ Prueba = $Prueba; Resultado = $estado })
    if (-not $Condicion) { throw $Prueba }
}

function Pedir {
    param([string]$Method, [string]$Path, $Body = $null, [int]$Esperado = 200)
    $r = Invoke-PruebaHttp $Method $Path $Body
    Comprobar "$Method $Path HTTP $Esperado (recibido $($r.Status))" ($r.Status -eq $Esperado)
    return $r.Data
}

function Comprobar-Administrativos {
    param($Caso, $Numero, $Beneficio, $Tipo, [string]$Etiqueta)
    $campos = @("numeroExpedienteAnses", "tipoBeneficioId", "tipoBeneficioNombre", "tipoBeneficioActivo", "tipoExpedienteAdministrativoId", "tipoExpedienteAdministrativoNombre", "tipoExpedienteAdministrativoActivo")
    $faltantes = @($campos | Where-Object { $null -eq $Caso -or $Caso.PSObject.Properties.Name -notcontains $_ })
    Comprobar "$Etiqueta / contrato completo" ($faltantes.Count -eq 0)
    Comprobar "$Etiqueta / numero ANSES" ($Caso.numeroExpedienteAnses -ceq $Numero)
    if ($null -eq $Beneficio) {
        Comprobar "$Etiqueta / beneficio sin asignacion" ($null -eq $Caso.tipoBeneficioId -and $null -eq $Caso.tipoBeneficioNombre -and $null -eq $Caso.tipoBeneficioActivo)
    }
    else {
        Comprobar "$Etiqueta / beneficio ID, nombre, estado" ($Caso.tipoBeneficioId -eq $Beneficio.Id -and $Caso.tipoBeneficioNombre -ceq $Beneficio.Nombre -and $Caso.tipoBeneficioActivo -eq $Beneficio.Activo)
    }
    if ($null -eq $Tipo) {
        Comprobar "$Etiqueta / tipo sin asignacion" ($null -eq $Caso.tipoExpedienteAdministrativoId -and $null -eq $Caso.tipoExpedienteAdministrativoNombre -and $null -eq $Caso.tipoExpedienteAdministrativoActivo)
    }
    else {
        Comprobar "$Etiqueta / tipo ID, nombre, estado" ($Caso.tipoExpedienteAdministrativoId -eq $Tipo.Id -and $Caso.tipoExpedienteAdministrativoNombre -ceq $Tipo.Nombre -and $Caso.tipoExpedienteAdministrativoActivo -eq $Tipo.Activo)
    }
}

function Nuevo-Payload {
    param([string]$Sufijo)
    return @{
        titulo = "$prefijo $Sufijo"; faseInterna = $script:fase
        tipoTramite = "PRUEBA LEGADO"
        clientes = @(@{ clienteId = $script:clienteId; tipoParticipacion = $script:participacion; esPrincipal = $true })
    }
}

function Con-Expediente {
    param([hashtable]$Caso)
    return @{
        caso = $Caso
        expediente = @{
            numeroExpediente = "JUD-" + [Guid]::NewGuid().ToString("N")
            caratula = "$prefijo PRINCIPAL"; juzgado = "PRUEBA"
            fechaInicio = $null; estadoLegal = $null
        }
    }
}

function Crear-Catalogo {
    param([string]$Ruta, [string]$CampoId, [string]$Sufijo)
    $nombre = "$prefijo $Sufijo"
    $r = Pedir "POST" $Ruta @{ nombre = $nombre } 201
    Comprobar "Catalogo $Sufijo creado" ($r.$CampoId -gt 0 -and $r.nombre -ceq $nombre -and $r.activo -eq $true)
    return [PSCustomObject]@{ Id = [long]$r.$CampoId; Nombre = $nombre; Activo = $true; Ruta = $Ruta }
}

function Cambiar-EstadoCatalogo {
    param($Catalogo, [bool]$Activo)
    $ruta = "$($Catalogo.Ruta)/$($Catalogo.Id)"
    if ($Activo) { $null = Pedir "PATCH" "$ruta/reactivar" $null 204 }
    else { $null = Pedir "DELETE" $ruta $null 204 }
    $r = Pedir "GET" $ruta
    Comprobar "Catalogo / estado persistido $Activo" ($r.activo -eq $Activo)
    $Catalogo.Activo = $Activo
}

function Comprobar-RechazoEdicion {
    param([long]$CasoId, [hashtable]$Payload, [string]$Etiqueta)
    $antes = Pedir "GET" "/api/casos/$CasoId"
    $null = Pedir "PUT" "/api/casos/$CasoId" $Payload 400
    $despues = Pedir "GET" "/api/casos/$CasoId"
    # Comparar GET contra GET: misma precision persistida, incluye auditoria y clientes.
    $a = ConvertTo-Json -InputObject $antes -Depth 12 -Compress
    $b = ConvertTo-Json -InputObject $despues -Depth 12 -Compress
    Comprobar "$Etiqueta / rechazo no altera datos ni auditoria" ($a -ceq $b)
}

function Comprobar-RechazoAltas {
    param([hashtable]$Payload, [string]$Etiqueta)
    $null = Pedir "POST" "/api/casos" $Payload 400
    $compuesto = Con-Expediente $Payload
    $null = Pedir "POST" "/api/casos/con-expediente-principal" $compuesto 400
    $busqueda = [Uri]::EscapeDataString([string]$Payload.titulo)
    $r = Pedir "GET" "/api/casos?soloActivos=false&pageSize=100&busqueda=$busqueda"
    Comprobar "$Etiqueta / sin caso creado" ($r.totalItems -eq 0)
    $numero = [Uri]::EscapeDataString([string]$compuesto.expediente.numeroExpediente)
    $r = Pedir "GET" "/api/expedientes?soloActivos=false&pageSize=100&busqueda=$numero"
    Comprobar "$Etiqueta / sin expediente huerfano" ($r.totalItems -eq 0)
}

Write-Host "Prueba Casos administrativos v1"
Write-Host "API: $ApiBaseUrl"
Write-Host "Marca: $prefijo"
Write-Host "Solo desarrollo. Se crearan registros propios y se intentara su baja logica al terminar."
Write-Host "Diario de IDs (sin tokens): $diario"

try {
    # Leer valores reales de los enums, sin editar los casos usados como referencia.
    $lista = Pedir "GET" "/api/casos?page=1&pageSize=100"
    $referencia = $lista.items | Where-Object { $null -ne $_.faseInterna -and @($_.clientes).Count -gt 0 } | Select-Object -First 1
    if ($null -eq $referencia) { throw "Se necesita un caso activo con clientes para obtener enums validos. No se realizaron altas." }
    Comprobar "API actualizada / campos nuevos existen realmente" ($referencia.PSObject.Properties.Name -contains "numeroExpedienteAnses" -and $referencia.PSObject.Properties.Name -contains "tipoBeneficioActivo" -and $referencia.PSObject.Properties.Name -contains "tipoExpedienteAdministrativoActivo")
    $script:fase = $referencia.faseInterna
    $script:participacion = @($referencia.clientes)[0].tipoParticipacion
    if ($null -eq $script:participacion) { throw "No se pudo obtener una participacion valida. No se realizaron altas." }
    $null = Pedir "GET" "/api/tipos-beneficio?page=1&pageSize=1"
    $null = Pedir "GET" "/api/tipos-expediente-administrativo?page=1&pageSize=1"
    # Long.MaxValue solo se usa si antes se comprobo que no existe en ambos catalogos.
    $inexistente = [long]::MaxValue
    $null = Pedir "GET" "/api/tipos-beneficio/$inexistente" $null 404
    $null = Pedir "GET" "/api/tipos-expediente-administrativo/$inexistente" $null 404
    "Api,Ruta,Id,CampoMarca,Orden,Marca" | Set-Content -LiteralPath $diario -Encoding UTF8

    $r = Pedir "POST" "/api/clientes" @{ nombre = "CONTROL AUTOMATICO"; apellido = $prefijo } 201
    $script:clienteId = [long]$r.clienteId
    Comprobar "Cliente propio creado" ($script:clienteId -gt 0)
    $b1 = Crear-Catalogo "/api/tipos-beneficio" "tipoBeneficioId" "B1"
    $b2 = Crear-Catalogo "/api/tipos-beneficio" "tipoBeneficioId" "B2"
    $t1 = Crear-Catalogo "/api/tipos-expediente-administrativo" "tipoExpedienteAdministrativoId" "T1"
    $t2 = Crear-Catalogo "/api/tipos-expediente-administrativo" "tipoExpedienteAdministrativoId" "T2"

    $payload = Nuevo-Payload "LEGADO"
    $r = Pedir "POST" "/api/casos" $payload 201
    $idLegado = [long]$r.casoId
    Comprobar-Administrativos $r $null $null $null "POST sin nuevos campos"
    $r = Pedir "GET" "/api/casos/$idLegado"
    Comprobar-Administrativos $r $null $null $null "GET legado"

    $payload = Nuevo-Payload "COMPLETO"
    $payload.numeroExpedienteAnses = "  $anses  "
    $payload.tipoBeneficioId = $b1.Id
    $payload.tipoExpedienteAdministrativoId = $t1.Id
    $r = Pedir "POST" "/api/casos" $payload 201
    $idCaso = [long]$r.casoId
    Comprobar-Administrativos $r $anses $b1 $t1 "POST completo"
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r $anses $b1 $t1 "GET completo"
    $r = Pedir "GET" "/api/casos?busqueda=$anses&pageSize=100"
    Comprobar "Busqueda exclusiva por ANSES" ($r.totalItems -eq 1 -and @($r.items).Count -eq 1 -and @($r.items)[0].casoId -eq $idCaso)
    Comprobar-Administrativos (@($r.items)[0]) $anses $b1 $t1 "Listado"

    $payload = Nuevo-Payload "CON PRINCIPAL"
    $payload.numeroExpedienteAnses = "OTRO-$marca"
    $payload.tipoBeneficioId = $b1.Id
    $payload.tipoExpedienteAdministrativoId = $t1.Id
    $r = Pedir "POST" "/api/casos/con-expediente-principal" (Con-Expediente $payload) 201
    $idCompuesto = [long]$r.casoId
    $idExpediente = [long]$r.expedienteId
    Comprobar-Administrativos $r "OTRO-$marca" $b1 $t1 "POST conjunto"
    $r = Pedir "GET" "/api/casos/$idCompuesto"
    Comprobar-Administrativos $r "OTRO-$marca" $b1 $t1 "GET conjunto"
    Comprobar "Principal vinculado" (@($r.expedientes | Where-Object { $_.expedienteId -eq $idExpediente -and $null -eq $_.expedientePadreId -and $_.tipoExpediente -ceq "Principal" }).Count -eq 1)

    $payload = Nuevo-Payload "EDITADO"
    $payload.numeroExpedienteAnses = "EDITADO-$marca"
    $payload.tipoBeneficioId = $b2.Id
    $payload.tipoExpedienteAdministrativoId = $t2.Id
    $r = Pedir "PUT" "/api/casos/$idCaso" $payload
    Comprobar-Administrativos $r "EDITADO-$marca" $b2 $t2 "PUT completo"
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r "EDITADO-$marca" $b2 $t2 "PUT persistido"
    $payload = Nuevo-Payload "OMISION"
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r "EDITADO-$marca" $b2 $t2 "Omitir conserva"

    $payload.tipoBeneficioId = $b1.Id
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r "EDITADO-$marca" $b1 $t2 "Solo beneficio"
    $payload = Nuevo-Payload "SOLO TIPO"
    $payload.tipoExpedienteAdministrativoId = $t1.Id
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r "EDITADO-$marca" $b1 $t1 "Solo tipo"
    $payload = Nuevo-Payload "SOLO NUMERO"
    $payload.numeroExpedienteAnses = "A" * 100
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r ("A" * 100) $b1 $t1 "ANSES 100 caracteres"
    $payload.numeroExpedienteAnses = "   "
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r $null $b1 $t1 "Espacios limpian numero"

    Cambiar-EstadoCatalogo $b1 $false
    Cambiar-EstadoCatalogo $t1 $false
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r $null $b1 $t1 "Historicos visibles"
    $payload = Nuevo-Payload "HISTORICOS OMITIDOS"
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r $null $b1 $t1 "Conservar historicos omitidos"
    $payload.tipoBeneficioId = $b1.Id
    $payload.tipoExpedienteAdministrativoId = $t1.Id
    $payload.numeroExpedienteAnses = "BORRAR-$marca"
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r "BORRAR-$marca" $b1 $t1 "Conservar historicos explicitos"

    $invalidos = @(
        @{ Nombre = "BENEFICIO INACTIVO"; Campo = "tipoBeneficioId"; Valor = $b1.Id },
        @{ Nombre = "TIPO INACTIVO"; Campo = "tipoExpedienteAdministrativoId"; Valor = $t1.Id },
        @{ Nombre = "BENEFICIO INEXISTENTE"; Campo = "tipoBeneficioId"; Valor = $inexistente },
        @{ Nombre = "TIPO INEXISTENTE"; Campo = "tipoExpedienteAdministrativoId"; Valor = $inexistente },
        @{ Nombre = "BENEFICIO CERO"; Campo = "tipoBeneficioId"; Valor = 0 },
        @{ Nombre = "TIPO CERO"; Campo = "tipoExpedienteAdministrativoId"; Valor = 0 },
        @{ Nombre = "BENEFICIO NEGATIVO"; Campo = "tipoBeneficioId"; Valor = -1 },
        @{ Nombre = "TIPO NEGATIVO"; Campo = "tipoExpedienteAdministrativoId"; Valor = -1 },
        @{ Nombre = "ANSES 101"; Campo = "numeroExpedienteAnses"; Valor = ("A" * 101) }
    )
    foreach ($invalido in $invalidos) {
        $payload = Nuevo-Payload $invalido.Nombre
        $payload.tipoBeneficioId = $b2.Id
        $payload.tipoExpedienteAdministrativoId = $t2.Id
        $payload.numeroExpedienteAnses = "NO GUARDAR"
        $payload[$invalido.Campo] = $invalido.Valor
        Comprobar-RechazoAltas $payload $invalido.Nombre
        Comprobar-RechazoEdicion $idLegado $payload $invalido.Nombre
    }

    $payload = Nuevo-Payload "NULL EXPLICITO"
    $payload.numeroExpedienteAnses = $null
    $payload.tipoBeneficioId = $null
    $payload.tipoExpedienteAdministrativoId = $null
    $r = Pedir "PUT" "/api/casos/$idCaso" $payload
    Comprobar-Administrativos $r $null $null $null "PUT null explicito"
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r $null $null $null "Null persistido"
    foreach ($invalido in $invalidos[0..1]) {
        $payload = Nuevo-Payload "REASIGNAR INACTIVO"
        $payload[$invalido.Campo] = $invalido.Valor
        Comprobar-RechazoEdicion $idCaso $payload "No reasignar tras limpiar / $($invalido.Nombre)"
    }
    Cambiar-EstadoCatalogo $b1 $true
    Cambiar-EstadoCatalogo $t1 $true
    $payload = Nuevo-Payload "REACTIVADOS"
    $payload.tipoBeneficioId = $b1.Id
    $payload.tipoExpedienteAdministrativoId = $t1.Id
    $null = Pedir "PUT" "/api/casos/$idCaso" $payload
    foreach ($cat in @($b1, $t1)) {
        $cat.Nombre = "$($cat.Nombre) RENOMBRADO"
        $null = Pedir "PUT" "$($cat.Ruta)/$($cat.Id)" @{ nombre = $cat.Nombre }
    }
    $r = Pedir "GET" "/api/casos/$idCaso"
    Comprobar-Administrativos $r $null $b1 $t1 "Reactivados y nombres actualizados"
}
catch {
    $fallo = $_.Exception.Message
}
finally {
    # Solo IDs devueltos por altas de esta ejecucion; validar la marca antes de cada baja.
    foreach ($item in ($creados | Sort-Object -Property @("Orden", "Id"))) {
        try {
            $ruta = "$($item.Ruta)/$($item.Id)"
            $r = Invoke-PruebaHttp "GET" $ruta
            if ($r.Status -ne 200 -or -not ([string]$r.Data.($item.CampoMarca)).StartsWith($prefijo, [StringComparison]::Ordinal)) {
                throw "No se pudo verificar propiedad del registro; no se intentara la baja."
            }
            $baja = Invoke-PruebaHttp "DELETE" $ruta
            if ($baja.Status -ne 204) { throw "Baja devolvio HTTP $($baja.Status)." }
            $r = Invoke-PruebaHttp "GET" $ruta
            if ($r.Status -ne 200 -or $r.Data.activo -ne $false) { throw "No se confirmo el estado inactivo." }
            Write-Host "Prueba desactivada: $ruta"
        }
        catch {
            $limpiezaFallida = $true
            Write-Warning "Limpieza pendiente $($item.Ruta)/$($item.Id): $($_.Exception.Message)"
        }
    }
}

$resultados | Format-Table -AutoSize
if ($fallo) { throw "Prueba interrumpida: $fallo. Marca: $prefijo. IDs: $diario" }
if ($limpiezaFallida -or $registroIncompleto) { throw "Revisar limpieza/altas no identificadas. Marca: $prefijo. IDs: $diario" }
Write-Host "OK: $($resultados.Count) verificaciones. Los $($creados.Count) registros propios quedaron inactivos."
