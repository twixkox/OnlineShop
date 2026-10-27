<#
  Обновление PlantShop на IIS. Запускать в PowerShell от администратора из папки, где лежат этот скрипт и папка site:
    .\deploy.ps1 -SitePath "C:\inetpub\OnlineShop" -AppPool "OnlineShop"
  При первом запуске можно сразу передать строку подключения:
    .\deploy.ps1 -SitePath "C:\inetpub\OnlineShop" -AppPool "OnlineShop" -ConnectionString "Server=.\SQLEXPRESS;Database=OnlineShop;Trusted_Connection=True;TrustServerCertificate=True;"

  Что делает:
    1. проверяет, что установлен .NET 10 Hosting Bundle;
    2. проверяет, что есть appsettings.Production.json со строкой подключения (или создаёт его);
    3. делает резервную копию текущей папки сайта (без картинок и журналов);
    4. показывает посетителям страницу «Обновляем магазин», копирует новые файлы, ничего не удаляя;
    5. выдаёт пулу права на запись в Logs, App_Data и wwwroot\uploads и включает загрузку профиля пула;
    6. включает сайт, открывает его и, если он не запустился, показывает причину из журнала.
  Миграции базы применяются сами при первом запуске сайта.
#>
param(
    [Parameter(Mandatory)] [string] $SitePath,
    [Parameter(Mandatory)] [string] $AppPool,
    [string] $ConnectionString
)
$ErrorActionPreference = 'Stop'
$pkg = Join-Path $PSScriptRoot 'site'
if (-not (Test-Path (Join-Path $pkg 'OnlineShopWebApp.dll'))) { throw "Рядом со скриптом нет папки site со сборкой сайта." }

# 1. Hosting Bundle
$runtimes = & dotnet --list-runtimes 2>$null
if (-not ($runtimes -match 'Microsoft\.AspNetCore\.App 10\.')) {
    throw "Не найден ASP.NET Core 10. Установите .NET 10 Hosting Bundle (https://dotnet.microsoft.com/download/dotnet/10.0) и выполните iisreset."
}
Import-Module WebAdministration
if (-not (Test-Path "IIS:\AppPools\$AppPool")) { throw "Пул приложений «$AppPool» не найден. Имена пулов: $((Get-ChildItem IIS:\AppPools).Name -join ', ')" }
if (-not (Test-Path $SitePath)) { New-Item -ItemType Directory -Path $SitePath | Out-Null }

# 2. Настройки сервера живут в appsettings.Production.json — публикация его не затирает
$prod = Join-Path $SitePath 'appsettings.Production.json'
if ($ConnectionString) {
    $cfg = if (Test-Path $prod) { Get-Content $prod -Raw -Encoding UTF8 | ConvertFrom-Json } else { [pscustomobject]@{} }
    $cfg | Add-Member -NotePropertyName ConnectionStrings -NotePropertyValue ([pscustomobject]@{ OnlineShop = $ConnectionString }) -Force
    $cfg | ConvertTo-Json -Depth 10 | Set-Content $prod -Encoding UTF8
    Write-Host "Строка подключения записана в appsettings.Production.json"
}
elseif (-not (Test-Path $prod)) {
    # Пробуем взять строку из старого appsettings.json, если там не значение для разработки
    $old = Join-Path $SitePath 'appsettings.json'
    $oldCs = $null
    if (Test-Path $old) { $oldCs = (Get-Content $old -Raw -Encoding UTF8 | ConvertFrom-Json).ConnectionStrings.OnlineShop }
    if ($oldCs -and $oldCs -notmatch '\(localdb\)') {
        @{ ConnectionStrings = @{ OnlineShop = $oldCs } } | ConvertTo-Json -Depth 5 | Set-Content $prod -Encoding UTF8
        Write-Host "Строка подключения перенесена из старого appsettings.json в appsettings.Production.json"
    }
    else {
        throw "Нет appsettings.Production.json со строкой подключения. Запустите скрипт с параметром -ConnectionString `"Server=...;Database=...;...`" или создайте файл по образцу appsettings.Production.example.json. Сайт не тронут."
    }
}
$cs = (Get-Content $prod -Raw -Encoding UTF8 | ConvertFrom-Json).ConnectionStrings.OnlineShop
if ($cs -match '\(localdb\)') { Write-Warning "Строка подключения указывает на LocalDB — под IIS это обычно не работает. Укажите SQL Server (например, Server=.\SQLEXPRESS)." }

# Пробное подключение к базе (от имени администратора, а не пула — права пула проверятся при запуске сайта)
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection(($cs -replace 'TrustServerCertificate=True;?', ''))
    $conn.Open(); $conn.Close()
    Write-Host "Подключение к базе: OK" -ForegroundColor Green
}
catch { Write-Warning "Не удалось подключиться к базе со строкой из appsettings.Production.json: $($_.Exception.Message)" }

# 3. Резервная копия
$backup = "$SitePath.backup-$(Get-Date -Format 'yyyyMMdd-HHmm')"
robocopy $SitePath $backup /E /XD uploads Logs logs /NFL /NDL /NJH /NJS /NP | Out-Null
Write-Host "Резервная копия: $backup"

# 4. Копирование
Copy-Item (Join-Path $PSScriptRoot 'app_offline.htm') $SitePath -Force
Start-Sleep -Seconds 3   # даём IIS выгрузить приложение и отпустить файлы
robocopy $pkg $SitePath /E /XF appsettings.Production.json /R:5 /W:3 /NFL /NDL /NJH /NP
if ($LASTEXITCODE -ge 8) {
    Remove-Item (Join-Path $SitePath 'app_offline.htm') -Force
    throw "Ошибка копирования (robocopy, код $LASTEXITCODE). Сайт оставлен на старой версии."
}

# 5. Права на запись и профиль пула (без профиля ключи входа теряются при каждом перезапуске)
foreach ($dir in 'Logs', 'App_Data', 'wwwroot\uploads') {
    $p = Join-Path $SitePath $dir
    if (-not (Test-Path $p)) { New-Item -ItemType Directory -Path $p | Out-Null }
    icacls $p /grant "IIS AppPool\${AppPool}:(OI)(CI)M" /Q | Out-Null
}
Set-ItemProperty "IIS:\AppPools\$AppPool" -Name processModel.loadUserProfile -Value $true

# 6. Включение и проверка
Remove-Item (Join-Path $SitePath 'app_offline.htm') -Force
Restart-WebAppPool $AppPool -ErrorAction SilentlyContinue

$site = Get-Website | Where-Object { $_.physicalPath.TrimEnd('\') -ieq $SitePath.TrimEnd('\') } | Select-Object -First 1
if ($site) {
    $b = $site.bindings.Collection | Where-Object protocol -in 'http', 'https' | Sort-Object { $_.protocol -ne 'http' } | Select-Object -First 1
    $parts = $b.bindingInformation -split ':'
    $hostName = if ($parts[2]) { $parts[2] } else { 'localhost' }
    $url = "$($b.protocol)://${hostName}:$($parts[1])/"
    Write-Host "Проверяю $url ..."
    try {
        $r = Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 120
        Write-Host "Сайт работает (HTTP $($r.StatusCode))." -ForegroundColor Green
    }
    catch {
        Write-Warning "Сайт не ответил: $($_.Exception.Message)"
        $log = Get-ChildItem (Join-Path $SitePath 'Logs') -Filter 'app-*.log' -ErrorAction SilentlyContinue | Sort-Object LastWriteTime | Select-Object -Last 1
        $fatal = if ($log) { Get-Content $log.FullName -Encoding UTF8 | Where-Object { $_ -match '"@l":"(Fatal|Error)"' } | Select-Object -Last 1 }
        if ($fatal) {
            $e = $fatal | ConvertFrom-Json
            Write-Host "Причина из журнала:" -ForegroundColor Red
            Write-Host ($e.'@x' -split "`n" | Select-Object -First 3 | Out-String)
        }
        else {
            Write-Host "Журнал сайта пуст — смотрите «Просмотр событий» → Журналы Windows → Приложение (источник IIS AspNetCore Module V2)."
        }
        Write-Host "Вернуть прошлую версию: robocopy `"$backup`" `"$SitePath`" /E"
    }
}
else {
    Write-Host "Готово. Откройте сайт в браузере." -ForegroundColor Green
}
