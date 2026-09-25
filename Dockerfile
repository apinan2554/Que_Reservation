# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# คัดลอกเฉพาะ csproj ก่อนเพื่อใช้ layer cache ตอน restore
COPY Reservation_Que.csproj ./
RUN dotnet restore Reservation_Que.csproj

# คัดลอกซอร์สที่เหลือแล้ว publish
COPY . ./
RUN dotnet publish Reservation_Que.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# ค่าเริ่มต้นเมื่อรันเอง (Render จะ override ด้วย env PORT ที่ Program.cs อ่านเอง)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Reservation_Que.dll"]
