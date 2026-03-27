@echo off
title Trinh khoi chay du an Khoa CNTT - TLU
color 0A
echo ======================================================
echo       DANG KHOI TAO HE THONG DOCKER (K65 TLU)
echo ======================================================

echo [1/3] Dang dung va xoa cac Container cu (neu co)...
docker-compose down

echo [2/3] Dang build va khoi chay cac thanh phan...
docker-compose up -d --build

echo.
echo Vui long cho 20 giay de SQL Server khoi dong hoan toan...
timeout /t 20 /nobreak > nul

echo [3/3] Dang nap du lieu mon hoc tu subjects.sql...
docker exec -i sql_server_tlu /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Password123!@#" -d KhoaCNTT -C -i /usr/config/subjects.sql

echo.
echo ======================================================
echo    CHUC MUNG PHONG! HE THONG DA SAN SANG!
echo ======================================================
echo - Website Frontend: http://localhost:3000
echo - Backend API (Swagger): http://localhost:5000/swagger
echo ======================================================
pause