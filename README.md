BeachHouseBrain

Install Packages 
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.InMemory
Microsoft.EntityFrameworkCore.SqlServer

To create-scaffold new models
Scaffold-DbContext "Server=CRH-LAP-106\SQLEXPRESS;Database=BeachHouseDB;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
