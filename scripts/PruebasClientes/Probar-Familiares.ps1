param(
    [string]$ApiBaseUrl = "http://localhost:5024",

    [Parameter(Mandatory = $true)]
    [hashtable]$Headers,

    [switch]$ConfirmarEscrituras
)

$ErrorActionPreference = "Stop"

if (-not $ConfirmarEscrituras) {
    throw "Para crear registros de prueba, agrega -ConfirmarEscrituras."
}

if ([string]::IsNullOrWhiteSpace($Headers.Authorization)) {
    throw "Falta el token. Inicia sesion antes de ejecutar la prueba."
}

$direccion = $null

if (
    -not [Uri]::TryCreate(
        $ApiBaseUrl,
        [UriKind]::Absolute,
        [ref]$direccion
    ) -or
    $direccion.Scheme -notin @("http", "https")
) {
    throw "ApiBaseUrl debe ser una URL, sin formato Markdown."
}

$ApiBaseUrl = $ApiBaseUrl.TrimEnd("/")
$marca = "PRUEBA FAMILIA " + [Guid]::NewGuid().ToString("N")
$diario = Join-Path $PSScriptRoot (
    "registros-familia-" + [Guid]::NewGuid().ToString("N") + ".csv"
)

$resultados = [Collections.Generic.List[object]]::new()
$creados = [Collections.Generic.List[long]]::new()
$erroresLimpieza = [Collections.Generic.List[string]]::new()

$fallo = $null
$a = 0L
$b = 0L
$idRelacion = 0L
$fechaAlta = $null
$autorAlta = $null
$inexistente = "9223372036854775807"

# Crea el diario antes de realizar escrituras.
'"ClienteId","Marca"' | Set-Content -LiteralPath $diario -Encoding UTF8

function Comprobar {
    param(
        [bool]$Condicion,
        [string]$Descripcion
    )

    $resultados.Add([pscustomobject]@{
            Prueba    = $Descripcion
            Resultado = $(if ($Condicion) { "OK" } else { "FALLO" })
        })

    if (-not $Condicion) {
        throw $Descripcion
    }
}

function Invocar {
    param(
        [string]$Metodo,
        [string]$Ruta,
        [int[]]$Esperados = @(200),
        [object]$Cuerpo = $null,
        [switch]$SinToken
    )

    $parametros = @{
        Method          = $Metodo
        Uri             = "$ApiBaseUrl$Ruta"
        UseBasicParsing = $true
        ErrorAction     = "Stop"
        TimeoutSec      = 30
        Headers         = $(if ($SinToken) { @{} } else { $Headers })
    }

    if ($null -ne $Cuerpo) {
        $json = ConvertTo-Json -InputObject $Cuerpo -Depth 10 -Compress
        $parametros.ContentType = "application/json; charset=utf-8"
        $parametros.Body = [Text.Encoding]::UTF8.GetBytes($json)
    }

    $respuesta = $null

    try {
        $respuesta = Invoke-WebRequest @parametros
        $estado = [int]$respuesta.StatusCode
    }
    catch {
        if ($null -eq $_.Exception.Response) {
            throw
        }

        $estado = [int]$_.Exception.Response.StatusCode
    }

    Comprobar ($estado -in $Esperados) (
        "$Metodo $Ruta / HTTP $estado"
    )

    if (
        $estado -ge 200 -and
        $estado -lt 300 -and
        $estado -ne 204 -and
        $null -ne $respuesta -and
        -not [string]::IsNullOrWhiteSpace($respuesta.Content)
    ) {
        $datos = ConvertFrom-Json -InputObject $respuesta.Content

        # Devuelve cada elemento por separado.
        # Un array vacio no debe producir ningun elemento en el pipeline.
        foreach ($elemento in $datos) {
            $elemento
        }
    }
}

function Crear-ClientePrueba {
    param([string]$Nombre)

    $cliente = Invocar -Metodo POST -Ruta "/api/clientes" `
        -Esperados 201 -Cuerpo @{
        nombre   = $Nombre
        apellido = $marca
    }

    $id = [long]$cliente.clienteId

    if ($id -gt 0) {
        # Conserva el ID en memoria incluso si falla la escritura del diario.
        $creados.Add($id)

        [pscustomobject]@{
            ClienteId = $id
            Marca     = $marca
        } | Export-Csv -LiteralPath $diario `
            -Append -NoTypeInformation -Encoding UTF8

        Write-Host "Cliente propio creado: $id"
    }

    Comprobar ($id -gt 0) "Cliente $Nombre creado con ID"
    return $id
}

function Leer-Relacion {
    param(
        [long]$Propietario,
        [long]$Familiar,
        [string]$Parentesco
    )

    $lista = @(Invocar GET "/api/clientes/$Propietario/familiares")

    Comprobar ($lista.Count -eq 1) (
        "Cliente $Propietario / exactamente un familiar"
    )

    $relacion = $lista[0]

    Comprobar (
        $relacion.familiarId -eq $Familiar -and
        $relacion.parentesco -eq $Parentesco
    ) "Cliente $Propietario / familiar y parentesco $Parentesco"

    return $relacion
}

function Comprobar-SinFamiliares {
    param([long]$ClienteId)

    $lista = @(Invocar GET "/api/clientes/$ClienteId/familiares")

    Comprobar ($lista.Count -eq 0) (
        "Cliente $ClienteId / sin relaciones activas"
    )
}

function Firma {
    param([object]$Valor)

    ConvertTo-Json -InputObject $Valor -Depth 10 -Compress
}

Write-Host "API: $ApiBaseUrl"
Write-Host "Marca: $marca"
Write-Host "Diario de IDs: $diario"
Write-Host "Solo desarrollo: localhost no garantiza la BD de destino."

try {
    # Verifica sesion y rutas antes de crear registros.
    $null = Invocar GET "/api/clientes?page=1&pageSize=1"
    $null = Invocar GET "/api/clientes/$inexistente" 404
    $null = Invocar GET "/api/clientes/$inexistente/familiares" 404

    $a = Crear-ClientePrueba "UNO"
    $b = Crear-ClientePrueba "DOS"

    Comprobar ($a -ne $b) "Clientes de prueba distintos"

    Comprobar-SinFamiliares $a
    Comprobar-SinFamiliares $b

    # La falta de token no debe permitir consultar ni crear relaciones.
    $null = Invocar GET "/api/clientes/$a/familiares" 401 -SinToken

    $null = Invocar POST "/api/clientes/$a/familiares" 401 @{
        familiarId = $b
        parentesco = "Hijo"
    } -SinToken

    Comprobar-SinFamiliares $a

    $parentescos = @(
        @{ Directo = "Progenitor"; Inverso = "Hijo" }
        @{ Directo = "Hijo"; Inverso = "Progenitor" }
        @{ Directo = "Hermano"; Inverso = "Hermano" }
        @{ Directo = "Conyuge"; Inverso = "Conyuge" }
        @{ Directo = "Pareja"; Inverso = "Pareja" }
        @{ Directo = "Abuelo"; Inverso = "Nieto" }
        @{ Directo = "Nieto"; Inverso = "Abuelo" }
        @{ Directo = "Tio"; Inverso = "Sobrino" }
        @{ Directo = "Sobrino"; Inverso = "Tio" }
        @{ Directo = "OtroFamiliar"; Inverso = "OtroFamiliar" }
    )

    foreach ($par in $parentescos) {
        # Prueba desde los dos extremos de la pareja.
        foreach ($desdeA in @($true, $false)) {
            $origen = $(if ($desdeA) { $a } else { $b })
            $destino = $(if ($desdeA) { $b } else { $a })

            $null = Invocar POST "/api/clientes/$origen/familiares" 200 @{
                familiarId = $destino
                parentesco = $par.Directo
            }

            $directa = Leer-Relacion $origen $destino $par.Directo
            $inversa = Leer-Relacion $destino $origen $par.Inverso

            Comprobar (
                $directa.relacionFamiliarId -eq $inversa.relacionFamiliarId
            ) "Ambas fichas usan la misma relacion"

            if ($idRelacion -eq 0) {
                $idRelacion = [long]$directa.relacionFamiliarId
                $fechaAlta = $directa.fechaCreacion
                $autorAlta = $directa.usuarioCreacion

                Comprobar ($idRelacion -gt 0) "Relacion con ID"
                Comprobar ($null -ne $fechaAlta) "Fecha de alta informada"
                Comprobar (
                    -not [string]::IsNullOrWhiteSpace($autorAlta)
                ) "Autor de alta informado"

                Comprobar (
                    $null -eq $directa.fechaModificacion
                ) "Primera alta sin fecha de modificacion"
            }
            else {
                Comprobar (
                    $directa.relacionFamiliarId -eq $idRelacion -and
                    $directa.fechaCreacion -eq $fechaAlta -and
                    $directa.usuarioCreacion -eq $autorAlta
                ) "Reactivacion conserva ID y auditoria de alta"

                Comprobar (
                    $null -ne $directa.fechaModificacion -and
                    -not [string]::IsNullOrWhiteSpace(
                        $directa.usuarioModificacion
                    )
                ) "Reactivacion registra auditoria de modificacion"
            }

            # La vinculacion equivalente desde la otra ficha es idempotente.
            $antes = Firma $directa

            $null = Invocar POST "/api/clientes/$destino/familiares" 200 @{
                familiarId = $origen
                parentesco = $par.Inverso
            }

            $despues = Leer-Relacion $origen $destino $par.Directo

            Comprobar ((Firma $despues) -eq $antes) (
                "Repetir desde otra ficha conserva datos y auditoria"
            )

            $otro = $(if ($par.Directo -eq "Pareja") {
                    "Hermano"
                }
                else {
                    "Pareja"
                })

            $null = Invocar POST "/api/clientes/$origen/familiares" 409 @{
                familiarId = $destino
                parentesco = $otro
            }

            $despues = Leer-Relacion $origen $destino $par.Directo

            Comprobar ((Firma $despues) -eq $antes) (
                "Parentesco diferente no altera la relacion"
            )

            # Desvincular afecta las dos fichas y admite repeticion.
            $null = Invocar DELETE (
                "/api/clientes/$origen/familiares/$destino"
            ) 204

            $null = Invocar DELETE (
                "/api/clientes/$destino/familiares/$origen"
            ) 204

            Comprobar-SinFamiliares $a
            Comprobar-SinFamiliares $b
        }
    }

    # Reactiva para verificar que los rechazos no alteren una relacion.
    $null = Invocar POST "/api/clientes/$a/familiares" 200 @{
        familiarId = $b
        parentesco = "Hijo"
    }

    $referencia = Firma (Leer-Relacion $a $b "Hijo")

    $invalidos = @(
        @{ familiarId = $a; parentesco = "Hijo" }
        @{ familiarId = 0; parentesco = "Hijo" }
        @{ familiarId = -1; parentesco = "Hijo" }
        @{ familiarId = $b; parentesco = 0 }
        @{ familiarId = $b; parentesco = 11 }
        @{ familiarId = $b; parentesco = "NoExiste" }
        @{ familiarId = $b }
        @{ parentesco = "Hijo" }
    )

    foreach ($cuerpo in $invalidos) {
        $null = Invocar POST "/api/clientes/$a/familiares" 400 $cuerpo

        Comprobar (
            (Firma (Leer-Relacion $a $b "Hijo")) -eq $referencia
        ) "Solicitud invalida conserva relacion y auditoria"
    }

    $null = Invocar POST "/api/clientes/$a/familiares" 404 @{
        familiarId = [long]$inexistente
        parentesco = "Hijo"
    }

    $null = Invocar POST "/api/clientes/$inexistente/familiares" 404 @{
        familiarId = $a
        parentesco = "Hijo"
    }

    $null = Invocar DELETE (
        "/api/clientes/$a/familiares/$inexistente"
    ) 404

    $null = Invocar DELETE "/api/clientes/$a/familiares/$b" 401 -SinToken

    Comprobar (
        (Firma (Leer-Relacion $a $b "Hijo")) -eq $referencia
    ) "Rechazos 401 y 404 conservan relacion y auditoria"

    # Un familiar inactivo sigue visible.
    $null = Invocar DELETE "/api/clientes/$b" 204

    $familiarInactivo = Leer-Relacion $a $b "Hijo"
    Comprobar ($familiarInactivo.activo -eq $false) (
        "Familiar inactivo sigue visible con su estado"
    )

    $null = Leer-Relacion $b $a "Progenitor"

    # Repetir la relacion existente no requiere reactivarla.
    $null = Invocar POST "/api/clientes/$a/familiares" 200 @{
        familiarId = $b
        parentesco = "Hijo"
    }

    Comprobar (
        (Firma (Leer-Relacion $a $b "Hijo")) -eq
        (Firma $familiarInactivo)
    ) "Relacion existente con inactivo no cambia auditoria"

    # Se permite desvincular desde la ficha del cliente inactivo.
    $null = Invocar DELETE "/api/clientes/$b/familiares/$a" 204

    $null = Invocar POST "/api/clientes/$a/familiares" 400 @{
        familiarId = $b
        parentesco = "Hijo"
    }

    $null = Invocar POST "/api/clientes/$b/familiares" 400 @{
        familiarId = $a
        parentesco = "Progenitor"
    }

    Comprobar-SinFamiliares $a
    Comprobar-SinFamiliares $b

    # Al reactivar al cliente se puede recuperar la misma relacion.
    $null = Invocar PATCH "/api/clientes/$b/reactivar" 204

    $null = Invocar POST "/api/clientes/$b/familiares" 200 @{
        familiarId = $a
        parentesco = "Progenitor"
    }

    $restaurada = Leer-Relacion $a $b "Hijo"

    Comprobar (
        $restaurada.relacionFamiliarId -eq $idRelacion -and
        $restaurada.fechaCreacion -eq $fechaAlta -and
        $restaurada.usuarioCreacion -eq $autorAlta -and
        $restaurada.activo -eq $true
    ) "Cliente reactivado recupera la misma relacion"
}
catch {
    $fallo = $_.Exception.Message
}
finally {
    if ($a -gt 0 -and $b -gt 0) {
        try {
            $null = Invocar DELETE (
                "/api/clientes/$a/familiares/$b"
            ) @(204, 404)

            Comprobar-SinFamiliares $a
            Comprobar-SinFamiliares $b
        }
        catch {
            $erroresLimpieza.Add(
                "Relacion $a / $b : $($_.Exception.Message)"
            )
        }
    }

    foreach ($id in $creados) {
        try {
            $null = Invocar DELETE "/api/clientes/$id" 204
            $cliente = Invocar GET "/api/clientes/$id"

            Comprobar ($cliente.activo -eq $false) (
                "Cliente propio $id quedo inactivo"
            )

            Write-Host "Cliente propio desactivado: $id"
        }
        catch {
            $erroresLimpieza.Add(
                "Cliente $id : $($_.Exception.Message)"
            )
        }
    }
}

$resultados | Format-Table -AutoSize -Wrap | Out-Host

if ($erroresLimpieza.Count -gt 0) {
    $erroresLimpieza | ForEach-Object { Write-Warning $_ }
    throw "Limpieza incompleta. Revisa el diario: $diario"
}

if ($null -ne $fallo) {
    throw "Prueba interrumpida: $fallo. Marca: $marca"
}

Write-Host ""
Write-Host "OK: $($resultados.Count) verificaciones."
Write-Host "Los dos clientes y su relacion quedaron inactivos."