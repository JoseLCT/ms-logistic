dotnet ef database update --project src/MsLogistic.Infrastructure --startup-project src/MsLogistic.WebApi --context PersistenceDbContext

dotnet ef migrations add GeneralUpdate --project src/MsLogistic.Infrastructure --startup-project src/MsLogistic.WebApi --context PersistenceDbContext
