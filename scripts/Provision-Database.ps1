param(
    [Parameter(Mandatory = $true)]
    [string]$AdminConnectionString,
    [string]$DatabaseName = "CEI",
    [string]$AppLogin = "cei_app",
    [Parameter(Mandatory = $true)]
    [string]$AppLoginPassword
)

Add-Type -AssemblyName System.Data

$createDatabaseSql = @"
IF DB_ID(N'$DatabaseName') IS NULL
BEGIN
    CREATE DATABASE [$DatabaseName];
END;
IF SUSER_ID(N'$AppLogin') IS NULL
BEGIN
    CREATE LOGIN [$AppLogin] WITH PASSWORD = N'$AppLoginPassword', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;
END
ELSE
BEGIN
    ALTER LOGIN [$AppLogin] WITH PASSWORD = N'$AppLoginPassword';
END;
"@

$adminConnection = New-Object System.Data.SqlClient.SqlConnection $AdminConnectionString
$adminConnection.Open()
$adminCommand = $adminConnection.CreateCommand()
$adminCommand.CommandText = $createDatabaseSql
$adminCommand.ExecuteNonQuery() | Out-Null
$adminConnection.Close()

$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $AdminConnectionString
$builder.InitialCatalog = $DatabaseName

$grantAccessSql = @"
IF DATABASE_PRINCIPAL_ID(N'$AppLogin') IS NULL
BEGIN
    CREATE USER [$AppLogin] FOR LOGIN [$AppLogin];
END;
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members rm
    JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
    JOIN sys.database_principals m ON rm.member_principal_id = m.principal_id
    WHERE r.name = N'db_owner' AND m.name = N'$AppLogin')
BEGIN
    ALTER ROLE [db_owner] ADD MEMBER [$AppLogin];
END;
"@

$dbConnection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
$dbConnection.Open()
$dbCommand = $dbConnection.CreateCommand()
$dbCommand.CommandText = $grantAccessSql
$dbCommand.ExecuteNonQuery() | Out-Null
$dbConnection.Close()

Write-Host "Provisioned database '$DatabaseName' and login '$AppLogin'."
