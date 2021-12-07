FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["./PDI.K8s.LoadBalancer_HPA.MinimalApi/PDI.K8s.LoadBalancer_HPA.MinimalApi.csproj", "PDI.K8s.LoadBalancer_HPA.MinimalApi/"]

RUN dotnet restore "PDI.K8s.LoadBalancer_HPA.MinimalApi/PDI.K8s.LoadBalancer_HPA.MinimalApi.csproj"
COPY . .
WORKDIR "PDI.K8s.LoadBalancer_HPA.MinimalApi"
RUN dotnet build "PDI.K8s.LoadBalancer_HPA.MinimalApi.csproj" -c Release -o /app/build

FROM build as publish
RUN dotnet publish "PDI.K8s.LoadBalancer_HPA.MinimalApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PDI.K8s.LoadBalancer_HPA.MinimalApi.dll"]