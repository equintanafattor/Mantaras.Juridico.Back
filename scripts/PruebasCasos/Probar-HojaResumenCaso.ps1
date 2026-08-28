[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [hashtable]$Headers,

    [string]$ApiBaseUrl = "http://localhost:5024",

    [switch]$ConfirmarEscrituras
)

$ErrorActionPreference = "Stop"
$ApiBaseUrl = $ApiBaseUrl.TrimEnd("/")

if (-not $ConfirmarEscrituras) {
    throw "Falta -ConfirmarEscrituras. Ejecutar solamente contra desarrollo."
}

if ([string]::IsNullOrWhiteSpace([string]$Headers["Authorization"])) {
    throw "Falta la autenticacion en Headers."
}

$marca = [Guid]::NewGuid().ToString("N")
$prefijo = "PRUEBA HOJA $marca"
$diario = Join-Path $PSScriptRoot "registros-hoja-$marca.csv"

$clienteIdPrueba = $null
$casoIdPrueba = $null
$fallo = $null
$erroresLimpieza = @()
$script:verificaciones = @()

function Comprobar {
    param(
        [bool]$Condicion,
        [string]$Nombre
    )

    if (-not $Condicion) {
        throw "Fallo: $Nombre"
    }

    $script:verificaciones += [PSCustomObject]@{
        Prueba = $Nombre
        Resultado = "OK"
    }
}

function Invoke-Prueba {
    param(
        [string]$Metodo,
        [string]$Ruta,
        [int]$Esperado,
        $Cuerpo = $null,
        [switch]$SinToken
    )

    $parametros = @{
        Method = $Metodo
        Uri = "$ApiBaseUrl$Ruta"
        UseBasicParsing = $true
        ErrorAction = "Stop"
    }

    if (-not $SinToken) {
        $parametros["Headers"] = $Headers
    }

    if ($null -ne $Cuerpo) {
        $json = ConvertTo-Json -InputObject $Cuerpo -Depth 10 -Compress
        $parametros["ContentType"] = "application/json; charset=utf-8"
        $parametros["Body"] = [Text.Encoding]::UTF8.GetBytes($json)
    }

    $contenido = $null

    try {
        $respuesta = Invoke-WebRequest @parametros
        $estadoHttp = [int]$respuesta.StatusCode
        $contenido = $respuesta.Content
    }
    catch {
        if ($null -eq $_.Exception.Response) {
            throw
        }

        $estadoHttp = [int]$_.Exception.Response.StatusCode
    }

    Comprobar ($estadoHttp -eq $Esperado) (
        "{0} {1}: esperado {2}, recibido {3}" -f
        $Metodo, $Ruta, $Esperado, $estadoHttp
    )

    if (-not [string]::IsNullOrWhiteSpace([string]$contenido)) {
        return ConvertFrom-Json -InputObject $contenido
    }
}

function Registrar-Id {
    param(
        [string]$Entidad,
        [long]$Id
    )

    [PSCustomObject]@{
        Marca = $prefijo
        Entidad = $Entidad
        Id = $Id
    } | Export-Csv -LiteralPath $diario -Append -NoTypeInformation -Encoding UTF8

    Write-Host "Creado para prueba: $Entidad/$Id"
}

function Comprobar-Datos {
    param(
        $Hoja,
        [hashtable]$Esperados,
        [string]$Etapa
    )

    foreach ($nombre in $Esperados.Keys) {
        $propiedad = $Hoja.PSObject.Properties[$nombre]

        Comprobar ($null -ne $propiedad) "$Etapa / existe $nombre"

        $actual = $propiedad.Value
        $esperado = $Esperados[$nombre]

        if ($null -eq $esperado) {
            $coincide = $null -eq $actual
        }
        elseif ($esperado -is [decimal]) {
            $coincide = ($null -ne $actual) -and ([decimal]$actual -eq $esperado)
        }
        else {
            $coincide = $actual -ceq $esperado
        }

        Comprobar $coincide "$Etapa / valor $nombre"
    }
}

function Obtener-Foto {
    param($Valor)

    return ConvertTo-Json -InputObject $Valor -Depth 10 -Compress
}

Write-Host "API: $ApiBaseUrl"
Write-Host "Marca: $prefijo"
Write-Host "Diario de IDs: $diario"
Write-Host "Solo desarrollo: localhost no garantiza que la BD sea de desarrollo."

try {
    # Usamos una fase existente y valida, sin modificar ese caso.
    $lista = Invoke-Prueba -Metodo GET -Ruta "/api/casos?page=1&pageSize=1" -Esperado 200
    $referencia = $lista.items | Select-Object -First 1

    Comprobar ($null -ne $referencia) "Existe caso de referencia"
    Comprobar ($null -ne $referencia.faseInterna) "Fase de referencia disponible"

    $rutaInexistente = "/api/casos/9223372036854775807/hoja-resumen"

    $null = Invoke-Prueba -Metodo GET -Ruta $rutaInexistente -Esperado 404
    $null = Invoke-Prueba -Metodo PUT -Ruta $rutaInexistente -Esperado 404 -Cuerpo @{}

    # Crear registros propios.
    $cliente = Invoke-Prueba -Metodo POST -Ruta "/api/clientes" -Esperado 201 -Cuerpo @{
        nombre = "PRUEBA HOJA"
        apellido = $marca
    }

    $clienteIdPrueba = [long]$cliente.clienteId
    Comprobar ($clienteIdPrueba -gt 0) "Cliente propio creado"
    Registrar-Id "clientes" $clienteIdPrueba

    $nuevoCaso = Invoke-Prueba -Metodo POST -Ruta "/api/casos" -Esperado 201 -Cuerpo @{
        titulo = $prefijo
        faseInterna = $referencia.faseInterna
        tipoTramite = "Prueba de hoja resumen"
        clientes = @(
            @{
                clienteId = $clienteIdPrueba
                tipoParticipacion = "Titular"
                esPrincipal = $true
            }
        )
    }

    $casoIdPrueba = [long]$nuevoCaso.casoId
    Comprobar ($casoIdPrueba -gt 0) "Caso propio creado"
    Registrar-Id "casos" $casoIdPrueba

    $rutaHoja = "/api/casos/$casoIdPrueba/hoja-resumen"

    # Ambos endpoints deben exigir autenticacion.
    $null = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 401 -SinToken
    $null = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 401 -Cuerpo @{} -SinToken

    $datos = @{
        tieneCalculoPrevio = $false
        haberInicialReajustadoCaracteristicas = "  Caracteristicas de prueba  "
        haberInicialPbu = [decimal]0
        haberInicialObservacion = "  Observacion inicial  "
        haberInicialMonto = [decimal]100000.50
        movilidadActualizacionMes = 8
        movilidadActualizacionAnio = 2026
        movilidadObservaciones = "  Movilidad de prueba  "
        movilidadMonto = [decimal]120000.75
        retroactivoFechaInicio = "2025-01-01"
        retroactivoFechaActualizacion = "2026-08-01"
        retroactivoObservacion = "  Retroactivo de prueba  "
        retroactivoMonto = [decimal]200000.25
    }

    $vacios = @{}
    $esperados = $datos.Clone()

    foreach ($nombre in $datos.Keys) {
        $vacios[$nombre] = $null

        if ($esperados[$nombre] -is [string]) {
            $esperados[$nombre] = $esperados[$nombre].Trim()
        }
    }

    # Leer dos veces no debe registrar una hoja.
    foreach ($numero in @(1, 2)) {
        $inicial = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200

        Comprobar ($inicial.casoId -eq $casoIdPrueba) "GET inicial $numero / caso correcto"
        Comprobar ($inicial.registrada -eq $false) "GET inicial $numero / sin registrar"
        Comprobar ($null -eq $inicial.fechaCreacion) "GET inicial $numero / sin fecha alta"

        Comprobar-Datos $inicial $vacios "GET inicial $numero"
    }

    # Primera escritura y lectura desde la BD.
    $creada = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 200 -Cuerpo $datos
    Comprobar ($creada.registrada -eq $true) "PUT / hoja registrada"
    Comprobar-Datos $creada $esperados "PUT inicial"

    $persistida = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200
    Comprobar-Datos $persistida $esperados "Alta persistida"

    Comprobar ($persistida.registrada -eq $true) "GET / hoja registrada"
    Comprobar (-not [string]::IsNullOrWhiteSpace([string]$persistida.fechaCreacion)) "Fecha alta informada"
    Comprobar (-not [string]::IsNullOrWhiteSpace([string]$persistida.usuarioCreacion)) "Autor de alta informado"
    Comprobar ($null -eq $persistida.fechaModificacion) "Alta sin fecha de modificacion"

    $fechaAlta = $persistida.fechaCreacion
    $autorAlta = $persistida.usuarioCreacion

    # Editar, incluyendo el limite valido de texto.
    $edicion = $datos.Clone()
    $edicion["tieneCalculoPrevio"] = $true
    $edicion["haberInicialReajustadoCaracteristicas"] = ("X" * 2000)
    $edicion["haberInicialPbu"] = [decimal]25000.25

    $esperadosEdicion = $esperados.Clone()
    $esperadosEdicion["tieneCalculoPrevio"] = $true
    $esperadosEdicion["haberInicialReajustadoCaracteristicas"] = ("X" * 2000)
    $esperadosEdicion["haberInicialPbu"] = [decimal]25000.25

    $null = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 200 -Cuerpo $edicion
    $editada = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200

    Comprobar-Datos $editada $esperadosEdicion "Edicion persistida"
    Comprobar ($editada.fechaCreacion -eq $fechaAlta) "Edicion conserva fecha alta"
    Comprobar ($editada.usuarioCreacion -eq $autorAlta) "Edicion conserva autor alta"
    Comprobar ($null -ne $editada.fechaModificacion) "Edicion registra fecha modificacion"
    Comprobar (-not [string]::IsNullOrWhiteSpace([string]$editada.usuarioModificacion)) "Edicion registra autor"

    # Repetir el mismo guardado no debe alterar datos ni auditoria.
    $fotoAntes = Obtener-Foto $editada

    $null = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 200 -Cuerpo $edicion
    $sinCambios = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200

    Comprobar ((Obtener-Foto $sinCambios) -ceq $fotoAntes) "Guardado identico conserva datos y auditoria"

    # Validaciones: cada rechazo debe conservar la hoja completa.
    $invalidos = @()

    foreach ($campo in @(
        "haberInicialReajustadoCaracteristicas",
        "haberInicialObservacion",
        "movilidadObservaciones",
        "retroactivoObservacion"
    )) {
        $invalidos += @{ Campo = $campo; Valor = ("X" * 2001); Nombre = "$campo / texto largo" }
    }

    foreach ($campo in @(
        "haberInicialPbu",
        "haberInicialMonto",
        "movilidadMonto",
        "retroactivoMonto"
    )) {
        $invalidos += @{ Campo = $campo; Valor = [decimal]1.001; Nombre = "$campo / tres decimales" }
        $invalidos += @{ Campo = $campo; Valor = [decimal]10000000000000000; Nombre = "$campo / fuera de rango" }
    }

    $invalidos += @{ Campo = "movilidadActualizacionMes"; Valor = 0; Nombre = "Mes cero" }
    $invalidos += @{ Campo = "movilidadActualizacionMes"; Valor = 13; Nombre = "Mes trece" }
    $invalidos += @{ Campo = "movilidadActualizacionAnio"; Valor = 0; Nombre = "Anio cero" }
    $invalidos += @{ Campo = "movilidadActualizacionAnio"; Valor = 10000; Nombre = "Anio fuera de rango" }
    $invalidos += @{ Campo = "movilidadActualizacionMes"; Valor = $null; Nombre = "Anio sin mes" }
    $invalidos += @{ Campo = "movilidadActualizacionAnio"; Valor = $null; Nombre = "Mes sin anio" }

    foreach ($prueba in $invalidos) {
        $invalido = $edicion.Clone()
        $invalido[$prueba.Campo] = $prueba.Valor

        $null = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 400 -Cuerpo $invalido
        $despues = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200

        Comprobar ((Obtener-Foto $despues) -ceq $fotoAntes) (
            "$($prueba.Nombre) / rechazo conserva datos y auditoria"
        )
    }

    # Null explicito limpia los campos, sin eliminar la hoja.
    $null = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 200 -Cuerpo $vacios
    $limpia = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200

    Comprobar-Datos $limpia $vacios "Null explicito"
    Comprobar ($limpia.registrada -eq $true) "Limpiar no elimina la hoja"
    Comprobar ($limpia.fechaCreacion -eq $fechaAlta) "Limpiar conserva fecha alta"

    # Restaurar datos y comprobar que omitir tambien limpia.
    $null = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 200 -Cuerpo $datos
    $restaurada = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200
    Comprobar-Datos $restaurada $esperados "Datos restaurados"

    $null = Invoke-Prueba -Metodo PUT -Ruta $rutaHoja -Esperado 200 -Cuerpo @{}
    $omitida = Invoke-Prueba -Metodo GET -Ruta $rutaHoja -Esperado 200

    Comprobar-Datos $omitida $vacios "Campos omitidos"
    Comprobar ($omitida.registrada -eq $true) "PUT vacio conserva hoja registrada"
    Comprobar ($omitida.fechaCreacion -eq $fechaAlta) "PUT vacio conserva fecha alta"
}
catch {
    $fallo = $_.Exception.Message
}
finally {
    if ($casoIdPrueba -gt 0) {
        try {
            $null = Invoke-Prueba -Metodo DELETE -Ruta "/api/casos/$casoIdPrueba" -Esperado 204
            $casoInactivo = Invoke-Prueba -Metodo GET -Ruta "/api/casos/$casoIdPrueba" -Esperado 200
            Comprobar ($casoInactivo.activo -eq $false) "Caso de prueba inactivo"
        }
        catch {
            $erroresLimpieza += "Caso ${casoIdPrueba}: $($_.Exception.Message)"
        }
    }

    if ($clienteIdPrueba -gt 0) {
        try {
            $null = Invoke-Prueba -Metodo DELETE -Ruta "/api/clientes/$clienteIdPrueba" -Esperado 204
            $clienteInactivo = Invoke-Prueba -Metodo GET -Ruta "/api/clientes/$clienteIdPrueba" -Esperado 200
            Comprobar ($clienteInactivo.activo -eq $false) "Cliente de prueba inactivo"
        }
        catch {
            $erroresLimpieza += "Cliente ${clienteIdPrueba}: $($_.Exception.Message)"
        }
    }
}

$script:verificaciones | Format-Table -AutoSize -Wrap

if ($erroresLimpieza.Count -gt 0) {
    Write-Warning ("Revisar limpieza: " + ($erroresLimpieza -join " | "))
}

if ($null -ne $fallo) {
    throw "Prueba interrumpida: $fallo. Marca: $prefijo. Diario: $diario"
}

if ($erroresLimpieza.Count -gt 0) {
    throw "Las pruebas terminaron, pero no se pudo completar la baja de los registros propios."
}

Write-Host ""
Write-Host "OK: $($script:verificaciones.Count) verificaciones."
Write-Host "El cliente y el caso propios quedaron inactivos."
Write-Host "La hoja permanece asociada al caso inactivo."