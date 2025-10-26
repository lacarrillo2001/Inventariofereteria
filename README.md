//Paquetes necesarios 

Install-Package EFCore.NamingConventions -Version 9.0.0
Install-Package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore -Version 9.0.10
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore -Version 9.0.10
Install-Package Microsoft.AspNetCore.Identity.UI -Version 9.0.10
Install-Package Microsoft.EntityFrameworkCore -Version 9.0.10
Install-Package Microsoft.EntityFrameworkCore.Abstractions -Version 9.0.10
Install-Package Microsoft.EntityFrameworkCore.Design -Version 9.0.10
Install-Package Microsoft.EntityFrameworkCore.Relational -Version 9.0.10
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 9.0.10
Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 9.0.0
Install-Package Npgsql -Version 9.0.4
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL -Version 9.0.4


//Nombre de la base de datos

Inventario_Ferreteria

//Desinstalar paquete de SQL Server


Uninstall-Package Microsoft.EntityFrameworkCore.SqlServer -ProjectName TuProyecto -RemoveDependencies

//Eliminar priermo la carpeta Migration despues ejecuta uno por uno estos comandos


Add-Migration Inv_Initial -Context ApplicationDbContext -OutputDir Migrations/Inventory

Aplicar la migración indicando el contexto

Update-Database -Context ApplicationDbContext
