// Uso: dotnet script GenerarHash.csx "TuNuevaPassword"
#r "nuget: BCrypt.Net-Next, 4.0.3"
var password = Args.Count > 0 ? Args[0] : "Admin123!";
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(password));
