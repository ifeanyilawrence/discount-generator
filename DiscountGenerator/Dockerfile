FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY DiscountGenerator.Entrypoint/*.csproj ./DiscountGenerator.Entrypoint/
COPY DiscountGenerator.Application/*.csproj ./DiscountGenerator.Application/
COPY DiscountGenerator.Domain/*.csproj ./DiscountGenerator.Domain/
COPY DiscountGenerator.Infrastructure/*.csproj ./DiscountGenerator.Infrastructure/
COPY DiscountGenerator.Tests/*.csproj ./DiscountGenerator.Tests/
RUN dotnet restore

COPY . .
WORKDIR /app/DiscountGenerator.Entrypoint
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/DiscountGenerator.Entrypoint/out ./

EXPOSE 7083

ENTRYPOINT ["dotnet", "DiscountGenerator.Entrypoint.dll"]