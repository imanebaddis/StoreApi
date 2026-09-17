# Stores the Supabase connection in this user's .NET user-secrets store.
# No credential is written to this repository or passed as a command-line argument.
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'StoreApi\StoreApi.csproj'
$uriText = Read-Host 'Paste the Session pooler URI from Supabase Connect (leave [YOUR-PASSWORD] in it)'
$uri = [Uri]$uriText
$user = [Uri]::UnescapeDataString(($uri.UserInfo -split ':', 2)[0])
if ($uri.Scheme -ne 'postgresql' -or
    $uri.Host -notlike '*.pooler.supabase.com' -or
    $uri.Port -ne 5432 -or
    $user -ne 'postgres.qpwjzzmnqxtexelhpfqm' -or
    $uri.AbsolutePath -ne '/postgres') {
    throw 'The URI does not match the expected Supabase project and Session pooler. No secret was saved.'
}

$securePassword = Read-Host 'Enter the new database password (hidden)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    $connection = New-Object System.Data.Common.DbConnectionStringBuilder
    $connection['Host'] = $uri.Host
    $connection['Port'] = $uri.Port
    $connection['Database'] = 'postgres'
    $connection['Username'] = $user
    $connection['Password'] = $password
    $connection['SSL Mode'] = 'Require'
    $connection['Maximum Pool Size'] = 10
    @{ 'ConnectionStrings:StoreDb' = $connection.ConnectionString } |
        ConvertTo-Json -Compress |
        dotnet user-secrets set --project $project | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Saving the local secret failed.' }
    Write-Host 'Connection saved locally for this Windows account. The password was not printed.'
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    Remove-Variable password, connection, securePassword -ErrorAction SilentlyContinue
}
