@echo off
echo.
echo ========================================
echo   Push to GitHub
echo ========================================
echo.

:: รับ commit message จากผู้ใช้
set /p MSG="Commit message (กด Enter ถ้าไม่ระบุ = 'update'): "

:: ถ้าไม่ใส่ message ให้ใช้ 'update'
if "%MSG%"=="" set MSG=update

echo.
echo [1/3] Staging all changes...
git add .

echo [2/3] Committing: %MSG%
git commit -m "%MSG%"

echo [3/3] Pushing to GitHub...
git push

echo.
echo ========================================
echo   Done! Render will auto-deploy shortly
echo ========================================
echo.
pause
