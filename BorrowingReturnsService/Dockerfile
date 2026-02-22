# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Optional: Copy solution if exists
COPY *.sln .

# Copy project file
COPY BorrowingReturnsService/BorrowingReturnsService/BorrowingReturnsService.csproj ./BorrowingReturnsService/
RUN dotnet restore ./BorrowingReturnsService/BorrowingReturnsService.csproj

# Copy the rest of the source
COPY BorrowingReturnsService/ ./BorrowingReturnsService/

# Publish
RUN dotnet publish ./BorrowingReturnsService/BorrowingReturnsService.csproj -c Release -o /app/publish /p:UseAppHost=false


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "BorrowingReturnsService.dll"]

