BeachHouseBrain

Install Packages 
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.InMemory
Microsoft.EntityFrameworkCore.SqlServer

To create-scaffold new models
Scaffold-DbContext "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -t tablename -f
