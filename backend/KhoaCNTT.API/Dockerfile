# Giai đoạn 1: Build mã nguồn (Sử dụng SDK .NET 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy các tệp project (.csproj) để restore thư viện trước (giúp build nhanh hơn ở các lần sau)
COPY ["KhoaCNTT.API/KhoaCNTT.API.csproj", "KhoaCNTT.API/"]
COPY ["KhoaCNTT.Application/KhoaCNTT.Application.csproj", "KhoaCNTT.Application/"]
COPY ["KhoaCNTT.Domain/KhoaCNTT.Domain.csproj", "KhoaCNTT.Domain/"]
COPY ["KhoaCNTT.Infrastructure/KhoaCNTT.Infrastructure.csproj", "KhoaCNTT.Infrastructure/"]

# Thực hiện khôi phục các thư viện NuGet
RUN dotnet restore "KhoaCNTT.API/KhoaCNTT.API.csproj"

# Copy toàn bộ mã nguồn còn lại vào container
COPY . .

# Biên dịch dự án ở chế độ Release
WORKDIR "/src/KhoaCNTT.API"
RUN dotnet build "KhoaCNTT.API.csproj" -c Release -o /app/build

# Giai đoạn 2: Xuất bản ứng dụng (Publish)
FROM build AS publish
RUN dotnet publish "KhoaCNTT.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 3: Chạy ứng dụng (Sử dụng Runtime ASP.NET 8.0 cho nhẹ)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Lệnh khởi chạy file dll của tầng API
ENTRYPOINT ["dotnet", "KhoaCNTT.API.dll"]